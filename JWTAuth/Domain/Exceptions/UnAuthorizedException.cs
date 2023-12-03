﻿namespace Domain.Exceptions
{

    public class UnAuthorizedException : Exception
    {
        public UnAuthorizedException() : base("Unauthorized client")
        {
        }
        public UnAuthorizedException(string message) : base(message)
        {
        }
        public UnAuthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
