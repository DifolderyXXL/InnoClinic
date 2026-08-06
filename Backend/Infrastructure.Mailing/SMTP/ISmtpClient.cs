using Deunde.IdentityServer.Services.SMTP;
using MailKit.Net.Smtp;
using MailKit.Security;
using MicroserviceApiKernel.Results;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Mailing.SMTP;

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
            using var message = new MimeMessage();
            message.From.Add(new MailboxAddress("noreply", options.Value.Email));
            message.To.Add (new MailboxAddress (receiverMail, receiverMail));

            message.Subject = subject ?? string.Empty;
            message.Body = new TextPart("html")
            {
                Text = body ?? string.Empty
            };
            
            using var client = new SmtpClient();
            await client.ConnectAsync(options.Value.Host, (int)options.Value.Port, SecureSocketOptions.Auto);
            await client.AuthenticateAsync(options.Value.Email, options.Value.AppPassword);
            
            await client.SendAsync (message);
            await client.DisconnectAsync (true);
            
            return Result.Success();
        }
        catch (Exception e)
        {
            return new Error(e.Message, ErrorType.Internal);
        }
    }
}