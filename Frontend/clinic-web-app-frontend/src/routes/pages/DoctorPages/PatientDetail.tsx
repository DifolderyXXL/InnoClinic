import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useSearchParams } from "react-router";
import type { AppointmentDto } from "../../../services/api/AppointmentApi.ts";
import {
    appointmentsApi,
    AppointmentState,
} from "../../../services/api/AppointmentApi.ts";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";
import { AppointmentCard } from "../common/Appointment/AppointmentCard.tsx";
import { TitledCard } from "../common/TitledCard.tsx";
import { PaginatedListView } from "../common/PaginatedListView.tsx";
import { PAGE_SIZE, PatientCard, type PatientDto } from "./types.tsx";
import "./PatientDetail.css";

interface PatientCardByIdProps {
    id: string;
}

export function PatientCardById({ id }: PatientCardByIdProps) {
    const [patient, setPatient] = useState<PatientDto | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        setLoading(true);
        profilesApi.getPatient(id).then((result) => {
            if (result.type === "ok") {
                setPatient(result.value);
            }
            setLoading(false);
        }).catch(() => setLoading(false));
    }, [id]);

    if (loading) {
        return <div className="status-message">Loading patient info...</div>;
    }

    if (!patient) {
        return <div className="status-message error">Patient not found</div>;
    }

    return (
        <TitledCard title="Patient Details">
            <div className="patient-details-container">
                <PatientCard patient={patient} />
                <PatientRecentAppointments id={patient.accountId} />
            </div>
        </TitledCard>
    );
}

export function DoctorScheduledAppointment() {
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
        appointmentsApi.getMyDoctorAppointmentById(targetId).then((result) => {
            if (result.type === "ok") {
                setAppointment(result.value);
            }
            setLoading(false);
        }).catch(() => setLoading(false));
    }, [targetId]);

    if (loading) {
        return <div className="status-message">Loading appointment details...</div>;
    }

    if (!appointment) {
        return <div className="status-message error">Appointment not found</div>;
    }

    return (
        <div className="doctor-scheduled-appointment-page">
            <AppointmentCard appointment={appointment} showResultLink={true} />
            <PatientCardById id={appointment.patientAccountId} />
        </div>
    );
}

interface PatientRecentAppointmentsProps {
    id: string;
}

function PatientRecentAppointments({ id }: PatientRecentAppointmentsProps) {
    const fetchAppointments = async (page: number) => {
        try {
            const result = await appointmentsApi.getAppointments(
                AppointmentState.Confirmed,
                id,
                page,
                PAGE_SIZE,
            );

            if (result.type === "ok") {
                return {
                    items: result.value.items ?? [],
                    total: result.value.totalCount ?? 0,
                };
            }

            return {
                items: [],
                total: 0,
                error: result.error?.title || "Failed to load patient appointments",
            };
        } catch {
            return {
                items: [],
                total: 0,
                error: "Unhandled error occurred",
            };
        }
    };

    return (
        <div className="patient-recent-appointments">
            <h4>Confirmed Recent Appointments</h4>
            <PaginatedListView<AppointmentDto>
                pageSize={PAGE_SIZE}
                fetchRequest={fetchAppointments}
                dependencies={[id]}
                renderItems={(appointments) => {
                    if (appointments.length === 0) {
                        return (
                            <div className="status-message">
                                No confirmed appointments for this patient.
                            </div>
                        );
                    }

                    return (
                        <div className="patient-appointments-grid">
                            {appointments.map((appointment: AppointmentDto) => (
                                <Link
                                    key={appointment.id}
                                    to={`/my-schedule/details?id=${appointment.id}`}
                                    className="patient-appointment-link"
                                >
                                    <AppointmentCard appointment={appointment} />
                                </Link>
                            ))}
                        </div>
                    );
                }}
            />
        </div>
    );
}