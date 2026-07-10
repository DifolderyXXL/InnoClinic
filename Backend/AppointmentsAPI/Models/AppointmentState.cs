namespace AppointmentsAPI.Models;

public enum AppointmentState
{
    Created,
    PendingReservation,
    PendingApproval,
    Approved,
    Failed,
    Confirmed
}