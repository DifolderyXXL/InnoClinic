import "./DoctorsPage.css";
import {useEffect, useState} from "react";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";
import {PageSelector} from "../../Shared/PageSelector.tsx";
import {AvatarFromSource} from "../../Shared/Avatar.tsx";
import {type OfficeDto} from "../../../../services/api/OfficesApi.ts";
import {OfficeInputFilter, SpecializationInputFilter} from "../../Shared/Inputs/OfficeInputFilter.tsx";
import type {SpecializationDto} from "../../../../services/api/ServicesApi.ts";


const pageSize: number = 50;

export function DoctorsPage() {
    const [fullName, setFullName] = useState("")
    const [queriedFullName, setQueriedFullName] = useState("")
    
    const [office, setOffice] = useState<OfficeDto | null>(null)
    const [specialization, setSpecialization] = useState<SpecializationDto | null>(null);

    const [doctors, setDoctors] = useState<any>(null);
    const [total, setTotal] = useState<number>(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const loadData = async (page: number) => {
        setLoading(true);
        setError(null);

        try {
            setQueriedFullName(fullName);
            const offices = office ? [office.id] : undefined;
            const specializations = specialization ? [Number(specialization.id)] : undefined;
            const result = await profilesApi.getDoctors({
                page, 
                pageSize, 
                officeIds: offices,
                specializationIds: specializations,
                fullName: fullName
            });
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
    }, [office, specialization]);
    
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
            <div className="filter-container">
                <div className="filter-block">
                    <form onSubmit={(e) => {
                        e.preventDefault();
                        loadData(1);
                    }}>
                        <label>Full name</label>
                        <input
                            type="text"
                            value={fullName}
                            onChange={e => setFullName(e.target.value)}
                        />
                        <button
                            type="submit"
                            disabled={queriedFullName === fullName}
                        >
                            Apply
                        </button>
                    </form>
                </div>
                <div className="filter-block">
                    <label>Office</label>
                    <OfficeInputFilter value={office} onChange={setOffice}/>
                </div>
                <div className="filter-block">
                    <label>Specialization</label>
                    <SpecializationInputFilter value={specialization} onChange={setSpecialization}/>

                </div>
            </div>


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
    officeId: string;
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