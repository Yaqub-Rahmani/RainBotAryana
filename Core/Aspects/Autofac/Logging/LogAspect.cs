using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Core.Aspects.Autofac.Logging
{
    public class LogAspect : MethodInterception
    {
        private readonly ILogger _logger;

        public LogAspect()
        {
            // Hedef sınıf tipi run-time'da biliniyor; generic logger yerine kategori adıyla üretelim
            var factory = ServiceTool.ServiceProvider.GetService<ILoggerFactory>();
            _logger = factory?.CreateLogger("BusinessMethodLogger")
                ?? throw new InvalidOperationException("ILoggerFactory is not available.");
        }

        protected override void OnBefore(IInvocation invocation)
        {
            var method = $"{invocation.Method.ReflectedType.FullName}.{invocation.Method.Name}";
            var args = string.Join(", ", invocation.Arguments.Select(a => a?.ToString() ?? "<null>"));
            _logger.LogInformation("→ Enter {Method} | Args: [{Args}]", method, args);
        }

        protected override void OnAfter(IInvocation invocation)
        {
            var method = $"{invocation.Method.ReflectedType.FullName}.{invocation.Method.Name}";
            _logger.LogInformation("← Exit  {Method}", method);
        }

        protected override void OnException(IInvocation invocation, Exception e)
        {
            var method = $"{invocation.Method.ReflectedType.FullName}.{invocation.Method.Name}";
            _logger.LogError(e, "✖ Exception in {Method}", method);
        }
    }
}
