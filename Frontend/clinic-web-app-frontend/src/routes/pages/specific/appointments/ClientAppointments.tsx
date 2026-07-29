import {useEffect, useState} from "react";
import {type AppointmentDto, appointmentsApi} from "../../../../services/api/AppointmentApi.ts";
import "./ClientAppointments.css"
import {PageSelector} from "../../Shared/PageSelector.tsx";
import {Link} from "react-router-dom";

const pageSize: number = 50;
export function ClientAppointments(){
    const [appointments, setAppointments] = useState<AppointmentDto[]>([])
    const [totalPages, setTotalPages] = useState<number>(1)
    
    const load = async (page: number) =>{
        appointmentsApi.getMyClientAppointments(undefined, page, pageSize)
            .then(result => {
                if(result.type === "ok") {
                    console.log(result.value)
                    setAppointments(result.value.items ?? []);
                    setTotalPages(result.value.totalCount)
                }
            })  
    };
    
    useEffect(() => {
        load(1)
    }, []);
    
    const appointmentViews = appointments.map(a =>(
        <Link key={a.id} className="appointment-link" to={`/my-appointments/details?id=${a.id}`}>
            <AppointmentCard appointment={a}/>
        </Link>
        )
    );
    
    return (
        <div style={{display:"flex", flexDirection:"column"}}>

            <div className="appointments-list-container">
                {appointmentViews}
            </div>
            <PageSelector total={totalPages??1} pageSize={pageSize} onPageChange={x=>load(x)}/>
        </div>
    );
}

interface AppointmentCardProps{
    appointment: AppointmentDto
}
export function AppointmentCard({appointment}:AppointmentCardProps){
    return (
        <div className="appointment-card">
            <div className="appointment-header">
                <span className="appointment-date">{appointment.date}</span>
                <span className={`appointment-status status-${appointment.state.toLowerCase()}`}>
                      {appointment.state}
                    </span>
            </div>

            <div className="appointment-body">
                <h3 className="service-name">{appointment.serviceName}</h3>

                <div className="appointment-details">
                    <p><strong>Doctor:</strong> {appointment.doctorFullName}</p>
                    <p><strong>Patient:</strong> {appointment.patientFullName}</p>
                    <p>
                        <strong>Time:</strong>{' '}
                        {appointment.beginTime && appointment.endTime ? (
                            <span>{appointment.beginTime} — {appointment.endTime}</span>
                        ) : (
                            <span>Slot {appointment.startSlotIndex} (Pending reservation)</span>
                        )}
                    </p>
                </div>
            </div>
        </div>
    );
}

