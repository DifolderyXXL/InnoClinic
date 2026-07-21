
interface DoctorCardProps {
    dateOfBirth: string;
    officeId: number;
    careerStartYear: number;
    specializationName: string;
}

export function DoctorCard({ dateOfBirth, officeId, careerStartYear, specializationName }: DoctorCardProps) {
    const formattedDate = dateOfBirth ? new Date(dateOfBirth).toLocaleDateString() : "—";

    const currentYear = 2026;
    const experience = careerStartYear > 0 && careerStartYear <= currentYear
        ? currentYear - careerStartYear
        : 0;

    return (
        <div style={{ display: "flex", flexDirection: "row", gap: "16px", padding: "20px", background: "#222", color: "#fff", borderRadius: "8px", alignItems: "center" }}>
            <div>
                <strong>Specialization:</strong> <span style={{ color: "#889a7e" }}>{specializationName}</span>
            </div>
            <div style={{ width: "1px", height: "20px", backgroundColor: "#444" }} />
            <div>
                <strong>Office ID:</strong> {officeId}
            </div>
            <div style={{ width: "1px", height: "20px", backgroundColor: "#444" }} />
            <div>
                <strong>Experience:</strong> {experience > 0 ? `${experience} yrs` : "Career Start"}
            </div>
            <div style={{ width: "1px", height: "20px", backgroundColor: "#444" }} />
            <div>
                <strong>Birthday:</strong> {formattedDate}
            </div>
        </div>
    );
}