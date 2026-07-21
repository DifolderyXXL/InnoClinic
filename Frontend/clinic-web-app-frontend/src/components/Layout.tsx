import { Outlet } from 'react-router-dom';
import {ProfileMinimalBlock} from "../routes/pages/Shared/ProfileMinimalBlock.tsx";
import {ClinicNavigation} from "./ClinicNavigation.tsx";

export function Layout() {
    return (
        <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
            <header style={{ padding: '10px', background: '#333', color: '#fff' }}>
                <h1>InnoClinic</h1>
                <div style={{ display: 'flex', flexDirection: 'row', minWidth: '100vh', justifyContent: 'space-between'  }}>
                    <ClinicNavigation/>
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