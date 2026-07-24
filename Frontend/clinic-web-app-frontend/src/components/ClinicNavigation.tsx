import {RouteLinkCard} from "./RouteLinkCard.tsx";

export function ClinicNavigation() {
    return (
        <nav style={{alignContent: "center"}}>
            <RouteLinkCard to="/" >Home</RouteLinkCard>
            <RouteLinkCard to="/doctors">Doctors</RouteLinkCard>
            <RouteLinkCard to="/view-offices">Offices</RouteLinkCard>
            <RouteLinkCard to="/view-services">Services</RouteLinkCard>
            <RouteLinkCard to="/make-appointment">Make appointment</RouteLinkCard>
            <RouteLinkCard to="/my-appointments">My appointments</RouteLinkCard>
        </nav>
    )
}