import {type AppointmentDto, appointmentsApi} from "../../../../services/api/AppointmentApi.ts";
import "./AppointmentCard.css"
import {Link} from "react-router-dom";
import {useEffect, useState} from "react";

interface AppointmentCardProps {
    appointment: AppointmentDto;
    showResultLink?: boolean;
}
export function AppointmentCard({appointment, showResultLink}:AppointmentCardProps){
    return (
        <div>
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

                {showResultLink && (
                    <div className="appointment-actions">
                        <Link to={`/medical-results/details?id=${appointment.id}&userId=${appointment.patientAccountId}`}>
                            View medical result
                        </Link>
                    </div>
                )}
            </div>
        </div>
       
    );
}

interface AppointmentByIdCardProps{
    appointmentId: string;
    showResultLink: boolean;
}
export function MyDoctorAppointmentByIdCard({appointmentId, showResultLink}:AppointmentByIdCardProps){
    const [appointment, setAppointment] = useState<AppointmentDto|null>(null);

    useEffect(() => {
        appointmentsApi.getMyDoctorAppointmentById(appointmentId)
            .then(result =>{
                if(result.type === "ok") setAppointment(result.value);
            })
    }, []);
    
    if(!appointment) return <></>
    
    return <AppointmentCard appointment={appointment} showResultLink={showResultLink}/>;
}

