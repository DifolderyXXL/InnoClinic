import {useEffect, useState} from "react";
import {type AppointmentDto, appointmentsApi} from "../../../../services/api/AppointmentApi.ts";
import "./ClientAppointments.css"
import {PageSelector} from "../../Shared/PageSelector.tsx";
import {Link} from "react-router-dom";
import {AppointmentCard} from "../../common/Appointment/AppointmentCard.tsx";

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

