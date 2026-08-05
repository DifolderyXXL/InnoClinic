namespace Deunde.IdentityServer.Services.SMTP;

public class SmtpClientOptions
{
    public const string SectionName = "SmtpClient";
    
    public string Email { get; set; }
    public string AppPassword { get; set; }
    public string Host { get; set; }
    public uint Port { get; set; }
}