import "./ClientAppointments.css";
import {useSearchParams} from "react-router";
import {type AppointmentDto, appointmentsApi} from "../../../../services/api/AppointmentApi.ts";
import {useEffect, useState} from "react";
import {AppointmentCard} from "../../common/Appointment/AppointmentCard.tsx";

export function ClientAppointment(){
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

    if(targetId == null)
    {
        return <div>Not found</div>
    }

    if(appointment == null)
    {
        return <div>Loading</div>
    }

    return(
        <AppointmentCard appointment={appointment}/>
    );
}