import { Outlet } from "react-router-dom";
import { ProfileMinimalBlock } from "../routes/pages/Shared/ProfileMinimalBlock.tsx";
import { ClinicNavigation, DoctorNavigation, ReceptionistNavigation } from "./ClinicNavigation.tsx";
import { RequireRole, Roles } from "./common/RequireRole.tsx";
import "./Layout.css";

export function Layout() {
    return (
        <div className="app-layout">
            <header className="app-header">
                <div className="header-brand">
                    <h1>InnoClinic</h1>
                </div>

                <div className="header-content">
                    <nav className="header-navigation">
                        <ClinicNavigation />
                        <RequireRole roles={[Roles.Doctor]}>
                            <DoctorNavigation />
                        </RequireRole>
                        <RequireRole roles={[Roles.Receptionist]}>
                            <ReceptionistNavigation />
                        </RequireRole>
                    </nav>

                    <div className="header-profile">
                        <ProfileMinimalBlock />
                    </div>
                </div>
            </header>

            <main className="app-main">
                <Outlet />
            </main>

            <footer className="app-footer">
                © 2026 InnoClinic
            </footer>
        </div>
    );
}