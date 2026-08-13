import {useEffect} from "react";

export const getLoginUrl = (role?: string) => {
    const baseUrl = "/bff/login";
    return role ? `${baseUrl}?prompt=login&acr_values=role:${role}` : baseUrl;
};

interface LoginPageProps {
    role?: string; 
}
export function LoginPage({ role }: LoginPageProps) {
    useEffect(() => {
        window.location.replace(getLoginUrl(role));
    }, [role]);

    return (
        <div style={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100vh" }}>
            <p>Redirecting...</p>
        </div>
    );
}
