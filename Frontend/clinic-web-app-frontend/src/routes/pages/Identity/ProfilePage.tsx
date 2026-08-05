import { useEffect, useState } from "react";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";
import { PatientCard, PatientCreateCard } from "./PatientCard.tsx";
import { AccountCard, AccountCreateCard } from "./AccountCard.tsx";
import { DoctorCard } from "./DoctorCard.tsx";
import "./ProfilePage.css";

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
                    <AccountCreateCard onSuccess={loadData} />
                )}
            </section>

            {profile && (
                <section className="profile-section">
                    {profile.onlyPatient ? (
                        <PatientCard {...profile.onlyPatient} />
                    ) : (
                        <PatientCreateCard onSuccess={loadData} />
                    )}
                </section>
            )}

            {profile && profile.onlyDoctor && (
                <section className="profile-section">
                    <DoctorCard {...profile.onlyDoctor} />
                </section>
            )}
        </div>
    );
}