using Castle.DynamicProxy;
using Core.Utilities.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Core.Aspects.Autofac.Transaction
{
    /// <summary>
    /// Method içindeki tüm DB işlemlerini tek transaction altında toplar.
    /// Başarıda Commit, hatada Rollback.
    /// Async methodları da destekler.
    /// </summary>
    public class TransactionScopeAspect : MethodInterception
    {
        public override void Intercept(IInvocation invocation)
        {
            // Async method desteği için TransactionScopeAsyncFlowOption.Enabled
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                invocation.Proceed();

                // Method Task/Task<T> dönüyorsa tamamlanmasını bekle
                if (invocation.Method.ReturnType == typeof(Task))
                {
                    var task = (Task)invocation.ReturnValue;
                    task.GetAwaiter().GetResult();
                }
                else if (invocation.Method.ReturnType.IsGenericType &&
                         invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    dynamic task = invocation.ReturnValue;
                    task.GetAwaiter().GetResult();
                }

                scope.Complete();
            }
            catch
            {
                // Dispose -> rollback
                throw;
            }
        }
    }
}
