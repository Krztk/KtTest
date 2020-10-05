using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace KtTest.Exceptions.ServiceExcepctions
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
