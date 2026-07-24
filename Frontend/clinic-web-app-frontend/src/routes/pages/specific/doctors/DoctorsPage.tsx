import "./DoctorsPage.css";
import {useEffect, useState} from "react";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";
import {PageSelector} from "../../Shared/PageSelector.tsx";
import {AvatarFromSource} from "../../Shared/Avatar.tsx";
import {OfficeInputFilter, SpecializationInputFilter} from "../../Shared/Inputs/OfficeInputFilter.tsx";
import {useSearchParams} from "react-router";
import {Link} from "react-router-dom";
import {OfficeAddress} from "../offices/OfficeCompactCard.tsx";


const pageSize: number = 50;

export function DoctorsPage() {
    const [searchParams, setSearchParams] = useSearchParams();

    const currentPage = Number(searchParams.get("page")) || 1;
    const urlFullName = searchParams.get("fullName") || "";
    const urlOfficeId = searchParams.get("officeId") || "";
    const urlSpecId = Number(searchParams.get("specId")) || null;
    
    const [fullName, setFullName] = useState(urlFullName)

    const [office, setOffice] = useState<string | null>(urlOfficeId)
    const [specialization, setSpecialization] = useState<number | null>(urlSpecId);

    const [doctors, setDoctors] = useState<any>(null);
    const [total, setTotal] = useState<number>(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const updateUrlParams = (newParams: Record<string, string | null | undefined>) => {
        const nextParams = new URLSearchParams(searchParams);

        Object.entries(newParams).forEach(([key, val]) => {
            if (val) {
                nextParams.set(key, val);
            } else {
                nextParams.delete(key);
            }
        });
        
        if (!("page" in newParams)) {
            nextParams.set("page", "1");
        }

        setSearchParams(nextParams);
    };

    useEffect(() => {
        const loadData = async () => {
            setLoading(true);
            setError(null);

            try {
                const result = await profilesApi.getDoctors({
                    page: currentPage,
                    pageSize,
                    officeIds: urlOfficeId ? [urlOfficeId] : undefined,
                    specializationIds: urlSpecId ? [Number(urlSpecId)] : undefined,
                    fullName: urlFullName || undefined
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

        loadData();
    }, [searchParams]);
    
    if (loading) {
        return <div style={{ textAlign: 'center', padding: '40px' }}>Loading doctors...</div>;
    }

    if (error) {
        return <div style={{ textAlign: 'center', padding: '40px', color: 'red' }}>{error}</div>;
    }

    const listItems = doctors.map(doctor =>
        <Link key={doctor.accountId} to={`/doctor?id=${doctor.accountId}`} style={{ color: 'inherit', textDecoration: 'none' }}>
            <DoctorViewCard key={doctor.accountId} doctor={doctor}/>
        </Link>
    );
    
    return (
        <div style={{ display: 'flex', flexDirection: 'column', flex: 1, overflow: 'hidden' }}>
            <div className="filter-container">
                <div className="filter-block">
                    <form onSubmit={(e) => {
                        e.preventDefault();
                        updateUrlParams({ fullName: fullName });
                    }}>
                        <label>Full name</label>
                        <input
                            type="text"
                            value={fullName}
                            onChange={e => setFullName(e.target.value)}
                        />
                        <button
                            type="submit"
                            disabled={urlFullName === fullName}
                        >
                            Apply
                        </button>
                    </form>
                </div>
                <div className="filter-block">
                    <label>Office</label>
                    <OfficeInputFilter valueId={office} onChange={x => {
                        setOffice(x?.id ?? null);
                        updateUrlParams({ officeId: x?.id });
                    }}/>
                </div>
                <div className="filter-block">
                    <label>Specialization</label>
                    <SpecializationInputFilter valueId={specialization} onChange={x => {
                        setSpecialization(Number(x?.id) ?? null);
                        updateUrlParams({ specId: String(x?.id) });
                    }}/>

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
                onPageChange={(page) => updateUrlParams({ page: String(page) })}
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
                    <OfficeAddress officeId={doctor.officeId}/>
                    <span>Exp: {experience > 0 ? `${experience} years` : 'Newbie'}</span>
                </div>
            </div>
        </div>
    );
}