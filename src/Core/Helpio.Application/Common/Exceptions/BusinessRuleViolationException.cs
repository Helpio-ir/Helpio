using System;

namespace Helpio.Ir.Application.Common.Exceptions
{
    public class BusinessRuleViolationException : Exception
    {
        public BusinessRuleViolationException()
            : base()
        {
        }

        public BusinessRuleViolationException(string message)
            : base(message)
        {
        }

        public BusinessRuleViolationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}