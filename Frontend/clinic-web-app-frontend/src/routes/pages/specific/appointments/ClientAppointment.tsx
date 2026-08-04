import { useEffect, useState } from "react";
import { useSearchParams } from "react-router";
import { type AppointmentDto, appointmentsApi } from "../../../../services/api/AppointmentApi.ts";
import { AppointmentCard } from "../../common/Appointment/AppointmentCard.tsx";
import "./ClientAppointments.css";

export function ClientAppointment() {
    const [searchParams] = useSearchParams();
    const [appointment, setAppointment] = useState<AppointmentDto | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    const targetId = searchParams.get("id") || null;

    useEffect(() => {
        if (!targetId) {
            setLoading(false);
            return;
        }

        setLoading(true);
        appointmentsApi
            .getMyClientAppointmentById(targetId)
            .then((result) => {
                if (result.type === "ok") {
                    setAppointment(result.value);
                }
                setLoading(false);
            })
            .catch(() => setLoading(false));
    }, [targetId]);

    if (!targetId) {
        return <div className="status-message error">Appointment ID is missing</div>;
    }

    if (loading) {
        return <div className="status-message">Loading appointment...</div>;
    }

    if (!appointment) {
        return <div className="status-message error">Appointment not found</div>;
    }

    return (
        <div className="client-appointment-details-page">
            <AppointmentCard appointment={appointment} showResultLink={true} />
        </div>
    );
}