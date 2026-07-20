import {profilesApi} from "../../../services/api/ProfilesApi.ts";
import {useEffect, useState} from "react";
import {PatientCard, PatientCreateCard} from "./PatientCard.tsx";
import {AccountCard, AccountCreateCard} from "./AccountCard.tsx";
import {DoctorCard} from "./DoctorCard.tsx";

export function ProfilePage() {
    const [profile, setProfile] = useState<any>(null);

    const loadData = async () => {
        const result = await profilesApi.getMyProfiles();
        if (result.type === "ok") {
            setProfile(result.value);
        }
    };

    useEffect(() => {
        loadData();
    }, []);
    
    return (
        <div style={{display: "flex", flexDirection: "column", justifyContent: "center", alignItems: "center", height: "100vh"}}>
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