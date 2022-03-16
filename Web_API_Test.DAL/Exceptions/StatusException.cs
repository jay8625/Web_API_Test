namespace Web_API_Test.DAL.Exceptions
{
    public class StatusException: Exception
    {
        public StatusException() : base() { }
        public StatusException(string message):base(message) { }
        public StatusException(string message,Exception exception):base(message, exception) { }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
