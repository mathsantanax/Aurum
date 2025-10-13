using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Application.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }
        public AppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
