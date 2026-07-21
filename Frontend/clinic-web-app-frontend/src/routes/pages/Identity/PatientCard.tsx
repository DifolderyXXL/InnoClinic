import {useState} from "react";
import {profilesApi} from "../../../services/api/ProfilesApi.ts";

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

interface PatientCreateCardProps {
    onSuccess?: () => void;
}
export function PatientCreateCard({ onSuccess }: PatientCreateCardProps){
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

        const result = await profilesApi.createPatientMe(dobInput);

        if (result.type === "error") {
            setError(result.error?.title || "Failed to create patient profile.");
        } else {
            setIsCreating(false);

            if (onSuccess) onSuccess();
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