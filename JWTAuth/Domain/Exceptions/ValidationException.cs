namespace Domain.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException()
        {

        }
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string keyName, IEnumerable<string> errorMessages)
        {
            Errors ??= new Dictionary<string, IEnumerable<string>>();

            Errors.Add(keyName, errorMessages);
        }

        public IDictionary<string, IEnumerable<string>> Errors { get; set; }
    }
}
