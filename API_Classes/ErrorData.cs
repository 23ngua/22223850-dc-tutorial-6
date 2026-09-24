using System;
using System.Collections.Generic;
using System.Text;

namespace API_Classes
{
    // Serialize error information passed between REST tiers
    public class ErrorData
    {
        public string exceptionType;
        public string message;
    }
}
