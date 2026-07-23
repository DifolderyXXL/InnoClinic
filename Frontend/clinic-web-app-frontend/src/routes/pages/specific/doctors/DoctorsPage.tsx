import "./DoctorsPage.css";
import {useEffect, useState} from "react";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";
import {PageSelector} from "../../Shared/PageSelector.tsx";
import {AvatarFromSource} from "../../Shared/Avatar.tsx";


const pageSize: number = 50;

export function DoctorsPage() {
    const [doctors, setDoctors] = useState<any>(null);
    const [total, setTotal] = useState<number>(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const loadData = async (page: number) => {
        setLoading(true);
        setError(null);

        try {
            const result = await profilesApi.getDoctors(page, pageSize);
            if (result.type === "ok") {
                setDoctors(result.value.items);
                setTotal(result.value.total);
            } else {
                setError(result.error?.title || "Error");
            }
        } catch (err) {
            setError("Unhandled error");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData(1);
    }, []);
    
    if (loading) {
        return <div style={{ textAlign: 'center', padding: '40px' }}>Loading doctors...</div>;
    }

    if (error) {
        return <div style={{ textAlign: 'center', padding: '40px', color: 'red' }}>{error}</div>;
    }

    const listItems = doctors.map(doctor =>
        <DoctorViewCard key={doctor.accountId} doctor={doctor}/>
    );
    
    return (
        <div style={{ display: 'flex', flexDirection: 'column', flex: 1, overflow: 'hidden' }}>
            <div style={{  flex: 1,
                overflowY: 'auto',
                display: 'flex',
                flexWrap: 'wrap',
                gap: '10px',
                justifyContent: 'flex-start',
                alignContent: 'flex-start',
                padding: '10px'  }}>
                {listItems}
            </div>
            <PageSelector
                pageSize={pageSize}
                total={total}
                onPageChange={(page) => loadData(page)}
            />
        </div>
    );
}

export interface DoctorProfile {
    accountId: string;
    accountFirstName: string;
    accountLastName: string;
    accountMiddleName?: string | null;
    accountPhotoId?: string | null;
    photoUrl?: string | null;   
    dateOfBirth: string; 
    specializationId: number;
    specializationSpecializationName: string;
    officeId: number;
    careerStartYear: number;
}

interface DoctorViewCardProps{
    doctor: DoctorProfile
}
export function DoctorViewCard({doctor}: DoctorViewCardProps){
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
                    <span>Office: {doctor.officeId}</span>
                    <span>Exp: {experience > 0 ? `${experience} years` : 'Newbie'}</span>
                    <span>Birth: {formattedDate}</span>
                </div>
            </div>
        </div>
    );
}