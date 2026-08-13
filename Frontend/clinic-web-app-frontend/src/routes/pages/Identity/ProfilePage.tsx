import { useEffect, useState } from "react";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";
import { PatientCard, PatientCreateCard } from "./PatientCard.tsx";
import { AccountCard, AccountCreateCard } from "./AccountCard.tsx";
import { DoctorCard } from "./DoctorCard.tsx";
import "./ProfilePage.css";
import {OfficeAddress} from "../specific/offices/OfficeCompactCard.tsx";
import {RequireRole, Roles} from "../../../components/common/RequireRole.tsx";

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
                setError(result.error?.title || "Failed to load profile");
            }
        } catch {
            setError("An unexpected error occurred while loading profile");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    if (loading) {
        return <div className="status-message">Loading profile...</div>;
    }

    return (
        <div className="profile-page-container">
            {error && <div className="status-message error">{error}</div>}

            <section className="profile-section">
                {profile && profile.account ? (
                    <AccountCard {...profile.account} />
                ) : (
                    <AccountCreateCard onSuccess={loadData} onCreate={(baseData) => profilesApi.createAccountMe(baseData)}/>
                )}
            </section>

            <RequireRole roles={[Roles.Patient]}>
                {profile && (
                    <section className="profile-section">
                        {profile.onlyPatient ? (
                            <PatientCard {...profile.onlyPatient} />
                        ) : (
                            <PatientCreateCard onSubmit={dob => profilesApi.createPatientMe(dob)} onSuccess={loadData} />
                        )}
                    </section>
                )}
            </RequireRole>

            {profile && profile.onlyDoctor && (
                <section className="profile-section">
                    <DoctorCard {...profile.onlyDoctor} />
                </section>
            )}
            
            <RequireRole roles={[Roles.Receptionist]}>
                {profile && profile.onlyReceptionist && (
                    <ReceptionistProfilePage officeId={profile.onlyReceptionist?.officeId}/>
                )}
            </RequireRole>
        </div>
    );
}

interface ReceptionistProfilePageProps {
    officeId: string | null | undefined;
}

export function ReceptionistProfilePage({ officeId }: ReceptionistProfilePageProps) {
    return (
        <div className="patient-create-card">
            <header className="card-header">
                <h3>Receptionist Profile</h3>
            </header>
            <div className="profile-section">
                <div className="info-item">
                    <span className="label">Office</span>
                    <OfficeAddress officeId={officeId} />
                </div>
            </div>
        </div>
    );
}