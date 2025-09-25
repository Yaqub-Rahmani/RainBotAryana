using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results
{
    public class DataResult<T> : IDataResult<T>
    {
        public DataResult(T data, bool success, string message = null)
        {
            Data = data;
            Success = success;
            Message = message;
        }

        public T Data { get; }
        public bool Success { get; }
        public string Message { get; }
    }
}
