import { useState } from "react";
import { profilesApi } from "../../../../../services/api/ProfilesApi.ts";
import {OfficeInputFilter, SpecializationInputFilter} from "../../../Shared/Inputs/OfficeInputFilter";
import { OfficeAddress } from "../../../specific/offices/OfficeCompactCard";
import { AlertMessage } from "./AccountProfileCard";

export interface DoctorFormData {
    dateOfBirth: string;
    careerStartYear: number;
    specializationId: number;
    status: number;
    officeId: string;
}

export function DoctorFormFields({
                                     form,
                                     onChange,
                                     disabled,
                                 }: {
    form: DoctorFormData;
    onChange: (updated: DoctorFormData) => void;
    disabled: boolean;
}) {
    return (
        <div className="form-grid">
            <div className="form-group">
                <label>Date of Birth</label>
                <input
                    type="date"
                    value={form.dateOfBirth}
                    onChange={(e) => onChange({ ...form, dateOfBirth: e.target.value })}
                    required
                    disabled={disabled}
                />
            </div>
            <div className="form-group">
                <label>Career Start Year</label>
                <input
                    type="number"
                    value={form.careerStartYear}
                    onChange={(e) => onChange({ ...form, careerStartYear: Number(e.target.value) })}
                    required
                    disabled={disabled}
                />
            </div>
            <div className="form-group">
                <label>Specialization</label>
                <SpecializationInputFilter
                    valueId={form.specializationId ? form.specializationId : null}
                    onChange={(spec) => {
                        onChange({
                            ...form,
                            specializationId: Number(spec?.id)
                        });
                    }}
                />
            </div>
            <div className="form-group">
                <label>Office ID</label>
                <OfficeInputFilter valueId={form.officeId} onChange={office => {onChange({...form, officeId :office?.id??""})}}/>
            </div>
        </div>
    );
}

export function DoctorProfileForm({ accountId, onSuccess }: { accountId: string; onSuccess: () => void }) {
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [actionMessage, setActionMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

    const [form, setForm] = useState<DoctorFormData>({
        dateOfBirth: "",
        careerStartYear: new Date().getFullYear(),
        specializationId: 0,
        status: 1,
        officeId: "",
    });

    const handleSubmit = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        setActionMessage(null);

        try {
            const result = await profilesApi.createDoctor(accountId, form);
            if (result.type === "ok") {
                setActionMessage({ type: "success", text: "Doctor profile created successfully!" });
                onSuccess();
            } else {
                setActionMessage({ type: "error", text: result.error?.title || "Failed to create doctor profile" });
            }
        } catch {
            setActionMessage({ type: "error", text: "Failed to create doctor profile" });
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="account-details-card profile-form-card">
            <h3>Create Doctor Profile</h3>
            {actionMessage && <AlertMessage message={actionMessage.text} type={actionMessage.type} />}

            <form className="details-form" onSubmit={handleSubmit}>
                <DoctorFormFields form={form} onChange={setForm} disabled={isSubmitting} />
                <div className="form-actions">
                    <button type="submit" className="submit-btn" disabled={isSubmitting}>
                        {isSubmitting ? "Creating..." : "Create Profile"}
                    </button>
                </div>
            </form>
        </div>
    );
}

export function DoctorCard({ accountId, initialData, onUpdateSuccess }: { accountId: string; initialData: any; onUpdateSuccess: () => void }) {
    const [isEditing, setIsEditing] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [actionMessage, setActionMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

    const [form, setForm] = useState<DoctorFormData>({
        dateOfBirth: initialData.dateOfBirth?.split("T")[0] || "",
        careerStartYear: initialData.careerStartYear || new Date().getFullYear(),
        specializationId: initialData.specializationId || 0,
        status: initialData.status || 1,
        officeId: initialData.officeId || "",
    });

    const handleUpdate = async (e: React.SyntheticEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        setActionMessage(null);

        try {
            const result = await profilesApi.updateDoctor(accountId, { ...form });
            if (result.type === "ok") {
                setIsEditing(false);
                onUpdateSuccess();
            } else {
                setActionMessage({ type: "error", text: result.error?.title || "Failed to update doctor profile" });
            }
        } catch {
            setActionMessage({ type: "error", text: "An unexpected error occurred" });
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="account-details-card profile-form-card">
            <header className="details-header">
                <h3>Doctor Profile</h3>
                {!isEditing && <button className="edit-btn" onClick={() => setIsEditing(true)}>Edit</button>}
            </header>

            {actionMessage && <AlertMessage message={actionMessage.text} type={actionMessage.type} />}

            {isEditing ? (
                <form className="details-form" onSubmit={handleUpdate}>
                    <DoctorFormFields form={form} onChange={setForm} disabled={isSubmitting} />
                    <div className="form-actions">
                        <button type="submit" className="submit-btn" disabled={isSubmitting}>Save Changes</button>
                        <button type="button" className="cancel-btn" onClick={() => setIsEditing(false)} disabled={isSubmitting}>Cancel</button>
                    </div>
                </form>
            ) : (
                <div className="info-grid">
                    <div className="info-item"><span className="label">Career Start</span><span className="value">{initialData.careerStartYear}</span></div>
                    <div className="info-item"><span className="label">Specialization</span><span className="value">{initialData.specializationName}</span></div>
                    <div className="info-item"><span className="label">Office ID</span><OfficeAddress officeId={initialData.officeId}/></div>
                    <div className="info-item"><span className="label">Date of Birth</span><span className="value">{initialData.dateOfBirth?.split("T")[0]}</span></div>
                </div>
            )}
        </div>
    );
}