using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using Core.Utilities.IoC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Aspects.Autofac.Performance
{
    public class PerformanceAspect : MethodInterception
    {
        private readonly int _intervalSeconds;
        private Stopwatch _stopwatch;
        private readonly ILogger<PerformanceAspect> _logger;


        public PerformanceAspect(int intervalSeconds = 2)
        {
            _intervalSeconds = intervalSeconds;
            _stopwatch = new Stopwatch();
            _logger = ServiceTool.ServiceProvider.GetService<ILogger<PerformanceAspect>>();
        }


        protected override void OnBefore(IInvocation invocation)
        {
            _stopwatch.Restart();
        }


        protected override void OnAfter(IInvocation invocation)
        {
            _stopwatch.Stop();
            var elapsed = _stopwatch.Elapsed;
            if (elapsed.TotalSeconds >= _intervalSeconds)
            {
                var method = $"{invocation.Method.ReflectedType.FullName}.{invocation.Method.Name}";
                var args = string.Join(", ", invocation.Arguments.Select(a => a?.ToString() ?? "<null>"));
                var msg = $"[Performance] {method} took {elapsed.TotalMilliseconds:F0} ms. Args: [{args}]";
                if (_logger != null) _logger.LogWarning(msg);
                else System.Diagnostics.Debug.WriteLine(msg);
            }
        }
    }
}
