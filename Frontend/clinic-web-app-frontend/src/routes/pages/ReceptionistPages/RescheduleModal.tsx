import {useEffect, useMemo, useState} from "react";
import { TimeSlotPicker } from "../actionable/TimeSlotPicker.tsx";
import { type AvailablePositionsOnDay, servicesApi } from "../../../services/api/ServicesApi.ts";
import DatePicker from "react-datepicker";

interface RescheduleModalProps {
    isOpen: boolean;
    doctorId: string;
    patientId?: string;
    initialDate: Date;
    initialSlotIndex: number;
    slotAmount: number;
    onClose: () => void;
    onSubmit: (newDate: Date, newStartSlotIndex: number) => Promise<void>;
}

export function RescheduleModal({
                                    isOpen,
                                    doctorId,
                                    patientId,
                                    initialDate,
                                    initialSlotIndex,
                                    slotAmount,
                                    onClose,
                                    onSubmit,
                                }: RescheduleModalProps) {
    const [newDate, setNewDate] = useState<Date | null>(initialDate);
    const [newStartSlotIndex, setNewStartSlotIndex] = useState(initialSlotIndex);
    const [timeSlots, setTimeSlots] = useState<AvailablePositionsOnDay | null>(null);
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        if (isOpen) {
            setNewDate(initialDate);
            setNewStartSlotIndex(initialSlotIndex);
        }
    }, [isOpen, initialDate, initialSlotIndex]);

    useEffect(() => {
        if (!isOpen || !doctorId || !newDate) {
            setTimeSlots(null);
            return;
        }

        servicesApi.getAvailableDoctorSlots(doctorId, newDate, patientId).then((res) => {
            if (res.type === "ok") {
                setTimeSlots(res.value);
            } else {
                setTimeSlots(null);
            }
        });
    }, [isOpen, doctorId, newDate, patientId]);


    const whitelistSlots = useMemo(() => {
        return Array.from({ length: slotAmount }, (_, i) => initialSlotIndex + i);
    }, [initialSlotIndex, slotAmount]);


    if (!isOpen) return null;

    const handleSave = async () => {
        if (!newDate) return;
        setLoading(true);
        try {
            await onSubmit(newDate, Number(newStartSlotIndex));
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="modal-overlay">
            <div className="modal-content">
                <h3>Reschedule Appointment</h3>

                <div className="modal-field">
                    <label>New Date:</label>
                    <DatePicker
                        selected={newDate}
                        onChange={(d) => setNewDate(d)}
                        dateFormat="yyyy-MM-dd"
                        placeholderText="Choose a date"
                        className="date-picker-input"
                        wrapperClassName="date-picker-wrapper"
                    />
                </div>

                <div className="modal-field">
                    <label>Select Time Slot:</label>
                    {timeSlots ? (
                        <TimeSlotPicker
                            positions={timeSlots}
                            selected={newStartSlotIndex}
                            slotAmount={slotAmount}
                            whitelistSlots={whitelistSlots}
                            onChange={(slot) => setNewStartSlotIndex(slot)}
                        />
                    ) : (
                        <div>Loading slots...</div>
                    )}
                </div>

                <div className="modal-actions" style={{ marginTop: 12 }}>
                    <button className="btn btn-secondary" onClick={onClose} disabled={loading}>
                        Close
                    </button>
                    <button className="btn btn-approve" onClick={handleSave} disabled={loading || !newDate}>
                        {loading ? "Saving..." : "Save Changes"}
                    </button>
                </div>
            </div>
        </div>
    );
}