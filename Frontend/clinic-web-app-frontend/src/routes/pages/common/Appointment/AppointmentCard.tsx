import type {AppointmentDto} from "../../../../services/api/AppointmentApi.ts";
import "./AppointmentCard.css"
import {TitledCard} from "../TitledCard.tsx";

interface AppointmentCardProps{
    appointment: AppointmentDto
}
export function AppointmentCard({appointment}:AppointmentCardProps){
    return (
        <TitledCard>
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
        </TitledCard>
       
    );
}
