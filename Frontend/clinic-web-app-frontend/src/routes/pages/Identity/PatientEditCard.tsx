import React, { useState } from "react";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";

interface PatientEditCardProps {
    dateOfBirth: string;
    onCancel: () => void;
    onSuccess: () => void;
}

export function PatientEditCard({ dateOfBirth, onCancel, onSuccess }: PatientEditCardProps) {
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [dobInput, setDobInput] = useState(dateOfBirth || "");

    const [touched, setTouched] = useState(false);
    const [fieldError, setFieldError] = useState<string | null>(null);
    const [apiError, setApiError] = useState<string | null>(null);

    const validateDob = (value: string) => {
        if (!value.trim()) {
            setFieldError("Please, select the date");
            return false;
        }
        setFieldError(null);
        return true;
    };

    const handleBlur = () => {
        setTouched(true);
        validateDob(dobInput);
    };

    const handleSubmit = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setApiError(null);
        setTouched(true);

        if (!validateDob(dobInput)) {
            return;
        }

        setIsSubmitting(true);
        try {
            const result = await profilesApi.updatePatientMe(dobInput);

            if (result?.type === "error") {
                setApiError(result.error?.title || result.error?.message || "Failed to update date of birth.");
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
        <form onSubmit={handleSubmit} className="patient-create-form" noValidate>
            {apiError && <div className="status-message error">{apiError}</div>}

            <div className="form-group">
                <label htmlFor="editPatientDob">Date of Birth *</label>
                <input
                    id="editPatientDob"
                    type="date"
                    value={dobInput}
                    className={touched && fieldError ? "has-error" : ""}
                    onChange={(e) => {
                        setDobInput(e.target.value);
                        if (touched) validateDob(e.target.value);
                    }}
                    onBlur={handleBlur}
                    disabled={isSubmitting}
                />
                {touched && fieldError && (
                    <span className="field-error-text">{fieldError}</span>
                )}
            </div>

            <div className="form-actions">
                <button type="submit" className="submit-btn" disabled={isSubmitting}>
                    {isSubmitting ? "Saving..." : "Save"}
                </button>
                <button
                    type="button"
                    onClick={onCancel}
                    className="cancel-btn"
                    disabled={isSubmitting}
                >
                    Cancel
                </button>
            </div>
        </form>
    );
}