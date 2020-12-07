using System;
using System.Runtime.Serialization;

namespace KtTest.Exceptions.ServiceExceptions
{
    public class ValueNotInTheCacheException : Exception
    {
        public ValueNotInTheCacheException()
        {
        }

        public ValueNotInTheCacheException(string message) : base(message)
        {
        }

        public ValueNotInTheCacheException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValueNotInTheCacheException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
