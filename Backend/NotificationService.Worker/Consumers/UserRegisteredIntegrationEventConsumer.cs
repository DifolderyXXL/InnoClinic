using Contracts.Notifications;
using Infrastructure.Mailing.SMTP;
using MassTransit;
using NotificationService.Worker.Services;

namespace NotificationService.Worker.Consumers;

public class UserRegisteredIntegrationEventConsumer(ISmtpClient mailClient) : IConsumer<UserRegisteredIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        await mailClient.Send(
            context.Message.Email, 
            "INNO-CLINIC Registration", 
            $"Account created, click to set up password: <a href=\"{context.Message.CreateAccountLink}\">Set up password</a>");
    }
}

public class UserAppointmentConfirmedIntegrationEventConsumer(ISmtpClient mailClient) : IConsumer<UserAppointmentConfirmedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<UserAppointmentConfirmedIntegrationEvent> context)
    {
        var m = context.Message;
        var dateFormatted = m.Date.ToString("dd.MM.yyyy");
        var timeFormatted = $"{m.BeginTime:hh\\:mm} - {m.EndTime:hh\\:mm}";

        var emailSubject = "INNO-CLINIC: Appointment Confirmed";
        var emailBody = $$"""
                          <!DOCTYPE html>
                          <html>
                          <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                              <h2>Appointment Confirmed</h2>
                              <p>Dear <strong>{{m.PatientName}}</strong>, your appointment has been successfully booked.</p>

                              <hr style="border: none; border-top: 1px solid #ccc; margin: 15px 0;" />

                              <p>
                                  <strong>Date:</strong> {{dateFormatted}}<br/>
                                  <strong>Time:</strong> {{timeFormatted}}
                              </p>

                              <p>
                                  <strong>Doctor:</strong> {{m.DoctorName}}<br/>
                                  <strong>Specialization & Category:</strong> {{m.SpecializationName}} ({{m.CategoryName}})<br/>
                                  <strong>Service:</strong> {{m.ServiceName}}
                              </p>

                              <hr style="border: none; border-top: 1px solid #ccc; margin: 15px 0;" />

                              <p style="font-size: 13px; color: #666;">
                                  If you need to reschedule or cancel your appointment, please use your personal account.
                              </p>
                          </body>
                          </html>
                          """;

        await mailClient.Send(
            m.PatientEmail, 
            emailSubject, 
            emailBody
        );
    }
}

public class MedicalResultUpdatedIntegrationEventConsumer(ISmtpClient mailClient, AppointmentApiClient client) : IConsumer<MedicalResultUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<MedicalResultUpdatedIntegrationEvent> context)
    {
        var appointment = await client.GetAppointmentInfoAsync(context.Message.AppointmentId, context.CancellationToken);

        if (appointment.IsError)
        {
            throw new Exception(appointment.Error!.ToString());
        }
        var info = appointment.Value!;
        
        var dateFormatted = info.Date.ToString("dd.MM.yyyy");
        var timeFormatted = info.BeginTime.HasValue && info.EndTime.HasValue
            ? $"{info.BeginTime.Value:hh\\:mm} - {info.EndTime.Value:hh\\:mm}"
            : "Time not specified";

        var emailSubject = "INNO-CLINIC: Medical Result Update";
        var emailBody = $$"""
                          <!DOCTYPE html>
                          <html>
                          <body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333;">
                              <h2>Medical Result Updated</h2>
                              <p><strong>Updated at:</strong> {{context.Message.UpdateStamp:dd.MM.yyyy HH:mm}}</p>
                              <p>Your medical result has been changed.</p>
                              
                              <hr style="border: none; border-top: 1px solid #ccc; margin: 15px 0;" />

                              <p>
                                  <strong>Date:</strong> {{dateFormatted}}<br/>
                                  <strong>Time:</strong> {{timeFormatted}}
                              </p>

                              <p>
                                  <strong>Doctor:</strong> {{context.Message.DoctorName}}<br/>
                                  <strong>Service & Specialization:</strong> {{context.Message.ServiceName}}, {{context.Message.Specialization}}
                              </p>

                              <hr style="border: none; border-top: 1px solid #ccc; margin: 15px 0;" />

                              <p>
                                  <strong>Complaints:</strong> {context.Message.Complaints}<br/>
                                  <strong>Diagnosis:</strong> {context.Message.Diagnosis}<br/>
                                  <strong>Recommendations:</strong> {context.Message.Recommendations}<br/>
                                  <strong>Conclusion:</strong> {context.Message.Conclusion}
                              </p>
                          </body>
                          </html>
                          """;
        
        await mailClient.Send(
            appointment.Value!.PatientEmail, 
            emailSubject, 
            emailBody);
    }
}