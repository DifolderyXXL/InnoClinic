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
}

public class MedicalResultsController : BaseApiController
{
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
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var doctorId)) return Unauthorized();
        
        var medicalResult = new MedicalResult
        {
            UserId = request.UserId,
            DoctorId = doctorId,
            AppointmentId = appointmentId,
            Complaints = request.Complaints,
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
        
        medicalResult.Complaints = medicalResultBody.Complaints;
        medicalResult.Conclusion = medicalResultBody.Conclusion;
        medicalResult.Recommendations = medicalResultBody.Recommendations;

        var updateResult = await context.UpdateAsync(medicalResult, ct);
        if (updateResult.IsError)
        {
            return Conflict();
        }
        
        return Ok();
    }
}


