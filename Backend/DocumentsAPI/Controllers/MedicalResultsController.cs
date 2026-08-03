using DocumentsAPI.Application;
using DocumentsAPI.Infrastructure;
using DocumentsAPI.Models;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions.Endpoints;
using MicroserviceApiKernel.Results;
using MicroserviceApiKernel.SharedControllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentsAPI.Controllers;

public class CreateMedicalResultRequest : MedicalResultBody
{
    public Guid UserId { get; set; }
    public UserFullName DoctorName { get; set; }
    public string Specialization { get; set; }
    public string ServiceName { get; set; }
    public UserFullName PatientName { get; set; }
    public DateOnly PatientDateOfBirth { get; set; }
}

public class MedicalResultsController : BaseApiController
{
    [HttpGet("appointments/{appointmentId:guid}/me/export")]
    [HasPermission(Permissions.MedicalResults.ReadOwn)]
    public async Task<IActionResult> ExportMedicalResultAsPdf(
        [FromRoute] Guid appointmentId,
        [FromServices] MedicalResultsDbContext context,
        [FromServices] MedicalResultService medicalResultService, 
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var guid)) return Unauthorized();

        var result = await MedicalResultsHelper.ExportMedicalResult(context, medicalResultService, appointmentId, guid, ct);
        if (result == null) return NotFound();
        
        return Ok(new { url = result.ToString() });
    }
    
    [HttpGet("appointments/{appointmentId:guid}/users/{userId:guid}/export")]
    [HasPermission(Permissions.MedicalResults.Read)]
    public async Task<IActionResult> ExportMedicalResultAsPdf(
        [FromRoute] Guid appointmentId,
        [FromRoute] Guid userId,
        [FromServices] MedicalResultsDbContext context,
        [FromServices] MedicalResultService medicalResultService, 
        CancellationToken ct)
    {
        var result = await MedicalResultsHelper.ExportMedicalResult(context, medicalResultService, appointmentId, userId, ct);
        if (result == null) return NotFound();
        
        return Ok(new { url = result.ToString() });
    }
    
    [HttpGet("appointments/{appointmentId:guid}/me")]
    [HasPermission(Permissions.MedicalResults.ReadOwn)]
    public async Task<IActionResult> GetMedicalResult(
        [FromRoute] Guid appointmentId,
        [FromServices] MedicalResultsDbContext context,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var userId)) return Unauthorized();

        var result = await MedicalResultsHelper.GetMedicalResult(context, appointmentId, userId, ct);

        if (result == null) return NotFound();
        
        return Ok(result);
    }
    
    [HttpGet("appointments/{appointmentId:guid}/users/{userId:guid}")]
    [HasPermission(Permissions.MedicalResults.Read)]
    public async Task<IActionResult> GetMedicalResult(
        [FromRoute] Guid appointmentId,
        [FromRoute] Guid userId,
        [FromServices] MedicalResultsDbContext context,
        CancellationToken ct)
    {
        var result = await MedicalResultsHelper.GetMedicalResult(context, appointmentId, userId, ct);

        if (result == null) return NotFound();
        
        return Ok(result);
    }
    
    [HttpPost("appointments/{appointmentId:guid}")]
    [HasPermission(Permissions.MedicalResults.Manage)]
    public async Task<IActionResult> AddMedicalResult(
        [FromRoute] Guid appointmentId,
        [FromBody] CreateMedicalResultRequest request,
        [FromServices] MedicalResultsDbContext context,
        [FromServices] MedicalResultService medicalResultService,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var doctorId)) return Unauthorized();
        
        var medicalResult = new MedicalResult
        {
            UserId = request.UserId,
            DoctorId = doctorId,
            UpdateStamp = DateTimeOffset.UtcNow,
            PatientName = request.PatientName,
            PatientDateOfBirth = request.PatientDateOfBirth,
            DoctorName = request.DoctorName,
            Specialization = request.Specialization,
            ServiceName = request.ServiceName,
            AppointmentId = appointmentId,
            Complaints = request.Complaints,
            Diagnosis = request.Diagnosis,
            Conclusion = request.Conclusion,
            Recommendations = request.Recommendations
        };

        var result = await context.InsertAsync(medicalResult, ct);
        if (result.IsError)
        {
            return Conflict();
        }
        
        return Created();
    }
    
    [HttpPut("appointments/{appointmentId:guid}")]
    [HasPermission(Permissions.MedicalResults.Manage)]
    public async Task<IActionResult> UpdateMedicalResult(
        [FromRoute] Guid appointmentId,
        [FromBody] MedicalResultBody medicalResultBody,
        [FromServices] MedicalResultsDbContext context,
        CancellationToken ct)
    {
        var result = await context.GetByAppointmentIdAsync(appointmentId, ct);
        if (result.IsError)
        {
            return NotFound();
        }

        var medicalResult = result.Value!;

        medicalResult.UpdateStamp = DateTimeOffset.UtcNow;
        medicalResult.Complaints = medicalResultBody.Complaints;
        medicalResult.Conclusion = medicalResultBody.Conclusion;
        medicalResult.Diagnosis = medicalResultBody.Diagnosis;
        medicalResult.Recommendations = medicalResultBody.Recommendations;

        var updateResult = await context.UpdateAsync(medicalResult, ct);
        if (updateResult.IsError)
        {
            return Conflict();
        }
        
        return Ok();
    }
}

public static class MedicalResultsHelper
{
    public static async Task<MedicalResultBody?> GetMedicalResult(MedicalResultsDbContext context,  Guid appointmentId, Guid patientId, CancellationToken ct)
    {
        var result = await context.GetByAppointmentIdAsync(appointmentId, ct);
        if (result.IsError)
        {
            return null;
        }

        if (result.Value!.UserId != patientId)
        {
            return null;
        }
        
        var bodyOnly = new MedicalResultBody 
        {
            Complaints = result.Value.Complaints,
            Conclusion = result.Value.Conclusion,
            Diagnosis = result.Value.Diagnosis,
            Recommendations = result.Value.Recommendations
        };

        return bodyOnly;
    }

    public static async Task<Uri?> ExportMedicalResult(MedicalResultsDbContext context, MedicalResultService medicalResultService, Guid appointmentId, Guid patientId, CancellationToken ct)
    {
        var result = await context.GetByAppointmentIdAsync(appointmentId, ct);
        if (result.IsError)
        {
            return null;
        }

        var medicalResult = result.Value!;
        
        if (medicalResult.UserId != patientId) return null;
        
        var request = new MedicalResultPdfData(
            medicalResult.AppointmentId,
            medicalResult.DoctorName,
            medicalResult.Specialization,
            medicalResult.ServiceName,
            medicalResult.PatientName,
            medicalResult.PatientDateOfBirth,
            medicalResult.Complaints,
            medicalResult.Conclusion,
            medicalResult.Diagnosis,
            medicalResult.Recommendations,
            medicalResult.UpdateStamp
        );

        var pdfResult = await medicalResultService.GetOrCreateMedicalResultPdfAsync(patientId, result.Value!.UpdateStamp, request, ct);
        if (pdfResult.IsSuccess)
        {
            return pdfResult.Value;
        }

        return null;
    }
}
