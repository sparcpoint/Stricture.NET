using System;

namespace CleanSample.Features.Payments.Exceptions
{
    internal sealed class PaymentFailedException : Exception
    {
        public PaymentFailedException()
        {
        }

        public PaymentFailedException(string message)
            : base(message)
        {
        }

        public PaymentFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
