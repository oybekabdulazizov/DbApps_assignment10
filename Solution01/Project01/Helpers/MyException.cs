using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project01.Helpers
{
    public class MyException : Exception
    {
        public ExceptionType Type { get; set; }

        public MyException(ExceptionType type, string message) : base(message: message)
        {
            Type = type;
        }

        public enum ExceptionType
        {
            NotFound = 1, 
            BadRequest = 2, 
            EmptyParameter = 3
        }
    } 
}
