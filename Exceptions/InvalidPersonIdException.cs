namespace Exceptions {
    public class InvalidPersonIdException : ArgumentException {
        public InvalidPersonIdException()
        {
        }
        public InvalidPersonIdException(string? message) : base(message) {

        }
        public InvalidPersonIdException(string? message, Exception? innerException)
        { 
        }
    }
}