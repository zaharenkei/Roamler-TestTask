using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Implementation
{
    internal class CsvProcessingException : Exception
    {
        public CsvProcessingException() : base()
        {

        }

        public CsvProcessingException(string? message) : base(message)
        {

        }

        public CsvProcessingException(string? message, Exception? innerException) : base(message, innerException)
        {

        }
    }
}
