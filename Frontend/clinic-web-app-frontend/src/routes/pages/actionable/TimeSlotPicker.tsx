import {useMemo, useState} from "react";
import {
    type AvailablePositionsOnDay,
    getSlotsInHour,
    minutesToTimeSpan,
    timeSpanToMinutes
} from "../../../services/api/ServicesApi.ts";

interface TimeSlotPickerProps{
    positions: AvailablePositionsOnDay;
    selected: number;
    slotAmount: number;
    onChange: (value: number) => void;
}
export function TimeSlotPicker({positions, selected, slotAmount, onChange}: TimeSlotPickerProps){
    const [hoverIndex, setHoverIndex] = useState<number | null>(null);

    const slotsInHour = getSlotsInHour(positions.timeSlotLength);
    const slotMinute = timeSpanToMinutes(positions.timeSlotLength);
    const hours = Math.ceil(positions.slotAmount / slotsInHour);

    const minuteLabels = Array.from({ length: slotsInHour }, (_, i) =>
        `${i*slotMinute}-${i*slotMinute+slotMinute}`);
    const dayBeginMinutes = timeSpanToMinutes(positions.dayBeginTime);

    const hourLabels = Array.from({ length: hours }, (_, i) =>
        `${minutesToTimeSpan(dayBeginMinutes + i*60)}`);

    function isSlotAvailable(slotStartMinutes: number): boolean {
        return positions.availableTimeWindows.some(w => {
            const windowStart = dayBeginMinutes + w.timeSlotStart * slotMinute;
            const windowEnd = windowStart + w.timeSlotSize * slotMinute;
            return slotStartMinutes >= windowStart && slotStartMinutes < windowEnd;
        });
    }

    const slotsByHour: Record<number, { index: number; startMinutes: number; isAvailable: boolean }[]> = {};
    hourLabels.forEach((_, idx) => { slotsByHour[idx] = []; });

    for (let i = 0; i < positions.slotAmount; i++) {
        const startMinutes = dayBeginMinutes + i * slotMinute;
        const hour = Math.floor(i / slotsInHour);
        if (slotsByHour[hour]) {
            slotsByHour[hour].push({
                index: i,
                startMinutes,
                isAvailable: isSlotAvailable(startMinutes),
            });
        }
    }

    const allSlots = useMemo(() => {
        const slots: { index: number; startMinutes: number; isAvailable: boolean }[] = [];
        for (let i = 0; i < positions.slotAmount; i++) {
            const startMinutes = dayBeginMinutes + i * slotMinute;
            slots.push({
                index: i,
                startMinutes,
                isAvailable: isSlotAvailable(startMinutes),
            });
        }
        return slots;
    }, [positions.slotAmount, positions.availableTimeWindows]);

    const isInPreviewRange = (index: number): boolean => {
        if (hoverIndex === null) return false;
        if (index < hoverIndex || index >= hoverIndex + slotAmount) return false;
        for (let i = hoverIndex; i < hoverIndex + slotAmount; i++) {
            if (!allSlots[i]?.isAvailable) return false;
        }
        return true;
    };

    const isInSelectedRange = (index: number): boolean => {
        return index >= selected && index < selected + slotAmount;
    };

    return(
        <table className="slots-table" style={{ borderCollapse: 'collapse' }}>
            <thead>
            <tr>
                <th style={{ padding: '4px 8px', border: '1px solid #555' }}>Час</th>
                {minuteLabels.map((min, idx) => (
                    <th key={idx} style={{ padding: '4px 8px', border: '1px solid #555', fontSize: '12px' }}>
                        {String(min).padStart(5, '0')}
                    </th>
                ))}
            </tr>
            </thead>
            <tbody>
            {hourLabels.map((hour, idx) => {
                const slots = slotsByHour[idx] || [];

                const paddedSlots = [...slots];
                while (paddedSlots.length < slotsInHour) {
                    paddedSlots.push({ index: -1, startMinutes: 0, isAvailable: false });
                }
                return (
                    <tr key={hour}>
                        <td style={{ padding: '4px 8px', border: '1px solid #555', fontWeight: 'bold' }}>
                            {String(hour).padStart(2, '0')}:00
                        </td>
                        {paddedSlots.map((slot, idx) => (
                            <td
                                key={`${hour}-${idx}`}
                                className={`slot-item ${slot.isAvailable ? 'available' : 'unavailable'} 
                                ${isInSelectedRange(slot.index)? 'selected' : ''} 
                                ${isInPreviewRange(slot.index) ? 'preview' : ''}`}
                                onMouseEnter={() => setHoverIndex(slot.index)}
                                onMouseLeave={() => setHoverIndex(null)}

                                onClick={() => {
                                    if (!slot.isAvailable) return;

                                    if (hoverIndex !== null && slot.index >= hoverIndex && slot.index < hoverIndex + slotAmount) {
                                        let allAvailable = true;
                                        for (let i = hoverIndex; i < hoverIndex + slotAmount; i++) {
                                            if (!allSlots[i]?.isAvailable) { allAvailable = false; break; }
                                        }
                                        if (allAvailable) {

                                            onChange(hoverIndex);
                                            setHoverIndex(null);
                                            return;
                                        }
                                    }
                                }}
                                title={slot.isAvailable ? `Слот ${slot.index+1}` : 'Занято'}
                            />
                        ))}
                    </tr>
                );
            })}
            </tbody>
        </table>
    );
}