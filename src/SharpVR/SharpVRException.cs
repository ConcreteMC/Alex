using System;

namespace SharpVR
{
    public class SharpVRException : Exception
    {
        public int ErrorCode { get; }

        public SharpVRException(string message) : base(message)
        {
            ErrorCode = -1;
        }

        public SharpVRException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}