import {type ChangeEvent, type SyntheticEvent, useState} from "react";
import {
    type CreateMedicalResultRequest,
    documentsApi,
    type UserFullName
} from "../../../../services/api/DocumentsApi.ts";
import {profilesApi} from "../../../../services/api/ProfilesApi.ts";
import {appointmentsApi} from "../../../../services/api/AppointmentApi.ts";

interface CreateMedicalResultFormProps {
    appointmentId: string;
    onSuccess?: () => void;
    onCancel?: () => void;
}

export function CreateMedicalResultForm({
                                            appointmentId,
                                            onSuccess,
                                            onCancel,
                                        }: CreateMedicalResultFormProps) {
    const [isAutofilling, setIsAutofilling] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const [doctorName, setDoctorName] = useState<UserFullName>({
        firstName: "",
        lastName: "",
        middleName: "",
    });

    const [patientName, setPatientName] = useState<UserFullName>({
        firstName: "",
        lastName: "",
        middleName: "",
    });

    const [formData, setFormData] = useState<{
        userId: string;
        specialization: string;
        serviceName: string;
        patientDateOfBirth: string;
        complaints: string;
        diagnosis: string;
        conclusion: string;
        recommendations: string;
    }>({
        userId: "",
        specialization: "",
        serviceName: "",
        patientDateOfBirth: "",
        complaints: "",
        diagnosis: "",
        conclusion: "",
        recommendations: "",
    });

    const handleAutofill = async () => {
        setIsAutofilling(true);
        setError(null);

        try {
            // 1. Fetch appointment details
            const appointmentRes = await appointmentsApi.getMyDoctorAppointmentById(appointmentId);

            if (appointmentRes.type !== "ok" || !appointmentRes.value) {
                setError("Failed to fetch appointment data for autofill.");
                return;
            }

            const appointment = appointmentRes.value;

            setFormData((prev) => ({
                ...prev,
                serviceName: appointment.serviceName || prev.serviceName,
                userId: appointment.patientAccountId || prev.userId,
            }));

            
            // 2. Fetch Doctor details (Name & Specialization)
            if (appointment.doctorAccountId) {
                const doctorRes = await profilesApi.getDoctorById(appointment.doctorAccountId);
                if (doctorRes.type === "ok" && doctorRes.value) {
                    const doctor = doctorRes.value;

                    setDoctorName({
                        firstName: doctor.accountFirstName || "",
                        lastName: doctor.accountLastName || "",
                        middleName: doctor.accountMiddleName || null,
                    });

                    setFormData((prev) => ({
                        ...prev,
                        specialization: doctor.specializationName || "",
                    }));
                }
            }

            // 3. Fetch Patient details (Name & Date of birth)
            if (appointment.patientAccountId) {
                const patientRes = await profilesApi.getPatient(appointment.patientAccountId);
                if (patientRes.type === "ok" && patientRes.value) {
                    const patient = patientRes.value;

                    setPatientName({
                        firstName: patient.accountFirstName || "",
                        lastName: patient.accountLastName || "",
                        middleName: patient.accountMiddleName || null,
                    });

                    if (patient.dateOfBirth) {
                        setFormData((prev) => ({
                            ...prev,
                            patientDateOfBirth: patient.dateOfBirth.split("T")[0],
                        }));
                    }
                }
            }
        } catch {
            setError("An error occurred while autofilling data.");
        } finally {
            setIsAutofilling(false);
        }
    };

    const handleInputChange = (
        e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    const handleDoctorNameChange = (e: ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setDoctorName((prev) => ({ ...prev, [name]: value }));
    };

    const handlePatientNameChange = (e: ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setPatientName((prev) => ({ ...prev, [name]: value }));
    };

    const handleSubmit = async (e: SyntheticEvent) => {
        e.preventDefault();
        setIsSubmitting(true);
        setError(null);

        const requestPayload: CreateMedicalResultRequest = {
            userId: formData.userId || undefined,
            specialization: formData.specialization || undefined,
            serviceName: formData.serviceName || undefined,
            patientDateOfBirth: formData.patientDateOfBirth || undefined,
            complaints: formData.complaints || undefined,
            diagnosis: formData.diagnosis || undefined,
            conclusion: formData.conclusion || undefined,
            recommendations: formData.recommendations || undefined,
            doctorName:
                doctorName.firstName || doctorName.lastName
                    ? {
                        firstName: doctorName.firstName,
                        lastName: doctorName.lastName,
                        middleName: doctorName.middleName || null,
                    }
                    : undefined,
            patientName:
                patientName.firstName || patientName.lastName
                    ? {
                        firstName: patientName.firstName,
                        lastName: patientName.lastName,
                        middleName: patientName.middleName || null,
                    }
                    : undefined,
        };

        try {
            const result = await documentsApi.createMedicalResult(
                appointmentId,
                requestPayload
            );

            if (result.type === "ok") {
                if (onSuccess) onSuccess();
            } else {
                setError(result.error?.title || "Failed to save medical result.");
            }
        } catch {
            setError("An error occurred while saving.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <header style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                <h2>Create Medical Result</h2>

                {/* Кнопка автозаполнения */}
                <button
                    type="button"
                    onClick={handleAutofill}
                    disabled={isAutofilling || isSubmitting}
                >
                    {isAutofilling ? "Autofilling..." : "Autofill Info"}
                </button>
            </header>

            {error && <div style={{ color: "red" }}>{error}</div>}

            {/* Doctor Info */}
            <fieldset>
                <legend>Doctor Information</legend>
                <div>
                    <label>Last Name:</label>
                    <input
                        type="text"
                        name="lastName"
                        value={doctorName.lastName}
                        onChange={handleDoctorNameChange}
                    />
                </div>
                <div>
                    <label>First Name:</label>
                    <input
                        type="text"
                        name="firstName"
                        value={doctorName.firstName}
                        onChange={handleDoctorNameChange}
                    />
                </div>
                <div>
                    <label>Middle Name:</label>
                    <input
                        type="text"
                        name="middleName"
                        value={doctorName.middleName || ""}
                        onChange={handleDoctorNameChange}
                    />
                </div>
                <div>
                    <label>Specialization:</label>
                    <input
                        type="text"
                        name="specialization"
                        value={formData.specialization}
                        onChange={handleInputChange}
                    />
                </div>
            </fieldset>

            {/* Patient Info */}
            <fieldset>
                <legend>Patient Information</legend>
                <div>
                    <label>Last Name:</label>
                    <input
                        type="text"
                        name="lastName"
                        value={patientName.lastName}
                        onChange={handlePatientNameChange}
                    />
                </div>
                <div>
                    <label>First Name:</label>
                    <input
                        type="text"
                        name="firstName"
                        value={patientName.firstName}
                        onChange={handlePatientNameChange}
                    />
                </div>
                <div>
                    <label>Middle Name:</label>
                    <input
                        type="text"
                        name="middleName"
                        value={patientName.middleName || ""}
                        onChange={handlePatientNameChange}
                    />
                </div>
                <div>
                    <label>Date of Birth:</label>
                    <input
                        type="date"
                        name="patientDateOfBirth"
                        value={formData.patientDateOfBirth}
                        onChange={handleInputChange}
                    />
                </div>
            </fieldset>

            {/* Service Info */}
            <fieldset>
                <legend>Service Details</legend>
                <div>
                    <label>Service Name:</label>
                    <input
                        type="text"
                        name="serviceName"
                        value={formData.serviceName}
                        onChange={handleInputChange}
                    />
                </div>
            </fieldset>

            {/* Clinical Record */}
            <fieldset>
                <legend>Clinical Record</legend>
                <div>
                    <label>Complaints:</label>
                    <textarea
                        name="complaints"
                        rows={3}
                        value={formData.complaints}
                        onChange={handleInputChange}
                    />
                </div>

                <div>
                    <label>Diagnosis:</label>
                    <textarea
                        name="diagnosis"
                        rows={3}
                        value={formData.diagnosis}
                        onChange={handleInputChange}
                    />
                </div>

                <div>
                    <label>Conclusion:</label>
                    <textarea
                        name="conclusion"
                        rows={4}
                        value={formData.conclusion}
                        onChange={handleInputChange}
                    />
                </div>

                <div>
                    <label>Recommendations:</label>
                    <textarea
                        name="recommendations"
                        rows={4}
                        value={formData.recommendations}
                        onChange={handleInputChange}
                    />
                </div>
            </fieldset>

            <div>
                <button type="submit" disabled={isSubmitting || isAutofilling}>
                    {isSubmitting ? "Saving..." : "Save Result"}
                </button>
                {onCancel && (
                    <button type="button" onClick={onCancel} disabled={isSubmitting || isAutofilling}>
                        Cancel
                    </button>
                )}
            </div>
        </form>
    );
}