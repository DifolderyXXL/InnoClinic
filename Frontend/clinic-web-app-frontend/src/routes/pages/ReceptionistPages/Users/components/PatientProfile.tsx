import { profilesApi } from "../../../../../services/api/ProfilesApi.ts";
import { PatientCreateCard } from "../../../Identity/PatientCard.tsx";

export function PatientProfileForm({ accountId, onSuccess }: { accountId: string; onSuccess: () => void }) {
    return (
        <div className="account-details-card profile-form-card">
            <h3>🩺 Create Patient Profile</h3>
            <PatientCreateCard
                onSubmit={(dob) => profilesApi.createPatient(accountId, dob)}
                onSuccess={onSuccess}
            />
        </div>
    );
}