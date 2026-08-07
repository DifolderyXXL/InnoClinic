import { type AppointmentDto, appointmentsApi } from "../../../../services/api/AppointmentApi.ts";
import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import "./AppointmentCard.css";

interface AppointmentCardProps {
    appointment: AppointmentDto;
    showResultLink?: boolean;
}

export function AppointmentCard({ appointment, showResultLink }: AppointmentCardProps) {
    const stateString = String(appointment.state ?? "").toLowerCase();

    return (
        <div className="appointment-card">
            <div className="appointment-header">
                <span className="appointment-date">{appointment.date}</span>
                <span className={`appointment-status status-${stateString}`}>
                    {appointment.state}
                </span>
            </div>

            <div className="appointment-body">
                <h3 className="service-name">{appointment.serviceName}</h3>

                <div className="appointment-details">
                    <p>
                        <strong>Doctor:</strong> {appointment.doctorFullName}
                    </p>
                    <p>
                        <strong>Patient:</strong> {appointment.patientFullName}
                    </p>
                    <p>
                        <strong>Time:</strong>{" "}
                        {appointment.beginTime && appointment.endTime ? (
                            <span>
                                {appointment.beginTime} — {appointment.endTime}
                            </span>
                        ) : (
                            <span>Slot {appointment.startSlotIndex} (Pending reservation)</span>
                        )}
                    </p>
                </div>
            </div>

            {showResultLink && (
                <div className="appointment-actions">
                    <Link
                        to={`/medical-results/details?id=${appointment.id}&userId=${appointment.patientAccountId}`}
                        className="result-link"
                    >
                        View medical result →
                    </Link>
                </div>
            )}
        </div>
    );
}

interface AppointmentByIdCardProps {
    appointmentId: string;
    showResultLink: boolean;
}

export function MyDoctorAppointmentByIdCard({ appointmentId, showResultLink }: AppointmentByIdCardProps) {
    const [appointment, setAppointment] = useState<AppointmentDto | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        setLoading(true);
        appointmentsApi
            .getMyDoctorAppointmentById(appointmentId)
            .then((result) => {
                if (result.type === "ok") setAppointment(result.value);
                setLoading(false);
            })
            .catch(() => setLoading(false));
    }, [appointmentId]);

    if (loading) return <div className="status-message">Loading appointment...</div>;
    if (!appointment) return null;

    return <AppointmentCard appointment={appointment} showResultLink={showResultLink} />;
}