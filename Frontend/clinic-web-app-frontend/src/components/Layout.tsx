import { Outlet } from 'react-router-dom';
import {ProfileMinimalBlock} from "../routes/pages/Shared/ProfileMinimalBlock.tsx";
import {ClinicNavigation, DoctorNavigation} from "./ClinicNavigation.tsx";
import {RequireRole, Roles} from "./common/RequireRole.tsx";

export function Layout() {
    return (
        <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
            <header style={{ padding: '10px', background: '#333', color: '#fff' }}>
                <h1>InnoClinic</h1>
                <div style={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between'  }}>
                    <div  style={{ display: 'flex', flexDirection: 'column', alignItems:"start" }}>
                        <ClinicNavigation/>
                        <RequireRole roles={[Roles.Doctor]}>
                            <DoctorNavigation/>
                        </RequireRole>
                    </div>
                    <ProfileMinimalBlock/>
                </div>
            </header>
            <main style={{
                flex: 1,
                display: 'flex',
                flexDirection: 'column',
                padding: '20px',
                overflow: 'hidden'
            }}>
                <Outlet />
            </main>
            <footer style={{ padding: '10px', background: '#333', color: '#fff', textAlign: 'center' }}>
                © 2026 InnoClinic
            </footer>
        </div>
    );
}