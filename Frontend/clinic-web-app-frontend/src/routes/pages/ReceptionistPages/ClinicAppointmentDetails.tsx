import { type AppointmentDto, appointmentsApi, AppointmentState } from "../../../services/api/AppointmentApi.ts";
import { AppointmentCard } from "../common/Appointment/AppointmentCard.tsx";
import { ItemDetails } from "../Shared/Layouts/ItemDetails.tsx";
import { useEffect, useState } from "react";
import { RescheduleModal } from "./RescheduleModal.tsx";
import { DateOnly, dateToDateOnly, servicesApi } from "../../../services/api/ServicesApi.ts";

export function ClinicAppointmentDetails() {
    const [item, setItem] = useState<AppointmentDto | null>(null);
    const [cancelId, setCancelId] = useState<string | null>(null);
    const [reason, setReason] = useState("");
    const [isRescheduleOpen, setIsRescheduleOpen] = useState(false);
    const [slotLength, setSlotLength] = useState<number>(1);

    const isPending = item && (item.state === "PendingApproval" || Number(item.state) === AppointmentState.PendingApproval);

    useEffect(() => {
        if (!item?.serviceId) return;
        servicesApi.getService(item.serviceId).then((res) => {
            if (res.type === "ok" && res.value?.slotLength) {
                setSlotLength(res.value.slotLength);
            }
        });
    }, [item?.serviceId]);

    const handleApprove = async () => {
        if (!item) return;
        const res = await appointmentsApi.approveAppointment(item.id);
        if (res.type === "ok") {
            setItem({ ...item, state: "Approved" });
        }
    };

    const handleCancel = async () => {
        if (!item || !reason.trim()) return;
        const res = await appointmentsApi.declineAppointment(item.id, { reason });
        if (res.type === "ok") {
            setItem({ ...item, state: "Failed" });
            setCancelId(null);
        }
    };

    const handleRescheduleSubmit = async (newDate: Date, newStartSlotIndex: number) => {
        if (!item) return;

        const dateOnly = dateToDateOnly(newDate);
        const dateString = dateOnly.toString();

        const res = await appointmentsApi.rescheduleAppointment(item.id, {
            newDate: dateString,
            newStartSlotIndex
        });

        if (res.type === "ok") {
            setItem({
                ...item,
                date: dateString,
                startSlotIndex: newStartSlotIndex,
                state: "PendingApproval"
            });
            setIsRescheduleOpen(false);
        } else {
            alert(res.error?.title || "Failed to reschedule appointment");
        }
    };

    return (
        <ItemDetails<AppointmentDto>
            provider={(id) => appointmentsApi.getAppointmentById(id)}
            extractor={(res) => res.value}
            onChange={(fetchedItem) => setItem(fetchedItem)}
        >
            {(appointment) => {
                const current = item ?? appointment;

                return (
                    <div>
                        <AppointmentCard appointment={current} showResultLink={false} />

                        <div className="action-buttons" style={{ marginTop: 12, display: "flex", gap: "8px" }}>
                            <button className="btn btn-secondary" onClick={() => setIsRescheduleOpen(true)}>
                                Reschedule
                            </button>
                            <button className="btn btn-decline" onClick={() => setCancelId(current.id)}>
                                Cancel
                            </button>
                            {isPending && (
                                <button className="btn btn-approve" onClick={handleApprove}>
                                    Approve
                                </button>
                            )}
                        </div>

                        <RescheduleModal
                            isOpen={isRescheduleOpen}
                            doctorId={current.doctorAccountId}
                            patientId={current.patientAccountId}
                            initialDate={DateOnly.fromString(current.date).toNativeDate()}
                            initialSlotIndex={current.startSlotIndex}
                            slotAmount={slotLength}
                            onClose={() => setIsRescheduleOpen(false)}
                            onSubmit={handleRescheduleSubmit}
                        />

                        {cancelId && (
                            <div className="modal-overlay">
                                <div className="modal-content">
                                    <textarea
                                        value={reason}
                                        onChange={(e) => setReason(e.target.value)}
                                        placeholder="Reason for cancellation..."
                                    />
                                    <button onClick={handleCancel}>Confirm</button>
                                    <button onClick={() => setCancelId(null)}>Close</button>
                                </div>
                            </div>
                        )}
                    </div>
                );
            }}
        </ItemDetails>
    );
}