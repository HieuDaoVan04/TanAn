// "Một sản phẩm của HieuDV"

using System;

namespace Service.Shared.Commons.Extensions
{
    public class InvalidInputException : Exception
    {
        public InvalidInputException() { }

        public InvalidInputException(string message) : base(message) { }

        public InvalidInputException(string message, Exception innerException) : base(message, innerException) { }
    }
}
