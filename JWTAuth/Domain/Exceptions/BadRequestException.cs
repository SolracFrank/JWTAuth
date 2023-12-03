namespace Domain.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }

        public BadRequestException(string keyName, IEnumerable<string> errorMessages)
        {
            Errors ??= new Dictionary<string, IEnumerable<string>>();

            Errors.Add(keyName, errorMessages);
        }

        public IDictionary<string, IEnumerable<string>> Errors { get; set; }
    }
}
