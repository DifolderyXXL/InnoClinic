import { documentsApi, type MedicalResultBody } from "../../../../services/api/DocumentsApi.ts";
import { useEffect, useState} from "react";
import "./MedicalResultCard.css"
import {MedicalResultCard} from "./MedicalResultCard.tsx";

interface PatientMedicalResultViewProps {
    appointmentId: string;
}

export function PatientMedicalResultView({ appointmentId }: PatientMedicalResultViewProps) {
    const [medicalResult, setMedicalResult] = useState<MedicalResultBody | null>(null);
    const [isLoadingData, setIsLoadingData] = useState(true);

    useEffect(() => {
        setIsLoadingData(true);
        documentsApi.getMyMedicalResult(appointmentId)
            .then((result) => {
                if (result.type === "ok") setMedicalResult(result.value);
            })
            .finally(() => setIsLoadingData(false));
    }, [appointmentId]);

    const handleExport = async () => {
        const result = await documentsApi.exportMyMedicalResult(appointmentId);
        if (result.type === "ok" && result.value?.url) {
            window.open(result.value.url, "_blank", "noopener,noreferrer");
        } else {
            throw new Error("Failed to export PDF");
        }
    };

    if (isLoadingData) return <div>Loading medical result...</div>;

    if (!medicalResult) {
        return <div className="medical-result-empty">Medical result not available yet.</div>;
    }

    return (
        <div className="medical-result-container">
            <MedicalResultCard
                medicalResult={medicalResult}
                onExport={handleExport}
                canEdit={false}
            />
        </div>
    );
}