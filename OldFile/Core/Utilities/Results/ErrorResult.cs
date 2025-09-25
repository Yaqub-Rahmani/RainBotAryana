using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results
{
    public class ErrorResult : IResult
    {
        public ErrorResult(string message = null)
        {
            Success = false;
            Message = message;
        }
        public bool Success { get; }
        public string Message { get; }
    }
}
