using SchoolAdmission.Domain.Exceptions;

namespace SchoolAdmission.Infrastructure.Services;

public static class StudentIdRules
{
    public static string Normalize(string? studentId)
    {
        studentId = studentId?.Trim();
        if (string.IsNullOrEmpty(studentId) || studentId.Length != 17 || !studentId.All(char.IsAsciiDigit))
            throw new BadRequestException("Student ID must be exactly 17 digits.");
        return studentId;
    }
}