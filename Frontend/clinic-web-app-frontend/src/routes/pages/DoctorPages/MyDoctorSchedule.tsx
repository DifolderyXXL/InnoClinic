import { useEffect, useState } from "react";
import type { AppointmentDto } from "../../../services/api/AppointmentApi.ts";
import { appointmentsApi } from "../../../services/api/AppointmentApi.ts";
import DatePicker from "react-datepicker";
import { useUpdateUrlParams } from "../specific/doctors/DoctorsPage.tsx";
import { DateOnly, dateToDateOnly } from "../../../services/api/ServicesApi.ts";
import { ScheduleCard } from "./ScheduleView.tsx";

export function MyDoctorSchedule() {
  const { searchParams, updateUrlParams } = useUpdateUrlParams();
  const [schedule, setSchedule] = useState<AppointmentDto[]>([]);

  const targetDate = searchParams.get("date") || null;
  const [date, setDate] = useState<Date | null>(
    DateOnly.parseToNative(targetDate),
  );

  useEffect(() => {
    const promise = targetDate
      ? appointmentsApi.getScheduleMe(targetDate)
      : appointmentsApi.getScheduleTodayMe();

    promise.then((x) => {
      if (x.type === "ok") setSchedule(x.value);
    });
  }, [targetDate]);

  return (
    <div>
      <h3>Schedule</h3>
      <DatePicker
        selected={date ?? new Date()}
        onChange={(x: Date | null) => {
          setDate(x);
          if (x) updateUrlParams({ date: dateToDateOnly(x) });
        }}
      />
      <table>
        <thead>
          <tr>
            <th>Time</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {schedule.map((slot, idx) => (
            <ScheduleCard key={idx} appointment={slot} isFree={false} />
          ))}
        </tbody>
      </table>
    </div>
  );
}

export { DoctorScheduledAppointment } from "./PatientDetail.tsx";
