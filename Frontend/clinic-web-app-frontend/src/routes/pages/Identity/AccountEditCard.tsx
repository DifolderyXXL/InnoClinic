import React, { useState } from "react";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";
import "./AccountCard.css";
import { documentsApi } from "../../../services/api/DocumentsApi.ts";
import {FormField} from "../Shared/FormField.tsx";

interface AccountEditCardProps {
    firstName: string;
    lastName: string;
    middleName?: string | null;
    phoneNumber?: string | null;
    photoUrl?: string | null;
    onCancel: () => void;
    onSuccess: () => void;
}

export function AccountEditCard({
                                    firstName,
                                    lastName,
                                    middleName = "",
                                    phoneNumber,
                                    photoUrl,
                                    onCancel,
                                    onSuccess,
                                }: AccountEditCardProps) {
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);

    const [form, setForm] = useState({
        firstName: firstName || "",
        lastName: lastName || "",
        middleName: middleName || "",
        phoneNumber: phoneNumber || "+",
    });

    const [touched, setTouched] = useState<Record<string, boolean>>({});
    const [previewUrl, setPreviewUrl] = useState<string | null>(photoUrl || null);
    const [photoId, setPhotoId] = useState<string | null>(null);
    const [photoIsUploading, setPhotoIsUploading] = useState<boolean>(false);

    // Dynamic Field Validation
    const getFieldError = (name: string, val: string) => {
        const strVal = val ?? "";
        if ((name === "firstName" || name === "lastName") && !strVal.trim()) {
            return `Please, enter the ${name === "firstName" ? "first name" : "last name"}`;
        }
        if (name === "phoneNumber") {
            const digits = strVal.replace(/^\+/, "");
            if (!digits.trim()) return "Please, enter the phone number";
            if (!/^\d+$/.test(digits)) return "You've entered an invalid phone number";
        }
        return "";
    };

    const errors = {
        firstName: getFieldError("firstName", form.firstName),
        lastName: getFieldError("lastName", form.lastName),
        phoneNumber: getFieldError("phoneNumber", form.phoneNumber),
    };

    const handleChange = (field: string, value: string) => {
        let cleanValue = value ?? "";
        if (field === "phoneNumber" && !cleanValue.startsWith("+")) {
            cleanValue = "+" + cleanValue.replace(/\+/g, "");
        }
        setForm((prev) => ({ ...prev, [field]: cleanValue }));
    };

    const handleBlur = (field: string) => setTouched((prev) => ({ ...prev, [field]: true }));

    const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            setPhotoIsUploading(true);
            setPreviewUrl(URL.createObjectURL(file));
            documentsApi.uploadUserAvatar(file)
                .then(result => {
                    if (result.type === "ok") setPhotoId(result.value.photoId);
                })
                .catch(() => {
                    setApiError("Failed to upload photo. Please try again.");
                })
                .finally(() => setPhotoIsUploading(false));
        }
    };

    const handleSubmit = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setApiError(null);
        setTouched({ firstName: true, lastName: true, phoneNumber: true });

        if (Object.values(errors).some(Boolean) || photoIsUploading) return;

        setIsSubmitting(true);
        try {
            const result = await profilesApi.updateAccountMe({
                firstName: form.firstName.trim() || null,
                lastName: form.lastName.trim() || null,
                middleName: form.middleName.trim() || null,
                phoneNumber: form.phoneNumber.trim() || null,
                photoId: photoId
            });

            if (result?.type === "error") {
                setApiError(result.error?.title || result.error?.message || "Failed to update account.");
            } else {
                onSuccess();
            }
        } catch (err: any) {
            const serverMsg = err?.response?.data?.title || err?.response?.data?.message || err?.message;
            setApiError(serverMsg || "An unexpected error occurred while updating.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="account-create-form" noValidate>
            {apiError && <div className="status-message error">{apiError}</div>}

            <div className="form-group">
                <label htmlFor="editPhoto">Profile Photo (optional)</label>
                <div className="photo-upload-wrapper">
                    <div className="avatar-preview-container">
                        {previewUrl && <img src={previewUrl} alt="Preview" className="account-avatar-preview" />}
                        {photoIsUploading && (
                            <div className="avatar-spinner-overlay">
                                <div className="spinner" />
                            </div>
                        )}
                    </div>
                    <input id="editPhoto" type="file" accept="image/*" className="file-input" onChange={handlePhotoChange} disabled={isSubmitting || photoIsUploading} />
                </div>
            </div>

            <div className="form-grid name-grid">
                <FormField id="editLastName" label="Last Name *" value={form.lastName} error={errors.lastName} isTouched={touched.lastName} disabled={isSubmitting} onChange={(v) => handleChange("lastName", v)} onBlur={() => handleBlur("lastName")} />
                <FormField id="editFirstName" label="First Name *" value={form.firstName} error={errors.firstName} isTouched={touched.firstName} disabled={isSubmitting} onChange={(v) => handleChange("firstName", v)} onBlur={() => handleBlur("firstName")} />
                <FormField id="editMiddleName" label="Middle Name (optional)" value={form.middleName} disabled={isSubmitting} onChange={(v) => handleChange("middleName", v)} />
            </div>

            <FormField id="editPhoneNumber" label="Phone Number *" value={form.phoneNumber} error={errors.phoneNumber} isTouched={touched.phoneNumber} disabled={isSubmitting} onChange={(v) => handleChange("phoneNumber", v)} onBlur={() => handleBlur("phoneNumber")} />

            <div className="form-actions">
                <button type="submit" className="submit-btn" disabled={isSubmitting || photoIsUploading}>
                    {isSubmitting ? "Saving..." : photoIsUploading ? "Uploading photo..." : "Save Changes"}
                </button>
                <button type="button" className="cancel-btn" onClick={onCancel} disabled={isSubmitting}>
                    Cancel
                </button>
            </div>
        </form>
    );
}