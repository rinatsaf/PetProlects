namespace Application.Exceptions;

public class RateLimitExceededException(string message) : Exception(message);
