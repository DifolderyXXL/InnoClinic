import { useSearchParams} from "react-router";
import {useEffect, useState} from "react";
import type {DoctorProfile} from "./DoctorsPage.tsx";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";
import {AvatarFromSource} from "../../Shared/Avatar.tsx";
import {OfficeCompactCard} from "../offices/OfficeCompactCard.tsx";
import "./DoctorsPage.css";
import type {ServiceDto} from "../services/ServicesPage.tsx";
import {servicesApi} from "../../../../services/api/ServicesApi.ts";
import {ServiceView} from "../services/GroupBySpecializationServices.tsx";
import {useAppointmentNavigation} from "../../actionable/hooks/useAppointmentNavigation.tsx";

export function DoctorPage(){
    const [ navigateToAppointment ]  = useAppointmentNavigation();
    const [searchParams] = useSearchParams();
    const [doctor, setDoctor] = useState<DoctorProfile>()

    const targetId = searchParams.get("id") || null;

    useEffect(() => {
        if(targetId == null) return;
        
        profilesApi.getDoctorById(targetId)
            .then(result =>{
                if( result.type === "ok") setDoctor(result.value);
            })
    }, []);
    
    if(targetId == null)
    {
        return <div>Not found</div>
    }
    
    if(doctor == null)
    {
        return <div>Loading</div>
    }

    const formattedDate = doctor.dateOfBirth
        ? new Date(doctor.dateOfBirth).toLocaleDateString('ru-RU')
        : '—';

    const currentYear = new Date().getFullYear();
    const experience =
        doctor.careerStartYear > 0 && doctor.careerStartYear <= currentYear
            ? currentYear - doctor.careerStartYear
            : 0;

    const fullName = [doctor.accountLastName, doctor.accountFirstName, doctor.accountMiddleName]
        .filter(Boolean)
        .join(' ');
    
    const bookAppointmentCommand = (serviceId: number) =>{
        navigateToAppointment({specId: doctor.specializationId, doctorId: doctor.accountId, serviceId: serviceId, officeId: doctor.officeId});
    };

    return (
        <div className="doctor-card">
            <AvatarFromSource PhotoUrl={doctor.photoUrl} TextIfPhotoNull={fullName[0] ?? "?"}/>

            <div className="doctor-info">
                <div className="doctor-name">
                    <strong>{fullName}</strong>
                    <span>{doctor.specializationName}</span>
                </div>

                <div className="doctor-details">
                    <span>Exp: {experience > 0 ? `${experience} years` : 'Newbie'}</span>
                    <span>Birth: {formattedDate}</span>
                </div>
                
                <OfficeCompactCard officeId={doctor.officeId}/>
            </div>

            <div className="services-container">
                <span>{`Services by ${doctor.specializationName}`}</span>
                <ServicesBySpecializationId specializationId={doctor.specializationId} onBookAppointment={bookAppointmentCommand}/>
            </div>
        </div>
    );
}

interface ServicesBySpecializationIdProps{
    specializationId: number;
    onBookAppointment?: (serviceId:number) => void;
}
export function ServicesBySpecializationId({specializationId, onBookAppointment = () => {}}: ServicesBySpecializationIdProps){
    const [services, setServices] = useState<Array<ServiceDto>>();

    useEffect(() => {
        const loadData = async () =>{
            try {
                const result = await servicesApi.getServices(undefined, specializationId);
                if (result.type === "ok") {
                    setServices(result.value.services);
                }
                else{
                    setServices([]);
                }
            } catch (err) {
                console.log(err)
            }
        }
        loadData();
    }, [specializationId]);

    if(!services)
    {
        return <></>
    }

    return (
        <div className="services-list">
            {services.map((service) => (
                <div key={service.id}>
                    <button onClick={_=>onBookAppointment?.(service.id)}>
                        Book
                    </button>
                    <ServiceView service={service}/>
                </div>
            ))}
        </div>
    );
}