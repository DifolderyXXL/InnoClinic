import {RouteLinkCard} from "./RouteLinkCard.tsx";
import {RequireRole, Roles} from "./common/RequireRole.tsx";

export function ClinicNavigation() {
    return (
        <nav style={{alignContent: "center", overflowX: "auto",
            overflowY: "hidden", scrollbarWidth: "thin",}}>
            <RouteLinkCard to="/" >Home</RouteLinkCard>
            <RouteLinkCard to="/doctors">Doctors</RouteLinkCard>
            <RouteLinkCard to="/view-offices">Offices</RouteLinkCard>
            <RouteLinkCard to="/view-services">Services</RouteLinkCard>
            <RequireRole roles={[Roles.Patient]}>
                <RouteLinkCard to="/make-appointment">Make appointment</RouteLinkCard>
                <RouteLinkCard to="/my-appointments">My appointments</RouteLinkCard>
            </RequireRole>
        </nav>
    )
}

export function DoctorNavigation() {
    return (
        <nav style={{alignContent: "center", overflowX: "auto",
            overflowY: "hidden", scrollbarWidth: "thin",}}>
            <RouteLinkCard to="/my-schedule" >My schedule</RouteLinkCard>

        </nav>
    )
}