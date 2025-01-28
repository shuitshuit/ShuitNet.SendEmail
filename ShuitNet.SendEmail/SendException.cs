namespace ShuitNet.SendEmail
{
    public class SendException : Exception
    {
        public SendState State { get; }

        public SendException(string message, SendState state) : base(message)
        {
            State = state;
        }
    }
}
