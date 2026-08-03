import { documentsApi, type MedicalResultBody } from "../../../../services/api/DocumentsApi.ts";
import {type ChangeEvent, type SyntheticEvent, useEffect, useState} from "react";
import { useSearchParams } from "react-router";
import { CreateMedicalResultForm } from "./CreateMedicalResult.tsx";
import "./MedicalResultCard.css"
import {MyDoctorAppointmentByIdCard} from "../Appointment/AppointmentCard.tsx";

interface MedicalResultCardProps {
    medicalResult: MedicalResultBody;
    appointmentId: string;
    userId: string;
    onRefresh: () => void;
}

export function MedicalResultCard({ medicalResult, appointmentId, userId, onRefresh }: MedicalResultCardProps) {
    const { complaints, diagnosis, conclusion, recommendations } = medicalResult;

    const [isEditing, setIsEditing] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleExport = async () => {
        setIsLoading(true);
        setError(null);

        try {
            const result = await documentsApi.exportUserMedicalResult(appointmentId, userId);

            if (result.type === "ok" && result.value?.url) {
                window.open(result.value.url, "_blank", "noopener,noreferrer");
            } else {
                setError("Error exporting file");
            }
        } catch {
            setError("Error exporting file");
        } finally {
            setIsLoading(false);
        }
    };

    if (isEditing) {
        return (
            <UpdateMedicalResultForm
                appointmentId={appointmentId}
                initialData={medicalResult}
                onSuccess={() => {
                    setIsEditing(false);
                    onRefresh();
                }}
                onCancel={() => setIsEditing(false)}
            />
        );
    }

    return (
        <article className="medical-result-card">
            <header className="medical-result-header">
                <h2 className="medical-result-title">Medical results</h2>
            </header>

            {complaints && (
                <section className="medical-result-section section-complaints">
                    <h3 className="section-title">Complaints</h3>
                    <p className="section-content">{complaints}</p>
                </section>
            )}

            {diagnosis && (
                <section className="medical-result-section section-diagnosis">
                    <h3 className="section-title">Diagnosis</h3>
                    <p className="section-content">{diagnosis}</p>
                </section>
            )}

            {conclusion && (
                <section className="medical-result-section section-conclusion">
                    <h3 className="section-title">Conclusion</h3>
                    <p className="section-content">{conclusion}</p>
                </section>
            )}

            {recommendations && (
                <section className="medical-result-section section-recommendations">
                    <h3 className="section-title">Recommendations</h3>
                    <p className="section-content">{recommendations}</p>
                </section>
            )}

            <section className="medical-result-actions" style={{ display: "flex", gap: "10px" }}>
                <button
                    className="download-btn"
                    onClick={handleExport}
                    disabled={isLoading}
                >
                    {isLoading ? "Downloading..." : "Download (PDF)"}
                </button>
                <button
                    type="button"
                    className="edit-btn"
                    onClick={() => setIsEditing(true)}
                >
                    Edit
                </button>
                {error && <p className="error-message">{error}</p>}
            </section>
        </article>
    );
}

interface MedicalResultByIdProps {
    id: string;
    userId: string;
}

export function MedicalResultById({ id, userId }: MedicalResultByIdProps) {
    const [medicalResult, setMedicalResult] = useState<MedicalResultBody | null>(null);

    const loadData = () => {
        documentsApi.getUserMedicalResult(id, userId).then((result) => {
            if (result.type === "ok") setMedicalResult(result.value);
        });
    };

    useEffect(() => {
        loadData();
    }, [id, userId]);

    return (
        <div className="medical-result-container">
            {medicalResult ? (
                <MedicalResultCard
                    medicalResult={medicalResult}
                    appointmentId={id}
                    userId={userId}
                    onRefresh={loadData}
                />
            ) : (
                <CreateMedicalResultForm
                    appointmentId={id}
                    onSuccess={loadData}
                />
            )}
        </div>
    );
}

export function MedicalResultPage() {
    const [searchParams] = useSearchParams();

    const targetId = searchParams.get("id") || null;
    const userId = searchParams.get("userId") || null;

    if (!targetId || !userId) {
        return <div className="medical-result-not-found">Not found</div>;
    }

    return (
        <div className="medical-result-page">
            <MyDoctorAppointmentByIdCard appointmentId={targetId} showResultLink={false}/>
            <MedicalResultById id={targetId} userId={userId} />
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