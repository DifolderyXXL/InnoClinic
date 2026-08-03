import { useState, useCallback } from "react";
import {
    appointmentsApi,
    type AppointmentDto,
    AppointmentState
} from "../../../services/api/AppointmentApi.ts";
import { PaginatedListView, type PaginatedResult } from "../common/PaginatedListView.tsx";
import "./ClinicAppointments.css";

export function ClinicAppointments() {
    // 1. Filter states (AC-7 – AC-11)
    const [date, setDate] = useState<string>("");
    const [doctorFullName, setDoctorFullName] = useState<string>("");
    const [serviceName, setServiceName] = useState<string>("");
    const [state, setState] = useState<AppointmentState | null>(null);
    const [officeId, setOfficeId] = useState<string>("");

    // 2. Pagination & items state
    const [page, setPage] = useState<number>(1);
    const pageSize = 50;

    const [items, setItems] = useState<AppointmentDto[]>([]);

    // State for Cancel confirmation modal (AC-1, AC-2) & Reason text
    const [cancelTargetId, setCancelTargetId] = useState<string | null>(null);
    const [declineReason, setDeclineReason] = useState<string>("");
    const [reasonError, setReasonError] = useState<string | null>(null);
    const [isCancelling, setIsCancelling] = useState<boolean>(false);

    const handleFilterChange = (setter: (val: any) => void, value: any) => {
        setter(value);
        setPage(1);
    };

    // 3. Adapter function: syncs API response with local `items`
    const fetchAppointments = useCallback(async (targetPage: number): Promise<PaginatedResult<AppointmentDto>> => {
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
            setItems(res.value.items);
            return {
                items: res.value.items,
                total: res.value.totalCount
            };
        }

        return {
            items: [],
            total: 0,
            error: res.error?.title || "Failed to load appointments"
        };
    }, [date, doctorFullName, serviceName, state, officeId, pageSize]);

    const approveAppointment = async (id: string) => {
        const res = await appointmentsApi.approveAppointment(id);
        if (res.type === "ok") {
            setItems(prev => prev.map(item => item.id === id ? { ...item, state: String(AppointmentState.Approved) } : item));
        }
    };

    const openCancelModal = (id: string) => {
        setCancelTargetId(id);
        setDeclineReason("");
        setReasonError(null);
    };

    // AC-3: Confirmation handler for deleting / cancelling appointment with Reason
    const handleConfirmCancel = async () => {
        if (!cancelTargetId) return;

        // Validate that reason is provided
        if (!declineReason.trim()) {
            setReasonError("Please provide a reason for cancellation.");
            return;
        }

        setIsCancelling(true);
        // Pass the actual reason entered by the receptionist
        const res = await appointmentsApi.declineAppointment(cancelTargetId, { reason: declineReason.trim() });
        setIsCancelling(false);

        if (res.type === "ok") {
            // AC-3: Remove from table view locally
            setItems(prev => prev.filter(item => item.id !== cancelTargetId));
            // AC-3: Close modal dialog
            setCancelTargetId(null);
            setDeclineReason("");
        } else {
            alert(res.error?.title || "Failed to cancel appointment");
        }
    };

    // AC-4: Close modal without changes
    const handleDismissCancel = () => {
        setCancelTargetId(null);
        setDeclineReason("");
        setReasonError(null);
    };

    return (
        <div>
            <h1>Clinic Appointments</h1>

            {/* Filters */}
            <fieldset>
                <legend>Filters</legend>
                <div>
                    <label>
                        Date:
                        <input
                            type="date"
                            value={date}
                            onChange={(e) => handleFilterChange(setDate, e.target.value)}
                        />
                    </label>

                    <label>
                        Doctor Name:
                        <input
                            type="text"
                            placeholder="Search doctor..."
                            value={doctorFullName}
                            onChange={(e) => handleFilterChange(setDoctorFullName, e.target.value)}
                        />
                    </label>

                    <label>
                        Service:
                        <input
                            type="text"
                            placeholder="Search service..."
                            value={serviceName}
                            onChange={(e) => handleFilterChange(setServiceName, e.target.value)}
                        />
                    </label>

                    <label>
                        State:
                        <select
                            value={state ?? ""}
                            onChange={(e) => {
                                const val = e.target.value;
                                handleFilterChange(setState, val === "" ? null : Number(val) as AppointmentState);
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
                            onChange={(e) => handleFilterChange(setOfficeId, e.target.value)}
                        />
                    </label>

                    <button type="button" onClick={() => {
                        setDate("");
                        setDoctorFullName("");
                        setServiceName("");
                        setState(null);
                        setOfficeId("");
                        setPage(1);
                    }}>
                        Reset Filters
                    </button>
                </div>
            </fieldset>

            <PaginatedListView<AppointmentDto>
                currentPage={page}
                pageSize={pageSize}
                onPageChange={setPage}
                fetchRequest={fetchAppointments}
                dependencies={[fetchAppointments]}
                renderItems={() => (
                    <table border={1} cellPadding={10} cellSpacing={0} style={{ borderColor: "#111" }}>
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
                        {items.length > 0 ? (
                            items.map((item) => (
                                <tr key={item.id}>
                                    <td>{item.date}</td>
                                    <td>{item.beginTime ?? `Slot #${item.startSlotIndex}`}</td>
                                    <td>{item.doctorFullName}</td>
                                    <td>{item.patientFullName}</td>
                                    <td>{item.serviceName}</td>
                                    <td>{item.officeId}</td>
                                    <td>{item.state}</td>
                                    <td>
                                        <div className="action-buttons">
                                            {(item.state === "PendingApproval" || Number(item.state) === AppointmentState.PendingApproval) && (
                                                <button className="btn btn-approve" onClick={() => approveAppointment(item.id)}>
                                                    Approve
                                                </button>
                                            )}
                                            {/* AC-1: Open Cancel modal dialog */}
                                            <button className="btn btn-decline" onClick={() => openCancelModal(item.id)}>
                                                Cancel
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))
                        ) : (
                            <tr>
                                <td colSpan={8} align="center">No appointments found</td>
                            </tr>
                        )}
                        </tbody>
                    </table>
                )}
            />

            {/* AC-1, AC-2: Confirmation Dialog Window + Reason Input */}
            {cancelTargetId && (
                <div style={{
                    position: "fixed",
                    top: 0,
                    left: 0,
                    right: 0,
                    bottom: 0,
                    backgroundColor: "rgba(0,0,0,0.5)",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    zIndex: 1000
                }}>
                    <div style={{
                        background: "#fff",
                        padding: "24px",
                        borderRadius: "8px",
                        maxWidth: "420px",
                        width: "100%",
                        textAlign: "center"
                    }}>
                        {/* AC-1 exact wording */}
                        <p style={{ fontWeight: 600, marginBottom: "16px" }}>
                            Do you really want to cancel the appointment? It will be permanently deleted.
                        </p>

                        {/* Reason Input */}
                        <div style={{ marginBottom: "16px", textAlign: "left" }}>
                            <label style={{ display: "block", fontSize: "14px", marginBottom: "4px" }}>
                                Reason for cancellation:
                            </label>
                            <textarea
                                rows={3}
                                style={{ width: "100%", padding: "8px", boxSizing: "border-box" }}
                                placeholder="Enter reason (e.g. Client requested cancellation)..."
                                value={declineReason}
                                onChange={(e) => {
                                    setDeclineReason(e.target.value);
                                    if (reasonError) setReasonError(null);
                                }}
                            />
                            {reasonError && (
                                <span style={{ color: "red", fontSize: "12px", display: "block", marginTop: "4px" }}>
                                    {reasonError}
                                </span>
                            )}
                        </div>

                        {/* AC-2: Buttons Yes and No */}
                        <div style={{ display: "flex", justifyContent: "flex-end", gap: "12px" }}>
                            <button
                                className="btn"
                                style={{ backgroundColor: "#6b7280", color: "#fff" }}
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