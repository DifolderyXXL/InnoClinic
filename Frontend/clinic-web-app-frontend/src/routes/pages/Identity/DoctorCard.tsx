import "./DoctorCard.css";
import {OfficeAddress} from "../specific/offices/OfficeCompactCard.tsx";

interface DoctorCardProps {
    dateOfBirth: string;
    officeId: string;
    careerStartYear: number;
    specializationName: string;
}

export function DoctorCard({ dateOfBirth, officeId, careerStartYear, specializationName }: DoctorCardProps) {
    const formattedDate = dateOfBirth ? new Date(dateOfBirth).toLocaleDateString() : "—";

    const currentYear = new Date().getFullYear();
    const experience = careerStartYear > 0 && careerStartYear <= currentYear
        ? currentYear - careerStartYear
        : 0;

    return (
        <div className="account-details-card profile-form-card">
            <header className="card-header">
                <h3>Doctor Profile</h3>
            </header>

            <div className="doctor-info-grid">
                <div className="doctor-info-item">
                    <span className="label">Specialization</span>
                    <span className="value">{specializationName || "—"}</span>
                </div>

                <div className="doctor-info-item">
                    <span className="label">Office</span>
                    <span className="value"><OfficeAddress officeId={officeId}/></span>
                </div>

                <div className="doctor-info-item">
                    <span className="label">Experience</span>
                    <span className="value">
                        {experience > 0 ? `${experience} yrs` : "Career Start"}
                    </span>
                </div>

                <div className="doctor-info-item">
                    <span className="label">Birthday</span>
                    <span className="value">{formattedDate}</span>
                </div>
            </div>
        </div>
    );
}