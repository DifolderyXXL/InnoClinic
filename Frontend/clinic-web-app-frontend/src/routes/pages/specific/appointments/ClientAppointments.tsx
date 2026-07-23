import {useEffect, useState} from "react";
import {type AppointmentDto, appointmentsApi} from "../../../../services/api/AppointmentApi.ts";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";
import {type DoctorProfile, DoctorViewCard} from "../doctors/DoctorsPage.tsx";

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
            <div className="appointment-container">
                <span>{a.date}</span>
                <span>{a.state}</span>
                <DoctorById id={a.doctorAccountId}/>
            </div>
        )
    );
    
    return (
        <div className="appointments-list-container">
            {appointmentViews}
        </div>
    );
}

interface DoctorByIdProps{
    id: string;
}
export function DoctorById({id}:DoctorByIdProps){
    const [doctor, setDoctor] = useState<DoctorProfile | null>(null);

    useEffect(() => {
        profilesApi.getDoctorById(String(id))
            .then(result =>{
                if(result.type === "ok") setDoctor(result.value);
            })
    }, []);
    
    if(!doctor)
    {
        return (<div>...</div>);
    }
    
    return (
        <DoctorViewCard doctor={doctor}/>
    );
}