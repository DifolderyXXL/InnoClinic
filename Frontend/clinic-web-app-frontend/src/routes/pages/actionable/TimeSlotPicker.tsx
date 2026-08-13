import { useMemo, useState } from "react";
import {
    type AvailablePositionsOnDay,
    getSlotsInHour,
    minutesToTimeSpan,
    timeSpanToMinutes
} from "../../../services/api/ServicesApi.ts";
import "./TimeSlotPicker.css"

interface TimeSlotPickerProps {
    positions: AvailablePositionsOnDay;
    selected: number;
    slotAmount: number;
    whitelistSlots?: number[];
    onChange: (value: number) => void;
}

export function TimeSlotPicker({ positions, selected, slotAmount, onChange, whitelistSlots = [] }: TimeSlotPickerProps) {
    const [hoverIndex, setHoverIndex] = useState<number | null>(null);

    const slotsInHour = getSlotsInHour(positions.timeSlotLength);
    const slotMinute = timeSpanToMinutes(positions.timeSlotLength);
    const hours = Math.ceil(positions.slotAmount / slotsInHour);
    const dayBeginMinutes = timeSpanToMinutes(positions.dayBeginTime);

    const minuteLabels = useMemo(() => {
        return Array.from({ length: slotsInHour }, (_, i) =>
            `${i * slotMinute}-${(i + 1) * slotMinute}`
        );
    }, [slotsInHour, slotMinute]);

    const hourLabels = useMemo(() => {
        return Array.from({ length: hours }, (_, i) =>
            minutesToTimeSpan(dayBeginMinutes + i * 60)
        );
    }, [hours, dayBeginMinutes]);

    const isSlotAvailable = (slotIndex: number, slotStartMinutes: number): boolean => {
        if (whitelistSlots.includes(slotIndex)) {
            return true;
        }

        return positions.availableTimeWindows.some(w => {
            const windowStart = dayBeginMinutes + w.timeSlotStart * slotMinute;
            const windowEnd = windowStart + w.timeSlotSize * slotMinute;
            return slotStartMinutes >= windowStart && slotStartMinutes < windowEnd;
        });
    };

    const allSlots = useMemo(() => {
        const slots: { index: number; startMinutes: number; isAvailable: boolean; isWhitelisted: boolean }[] = [];
        for (let i = 0; i < positions.slotAmount; i++) {
            const startMinutes = dayBeginMinutes + i * slotMinute;
            const isWhitelisted = whitelistSlots.includes(i);
            slots.push({
                index: i,
                startMinutes,
                isWhitelisted,
                isAvailable: isSlotAvailable(i, startMinutes),
            });
        }
        return slots;
    }, [positions.slotAmount, positions.availableTimeWindows, dayBeginMinutes, slotMinute, whitelistSlots]);

    const slotsByHour = useMemo(() => {
        const result: Record<number, { index: number; startMinutes: number; isAvailable: boolean; isWhitelisted: boolean }[]> = {};
        hourLabels.forEach((_, idx) => { result[idx] = []; });

        allSlots.forEach(slot => {
            const hour = Math.floor(slot.index / slotsInHour);
            if (result[hour]) {
                result[hour].push(slot);
            }
        });

        return result;
    }, [allSlots, hourLabels, slotsInHour]);

    const isInPreviewRange = (index: number): boolean => {
        if (hoverIndex === null || index < 0) return false;
        if (index < hoverIndex || index >= hoverIndex + slotAmount) return false;
        for (let i = hoverIndex; i < hoverIndex + slotAmount; i++) {
            if (!allSlots[i]?.isAvailable) return false;
        }
        return true;
    };

    const isInSelectedRange = (index: number): boolean => {
        if (index < 0) return false;
        return index >= selected && index < selected + slotAmount;
    };

    return (
        <div className="time-slot-picker-container">
            <table className="slots-table">
                <thead>
                <tr>
                    <th className="slot-header-cell">Hour</th>
                    {minuteLabels.map((min, idx) => (
                        <th key={idx} className="slot-header-cell minutes">
                            {min}
                        </th>
                    ))}
                </tr>
                </thead>
                <tbody>
                {hourLabels.map((hour, idx) => {
                    const slots = slotsByHour[idx] || [];
                    const paddedSlots = [...slots];
                    while (paddedSlots.length < slotsInHour) {
                        paddedSlots.push({ index: -1, startMinutes: 0, isAvailable: false, isWhitelisted: false });
                    }

                    return (
                        <tr key={hour}>
                            <td className="slot-hour-cell">
                                {hour}
                            </td>
                            {paddedSlots.map((slot, slotIdx) => {
                                const isSelected = isInSelectedRange(slot.index);
                                const isPreview = isInPreviewRange(slot.index);
                                const isWhitelisted = slot.isWhitelisted;

                                return (
                                    <td
                                        key={`${hour}-${slotIdx}`}
                                        className={`slot-item ${slot.isAvailable ? 'available' : 'unavailable'} ${isWhitelisted ? 'whitelisted' : ''} ${isSelected ? 'selected' : ''} ${isPreview ? 'preview' : ''}`}
                                        onMouseEnter={() => slot.index !== -1 && setHoverIndex(slot.index)}
                                        onMouseLeave={() => setHoverIndex(null)}
                                        onClick={() => {
                                            if (!slot.isAvailable || slot.index === -1) return;

                                            let allAvailable = true;
                                            for (let i = slot.index; i < slot.index + slotAmount; i++) {
                                                if (!allSlots[i]?.isAvailable) {
                                                    allAvailable = false;
                                                    break;
                                                }
                                            }

                                            if (allAvailable) {
                                                onChange(slot.index);
                                                setHoverIndex(null);
                                            }
                                        }}
                                        title={
                                            slot.index === -1
                                                ? ''
                                                : isWhitelisted
                                                    ? `Current Reservation Slot ${slot.index + 1}`
                                                    : slot.isAvailable
                                                        ? `Slot ${slot.index + 1}`
                                                        : 'Occupied'
                                        }
                                    />
                                );
                            })}
                        </tr>
                    );
                })}
                </tbody>
            </table>
        </div>
    );
}