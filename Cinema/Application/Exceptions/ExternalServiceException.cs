namespace Application.Exceptions;

public sealed class ExternalServiceException(string message) : Exception(message);
