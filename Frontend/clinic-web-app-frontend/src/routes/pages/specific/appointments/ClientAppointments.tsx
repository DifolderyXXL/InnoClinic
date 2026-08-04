import { useCallback } from "react";
import { Link } from "react-router-dom";
import { type AppointmentDto, appointmentsApi } from "../../../../services/api/AppointmentApi.ts";
import { AppointmentCard } from "../../common/Appointment/AppointmentCard.tsx";
import { PaginatedListView, type PaginatedResult } from "../../common/PaginatedListView.tsx";
import "./ClientAppointments.css";

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
                    error: result.error?.title || "Failed to load appointments",
                };
            } catch {
                return {
                    items: [],
                    total: 0,
                    error: "An unexpected error occurred",
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