import {useSearchParams} from "react-router";
import {useEffect, useState} from "react";
import type {DoctorProfile} from "./DoctorsPage.tsx";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";
import {AvatarFromSource} from "../../Shared/Avatar.tsx";
import {OfficeCompactCard} from "../offices/OfficeCompactCard.tsx";
import "./DoctorsPage.css";

export function DoctorPage(){
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

    return (
        <div className="doctor-card">
            <AvatarFromSource PhotoUrl={doctor.photoUrl} TextIfPhotoNull={fullName[0] ?? "?"}/>

            <div className="doctor-info">
                <div className="doctor-name">
                    <strong>{fullName}</strong>
                    <span>{doctor.specializationSpecializationName}</span>
                </div>

                <div className="doctor-details">
                    <span>Exp: {experience > 0 ? `${experience} years` : 'Newbie'}</span>
                    <span>Birth: {formattedDate}</span>
                </div>
                
                <OfficeCompactCard officeId={doctor.officeId}/>
            </div>
        </div>
    );
}