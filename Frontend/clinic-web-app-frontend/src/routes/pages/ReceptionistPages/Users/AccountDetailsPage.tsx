import React, { useEffect, useState } from "react";
import { useSearchParams, useNavigate } from "react-router";
import { profilesApi } from "../../../../services/api/ProfilesApi.ts";
import { AvatarFromSource } from "../../Shared/Avatar.tsx";
import type { AccountDto } from "./AccountsPage.tsx";
import "./AccountDetailsPage.css";

export function AccountDetailsPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const accountId = searchParams.get("id");

    const [account, setAccount] = useState<AccountDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Editing State
    const [isEditing, setIsEditing] = useState(false);
    const [form, setForm] = useState({
        firstName: "",
        lastName: "",
        middleName: "",
        phoneNumber: "",
    });
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [actionMessage, setActionMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

    // Role state
    const [selectedRole, setSelectedRole] = useState("Patient");
    const [isUpdatingRole, setIsUpdatingRole] = useState(false);

    const loadAccount = async () => {
        if (!accountId) {
            setError("Account ID is missing");
            setLoading(false);
            return;
        }

        setLoading(true);
        try {
            const result = await profilesApi.getAccount(accountId);
            if (result.type === "ok") {
                setAccount(result.value);
                setForm({
                    firstName: result.value.firstName || "",
                    lastName: result.value.lastName || "",
                    middleName: result.value.middleName || "",
                    phoneNumber: result.value.phoneNumber || "",
                });
            } else {
                setError(result.error?.title || "Failed to load account");
            }
        } catch {
            setError("An unexpected error occurred");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadAccount();
    }, [accountId]);

    const handleSaveAccount = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        if (!account) return;

        setIsSubmitting(true);
        setActionMessage(null);

        try {
            const result = await profilesApi.updateAccount(accountId!, {
                firstName: form.firstName.trim() || null,
                lastName: form.lastName.trim() || null,
                middleName: form.middleName.trim() || null,
                phoneNumber: form.phoneNumber.trim() || null,
            });

            if (result.type === "ok") {
                setActionMessage({ type: "success", text: "Account details updated successfully!" });
                setIsEditing(false);
                loadAccount();
            } else {
                setActionMessage({ type: "error", text: result.error?.title || "Failed to update account" });
            }
        } catch {
            setActionMessage({ type: "error", text: "An unexpected error occurred" });
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleRoleChange = async () => {
        if (!accountId) return;

        setIsUpdatingRole(true);
        setActionMessage(null);

        try {
            const result = await profilesApi.updateRole(accountId, selectedRole);

            if (result.type === "ok") {
                setActionMessage({ type: "success", text: `Role successfully updated to ${selectedRole}!` });
            } else {
                setActionMessage({ type: "error", text: result.error?.title || "Failed to update role" });
            }
        } catch {
            setActionMessage({ type: "error", text: "An unexpected error occurred while changing role" });
        } finally {
            setIsUpdatingRole(false);
        }
    };

    if (loading) return <div className="status-message">Loading account details...</div>;
    if (error || !account) return <div className="status-message error">{error || "Account not found"}</div>;

    const fullName = [account.lastName, account.firstName, account.middleName].filter(Boolean).join(" ") || "No Name";
    const initial = (account.firstName?.[0] || account.email?.[0] || "U").toUpperCase();

    return (
        <div className="account-details-page">
            <button className="back-btn" onClick={() => navigate(-1)}>
                &larr; Back to Accounts
            </button>

            {actionMessage && (
                <div className={`status-message ${actionMessage.type}`}>
                    {actionMessage.text}
                </div>
            )}

            <div className="account-details-card">
                <header className="details-header">
                    <div className="header-user-info">
                        <AvatarFromSource PhotoUrl={account.photoUrl} TextIfPhotoNull={initial} />
                        <div>
                            <h2>{fullName}</h2>
                            <span className="user-email">{account.email}</span>
                        </div>
                    </div>

                    {!isEditing && (
                        <button className="edit-btn" onClick={() => setIsEditing(true)}>
                            Edit Profile
                        </button>
                    )}
                </header>

                {isEditing ? (
                    <form className="details-form" onSubmit={handleSaveAccount}>
                        <div className="form-grid">
                            <div className="form-group">
                                <label>Last Name</label>
                                <input
                                    type="text"
                                    value={form.lastName}
                                    onChange={(e) => setForm({ ...form, lastName: e.target.value })}
                                    disabled={isSubmitting}
                                />
                            </div>

                            <div className="form-group">
                                <label>First Name</label>
                                <input
                                    type="text"
                                    value={form.firstName}
                                    onChange={(e) => setForm({ ...form, firstName: e.target.value })}
                                    disabled={isSubmitting}
                                />
                            </div>

                            <div className="form-group">
                                <label>Middle Name</label>
                                <input
                                    type="text"
                                    value={form.middleName}
                                    onChange={(e) => setForm({ ...form, middleName: e.target.value })}
                                    disabled={isSubmitting}
                                />
                            </div>

                            <div className="form-group">
                                <label>Phone Number</label>
                                <input
                                    type="text"
                                    value={form.phoneNumber}
                                    onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })}
                                    disabled={isSubmitting}
                                />
                            </div>
                        </div>

                        <div className="form-actions">
                            <button type="submit" className="submit-btn" disabled={isSubmitting}>
                                {isSubmitting ? "Saving..." : "Save Changes"}
                            </button>
                            <button
                                type="button"
                                className="cancel-btn"
                                onClick={() => setIsEditing(false)}
                                disabled={isSubmitting}
                            >
                                Cancel
                            </button>
                        </div>
                    </form>
                ) : (
                    <div className="details-body">
                        <div className="info-grid">
                            <div className="info-item">
                                <span className="label">User ID</span>
                                <span className="value code">{account.id}</span>
                            </div>

                            <div className="info-item">
                                <span className="label">Phone</span>
                                <span className="value">{account.phoneNumber || "—"}</span>
                            </div>

                            <div className="info-item">
                                <span className="label">Email Verification</span>
                                <span className={`status-badge ${account.isEmailVerified ? "verified" : "unverified"}`}>
                                    {account.isEmailVerified ? "Verified" : "Unverified"}
                                </span>
                            </div>

                            <div className="info-item">
                                <span className="label">Created At</span>
                                <span className="value">{new Date(account.createdAt).toLocaleString()}</span>
                            </div>
                        </div>
                    </div>
                )}
            </div>

            {/* Role Management Panel */}
            <div className="account-role-card">
                <h3>Manage User Role</h3>
                <div className="role-selector-row">
                    <select
                        value={selectedRole}
                        onChange={(e) => setSelectedRole(e.target.value)}
                        disabled={isUpdatingRole}
                    >
                        <option value="Patient">Patient</option>
                        <option value="Doctor">Doctor</option>
                        <option value="Receptionist">Receptionist</option>
                    </select>

                    <button
                        type="button"
                        className="submit-btn"
                        onClick={handleRoleChange}
                        disabled={isUpdatingRole}
                    >
                        {isUpdatingRole ? "Updating..." : "Assign Role"}
                    </button>
                </div>
            </div>
        </div>
    );
}