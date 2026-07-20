import {profilesApi} from "../../../services/api/ProfilesApi.ts";
import {useEffect, useState} from "react";

export function ProfilePage() {
    const [profile, setProfile] = useState<any>(null);

    useEffect(() => {
        async function loadData() {
            const result = await profilesApi.getMyProfiles();
            if (result.type === "ok") {
                setProfile(result.value);
            }
        }
        loadData();
    }, []);
    
    return (
        <div style={{display: "flex", flexDirection: "column", justifyContent: "center", alignItems: "center", height: "100vh"}}>
            {profile && profile.account && <AccountCard {...profile.account} />}
            {profile && (
                profile.patient ? <PatientCard {...profile.patient} /> : <PatientCreateCard/> 
            )}
            {profile && profile.doctor && <DoctorCard {...profile.doctor} />}
        </div>
    );
}

interface AccountCardProps {
    email: string;
    phoneNumber: string;
    firstName: string;
    lastName: string;
    middleName?: string;
}

export function AccountCard({ email, phoneNumber, firstName, lastName, middleName }: AccountCardProps) {
    return (
        <div style={{ display: "flex", flexDirection: "row", gap: "16px", padding: "20px", background: "#222", color: "#fff", borderRadius: "8px" }}>
            <p><strong>Name:</strong> {lastName} {firstName} {middleName}</p>
            <p><strong>Email:</strong> {email}</p>
            <p><strong>Phone:</strong> {phoneNumber}</p>
        </div>
    );
}

interface PatientCardProps {
    dateOfBirth: string;
}

export function PatientCard({ dateOfBirth }: PatientCardProps) {
    return (
        <div style={{ display: "flex", flexDirection: "row", gap: "16px", padding: "20px", background: "#222", color: "#fff", borderRadius: "8px" }}>
            <p><strong>Birthday:</strong> {dateOfBirth}</p>
        </div>
    );
}

export function PatientCreateCard(){
    const [isCreating, setIsCreating] = useState(false);
    const [dobInput, setDobInput] = useState("");
    const [error, setError] = useState<any>();

    const handleCreatePatient = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setError(null);

        if (!dobInput) {
            setError("Please select a valid date of birth.");
            return;
        }
        
        const result = await profilesApi.createMyProfiles(dobInput);

        if (result.type === "error") {
            setError(result.error?.title || "Failed to create patient profile.");
        } else {
            setIsCreating(false);
        }
    };
    
    return (
        <div className="patient-create-card">
            {error && <div className="card-error">{error}</div>}

            {isCreating ? (
                <form onSubmit={handleCreatePatient} className="card-form">
                    <label>Select Date of Birth:</label>
                    <input
                        type="date"
                        value={dobInput}
                        onChange={(e) => setDobInput(e.target.value)}
                        className="card-input-date"
                    />
                    <div className="card-btn-group">
                        <button type="submit" className="card-btn save">
                            Save
                        </button>
                        <button type="button" onClick={() => setIsCreating(false)} className="card-btn cancel">
                            Cancel
                        </button>
                    </div>
                </form>
            ) : (
                <>
                    <p className="card-status-text">No patient profile found.</p>
                    <button
                        type="button"
                        onClick={() => setIsCreating(true)}
                        className="card-btn create"
                    >
                        + Create Patient Profile
                    </button>
                </>
            )}
        </div>
    );
}

interface DoctorCardProps {
    dateOfBirth: string;
    officeId: number;
    careerStartYear: number;
    specializationName: string;
}

export function DoctorCard({ dateOfBirth, officeId, careerStartYear, specializationName }: DoctorCardProps) {
    const formattedDate = dateOfBirth ? new Date(dateOfBirth).toLocaleDateString() : "—";
    
    const currentYear = 2026;
    const experience = careerStartYear > 0 && careerStartYear <= currentYear
        ? currentYear - careerStartYear
        : 0;
    
    return (
        <div style={{ display: "flex", flexDirection: "row", gap: "16px", padding: "20px", background: "#222", color: "#fff", borderRadius: "8px", alignItems: "center" }}>
            <div>
                <strong>Specialization:</strong> <span style={{ color: "#889a7e" }}>{specializationName}</span>
            </div>
            <div style={{ width: "1px", height: "20px", backgroundColor: "#444" }} />
            <div>
                <strong>Office ID:</strong> {officeId}
            </div>
            <div style={{ width: "1px", height: "20px", backgroundColor: "#444" }} />
            <div>
                <strong>Experience:</strong> {experience > 0 ? `${experience} yrs` : "Career Start"}
            </div>
            <div style={{ width: "1px", height: "20px", backgroundColor: "#444" }} />
            <div>
                <strong>Birthday:</strong> {formattedDate}
            </div>
        </div>
    );
}