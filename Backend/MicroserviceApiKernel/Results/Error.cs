using System.Runtime.CompilerServices;

namespace MicroserviceApiKernel.Results;

public class Error
{
    public Error(string errorName, ErrorType errorType)
    {
        ErrorName = errorName;
        ErrorType = errorType;
    }

    public Error(string errorName, string errorDescription, ErrorType errorType)
    {
        ErrorName = errorName;
        ErrorDescription = errorDescription;
        ErrorType = errorType;
    }

    public string ErrorName { get; }
    public string? ErrorDescription { get; }
    public ErrorType ErrorType { get; }

    public override string ToString()
    {
        if (ErrorDescription == null)
            return $"Error type: {ErrorType}; Error Name: {ErrorName}";

        return $"Error type: {ErrorType}; Error Name: {ErrorName}; Error Description: {ErrorDescription}";
    }

    public static Error Create(
        ErrorType type,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "")
    {
        string className = Path.GetFileNameWithoutExtension(filePath);
        return new Error($"{className}.{memberName}", type);
    }
}
