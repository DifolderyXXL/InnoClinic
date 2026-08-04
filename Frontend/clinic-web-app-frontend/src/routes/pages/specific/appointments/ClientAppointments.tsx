import { useCallback } from "react";
import { type AppointmentDto, appointmentsApi } from "../../../../services/api/AppointmentApi.ts";
import { Link } from "react-router-dom";
import { AppointmentCard } from "../../common/Appointment/AppointmentCard.tsx";
import "./ClientAppointments.css";
import {PaginatedListView, type PaginatedResult} from "../../common/PaginatedListView.tsx";

const pageSize: number = 50;

export function ClientAppointments() {
    const fetchAppointments = useCallback(
        async (page: number): Promise<PaginatedResult<AppointmentDto>> => {
            try {
                const result = await appointmentsApi.getMyClientAppointments(undefined, page, pageSize);

                if (result.type === "ok") {
                    return {
                        items: result.value.items ?? [],
                        total: result.value.totalCount ?? 0,
                    };
                }

                return {
                    items: [],
                    total: 0,
                    error: result.error?.title || "Error loading appointments",
                };
            } catch {
                return {
                    items: [],
                    total: 0,
                    error: "Unhandled error occurred",
                };
            }
        },
        []
    );

    return (
        <div className="client-appointments-page">
            <PaginatedListView<AppointmentDto>
                pageSize={pageSize}
                fetchRequest={fetchAppointments}
                renderItems={(items) => {
                    if (items.length === 0) {
                        return (
                            <div className="status-message">
                                You don't have any appointments yet.
                            </div>
                        );
                    }

                    return (
                        <div className="appointments-list-container">
                            {items.map((appointment) => (
                                <Link
                                    key={appointment.id}
                                    className="appointment-link"
                                    to={`/my-appointments/details?id=${appointment.id}`}
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