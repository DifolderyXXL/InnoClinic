import {useEffect, useState} from "react";
import {type AppointmentDto, appointmentsApi} from "../../../../services/api/AppointmentApi.ts";
import "./ClientAppointments.css"

export function ClientAppointments(){
    const [appointments, setAppointments] = useState<AppointmentDto[]>([])       
    
    useEffect(() => {
        appointmentsApi.getMyClientAppointments()
            .then(result => {
                if(result.type === "ok")
                    setAppointments(result.value.items ?? []);
            })
    }, []);
    
    const appointmentViews = appointments.map(a =>(
           <AppointmentCard appointment={a}/>
        )
    );
    
    return (
        <div className="appointments-list-container">
            {appointmentViews}
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