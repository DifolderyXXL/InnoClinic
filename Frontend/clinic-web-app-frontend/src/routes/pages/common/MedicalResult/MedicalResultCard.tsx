import { type MedicalResultBody } from "../../../../services/api/DocumentsApi.ts";
import { useState} from "react";
import { useSearchParams } from "react-router";
import "./MedicalResultCard.css"
import {MyDoctorAppointmentByIdCard} from "../Appointment/AppointmentCard.tsx";
import {RequireRole, Roles} from "../../../../components/common/RequireRole.tsx";
import {DoctorMedicalResultView} from "./DoctorMedicalResultView.tsx";
import {PatientMedicalResultView} from "./PatientMedicalResultView.tsx";

interface MedicalResultCardProps {
    medicalResult: MedicalResultBody;
    onExport: () => Promise<void>;
    canEdit?: boolean;
    onEdit?: () => void;
}

export function MedicalResultCard({ medicalResult, onExport, canEdit = false, onEdit }: MedicalResultCardProps) {
    const { complaints, diagnosis, conclusion, recommendations } = medicalResult;

    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleExportClick = async () => {
        setIsLoading(true);
        setError(null);
        try {
            await onExport();
        } catch {
            setError("Error exporting file");
        } finally {
            setIsLoading(false);
        }
    };

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

            <section className="medical-result-actions" style={{ display: "flex", gap: "10px", alignItems: "center" }}>
                <button
                    type="button"
                    className="download-btn"
                    onClick={handleExportClick}
                    disabled={isLoading}
                >
                    {isLoading ? "Downloading..." : "Download (PDF)"}
                </button>

                {canEdit && onEdit && (
                    <button
                        type="button"
                        className="edit-btn"
                        onClick={onEdit}
                    >
                        Edit
                    </button>
                )}

                {error && <p className="error-message">{error}</p>}
            </section>
        </article>
    );
}

export function MedicalResultPage() {
    const [searchParams] = useSearchParams();

    const targetId = searchParams.get("id");
    const userId = searchParams.get("userId");

    if (!targetId) {
        return <div className="medical-result-not-found">Not found</div>;
    }

    return (
        <div className="medical-result-page">
            <MyDoctorAppointmentByIdCard appointmentId={targetId} showResultLink={false} />

            <RequireRole roles={[Roles.Doctor]}>
                {userId ? (
                    <DoctorMedicalResultView appointmentId={targetId} userId={userId} />
                ) : (
                    <div>User ID is missing for Doctor view</div>
                )}
            </RequireRole>

            <RequireRole roles={[Roles.Patient]}>
                <PatientMedicalResultView appointmentId={targetId} />
            </RequireRole>
        </div>
    );
}

