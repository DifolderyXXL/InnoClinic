import {profilesApi} from "../../../services/api/ProfilesApi.ts";
import {useEffect, useState} from "react";
import {PatientCard, PatientCreateCard} from "./PatientCard.tsx";
import {AccountCard, AccountCreateCard} from "./AccountCard.tsx";
import {DoctorCard} from "./DoctorCard.tsx";

export function ProfilePage() {
    const [profile, setProfile] = useState<any>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const loadData = async () => {
        setLoading(true);
        setError(null);

        try {
            const result = await profilesApi.getMyProfiles();
            if (result.type === "ok") {
                setProfile(result.value);
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
        loadData();
    }, []);

    if (loading) {
        return <div style={{ textAlign: 'center', padding: '40px' }}>Загрузка профиля...</div>;
    }

    if (error) {
        return <div style={{ textAlign: 'center', padding: '40px', color: 'red' }}>{error}</div>;
    }
    
    return (
        <div style={{display: "flex", flexDirection: "column", justifyContent: "center", alignItems: "center"}}>
            {
                profile && profile.account ? <AccountCard {...profile.account} /> : <AccountCreateCard onSuccess={loadData}/>
            }
            {profile && (
                profile.patient ? <PatientCard {...profile.patient} /> : <PatientCreateCard onSuccess={loadData}/> 
            )}
            {profile && profile.doctor && <DoctorCard {...profile.doctor} />}
        </div>
    );
}