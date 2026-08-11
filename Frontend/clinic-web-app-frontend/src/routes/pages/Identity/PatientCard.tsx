import React, { useState } from "react";
import { PatientEditCard } from "./PatientEditCard.tsx";
import "./PatientCard.css";

interface PatientCardProps {
    dateOfBirth: string;
    onUpdateSuccess?: () => void;
}

export function PatientCard({ dateOfBirth, onUpdateSuccess }: PatientCardProps) {
    const [isEditing, setIsEditing] = useState(false);

    return (
        <div className="patient-info-card">
            <header className="card-header">
                <h3>Patient Profile</h3>
                {!isEditing && (
                    <button
                        type="button"
                        className="edit-btn"
                        onClick={() => setIsEditing(true)}
                    >
                        Edit
                    </button>
                )}
            </header>

            {isEditing ? (
                <PatientEditCard
                    dateOfBirth={dateOfBirth}
                    onCancel={() => setIsEditing(false)}
                    onSuccess={() => {
                        setIsEditing(false);
                        if (onUpdateSuccess) onUpdateSuccess();
                    }}
                />
            ) : (
                <div className="patient-info-field">
                    <span className="field-label">Birthday</span>
                    <span className="field-value">{dateOfBirth || "—"}</span>
                </div>
            )}
        </div>
    );
}

interface PatientCreateCardProps {
    onSubmit: (dateOfBirth: string) => Promise<{ type: "ok" } | { type: "error"; error?: { title: string } }>;
    onSuccess?: () => void;
}

export function PatientCreateCard({ onSubmit, onSuccess }: PatientCreateCardProps) {    const [isCreating, setIsCreating] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [dobInput, setDobInput] = useState("");

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

    const handleCreatePatient = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setApiError(null);
        setTouched(true);

        if (!validateDob(dobInput)) {
            return;
        }

        setIsSubmitting(true);
        try {
            const result = await onSubmit(dobInput);

            if (result.type === "error") {
                setApiError(result.error?.title || "Failed to create patient profile.");
            } else {
                setIsCreating(false);
                setDobInput("");
                setTouched(false);
                setFieldError(null);

                if (onSuccess) onSuccess();
            }
        } catch {
            setApiError("An unexpected error occurred while creating patient profile.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="patient-create-card">
            <header className="card-header">
                <h3>Patient Profile</h3>
            </header>

            {apiError && <div className="status-message error">{apiError}</div>}

            {isCreating ? (
                <form onSubmit={handleCreatePatient} className="patient-create-form" noValidate>
                    <div className="form-group">
                        <label htmlFor="patientDob">Date of Birth *</label>
                        <input
                            id="patientDob"
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
                            {isSubmitting ? "Saving..." : "Save Profile"}
                        </button>
                        <button
                            type="button"
                            onClick={() => {
                                setIsCreating(false);
                                setTouched(false);
                                setFieldError(null);
                                setApiError(null);
                            }}
                            className="cancel-btn"
                            disabled={isSubmitting}
                        >
                            Cancel
                        </button>
                    </div>
                </form>
            ) : (
                <div className="empty-profile-state">
                    <p className="card-status-text">No patient profile found.</p>
                    <button
                        type="button"
                        onClick={() => setIsCreating(true)}
                        className="submit-btn"
                    >
                        + Create Patient Profile
                    </button>
                </div>
            )}
        </div>
    );
}