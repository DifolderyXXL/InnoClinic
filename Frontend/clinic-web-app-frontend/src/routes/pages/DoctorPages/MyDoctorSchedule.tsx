import {type ScheduleDto, servicesApi} from "../../../services/api/ServicesApi.ts";
import {useEffect, useState} from "react";
import {type AppointmentDto, appointmentsApi} from "../../../services/api/AppointmentApi.ts";
import {Link} from "react-router-dom";
import {useSearchParams} from "react-router";
import {AppointmentCard} from "../common/Appointment/AppointmentCard.tsx";

export function MyDoctorSchedule(){
    const [schedule, setSchedule] = useState<ScheduleDto[]>([])

    useEffect(() => {
        servicesApi.getScheduleMe("2026-07-29")
            .then(x=>{
                if(x.type === "ok") setSchedule(x.value.schedule);
            })
    }, []);
    
    return (
        <div>
            <h3>Schedule</h3>
            <table>
                <thead>
                <tr>
                    <th>Time</th>
                    <th>Status</th>
                </tr>
                </thead>
                <tbody>
                {schedule.map((slot, idx) => (
                    <Link key={slot.appointmentId} to={`/my-schedule/details?id=${slot.appointmentId}`} style={{textDecoration:"none"}}>
                        <ScheduleCard key={idx} schedule={slot} isFree={false}/>
                    </Link>
                ))}
                </tbody>
            </table>
        </div>
    );
}

interface ScheduleCardProps{
    schedule: ScheduleDto;
    isFree: boolean;
}
export function ScheduleCard({ schedule, isFree }: ScheduleCardProps) {
    const { beginTime, endTime, appointmentId } = schedule;

    return (
        <tr style={{ backgroundColor: isFree ? "green" : "#333" }}>
            <td style={{ color: "white",}}>
                {beginTime} – {endTime}
            </td>
            <td>
                {isFree ? (
                    <span style={{ color: "white"}}>Free</span>
                ) : (
                    <MinimalAppointmentCard id={appointmentId}/>
                )}
            </td>
        </tr>
    );
}

interface MinimalAppointmentCardProps {
    id: string;
}

export function MinimalAppointmentCard({ id }: MinimalAppointmentCardProps) {
    const [appointment, setAppointment] = useState<AppointmentDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        appointmentsApi
            .getMyClientAppointmentById(id)
            .then((data) => {
                if (data.type === 'ok') setAppointment(data.value);
                setLoading(false);
            })
            .catch(() => {
                setError('Не удалось загрузить запись');
                setLoading(false);
            });
    }, [id]);

    if (loading) return <div>Загрузка...</div>;
    if (error) return <div>{error}</div>;
    if (!appointment) return <div>Запись не найдена</div>;

    return (
        <div>
            <h3>{appointment.serviceName}</h3>  
            <p>Пациент: {appointment.patientFullName}</p>
            <p>Статус: {appointment.state}</p>
        </div>
    );
}

export function DoctorScheduledAppointment() {
    const [searchParams] = useSearchParams();
    const [appointment, setAppointment] = useState<AppointmentDto>()

    const targetId = searchParams.get("id") || null;

    useEffect(() => {
        if(targetId == null) return;

        appointmentsApi.getMyClientAppointmentById(targetId)
            .then(result =>{
                if( result.type === "ok") setAppointment(result.value);
            })
    }, []);

    return (
        <div>
            {appointment && (<AppointmentCard appointment={appointment}/>)}
            
        </div>
    );
}
