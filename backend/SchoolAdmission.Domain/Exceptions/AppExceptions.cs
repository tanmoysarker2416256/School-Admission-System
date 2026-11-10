namespace SchoolAdmission.Domain.Exceptions;

public abstract class AppException(string message) : Exception(message);

public class BadRequestException(string message) : AppException(message);   // 400
public class NotFoundException(string message) : AppException(message);     // 404
public class ConflictException(string message) : AppException(message);     // 409
public class ForbiddenException(string message) : AppException(message);    // 403