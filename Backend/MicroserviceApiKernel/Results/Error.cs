using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace MicroserviceApiKernel.Results;

public class Error
{
    public Error(string errorName, ErrorType errorType)
    {
        ErrorName = errorName;
        ErrorType = errorType;
    }

    public Error(string errorName, string errorDescription, ErrorType errorType, IDictionary<string, string[]>? validationResults = null)
    {
        ErrorName = errorName;
        ErrorDescription = errorDescription;
        ErrorType = errorType;
        ValidationResults = validationResults;
    }

    public string ErrorName { get; }
    public string? ErrorDescription { get; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorType ErrorType { get; }
    public IDictionary<string, string[]>? ValidationResults { get; }

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
    
    public static Error Validation(
        string errorName, 
        string errorDescription, 
        IDictionary<string, string[]>? validationResults = null) 
        => new(errorName, errorDescription, ErrorType.Validation, validationResults);

    public static Error NotFound(string errorName, string errorDescription) 
        => new(errorName, errorDescription, ErrorType.NotFound);

    public static Error Conflict(string errorName, string errorDescription) 
        => new(errorName, errorDescription, ErrorType.Conflict);

    public static Error Failure(string errorName, string errorDescription) 
        => new(errorName, errorDescription, ErrorType.Internal);
}

public abstract class DomainErrors<T>
{
    private static readonly string ClassName = typeof(T).Name;

    public static Error NotFound() => 
        new Error($"{ClassName}.NotFound", ErrorType.NotFound);

    public static Error AlreadyExists() => 
        new Error($"{ClassName}.AlreadyExists", ErrorType.Conflict);
}