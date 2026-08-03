import { Link } from "react-router-dom";
import type { AppointmentDto } from "../../../services/api/AppointmentApi.ts";

interface ScheduleCardProps {
  appointment: AppointmentDto;
  isFree: boolean;
}

export function ScheduleCard({ appointment, isFree }: ScheduleCardProps) {
  return (
    <tr style={{ backgroundColor: isFree ? "green" : "#333" }}>
      <td style={{ color: "white" }}>
        {appointment.beginTime} – {appointment.endTime}
      </td>
      <td>
        {isFree ? (
          <span style={{ color: "white" }}>Free</span>
        ) : (
          <Link
            to={`/my-schedule/details?id=${appointment.id}`}
            style={{ textDecoration: "none" }}
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

export function MinimalAppointmentCard({
  appointment,
}: MinimalAppointmentCardProps) {
  return (
    <div>
      <h3>{appointment.serviceName}</h3>
      <p>Patient: {appointment.patientFullName}</p>
      <p>Status: {appointment.state}</p>
    </div>
  );
}
