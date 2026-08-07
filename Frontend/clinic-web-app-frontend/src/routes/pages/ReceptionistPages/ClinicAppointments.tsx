import { useState, useCallback } from "react";
import {
    appointmentsApi,
    type AppointmentDto,
    AppointmentState
} from "../../../services/api/AppointmentApi.ts";
import { PaginatedListView, type PaginatedResult } from "../common/PaginatedListView.tsx";
import { useSearchParams } from "react-router";
import "./ClinicAppointments.css";

// Хелпер для корректного отображения названия статуса
function formatAppointmentState(state: AppointmentState | string | number): string {
    if (typeof state === "string" && isNaN(Number(state))) {
        return state;
    }
    const numState = Number(state);
    switch (numState) {
        case AppointmentState.Created: return "Created";
        case AppointmentState.PendingReservation: return "Pending Reservation";
        case AppointmentState.PendingApproval: return "Pending Approval";
        case AppointmentState.Approved: return "Approved";
        case AppointmentState.Confirmed: return "Confirmed";
        case AppointmentState.Failed: return "Failed";
        default: return String(state ?? "—");
    }
}

export function ClinicAppointments() {
    const [searchParams, setSearchParams] = useSearchParams();

    const [date, setDate] = useState<string>(searchParams.get("date") || "");
    const [doctorFullName, setDoctorFullName] = useState<string>(searchParams.get("doctorFullName") || "");
    const [serviceName, setServiceName] = useState<string>(searchParams.get("serviceName") || "");
    const [state, setState] = useState<AppointmentState | null>(
        searchParams.get("state") ? (Number(searchParams.get("state")) as AppointmentState) : null
    );
    const [officeId, setOfficeId] = useState<string>(searchParams.get("officeId") || "");

    const pageSize = 50;

    const [localItems, setLocalItems] = useState<AppointmentDto[]>([]);

    const [cancelTargetId, setCancelTargetId] = useState<string | null>(null);
    const [declineReason, setDeclineReason] = useState<string>("");
    const [reasonError, setReasonError] = useState<string | null>(null);
    const [isCancelling, setIsCancelling] = useState<boolean>(false);

    const updateFilter = (newParams: Record<string, string | number | null>) => {
        const nextParams = new URLSearchParams(searchParams);

        Object.entries(newParams).forEach(([key, val]) => {
            if (val !== null && val !== undefined && val !== "") {
                nextParams.set(key, String(val));
            } else {
                nextParams.delete(key);
            }
        });

        nextParams.set("page", "1");
        setSearchParams(nextParams, { replace: true });
    };

    const resetFilters = () => {
        setDate("");
        setDoctorFullName("");
        setServiceName("");
        setState(null);
        setOfficeId("");
        setSearchParams({ page: "1" }, { replace: true });
    };

    const fetchAppointments = useCallback(async (targetPage: number): Promise<PaginatedResult<AppointmentDto>> => {
        try {
            const res = await appointmentsApi.getClinicAppointments({
                date: date || null,
                doctorFullName: doctorFullName || null,
                serviceName: serviceName || null,
                status: state,
                officeId: officeId || null,
                page: targetPage,
                pageSize
            });

            if (res.type === "ok") {
                const fetchedItems = res.value.items ?? [];
                setLocalItems(fetchedItems);
                return {
                    items: fetchedItems,
                    total: res.value.totalCount ?? 0
                };
            }

            return {
                items: [],
                total: 0,
                error: res.error?.title || "Failed to load appointments"
            };
        } catch {
            return {
                items: [],
                total: 0,
                error: "Unhandled error occurred"
            };
        }
    }, [date, doctorFullName, serviceName, state, officeId, pageSize]);

    const approveAppointment = async (id: string) => {
        const res = await appointmentsApi.approveAppointment(id);
        if (res.type === "ok") {
            // Устанавливаем строковое значение "Approved" вместо enum-индекса
            setLocalItems(prev => prev.map(item => item.id === id ? { ...item, state: "Approved" } : item));
        }
    };

    const openCancelModal = (id: string) => {
        setCancelTargetId(id);
        setDeclineReason("");
        setReasonError(null);
    };

    const handleConfirmCancel = async () => {
        if (!cancelTargetId) return;

        if (!declineReason.trim()) {
            setReasonError("Please provide a reason for cancellation.");
            return;
        }

        setIsCancelling(true);
        const res = await appointmentsApi.declineAppointment(cancelTargetId, { reason: declineReason.trim() });
        setIsCancelling(false);

        if (res.type === "ok") {
            setLocalItems(prev => prev.filter(item => item.id !== cancelTargetId));
            setCancelTargetId(null);
            setDeclineReason("");
        } else {
            alert(res.error?.title || "Failed to cancel appointment");
        }
    };

    const handleDismissCancel = () => {
        setCancelTargetId(null);
        setDeclineReason("");
        setReasonError(null);
    };

    return (
        <div className="clinic-appointments-page">
            <h1>Clinic Appointments</h1>

            <fieldset className="filters-fieldset">
                <legend>Filters</legend>
                <div className="filters-row">
                    <label>
                        Date:
                        <input
                            type="date"
                            value={date}
                            onChange={(e) => {
                                const val = e.target.value;
                                setDate(val);
                                updateFilter({ date: val });
                            }}
                        />
                    </label>

                    <label>
                        Doctor Name:
                        <input
                            type="text"
                            placeholder="Search doctor..."
                            value={doctorFullName}
                            onChange={(e) => {
                                const val = e.target.value;
                                setDoctorFullName(val);
                                updateFilter({ doctorFullName: val });
                            }}
                        />
                    </label>

                    <label>
                        Service:
                        <input
                            type="text"
                            placeholder="Search service..."
                            value={serviceName}
                            onChange={(e) => {
                                const val = e.target.value;
                                setServiceName(val);
                                updateFilter({ serviceName: val });
                            }}
                        />
                    </label>

                    <label>
                        State:
                        <select
                            value={state ?? ""}
                            onChange={(e) => {
                                const val = e.target.value;
                                const newState = val === "" ? null : (Number(val) as AppointmentState);
                                setState(newState);
                                updateFilter({ state: newState });
                            }}
                        >
                            <option value="">All States</option>
                            <option value={AppointmentState.Created}>Created</option>
                            <option value={AppointmentState.PendingReservation}>Pending Reservation</option>
                            <option value={AppointmentState.PendingApproval}>Pending Approval</option>
                            <option value={AppointmentState.Approved}>Approved</option>
                            <option value={AppointmentState.Confirmed}>Confirmed</option>
                            <option value={AppointmentState.Failed}>Failed</option>
                        </select>
                    </label>

                    <label>
                        Office ID:
                        <input
                            type="text"
                            placeholder="Office ID"
                            value={officeId}
                            onChange={(e) => {
                                const val = e.target.value;
                                setOfficeId(val);
                                updateFilter({ officeId: val });
                            }}
                        />
                    </label>

                    <button type="button" onClick={resetFilters}>
                        Reset Filters
                    </button>
                </div>
            </fieldset>

            <PaginatedListView<AppointmentDto>
                pageSize={pageSize}
                fetchRequest={fetchAppointments}
                dependencies={[date, doctorFullName, serviceName, state, officeId]}
                renderItems={() => (
                    <div className="clinic-table-wrapper">
                        <table className="clinic-appointments-table">
                            <thead>
                            <tr>
                                <th>Date</th>
                                <th>Time / Slot</th>
                                <th>Doctor</th>
                                <th>Patient</th>
                                <th>Service</th>
                                <th>Office</th>
                                <th>State</th>
                                <th>Action</th>
                            </tr>
                            </thead>
                            <tbody>
                            {localItems.length > 0 ? (
                                localItems.map((item) => {
                                    const formattedState = formatAppointmentState(item.state);
                                    const isPendingApproval =
                                        item.state === "PendingApproval" ||
                                        Number(item.state) === AppointmentState.PendingApproval;

                                    return (
                                        <tr key={item.id}>
                                            <td>{item.date}</td>
                                            <td>{item.beginTime ?? `Slot #${item.startSlotIndex}`}</td>
                                            <td>{item.doctorFullName}</td>
                                            <td>{item.patientFullName}</td>
                                            <td>{item.serviceName}</td>
                                            <td>{item.officeId}</td>
                                            <td>{formattedState}</td>
                                            <td>
                                                <div className="action-buttons">
                                                    <button className="btn btn-decline" onClick={() => openCancelModal(item.id)}>
                                                        Cancel
                                                    </button>

                                                    {isPendingApproval && (
                                                        <button className="btn btn-approve" onClick={() => approveAppointment(item.id)}>
                                                            Approve
                                                        </button>
                                                    )}
                                                </div>
                                            </td>
                                        </tr>
                                    );
                                })
                            ) : (
                                <tr>
                                    <td colSpan={8} style={{ textAlign: "center", color: "#606070" }}>
                                        No appointments found
                                    </td>
                                </tr>
                            )}
                            </tbody>
                        </table>
                    </div>
                )}
            />

            {cancelTargetId && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <p className="modal-title">
                            Do you really want to cancel the appointment? It will be permanently deleted.
                        </p>

                        <div className="modal-field">
                            <label>Reason for cancellation:</label>
                            <textarea
                                placeholder="Enter reason (e.g. Client requested cancellation)..."
                                value={declineReason}
                                onChange={(e) => {
                                    setDeclineReason(e.target.value);
                                    if (reasonError) setReasonError(null);
                                }}
                            />
                            {reasonError && (
                                <span style={{ color: "#ef4444", fontSize: "12px", marginTop: "2px" }}>
                                    {reasonError}
                                </span>
                            )}
                        </div>

                        <div className="modal-actions">
                            <button
                                className="btn btn-secondary"
                                onClick={handleDismissCancel}
                                disabled={isCancelling}
                            >
                                No
                            </button>
                            <button
                                className="btn btn-decline"
                                onClick={handleConfirmCancel}
                                disabled={isCancelling}
                            >
                                {isCancelling ? "Cancelling..." : "Yes"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}