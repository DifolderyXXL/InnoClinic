import {RouteLinkCard} from "./RouteLinkCard.tsx";

export function ClinicNavigation() {
    return (
        <nav style={{alignContent: "center"}}>
            <RouteLinkCard to="/" >Home</RouteLinkCard>
            <RouteLinkCard to="/doctors">Doctors</RouteLinkCard>
            <RouteLinkCard to="/view-services">Services</RouteLinkCard>
        </nav>
    )
}