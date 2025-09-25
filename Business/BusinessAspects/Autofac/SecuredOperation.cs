using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Business.BusinessAspects.Autofac
{
    /// <summary>
    /// [SecuredOperation("admin,product.write")] şeklinde kullanılır.
    /// Kullanıcının Claims'inde en az bir eşleşen rol olmalı.
    /// </summary>
    public sealed class SecuredOperation : MethodInterception
    {
        private readonly string[] _requiredRoles;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SecuredOperation>? _logger;

        public SecuredOperation(string rolesCsv)
        {
            // "admin, product.write" -> ["admin","product.write"]
            _requiredRoles = (rolesCsv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // Hizmetleri alın
            _httpContextAccessor = ServiceTool.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            _logger = ServiceTool.ServiceProvider.GetService<ILogger<SecuredOperation>>();
        }

        protected override void OnBefore(IInvocation invocation)
        {
            var httpCtx = _httpContextAccessor.HttpContext
                ?? throw new UnauthorizedAccessException("Authorization denied (no HttpContext).");

            var user = httpCtx.User;
            if (user?.Identity?.IsAuthenticated != true)
                throw new UnauthorizedAccessException("Authorization denied (unauthenticated).");

            // Kullanıcının rol claim'lerini topla ve set'e koy (case-insensitive)
            var userRoles = new HashSet<string>(
                user.Claims
                    .Where(c => c.Type == ClaimTypes.Role && !string.IsNullOrWhiteSpace(c.Value))
                    .Select(c => c.Value),
                StringComparer.OrdinalIgnoreCase
            );

            // En az bir rol uyuşmalı
            var authorized = _requiredRoles.Length == 0 || _requiredRoles.Any(r => userRoles.Contains(r));

            if (!authorized)
            {
                _logger?.LogWarning("Authorization denied. Path: {Path}, User: {Name}, RequiredRoles: {Required}, UserRoles: {UserRoles}",
                    httpCtx.Request.Path, user.Identity?.Name ?? "<anonymous>",
                    string.Join(",", _requiredRoles), string.Join(",", userRoles));

                throw new UnauthorizedAccessException("Authorization denied.");
            }
        }
    }
}
