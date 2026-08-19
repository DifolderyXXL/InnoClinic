import { useSearchParams, useNavigate } from "react-router";
import { AccountProfileCard } from "./components/AccountProfileCard";
import { RoleManagementCard } from "./components/RoleManagementCard";
import { ReceptionistCreateForm, ReceptionistCard } from "./components/ReceptionistProfile";
import { useAccountDetails, useUserProfiles } from "./hooks/useAccountDetails";
import { useAccountRoles } from "./hooks/useAccountDetails";
import {PatientProfileForm} from "./components/PatientProfile.tsx";
import {DoctorCard, DoctorProfileForm} from "./components/DoctorProfile.tsx";
import {PatientCard} from "../../Identity/PatientCard.tsx";

import "./AccountDetailsPage.css"
import {Link} from "react-router-dom";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";

export function AccountDetailsPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const accountId = searchParams.get("id");

    const { account, loading: accLoading, error, updateAccount } = useAccountDetails(accountId);
    const { roles, loading: rolesLoading } = useAccountRoles(accountId);
    const { profiles, loading: profLoading, fetchProfiles } = useUserProfiles(accountId);

    if (accLoading || rolesLoading || profLoading) return <div className="status-message">Loading account details...</div>;
    if (error || !account) return <div className="status-message error">{error || "Account not found"}</div>;

    return (
        <div className="account-details-page">
            <button className="back-btn" onClick={() => navigate(-1)}>
                &larr; Back to Accounts
            </button>

            <AccountProfileCard
                account={account}
                onUpdate={updateAccount}
                onDelete={async () => {
                    const res = await profilesApi.deleteAccount(account.id);
                    if (res.type === "ok") {
                        navigate("/accounts");
                        return { success: true, message: "Account deleted successfully" };
                    }
                    return {
                        success: false,
                        message: res.error?.title || "Failed to delete account"
                    };
                }}
            />
            <RoleManagementCard accountId={account.id} />

            <div className="profiles-container">
                {/* PATIENT */}
                {roles.includes("client") && (
                    profiles?.onlyPatient ? (
                        <div>
                            <PatientCard
                                dateOfBirth={profiles.onlyPatient.dateOfBirth}
                                onUpdateSuccess={fetchProfiles}
                            />
                            <Link to={`/make-appointment?userId=${account.id}`} className="btn-appointment">
                                Make appointment
                            </Link>
                        </div>

                    ) : (
                        <PatientProfileForm accountId={account.id} onSuccess={fetchProfiles} />
                    )
                )}

                {/* DOCTOR */}
                {roles.includes("doctor") && (
                    profiles?.onlyDoctor ? (
                        <DoctorCard
                            accountId={account.id}
                            initialData={profiles.onlyDoctor}
                            onUpdateSuccess={fetchProfiles}
                        />
                    ) : (
                        <DoctorProfileForm accountId={account.id} onSuccess={fetchProfiles} />
                    )
                )}

                {/* RECEPTIONIST */}
                {roles.includes("receptionist") && (
                    profiles?.onlyReceptionist ? (
                        <ReceptionistCard
                            accountId={account.id}
                            initialOfficeId={profiles.onlyReceptionist.officeId}
                            onUpdateSuccess={fetchProfiles}
                        />
                    ) : (
                        <ReceptionistCreateForm accountId={account.id} onSuccess={fetchProfiles} />
                    )
                )}
            </div>
        </div>
    );
}