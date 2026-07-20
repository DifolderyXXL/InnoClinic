import { Outlet } from 'react-router-dom';
import {ProfileMinimalBlock} from "../routes/pages/Shared/ProfileMinimalBlock.tsx";

export function Layout() {
    return (
        <div style={{ display: 'flex', flexDirection: 'column', minHeight: '50vh' }}>
            <header style={{ padding: '20px', background: '#333', color: '#fff' }}>
                <h1>InnoClinic</h1>
                <ProfileMinimalBlock/>
            </header>
            <main style={{ flex: 1, padding: '20px' }}>
                <Outlet />
            </main>
            <footer style={{ padding: '10px', background: '#333', color: '#fff', textAlign: 'center' }}>
                © 2026 InnoClinic
            </footer>
        </div>
    );
}