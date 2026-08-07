import { useEffect, useState } from "react";
import { useSearchParams } from "react-router";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";

import type { AppointmentDto } from "../../../services/api/AppointmentApi.ts";
import { appointmentsApi } from "../../../services/api/AppointmentApi.ts";
import { DateOnly, dateToDateOnly } from "../../../services/api/ServicesApi.ts";
import { ScheduleCard } from "./ScheduleView.tsx";
import "./MyDoctorSchedule.css";

export function MyDoctorSchedule() {
    const [searchParams, setSearchParams] = useSearchParams();

    const targetDate = searchParams.get("date") || null;
    const [date, setDate] = useState<Date | null>(
        targetDate ? DateOnly.parseToNative(targetDate) : new Date()
    );
    const [schedule, setSchedule] = useState<AppointmentDto[]>([]);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        setLoading(true);
        const promise = targetDate
            ? appointmentsApi.getScheduleMe(targetDate)
            : appointmentsApi.getScheduleTodayMe();

        promise.then((x) => {
            if (x.type === "ok") {
                setSchedule(x.value ?? []);
            } else {
                setSchedule([]);
            }
            setLoading(false);
        }).catch(() => {
            setSchedule([]);
            setLoading(false);
        });
    }, [targetDate]);

    const handleDateChange = (newDate: Date | null) => {
        setDate(newDate);
        const nextParams = new URLSearchParams(searchParams);
        if (newDate) {
            nextParams.set("date", dateToDateOnly(newDate));
        } else {
            nextParams.delete("date");
        }
        setSearchParams(nextParams, { replace: true });
    };

    return (
        <div className="doctor-schedule-page">
            <div className="schedule-header">
                <h2>Doctor Schedule</h2>

                <div className="schedule-datepicker-field">
                    <label>Select Date:</label>
                    <DatePicker
                        selected={date ?? new Date()}
                        onChange={handleDateChange}
                        dateFormat="yyyy-MM-dd"
                        className="date-picker-input"
                    />
                </div>
            </div>

            <div className="schedule-table-wrapper">
                <table className="schedule-table">
                    <thead>
                    <tr>
                        <th>Time / Slot</th>
                        <th>Status & Details</th>
                    </tr>
                    </thead>
                    <tbody>
                    {loading ? (
                        <tr>
                            <td colSpan={2} className="schedule-status-cell">
                                Loading schedule...
                            </td>
                        </tr>
                    ) : schedule.length > 0 ? (
                        schedule.map((slot, idx) => (
                            <ScheduleCard key={slot.id ?? idx} appointment={slot} isFree={false} />
                        ))
                    ) : (
                        <tr>
                            <td colSpan={2} className="schedule-status-cell">
                                No appointments scheduled for this date.
                            </td>
                        </tr>
                    )}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

export { DoctorScheduledAppointment } from "./PatientDetail.tsx";