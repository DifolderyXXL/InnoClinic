import { documentsApi, type MedicalResultBody } from "../../../../services/api/DocumentsApi.ts";
import {type ChangeEvent, type SyntheticEvent, useEffect, useState} from "react";
import { CreateMedicalResultForm } from "./CreateMedicalResult.tsx";
import "./MedicalResultCard.css"
import {MedicalResultCard} from "./MedicalResultCard.tsx";



interface DoctorMedicalResultViewProps {
    appointmentId: string;
    userId: string;
}

export function DoctorMedicalResultView({ appointmentId, userId }: DoctorMedicalResultViewProps) {
    const [medicalResult, setMedicalResult] = useState<MedicalResultBody | null>(null);
    const [isEditing, setIsEditing] = useState(false);
    const [isLoadingData, setIsLoadingData] = useState(true);

    const loadData = () => {
        setIsLoadingData(true);
        documentsApi.getUserMedicalResult(appointmentId, userId)
            .then((result) => {
                if (result.type === "ok") setMedicalResult(result.value);
            })
            .finally(() => setIsLoadingData(false));
    };

    useEffect(() => {
        loadData();
    }, [appointmentId, userId]);

    const handleExport = async () => {
        const result = await documentsApi.exportUserMedicalResult(appointmentId, userId);
        if (result.type === "ok" && result.value?.url) {
            window.open(result.value.url, "_blank", "noopener,noreferrer");
        } else {
            throw new Error("Failed to export PDF");
        }
    };

    if (isLoadingData) return <div>Loading medical result...</div>;

    if (isEditing && medicalResult) {
        return (
            <UpdateMedicalResultForm
                appointmentId={appointmentId}
                initialData={medicalResult}
                onSuccess={() => {
                    setIsEditing(false);
                    loadData();
                }}
                onCancel={() => setIsEditing(false)}
            />
        );
    }

    return (
        <div className="medical-result-container">
            {medicalResult ? (
                <MedicalResultCard
                    medicalResult={medicalResult}
                    onExport={handleExport}
                    canEdit={true}
                    onEdit={() => setIsEditing(true)}
                />
            ) : (
                <CreateMedicalResultForm
                    appointmentId={appointmentId}
                    onSuccess={loadData}
                />
            )}
        </div>
    );
}


interface UpdateMedicalResultFormProps {
    appointmentId: string;
    initialData: MedicalResultBody;
    onSuccess?: () => void;
    onCancel?: () => void;
}

export function UpdateMedicalResultForm({
                                            appointmentId,
                                            initialData,
                                            onSuccess,
                                            onCancel,
                                        }: UpdateMedicalResultFormProps) {
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [formData, setFormData] = useState<MedicalResultBody>({
        complaints: initialData.complaints || "",
        diagnosis: initialData.diagnosis || "",
        conclusion: initialData.conclusion || "",
        recommendations: initialData.recommendations || "",
    });

    const handleInputChange = (
        e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e: SyntheticEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        setError(null);

        const requestPayload: MedicalResultBody = {
            complaints: formData.complaints || undefined,
            diagnosis: formData.diagnosis || undefined,
            conclusion: formData.conclusion || undefined,
            recommendations: formData.recommendations || undefined,
        };

        try {
            const result = await documentsApi.updateMedicalResult(
                appointmentId,
                requestPayload
            );

            if (result.type === "ok") {
                if (onSuccess) onSuccess();
            } else {
                setError(result.error?.title || "Failed to update medical result.");
            }
        } catch {
            setError("An error occurred while updating.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="medical-result-form">
            <header className="form-header" style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <h2>Edit Medical Result</h2>
            </header>

            {error && <div className="error-message" style={{ color: "red" }}>{error}</div>}

            <fieldset className="form-section">
                <legend>Clinical Record</legend>

                <div className="form-group">
                    <label>Complaints:</label>
                    <textarea
                        name="complaints"
                        rows={3}
                        value={formData.complaints || ""}
                        onChange={handleInputChange}
                    />
                </div>

                <div className="form-group">
                    <label>Diagnosis:</label>
                    <textarea
                        name="diagnosis"
                        rows={3}
                        value={formData.diagnosis || ""}
                        onChange={handleInputChange}
                    />
                </div>

                <div className="form-group">
                    <label>Conclusion:</label>
                    <textarea
                        name="conclusion"
                        rows={4}
                        value={formData.conclusion || ""}
                        onChange={handleInputChange}
                    />
                </div>

                <div className="form-group">
                    <label>Recommendations:</label>
                    <textarea
                        name="recommendations"
                        rows={4}
                        value={formData.recommendations || ""}
                        onChange={handleInputChange}
                    />
                </div>
            </fieldset>

            <div className="form-actions" style={{ display: "flex", gap: "10px", marginTop: "10px" }}>
                <button type="submit" className="submit-btn" disabled={isSubmitting}>
                    {isSubmitting ? "Saving..." : "Update Result"}
                </button>
                {onCancel && (
                    <button type="button" className="cancel-btn" onClick={onCancel} disabled={isSubmitting}>
                        Cancel
                    </button>
                )}
            </div>
        </form>
    );
}

