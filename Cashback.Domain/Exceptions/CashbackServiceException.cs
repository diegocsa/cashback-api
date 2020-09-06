using System;

namespace Cashback.Domain.Exceptions
{

    [Serializable]
    public class CashbackServiceException : Exception
    {
        public CashbackServiceException() { }
        public CashbackServiceException(string message) : base(message) {}
        public CashbackServiceException(string message, Exception inner) : base(message, inner) { }
        protected CashbackServiceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
