import React, { useState } from "react";
import { officesApi, type OfficeDto } from "../../../../services/api/OfficesApi.ts";
import { documentsService } from "../../../../services/DocumentsService.ts";
import {FormField} from "../../Shared/FormField.tsx";
import {documentsApi} from "../../../../services/api/DocumentsApi.ts";

interface OfficeEditCardProps {
    office: OfficeDto;
    onCancel: () => void;
    onSuccess: (updatedOffice?: OfficeDto) => void;
}

export function OfficeEditCard({ office, onCancel, onSuccess }: OfficeEditCardProps) {
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [apiError, setApiError] = useState<string | null>(null);

    const [form, setForm] = useState({
        city: office.city || "",
        street: office.street || "",
        houseNumber: office.houseNumber || "",
        officeNumber: office.officeNumber || "",
        registryPhoneNumber: office.registryPhoneNumber || "+",
        isActive: office.isActive ?? true,
    });

    const [touched, setTouched] = useState<Record<string, boolean>>({});
    const [photoId, setPhotoId] = useState<string | null>(null);
    const [previewUrl, setPreviewUrl] = useState<string | null>(
        office.photoId ? documentsService.getOfficePhotoUrl(office.id, office.photoId) : null
    );
    const [photoIsUploading, setPhotoIsUploading] = useState<boolean>(false);

    const getFieldError = (name: string, val: string) => {
        const strVal = val ?? "";
        if (["city", "street", "houseNumber"].includes(name) && !strVal.trim()) {
            return "This field is required";
        }
        if (name === "registryPhoneNumber") {
            const digits = strVal.replace(/^\+/, "");
            if (!digits.trim()) return "Please enter phone number";
            if (!/^\d+$/.test(digits)) return "Invalid phone number";
        }
        return "";
    };

    const errors = {
        city: getFieldError("city", form.city),
        street: getFieldError("street", form.street),
        houseNumber: getFieldError("houseNumber", form.houseNumber),
        registryPhoneNumber: getFieldError("registryPhoneNumber", form.registryPhoneNumber),
    };

    const handleChange = (field: string, value: any) => {
        let cleanValue = value ?? "";
        if (field === "registryPhoneNumber" && typeof cleanValue === "string" && !cleanValue.startsWith("+")) {
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
            documentsApi.uploadOfficeAvatar(office.id, file)
                .then((result: any) => {
                    if (result.type === "ok") {
                        setPhotoId(result.value.photoId);
                    }
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
        setTouched({
            city: true,
            street: true,
            houseNumber: true,
            registryPhoneNumber: true,
        });

        if (Object.values(errors).some(Boolean) || photoIsUploading) return;

        setIsSubmitting(true);
        try {
            const payload = {
                city: form.city.trim(),
                street: form.street.trim(),
                houseNumber: form.houseNumber.trim(),
                officeNumber: form.officeNumber.trim() || null,
                registryPhoneNumber: form.registryPhoneNumber.trim(),
                isActive: form.isActive,
                photoId: photoId,
            };

            const result = await officesApi.updateOffice(office.id, payload);

            if (result?.type === "error") {
                setApiError(result.error?.title || result.error?.message || "Failed to update office.");
            } else {
                onSuccess(result?.value || { ...office, ...payload });
            }
        } catch (err: any) {
            const serverMsg = err?.response?.data?.title || err?.response?.data?.message || err?.message;
            setApiError(serverMsg || "An unexpected error occurred while updating office.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="office-edit-form" noValidate>
            {apiError && <div className="status-message error">{apiError}</div>}

            <div className="form-group">
                <label htmlFor="officePhoto">Office Photo</label>
                <div className="photo-upload-wrapper">
                    <div className="avatar-preview-container">
                        {previewUrl ? (
                            <img src={previewUrl} alt="Office Preview" className="office-avatar-preview" />
                        ) : (
                            <div className="office-avatar-placeholder">{form.city[0] ?? "O"}</div>
                        )}
                        {photoIsUploading && (
                            <div className="avatar-spinner-overlay">
                                <div className="spinner" />
                            </div>
                        )}
                    </div>
                    <input
                        id="officePhoto"
                        type="file"
                        accept="image/*"
                        className="file-input"
                        onChange={handlePhotoChange}
                        disabled={isSubmitting || photoIsUploading}
                    />
                </div>
            </div>

            <div className="form-grid">
                <FormField
                    id="officeCity"
                    label="City *"
                    value={form.city}
                    error={errors.city}
                    isTouched={touched.city}
                    disabled={isSubmitting}
                    onChange={(v) => handleChange("city", v)}
                    onBlur={() => handleBlur("city")}
                />
                <FormField
                    id="officeStreet"
                    label="Street *"
                    value={form.street}
                    error={errors.street}
                    isTouched={touched.street}
                    disabled={isSubmitting}
                    onChange={(v) => handleChange("street", v)}
                    onBlur={() => handleBlur("street")}
                />
            </div>

            <div className="form-grid">
                <FormField
                    id="officeHouseNumber"
                    label="House Number *"
                    value={form.houseNumber}
                    error={errors.houseNumber}
                    isTouched={touched.houseNumber}
                    disabled={isSubmitting}
                    onChange={(v) => handleChange("houseNumber", v)}
                    onBlur={() => handleBlur("houseNumber")}
                />
                <FormField
                    id="officeNumber"
                    label="Office Number (optional)"
                    value={form.officeNumber}
                    disabled={isSubmitting}
                    onChange={(v) => handleChange("officeNumber", v)}
                />
            </div>

            <FormField
                id="registryPhoneNumber"
                label="Reception Phone *"
                value={form.registryPhoneNumber}
                error={errors.registryPhoneNumber}
                isTouched={touched.registryPhoneNumber}
                disabled={isSubmitting}
                onChange={(v) => handleChange("registryPhoneNumber", v)}
                onBlur={() => handleBlur("registryPhoneNumber")}
            />

            <div className="form-group checkbox-group">
                <label className="checkbox-label">
                    <input
                        type="checkbox"
                        checked={form.isActive}
                        onChange={(e) => handleChange("isActive", e.target.checked)}
                        disabled={isSubmitting}
                    />
                    Is Active
                </label>
            </div>

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