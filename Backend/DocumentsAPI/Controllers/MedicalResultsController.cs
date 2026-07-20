using DocumentsAPI.Application;
using DocumentsAPI.Data;
using DocumentsAPI.Models;
using MicroserviceApiKernel;
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
    [Authorize(RolePolicy.Client)]
    public async Task<IActionResult> ExportMedicalResultAsPdf(
        [FromRoute] Guid appointmentId,
        [FromServices] MedicalResultsDbContext context,
        [FromServices] MedicalResultService medicalResultService, 
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var guid)) return Unauthorized();

        var result = await context.GetByAppointmentIdAsync(appointmentId, ct);
        if (result.IsError)
        {
            return NotFound();
        }

        var medicalResult = result.Value!;
        
        if (medicalResult.UserId != guid) return NotFound();
        
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

        var pdfResult = await medicalResultService.GetOrCreateMedicalResultPdfAsync(guid, result.Value!.UpdateStamp, request, ct);
        if (pdfResult.IsSuccess)
        {
            return Ok(new { url = pdfResult.Value!.ToString() });
        }

        return Problem(statusCode: StatusCodes.Status429TooManyRequests);
    }
    
    [HttpGet("appointments/{appointmentId:guid}/me")]
    [Authorize(Policy = RolePolicy.Client)]
    public async Task<IActionResult> GetMedicalResult(
        [FromRoute] Guid appointmentId,
        [FromServices] MedicalResultsDbContext context,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var guid)) return Unauthorized();

        var result = await context.GetByAppointmentIdAsync(appointmentId, ct);
        if (result.IsError)
        {
            return NotFound();
        }

        if (result.Value!.UserId != guid)
        {
            return NotFound();
        }
        
        var bodyOnly = new MedicalResultBody 
        {
            Complaints = result.Value.Complaints,
            Conclusion = result.Value.Conclusion,
            Recommendations = result.Value.Recommendations
        };

        return Ok(bodyOnly);
    }
    
    [HttpPost("appointments/{appointmentId:guid}")]
    [Authorize(Policy = RolePolicy.Doctor)]
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
    [Authorize(Policy = RolePolicy.Doctor)]
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


