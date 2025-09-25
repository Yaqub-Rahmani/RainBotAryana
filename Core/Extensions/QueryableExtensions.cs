using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyIncludes<T>(this IQueryable<T> q, IEnumerable<string> includes)
            where T : class
        {
            foreach (var path in includes.Where(p => !string.IsNullOrWhiteSpace(p)))
                q = q.Include(path.Trim());
            return q;
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> q, string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort)) return q;

            var s = sort.Trim();
            var desc = s.StartsWith("-");
            var propName = desc ? s[1..] : s;

            var param = Expression.Parameter(typeof(T), "x");
            var prop = Expression.PropertyOrField(param, propName);
            var keySelector = Expression.Lambda(prop, param);

            var method = desc ? "OrderByDescending" : "OrderBy";
            var call = Expression.Call(typeof(Queryable), method,
                new Type[] { typeof(T), prop.Type }, q.Expression, Expression.Quote(keySelector));

            return q.Provider.CreateQuery<T>(call);
        }

        // Basit eşitlik filtreleri: alan=değer (bool,int,string)
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> q, IEnumerable<string> filters)
        {
            foreach (var f in filters)
            {
                if (string.IsNullOrWhiteSpace(f) || !f.Contains('=')) continue;
                var parts = f.Split('=', 2, StringSplitOptions.TrimEntries);
                var name = parts[0]; var value = parts[1];

                var param = Expression.Parameter(typeof(T), "x");
                var member = Expression.PropertyOrField(param, name);

                Expression constant;
                if (member.Type == typeof(bool) && bool.TryParse(value, out var b))
                    constant = Expression.Constant(b);
                else if ((member.Type == typeof(int) || member.Type == typeof(int?)) && int.TryParse(value, out var i))
                    constant = Expression.Constant(i, member.Type);
                else
                    constant = Expression.Constant(value);

                // convert string to target if needed
                if (member.Type == typeof(string) == false && constant.Type == typeof(string))
                {
                    // skip if incompatible
                    continue;
                }

                var eq = Expression.Equal(member, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(eq, param);
                q = q.Where(lambda);
            }
            return q;
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> q, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 10;
            return q.Skip((page - 1) * pageSize).Take(pageSize);
        }
    }
}
