import {useEffect} from "react";
import {loginUrl} from "../../../services/bffEndpoints.tsx";


export function LoginPage() {
    useEffect(() => {
        window.location.assign(loginUrl);
    }, []);

    return (
        <div style={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100vh" }}>
            <p>Redirecting</p>
        </div>
    );
}

