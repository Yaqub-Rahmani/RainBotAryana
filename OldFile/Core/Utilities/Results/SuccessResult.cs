using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Results
{
    public class SuccessResult : IResult
    {
        public SuccessResult(string message = null)
        {
            Success = true;
            Message = message;
        }
        public bool Success { get; }
        public string Message { get; }
    }
}
