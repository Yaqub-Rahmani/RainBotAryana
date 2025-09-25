using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Interceptors
{
    public abstract class MethodInterception : MethodInterceptionBaseAttribute
    {
        protected virtual void OnBefore(IInvocation i) { }
        protected virtual void OnAfter(IInvocation i) { }
        protected virtual void OnException(IInvocation i, Exception e) { }
        protected virtual void OnSuccess(IInvocation i) { }
        public override void Intercept(IInvocation i)
        {
            var isSuccess = false; try { OnBefore(i); i.Proceed(); isSuccess = true; }
            catch (Exception e) { OnException(i, e); throw; }
            finally { if (isSuccess) OnSuccess(i); OnAfter(i); }
        }
    }

}
