namespace API.Exceptions;

// What kind of failure happened. ApiExceptionHandler turns each one into an HTTP status:
// Validation 400, NotFound 404, Forbidden 403, Conflict 409, Cancelled 499, Unexpected 500.
public enum ErrorType { Validation, NotFound, Forbidden, Conflict, Cancelled, Unexpected }
