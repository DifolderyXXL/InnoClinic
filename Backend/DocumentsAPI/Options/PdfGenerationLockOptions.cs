namespace DocumentsAPI.Options;

public class PdfGenerationLockOptions
{
    public const string SectionName = "PdfLock";
    
    public TimeSpan ExpireTime { get; set; }
    public TimeSpan WaitTime { get; set; }
    public TimeSpan AcquireRetryTime { get; set; }
}