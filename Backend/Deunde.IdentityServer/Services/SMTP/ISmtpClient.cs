using System.Net;
using System.Net.Mail;
using MicroserviceApiKernel.Results;
using Microsoft.Extensions.Options;

namespace Deunde.IdentityServer.Services.SMTP;

public interface ISmtpClient
{
    public Task<Result> Send(string receiverMail, string? subject, string? body);
}

public class BasicSmtpClient(IOptions<SmtpClientOptions> options) : ISmtpClient
{
    public async Task<Result> Send(string receiverMail, string? subject, string? body)
    {
        try
        {
            using var client = new SmtpClient(options.Value.Host, (int)options.Value.Port);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(options.Value.Email, options.Value.AppPassword);

            using var message = new MailMessage(options.Value.Email, receiverMail, subject, body);
            message.IsBodyHtml = true;
            
            await client.SendMailAsync(message);
            
            return Result.Success();
        }
        catch (Exception e)
        {
            return new Error(e.Message, ErrorType.Internal);
        }
    }
}