using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Applications.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }
        public AppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        //
        public AppException(string message) : base(message)
        {
            // Padrão 500 para exceções genéricas de aplicação não mapeadas.
            StatusCode = 500;
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }

    public class ValidationException : AppException
    {
        public ValidationException(string message) : base(message, 400) { }
    }
}
