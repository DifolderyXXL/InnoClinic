import { useMemo, useState} from "react";
import {type AppointmentDto, appointmentsApi, AppointmentState} from "../../../../services/api/AppointmentApi.ts";
import { AppointmentCard } from "../../common/Appointment/AppointmentCard.tsx";
import "./ClientAppointments.css";
import {DateOnly, dateToDateOnly} from "../../../../services/api/ServicesApi.ts";
import {RescheduleModal} from "../../ReceptionistPages/RescheduleModal.tsx";
import {ItemDetails} from "../../Shared/Layouts/ItemDetails.tsx";
import {CancelAppointmentModal} from "../../ReceptionistPages/CancelAppointmentModal.tsx";

function getNumericState(stateValue: string | number): number {
    if (typeof stateValue === "number") return stateValue;

    const stateMap: Record<string, number> = {
        Created: AppointmentState.Created,
        PendingReservation: AppointmentState.PendingReservation,
        PendingApproval: AppointmentState.PendingApproval,
        Approved: AppointmentState.Approved,
        Failed: AppointmentState.Failed,
        Confirmed: AppointmentState.Confirmed,
    };

    return stateMap[stateValue] ?? Number(stateValue);
}

export function ClientAppointment() {
    const [item, setItem] = useState<AppointmentDto | null>(null);
    const [cancelId, setCancelId] = useState<string | null>(null);
    const [reason, setReason] = useState("");
    const [reasonError, setReasonError] = useState<string | null>(null);
    const [isCancelling, setIsCancelling] = useState(false);
    const [isRescheduleOpen, setIsRescheduleOpen] = useState(false);

    const currentStateNumber = item ? getNumericState(item.state) : null;
    const canManageAppointment = currentStateNumber !== null && currentStateNumber <= AppointmentState.PendingApproval;

    const initialDate = useMemo(() => {
        if (!item?.date) return new Date();
        return DateOnly.fromString(item.date).toNativeDate();
    }, [item?.date]);

    const handleCancel = async () => {
        if (!item) return;

        if (!reason.trim()) {
            setReasonError("Please provide a reason for cancellation.");
            return;
        }

        setIsCancelling(true);
        const res = await appointmentsApi.declineMyAppointment(item.id, { reason: reason.trim() });
        setIsCancelling(false);

        if (res.type === "ok") {
            setItem({ ...item, state: "Failed" });
            setCancelId(null);
            setReason("");
            setReasonError(null);
        } else {
            alert(res.error?.title || "Failed to cancel appointment");
        }
    };

    const handleRescheduleSubmit = async (newDate: Date, newStartSlotIndex: number) => {
        if (!item) return;

        const dateOnly = dateToDateOnly(newDate);
        const dateString = dateOnly.toString();

        const res = await appointmentsApi.rescheduleMyAppointment(item.id, {
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
            provider={(id) => appointmentsApi.getMyClientAppointmentById(id)}
            extractor={(res) => res.value}
            onChange={(fetchedItem) => setItem(fetchedItem)}
        >
            {(appointment) => {
                const current = item ?? appointment;

                return (
                    <div className="client-appointment-details-page">
                        <AppointmentCard appointment={current} showResultLink={true} />

                        {canManageAppointment && (
                            <div className="action-buttons" style={{ marginTop: 12, display: "flex", gap: "8px" }}>
                                <button className="btn btn-secondary" onClick={() => setIsRescheduleOpen(true)}>
                                    Reschedule
                                </button>
                                <button
                                    className="btn btn-decline"
                                    onClick={() => {
                                        setCancelId(current.id);
                                        setReason("");
                                        setReasonError(null);
                                    }}
                                >
                                    Cancel
                                </button>
                            </div>
                        )}

                        <RescheduleModal
                            isOpen={isRescheduleOpen}
                            doctorId={current.doctorAccountId}
                            patientId={current.patientAccountId}
                            initialDate={initialDate}
                            initialSlotIndex={current.startSlotIndex}
                            slotAmount={current.slotAmount}
                            onClose={() => setIsRescheduleOpen(false)}
                            onSubmit={handleRescheduleSubmit}
                        />

                        <CancelAppointmentModal
                            isOpen={!!cancelId}
                            reason={reason}
                            reasonError={reasonError}
                            isCancelling={isCancelling}
                            onReasonChange={(val) => {
                                setReason(val);
                                if (reasonError) setReasonError(null);
                            }}
                            onConfirm={handleCancel}
                            onDismiss={() => {
                                setCancelId(null);
                                setReason("");
                                setReasonError(null);
                            }}
                        />
                    </div>
                );
            }}
        </ItemDetails>
    );
}