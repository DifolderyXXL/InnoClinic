import { Link } from "react-router-dom";
import type { AppointmentDto } from "../../../services/api/AppointmentApi.ts";
import "./ScheduleView.css";

interface ScheduleCardProps {
    appointment: AppointmentDto;
    isFree: boolean;
}

export function ScheduleCard({ appointment, isFree }: ScheduleCardProps) {
    return (
        <tr className={`schedule-row ${isFree ? "slot-free" : "slot-occupied"}`}>
            <td className="time-cell">
                <span className="time-badge">
                    {appointment.beginTime ?? `Slot #${appointment.startSlotIndex}`}
                    {appointment.endTime && ` – ${appointment.endTime}`}
                </span>
            </td>
            <td className="details-cell">
                {isFree ? (
                    <span className="free-badge">Available Slot</span>
                ) : (
                    <Link
                        to={`/my-schedule/details?id=${appointment.id}`}
                        className="appointment-card-link"
                    >
                        <MinimalAppointmentCard appointment={appointment} />
                    </Link>
                )}
            </td>
        </tr>
    );
}

interface MinimalAppointmentCardProps {
    appointment: AppointmentDto;
}

export function MinimalAppointmentCard({ appointment }: MinimalAppointmentCardProps) {
    return (
        <div className="minimal-appointment-card">
            <div className="card-header">
                <span className="service-name">{appointment.serviceName}</span>
                {appointment.state && (
                    <span className={`state-badge state-${String(appointment.state).toLowerCase()}`}>
                        {appointment.state}
                    </span>
                )}
            </div>

            {appointment.patientFullName && (
                <div className="patient-name">
                    <span>Patient:</span> <strong>{appointment.patientFullName}</strong>
                </div>
            )}
        </div>
    );
}