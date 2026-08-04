import { RouteLinkCard } from "./RouteLinkCard.tsx";
import { RequireRole, Roles } from "./common/RequireRole.tsx";
import "./ClinicNavigation.css";

export function ClinicNavigation() {
    return (
        <nav className="nav-container">
            <RouteLinkCard to="/">Home</RouteLinkCard>
            <RouteLinkCard to="/doctors">Doctors</RouteLinkCard>
            <RouteLinkCard to="/view-offices">Offices</RouteLinkCard>
            <RouteLinkCard to="/view-services">Services</RouteLinkCard>
            <RequireRole roles={[Roles.Patient]}>
                <RouteLinkCard to="/make-appointment">Make appointment</RouteLinkCard>
                <RouteLinkCard to="/my-appointments">My appointments</RouteLinkCard>
            </RequireRole>
        </nav>
    );
}

export function DoctorNavigation() {
    return (
        <nav className="nav-container">
            <RouteLinkCard to="/my-schedule">My schedule</RouteLinkCard>
        </nav>
    );
}

export function ReceptionistNavigation() {
    return (
        <nav className="nav-container">
            <RouteLinkCard to="/clinic-schedule">Clinic schedule</RouteLinkCard>
        </nav>
    );
}