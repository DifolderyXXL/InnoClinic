export interface PatientDto {
    id: number;
    accountId: string;
    dateOfBirth: string;
    accountFirstName: string;
    accountLastName: string;
    accountMiddleName?: string | null;
    accountEmail: string;
}

export interface PatientCardProps {
    patient: PatientDto;
}

export function PatientCard({ patient }: PatientCardProps) {
    const fullName = [patient.accountLastName, patient.accountFirstName, patient.accountMiddleName]
        .filter(Boolean)
        .join(' ');

    return (
        <div>
            <h3>{fullName}</h3>
            <p>Birth: {patient.dateOfBirth}</p>
            <p>Email: {patient.accountEmail}</p>
        </div>
    );
}

export const PAGE_SIZE: number = 10;
