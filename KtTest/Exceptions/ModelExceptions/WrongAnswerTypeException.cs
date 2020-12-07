using System;
using System.Runtime.Serialization;

namespace KtTest.Exceptions.ModelExceptions
{
    public class WrongAnswerTypeException : Exception
    {
        public WrongAnswerTypeException()
        {
        }

        public WrongAnswerTypeException(string message) : base(message)
        {
        }

        public WrongAnswerTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WrongAnswerTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
