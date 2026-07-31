import {useEffect, useState} from "react";
import {type AppointmentDto, appointmentsApi, AppointmentState} from "../../../services/api/AppointmentApi.ts";
import {Link} from "react-router-dom";
import {useSearchParams} from "react-router";
import {AppointmentCard} from "../common/Appointment/AppointmentCard.tsx";
import {profilesApi} from "../../../services/api/ProfilesApi.ts";
import {TitledCard} from "../common/TitledCard.tsx";
import {PaginatedListView} from "../common/PaginatedListView.tsx";
import {useUpdateUrlParams} from "../specific/doctors/DoctorsPage.tsx";
import DatePicker from "react-datepicker";
import {DateOnly, dateToDateOnly} from "../../../services/api/ServicesApi.ts";

export function MyDoctorSchedule(){
    const { searchParams, updateUrlParams } = useUpdateUrlParams();
    const [schedule, setSchedule] = useState<AppointmentDto[]>([])

    const targetDate = searchParams.get("date") || null;
    const [date, setDate] = useState<Date | null>(DateOnly.parseToNative(targetDate));
    
    useEffect(() => {
        const promise = targetDate 
            ? appointmentsApi.getScheduleMe(targetDate) 
            : appointmentsApi.getScheduleTodayMe()

        promise.then(x=>{
                if(x.type === "ok") setSchedule(x.value);
            })
    }, [targetDate]);
    
    return (
        <div>
            <h3>Schedule</h3>
            <DatePicker selected={date ?? new Date()} onChange={(x:Date | null)=>{
                setDate(x);
                if(x)  updateUrlParams({date: dateToDateOnly(x)});
                
                
            }}/>
            <table>
                <thead>
                <tr>
                    <th>Time</th>
                    <th>Status</th>
                </tr>
                </thead>
                <tbody>
                {schedule.map((slot, idx) => (
                    <ScheduleCard key={idx} appointment={slot} isFree={false}/>
                ))}
                </tbody>
            </table>
        </div>
    );
}

interface ScheduleCardProps{
    appointment: AppointmentDto;
    isFree: boolean;
}
export function ScheduleCard({ appointment, isFree }: ScheduleCardProps) {
    return (
        <tr style={{ backgroundColor: isFree ? "green" : "#333" }}>
            <td style={{ color: "white",}}>
                {appointment.beginTime} – {appointment.endTime}
            </td>
            <td>
                {isFree ? (
                    <span style={{ color: "white"}}>Free</span>
                ) : (
                    <Link key={appointment.id} to={`/my-schedule/details?id=${appointment.id}`} style={{textDecoration:"none"}}>
                        <MinimalAppointmentCard appointment={appointment}/>
                    </Link>
                )}
            </td>
        </tr>
    );
}


interface MinimalAppointmentCardProps {
    appointment: AppointmentDto;
}

export function MinimalAppointmentCard({ appointment }: MinimalAppointmentCardProps) {
    return (
        <div>
            <h3>{appointment.serviceName}</h3>  
            <p>Patient: {appointment.patientFullName}</p>
            <p>Status: {appointment.state}</p>
        </div>
    );
}

export function DoctorScheduledAppointment() {
    const [searchParams] = useSearchParams();
    const [appointment, setAppointment] = useState<AppointmentDto>()

    const targetId = searchParams.get("id") || null;

    useEffect(() => {
        if(targetId == null) return;

        appointmentsApi.getMyDoctorAppointmentById(targetId)
            .then(result =>{
                if( result.type === "ok") setAppointment(result.value);
            })
    }, [targetId]);

    return (
        <div>
            {appointment && (
                <>
                    <AppointmentCard appointment={appointment}/>
                    <PatientCardById id={appointment.patientAccountId}/>
                </>
            )}
        </div>
    );
}


interface PatientCardByIdProps{
    id: string;
}
export function PatientCardById({id}: PatientCardByIdProps){
    const [patient, setPatient] = useState<PatientDto | null>(null);

    useEffect(() => {
        profilesApi.getPatient(id).then(result =>{
            if(result.type === "ok") setPatient(result.value);
        })
    }, []);
    
    if(patient == null) return <></>
    
    return (
          <TitledCard title="Patient">
              <PatientCard patient={patient}/>
              <PatientRecentAppointments id={patient.accountId}/>
          </TitledCard>
    );
}

export interface PatientDto {
    id: number;
    accountId: string;
    dateOfBirth: string;
    accountFirstName: string;
    accountLastName: string;
    accountMiddleName?: string | null;
    accountEmail: string;
}
interface PatientCardProps {
    patient: PatientDto;
}

export function PatientCard({ patient }: PatientCardProps) {
    const fullName = [patient.accountLastName, patient.accountFirstName, patient.accountMiddleName]
        .filter(Boolean)
        .join(' ');

    return (
        <div>
            <h3>{fullName}</h3>
            <p>Birth: {patient.dateOfBirth}</p>
            <p>Email: {patient.accountEmail}</p>
        </div>
    );
}

const PAGE_SIZE: number = 10;
interface PatientRecentAppointmentsProps{
    id: string;
}
export function PatientRecentAppointments({id}: PatientRecentAppointmentsProps){
    const { searchParams, updateUrlParams } = useUpdateUrlParams();
    const currentPage = Number(searchParams.get("page")) || 1;

    const fetchDoctors = async (page: number) => {
        const result = await appointmentsApi.getAppointments(
            AppointmentState.Confirmed, 
            id,
            page,
            PAGE_SIZE,
        );

        if (result.type === "ok") {
            return { items: result.value.items, total: result.value.total };
        }

        return { items: [], total: 0, error: result.error?.title || "Failed to load" };
    };
    
    return (
        <PaginatedListView
            currentPage={currentPage}
            pageSize={PAGE_SIZE}
            onPageChange={(page) => updateUrlParams({ page: String(page) })}
            fetchRequest={fetchDoctors}
            dependencies={[searchParams]}
            renderItems={(schedule) => (
                <div style={{
                    flex: 1, overflowY: 'auto', display: 'flex',
                    flexWrap: 'wrap', gap: '10px',
                    justifyContent: 'flex-start', alignContent: 'flex-start',
                    padding: '10px'
                }}>
                    {schedule.map((appointment: AppointmentDto) => (
                        <Link key={appointment.id} to={`/my-schedule/details?id=${appointment.id}`} style={{textDecoration:"none"}}>
                            <AppointmentCard appointment={appointment}/>
                        </Link>
                        
                    ))}
                </div>
            )}
        />  
    );
}