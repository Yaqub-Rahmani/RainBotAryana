using Core.Utilities.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Extensions
{
    public static class QueryableExtensions
    {
        /* ---------------- Includes ---------------- */
        public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> q, IEnumerable<string> includes)
            where T : class
        {
            if (includes == null) return q;

            foreach (var path in includes.Where(p => !string.IsNullOrWhiteSpace(p)))
                q = q.Include(path.Trim());

            return q;
        }

        /* ---------------- Sort (multi) ----------------
           supports:
             "OccurredAtUtc:desc,Amount:asc"
             "-OccurredAtUtc,+Amount"
        */
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> q, string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort)) return q;

            var parts = sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            bool isFirst = true;

            foreach (var raw in parts)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                var (field, desc) = ParseSortPart(raw);
                if (string.IsNullOrWhiteSpace(field)) continue;

                if (!TryBuildMemberAccess(typeof(T), field, out var param, out var member))
                    continue;

                var keySelector = Expression.Lambda(member, param);

                var method = isFirst
                    ? (desc ? "OrderByDescending" : "OrderBy")
                    : (desc ? "ThenByDescending" : "ThenBy");

                q = ApplyOrderingCall(q, method, member.Type, keySelector);
                isFirst = false;
            }

            return q;
        }

        private static (string field, bool desc) ParseSortPart(string raw)
        {
            var s = raw.Trim();
            if (s.StartsWith("-")) return (s[1..].Trim(), true);
            if (s.StartsWith("+")) return (s[1..].Trim(), false);

            if (s.Contains(':'))
            {
                var p = s.Split(':', 2, StringSplitOptions.TrimEntries);
                var field = p[0];
                var dir = (p.Length > 1 ? p[1] : "asc").ToLowerInvariant();
                return (field, dir == "desc" || dir == "d");
            }

            // default asc
            return (s, false);
        }

        private static IQueryable<T> ApplyOrderingCall<T>(
            IQueryable<T> q,
            string methodName,
            Type keyType,
            LambdaExpression keySelector)
        {
            var call = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { typeof(T), keyType },
                q.Expression,
                Expression.Quote(keySelector));

            return q.Provider.CreateQuery<T>(call);
        }

        /* ---------------- Filters (advanced + backward compatible) ----------------
           Supports:
             - legacy: "Field=Value"  => eq
             - token : "Field:op:value"
               ops: eq, neq, contains, startswith, endswith,
                    gt,gte,lt,lte,
                    in (comma or |),
                    from/to (range),
                    between (value "from,to"),
                    isnull (true/false)
             - meta tokens ignored here:
                 "__searchFields:in:FieldA|FieldB"
                 "__depends:..."
        */
        public static IQueryable<T> ApplyFilters<T>(
            this IQueryable<T> q,
            IEnumerable<string> filters,
            out List<string> deferredFilters) // fields not found on T -> caller can apply later
        {
            deferredFilters = new List<string>();
            if (filters == null) return q;

            // range accumulation: Field -> (from,to)
            var rangeMap = new Dictionary<string, (string? From, string? To)>(StringComparer.OrdinalIgnoreCase);
            var direct = new List<(string Field, string Op, string Value)>();

            foreach (var raw in filters)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                var s = raw.Trim();

                // meta tokens
                if (s.StartsWith("__", StringComparison.OrdinalIgnoreCase))
                    continue;

                // legacy "Field=Value"
                if (s.Contains('=') && !s.Contains(':'))
                {
                    var p = s.Split('=', 2, StringSplitOptions.TrimEntries);
                    if (p.Length == 2 && !string.IsNullOrWhiteSpace(p[0]))
                        direct.Add((p[0], "eq", p[1]));
                    continue;
                }

                // token "Field:op:value"
                var parts = s.Split(':', 3, StringSplitOptions.TrimEntries);
                if (parts.Length < 2) continue;

                var field = parts[0];
                var op = parts[1].ToLowerInvariant();
                var value = parts.Length >= 3 ? parts[2] : "";

                if (string.IsNullOrWhiteSpace(field)) continue;

                if (op == "from" || op == "to")
                {
                    if (!rangeMap.TryGetValue(field, out var cur)) cur = (null, null);
                    if (op == "from") cur.From = value;
                    else cur.To = value;
                    rangeMap[field] = cur;
                    continue;
                }

                if (op == "between")
                {
                    // value can be "from,to"
                    var pv = value.Split(',', 2, StringSplitOptions.TrimEntries);
                    var from = pv.Length > 0 ? pv[0] : "";
                    var to = pv.Length > 1 ? pv[1] : "";
                    rangeMap[field] = (string.IsNullOrWhiteSpace(from) ? null : from,
                                      string.IsNullOrWhiteSpace(to) ? null : to);
                    continue;
                }

                direct.Add((field, op, value));
            }

            // apply direct predicates
            foreach (var r in direct)
            {
                if (!TryBuildPredicate<T>(r.Field, r.Op, r.Value, out var predicate))
                {
                    deferredFilters.Add($"{r.Field}:{r.Op}:{r.Value}");
                    continue;
                }
                q = q.Where(predicate);
            }

            // apply ranges
            foreach (var kv in rangeMap)
            {
                var field = kv.Key;
                var (from, to) = kv.Value;

                if (!TryBuildBetweenPredicate<T>(field, from, to, out var predicate))
                {
                    // defer as two tokens
                    if (!string.IsNullOrWhiteSpace(from)) deferredFilters.Add($"{field}:from:{from}");
                    if (!string.IsNullOrWhiteSpace(to)) deferredFilters.Add($"{field}:to:{to}");
                    continue;
                }
                q = q.Where(predicate);
            }

            return q;
        }

        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> q, IEnumerable<string> filters)
        {
            return q.ApplyFilters(filters, out _);
        }

        /* ---------------- Search (OR on fields) ----------------
           - only applies on string fields (translatable)
           - if fields empty -> try Notes (if exists) else all string props (limited)
        */
        public static IQueryable<T> ApplySearch<T>(
            this IQueryable<T> q,
            string? search,
            IEnumerable<string>? fields)
        {
            if (string.IsNullOrWhiteSpace(search)) return q;

            var key = search.Trim();
            var fieldList = (fields ?? Array.Empty<string>()).Where(f => !string.IsNullOrWhiteSpace(f)).ToList();

            // fallback if not provided
            if (fieldList.Count == 0)
            {
                if (HasMember(typeof(T), "Notes")) fieldList.Add("Notes");
                else
                {
                    // limited fallback: top-level string props (max 5)
                    fieldList.AddRange(typeof(T)
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.PropertyType == typeof(string))
                        .Select(p => p.Name)
                        .Take(5));
                }
            }

            var param = Expression.Parameter(typeof(T), "x");
            Expression? or = null;

            foreach (var f in fieldList)
            {
                if (!TryBuildMemberAccess(typeof(T), f, out _, out var member, param))
                    continue;

                if (member.Type != typeof(string))
                    continue;

                // (x.Field ?? "").Contains(key)
                var coalesce = Expression.Coalesce(member, Expression.Constant(""));
                var contains = Expression.Call(
                    coalesce,
                    typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!,
                    Expression.Constant(key));

                or = or == null ? contains : Expression.OrElse(or, contains);
            }

            if (or == null) return q;
            var lambda = Expression.Lambda<Func<T, bool>>(or, param);
            return q.Where(lambda);
        }

        /* ---------------- Paging ---------------- */
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> q, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 10;
            return q.Skip((page - 1) * pageSize).Take(pageSize);
        }

        /* =================== Expression helpers =================== */

        private static bool TryBuildPredicate<T>(string fieldPath, string op, string rawValue, out Expression<Func<T, bool>> predicate)
        {
            predicate = default!;
            if (!TryBuildMemberAccess(typeof(T), fieldPath, out var param, out var member))
                return false;

            var memberType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;

            // isnull
            if (op == "isnull")
            {
                if (!bool.TryParse(rawValue, out var isNull)) return false;
                if (!IsNullable(member.Type)) return false; // non-nullable cannot be isnull:true
                var test = isNull
                    ? Expression.Equal(member, Expression.Constant(null, member.Type))
                    : Expression.NotEqual(member, Expression.Constant(null, member.Type));
                predicate = Expression.Lambda<Func<T, bool>>(test, param);
                return true;
            }

            // string ops
            if (memberType == typeof(string))
            {
                var left = Expression.Coalesce(member, Expression.Constant(""));
                var val = Expression.Constant(rawValue ?? "");

                Expression body = op switch
                {
                    "eq" => Expression.Equal(left, val),
                    "neq" => Expression.NotEqual(left, val),
                    "contains" => Expression.Call(left, nameof(string.Contains), Type.EmptyTypes, val),
                    "startswith" => Expression.Call(left, nameof(string.StartsWith), Type.EmptyTypes, val),
                    "endswith" => Expression.Call(left, nameof(string.EndsWith), Type.EmptyTypes, val),
                    _ => null!
                };

                if (body == null) return false;
                predicate = Expression.Lambda<Func<T, bool>>(body, param);
                return true;
            }

            // in
            if (op == "in")
            {
                var values = (rawValue ?? "")
                    .Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToArray();

                if (values.Length == 0) return false;

                // build (values.Contains(member))
                var typedList = Array.CreateInstance(memberType, values.Length);
                for (int i = 0; i < values.Length; i++)
                {
                    if (!TryConvert(values[i], memberType, out var cv)) return false;
                    typedList.SetValue(cv, i);
                }

                var enumerableType = typeof(IEnumerable<>).MakeGenericType(memberType);
                var constant = Expression.Constant(typedList, typedList.GetType());

                Expression memberValue = member;
                if (Nullable.GetUnderlyingType(member.Type) != null)
                {
                    // x.Field.HasValue && list.Contains(x.Field.Value)
                    var hasValue = Expression.Property(member, "HasValue");
                    var valueProp = Expression.Property(member, "Value");
                    var containsMethod = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
                        .MakeGenericMethod(memberType);

                    var containsCall = Expression.Call(containsMethod, constant, valueProp);
                    var bodyN = Expression.AndAlso(hasValue, containsCall);
                    predicate = Expression.Lambda<Func<T, bool>>(bodyN, param);
                    return true;
                }
                else
                {
                    var containsMethod = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2)
                        .MakeGenericMethod(memberType);

                    var body = Expression.Call(containsMethod, constant, memberValue);
                    predicate = Expression.Lambda<Func<T, bool>>(body, param);
                    return true;
                }
            }

            // comparisons
            if (!TryConvert(rawValue, memberType, out var converted))
                return false;

            var constantExpr = Expression.Constant(converted, memberType);

            Expression leftComp = member;
            if (Nullable.GetUnderlyingType(member.Type) != null)
            {
                // x.Field.HasValue && x.Field.Value OP constant
                var hasValue = Expression.Property(member, "HasValue");
                var valueProp = Expression.Property(member, "Value");
                var comp = BuildComparison(valueProp, op, constantExpr);
                if (comp == null) return false;
                var bodyN = Expression.AndAlso(hasValue, comp);
                predicate = Expression.Lambda<Func<T, bool>>(bodyN, param);
                return true;
            }
            else
            {
                var comp = BuildComparison(leftComp, op, constantExpr);
                if (comp == null) return false;
                predicate = Expression.Lambda<Func<T, bool>>(comp, param);
                return true;
            }
        }

        private static bool TryBuildBetweenPredicate<T>(string fieldPath, string? from, string? to, out Expression<Func<T, bool>> predicate)
        {
            predicate = default!;
            if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to)) return false;

            if (!TryBuildMemberAccess(typeof(T), fieldPath, out var param, out var member))
                return false;

            var memberType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;

            Expression? body = null;

            if (!string.IsNullOrWhiteSpace(from))
            {
                if (!TryConvert(from!, memberType, out var fromVal)) return false;
                var fromConst = Expression.Constant(fromVal, memberType);
                var ge = Nullable.GetUnderlyingType(member.Type) != null
                    ? (Expression)Expression.GreaterThanOrEqual(Expression.Property(member, "Value"), fromConst)
                    : Expression.GreaterThanOrEqual(member, fromConst);

                if (Nullable.GetUnderlyingType(member.Type) != null)
                    ge = Expression.AndAlso(Expression.Property(member, "HasValue"), ge);

                body = body == null ? ge : Expression.AndAlso(body, ge);
            }

            if (!string.IsNullOrWhiteSpace(to))
            {
                if (!TryConvert(to!, memberType, out var toVal)) return false;
                var toConst = Expression.Constant(toVal, memberType);
                var le = Nullable.GetUnderlyingType(member.Type) != null
                    ? (Expression)Expression.LessThanOrEqual(Expression.Property(member, "Value"), toConst)
                    : Expression.LessThanOrEqual(member, toConst);

                if (Nullable.GetUnderlyingType(member.Type) != null)
                    le = Expression.AndAlso(Expression.Property(member, "HasValue"), le);

                body = body == null ? le : Expression.AndAlso(body, le);
            }

            if (body == null) return false;
            predicate = Expression.Lambda<Func<T, bool>>(body, param);
            return true;
        }

        private static Expression? BuildComparison(Expression left, string op, Expression right)
        {
            return op switch
            {
                "eq" => Expression.Equal(left, right),
                "neq" => Expression.NotEqual(left, right),
                "gt" => Expression.GreaterThan(left, right),
                "gte" => Expression.GreaterThanOrEqual(left, right),
                "lt" => Expression.LessThan(left, right),
                "lte" => Expression.LessThanOrEqual(left, right),
                _ => null
            };
        }

        private static bool TryConvert(string? raw, Type target, out object? converted)
        {
            converted = null;
            raw ??= "";

            try
            {
                if (target == typeof(string)) { converted = raw; return true; }
                if (target == typeof(Guid)) { return Guid.TryParse(raw, out var g) ? (converted = g) != null : false; }
                if (target == typeof(bool)) { return bool.TryParse(raw, out var b) ? (converted = b) != null : false; }
                if (target == typeof(int)) { return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? (converted = i) != null : false; }
                if (target == typeof(long)) { return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l) ? (converted = l) != null : false; }
                if (target == typeof(decimal)) { return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? (converted = d) != null : false; }
                if (target == typeof(double)) { return double.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var db) ? (converted = db) != null : false; }

                if (target == typeof(DateTime))
                {
                    // accepts "YYYY-MM-DD" or full ISO
                    if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                    {
                        converted = dt;
                        return true;
                    }
                    return false;
                }

                if (target == typeof(DateOnly))
                {
                    if (DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d0))
                    {
                        converted = d0;
                        return true;
                    }
                    if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        converted = DateOnly.FromDateTime(dt);
                        return true;
                    }
                    return false;
                }

                if (target.IsEnum)
                {
                    if (Enum.TryParse(target, raw, true, out var ev))
                    {
                        converted = ev;
                        return true;
                    }
                    return false;
                }

                // fallback (may fail for complex)
                converted = Convert.ChangeType(raw, target, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryBuildMemberAccess(Type rootType, string fieldPath, out ParameterExpression param, out MemberExpression member)
        {
            param = Expression.Parameter(rootType, "x");
            return TryBuildMemberAccess(rootType, fieldPath, out _, out member, param);
        }

        private static bool TryBuildMemberAccess(Type rootType, string fieldPath, out ParameterExpression? _unused, out MemberExpression member, ParameterExpression param)
        {
            _unused = null;
            member = null!;

            if (string.IsNullOrWhiteSpace(fieldPath)) return false;

            // support "A.B.C"
            Expression cur = param;
            Type curType = rootType;

            foreach (var part in fieldPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var pi = curType.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pi != null)
                {
                    cur = Expression.Property(cur, pi);
                    curType = pi.PropertyType;
                    continue;
                }

                var fi = curType.GetField(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (fi != null)
                {
                    cur = Expression.Field(cur, fi);
                    curType = fi.FieldType;
                    continue;
                }

                return false;
            }

            member = (MemberExpression)cur;
            return true;
        }

        private static bool HasMember(Type t, string name)
        {
            return t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) != null
                || t.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) != null;
        }

        private static bool IsNullable(Type t) => !t.IsValueType || Nullable.GetUnderlyingType(t) != null;

        /* ---------------- SearchFields meta extraction ----------------
           token: "__searchFields:in:FranchiseOwnerName|Notes|ShopCode"
        */
        public static List<string> ExtractSearchFields(this QueryOptions opt, out List<string> remainingFilters)
        {
            remainingFilters = new List<string>();
            var fields = new List<string>();

            if (opt?.Filters == null) return fields;

            foreach (var raw in opt.Filters)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;

                var s = raw.Trim();
                if (s.StartsWith("__searchFields:in:", StringComparison.OrdinalIgnoreCase))
                {
                    var val = s.Substring("__searchFields:in:".Length);
                    var arr = val.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    fields.AddRange(arr);
                }
                else
                {
                    remainingFilters.Add(s);
                }
            }

            return fields.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

}



//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace Core.Extensions
//{
//    public static class QueryableExtensions
//    {
//        public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> q, IEnumerable<string> includes)
//            where T : class
//        {
//            foreach (var path in includes.Where(p => !string.IsNullOrWhiteSpace(p)))
//                q = q.Include(path.Trim());
//            return q;
//        }

//        public static IQueryable<T> ApplySort<T>(this IQueryable<T> q, string? sort)
//        {
//            if (string.IsNullOrWhiteSpace(sort)) return q;

//            var s = sort.Trim();
//            var desc = s.StartsWith("-");
//            var propName = desc ? s[1..] : s;

//            var param = Expression.Parameter(typeof(T), "x");
//            var prop = Expression.PropertyOrField(param, propName);
//            var keySelector = Expression.Lambda(prop, param);

//            var method = desc ? "OrderByDescending" : "OrderBy";
//            var call = Expression.Call(typeof(Queryable), method,
//                new Type[] { typeof(T), prop.Type }, q.Expression, Expression.Quote(keySelector));

//            return q.Provider.CreateQuery<T>(call);
//        }

//        // Basit eşitlik filtreleri: alan=değer (bool,int,string)
//        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> q, IEnumerable<string> filters)
//        {
//            foreach (var f in filters)
//            {
//                if (string.IsNullOrWhiteSpace(f) || !f.Contains('=')) continue;
//                var parts = f.Split('=', 2, StringSplitOptions.TrimEntries);
//                var name = parts[0]; var value = parts[1];

//                var param = Expression.Parameter(typeof(T), "x");
//                var member = Expression.PropertyOrField(param, name);

//                Expression constant;
//                if (member.Type == typeof(bool) && bool.TryParse(value, out var b))
//                    constant = Expression.Constant(b);
//                else if ((member.Type == typeof(int) || member.Type == typeof(int?)) && int.TryParse(value, out var i))
//                    constant = Expression.Constant(i, member.Type);
//                else
//                    constant = Expression.Constant(value);

//                // convert string to target if needed
//                if (member.Type == typeof(string) == false && constant.Type == typeof(string))
//                {
//                    // skip if incompatible
//                    continue;
//                }

//                var eq = Expression.Equal(member, constant);
//                var lambda = Expression.Lambda<Func<T, bool>>(eq, param);
//                q = q.Where(lambda);
//            }
//            return q;
//        }

//        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> q, int page, int pageSize)
//        {
//            if (page <= 0) page = 1;
//            if (pageSize <= 0 || pageSize > 500) pageSize = 10;
//            return q.Skip((page - 1) * pageSize).Take(pageSize);
//        }
//    }
//}
