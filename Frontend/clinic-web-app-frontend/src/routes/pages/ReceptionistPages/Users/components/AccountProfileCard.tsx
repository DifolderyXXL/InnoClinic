import { useState } from "react";
import { AvatarFromSource } from "../../../Shared/Avatar";
import type { AccountDto } from "../AccountsPage";

export const AlertMessage = ({ message, type }: { message: string; type: "success" | "error" }) => (
    <div className={`status-message ${type}`}>{message}</div>
);

interface AccountProfileCardProps {
    account: AccountDto;
    onUpdate: (data: Partial<AccountDto>) => Promise<{ success: boolean; message: string }>;
    onDelete: () => Promise<{ success: boolean; message: string }>;
}

export function AccountProfileCard({ account, onUpdate, onDelete }: AccountProfileCardProps) {
    const [isEditing, setIsEditing] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);
    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [actionMessage, setActionMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

    const [form, setForm] = useState({
        firstName: account.firstName || "",
        lastName: account.lastName || "",
        middleName: account.middleName || "",
        phoneNumber: account.phoneNumber || "",
    });

    const fullName = [account.lastName, account.firstName, account.middleName].filter(Boolean).join(" ") || "No Name";
    const initial = (account.firstName?.[0] || account.email?.[0] || "U").toUpperCase();

    const handleSubmit = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        setActionMessage(null);

        const result = await onUpdate({
            firstName: form.firstName.trim() || null,
            lastName: form.lastName.trim() || null,
            middleName: form.middleName.trim() || null,
            phoneNumber: form.phoneNumber.trim() || null,
        });

        setActionMessage({ type: result.success ? "success" : "error", text: result.message });
        if (result.success) setIsEditing(false);
        setIsSubmitting(false);
    };

    const handleDelete = async () => {
        setIsDeleting(true);
        setActionMessage(null);

        const result = await onDelete();

        if (!result.success) {
            setActionMessage({ type: "error", text: result.message });
            setShowDeleteModal(false);
            setIsDeleting(false);
        }
    };

    return (
        <div className="account-details-card">
            {actionMessage && <AlertMessage message={actionMessage.text} type={actionMessage.type} />}

            <header className="details-header">
                <div className="header-user-info">
                    <AvatarFromSource PhotoUrl={account.photoUrl} TextIfPhotoNull={initial} />
                    <div>
                        <h2>{fullName}</h2>
                        <span className="user-email">{account.email}</span>
                    </div>
                </div>
                <div className="header-actions">
                    {!isEditing && (
                        <button className="edit-btn" onClick={() => setIsEditing(true)}>Edit Profile</button>
                    )}
                    <button className="btn btn-decline" onClick={() => setShowDeleteModal(true)}>
                        Delete User
                    </button>
                </div>
            </header>

            {isEditing ? (
                <form className="details-form" onSubmit={handleSubmit}>
                    <div className="form-grid">
                        <div className="form-group">
                            <label>Last Name</label>
                            <input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} disabled={isSubmitting} />
                        </div>
                        <div className="form-group">
                            <label>First Name</label>
                            <input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} disabled={isSubmitting} />
                        </div>
                        <div className="form-group">
                            <label>Middle Name</label>
                            <input value={form.middleName} onChange={(e) => setForm({ ...form, middleName: e.target.value })} disabled={isSubmitting} />
                        </div>
                        <div className="form-group">
                            <label>Phone Number</label>
                            <input value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} disabled={isSubmitting} />
                        </div>
                    </div>
                    <div className="form-actions">
                        <button type="submit" className="submit-btn" disabled={isSubmitting}>
                            {isSubmitting ? "Saving..." : "Save Changes"}
                        </button>
                        <button type="button" className="cancel-btn" onClick={() => setIsEditing(false)} disabled={isSubmitting}>
                            Cancel
                        </button>
                    </div>
                </form>
            ) : (
                <div className="details-body">
                    <div className="info-grid">
                        <div className="info-item"><span className="label">User ID</span><span className="value code">{account.id}</span></div>
                        <div className="info-item"><span className="label">Phone</span><span className="value">{account.phoneNumber || "—"}</span></div>
                        <div className="info-item">
                            <span className="label">Email Verification</span>
                            <span className={`status-badge ${account.isEmailVerified ? "verified" : "unverified"}`}>
                                {account.isEmailVerified ? "Verified" : "Unverified"}
                            </span>
                        </div>
                        <div className="info-item"><span className="label">Created At</span><span className="value">{new Date(account.createdAt).toLocaleString()}</span></div>
                    </div>
                </div>
            )}

            {showDeleteModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <p className="modal-title">
                            Are you sure you want to permanently delete this user profile?
                        </p>
                        <p style={{ fontSize: "14px", color: "#a0a0b0", marginTop: "8px" }}>
                            This action cannot be undone. All associated data will be removed.
                        </p>

                        <div className="modal-actions" style={{ marginTop: "20px" }}>
                            <button
                                className="btn btn-secondary"
                                onClick={() => setShowDeleteModal(false)}
                                disabled={isDeleting}
                            >
                                Cancel
                            </button>
                            <button
                                className="btn btn-decline"
                                onClick={handleDelete}
                                disabled={isDeleting}
                            >
                                {isDeleting ? "Deleting..." : "Delete Permanently"}
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}