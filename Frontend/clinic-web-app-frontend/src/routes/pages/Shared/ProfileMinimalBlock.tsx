import {useAuth} from "../../../services/states/userState.tsx";
import {Link} from "react-router-dom";
import {AvatarFromSource} from "./Avatar.tsx";
import {useEffect, useState} from "react";
import {profilesApi} from "../../../services/api/ProfilesApi.ts";

export const ProfileMinimalBlock: React.FC = () => {
    const { state } = useAuth();
    const [photoUrl, setPhotoUrl] = useState<string | null>(null);

    useEffect(() => {
        if (state.status === "authorized") {
            profilesApi.getAccountMe()
                .then(result => {
                    if (result.type === "ok") {
                        const account = result.value;
                        setPhotoUrl(account.photoUrl);
                    }
                })
                .catch(() => setPhotoUrl(null));
        } else {
            setPhotoUrl(null);
        }
    }, [state.status]);
    
    if(state.status === "loading")
    {
        return ( <div style={{minHeight:"60px"}}>
                
            </div>
        );
    }

    if(state.status === "authorized")
    {
        const email = state.data.getEmail();
        return <div >
            <Link to="/profile" style={{ textDecoration: "none", display: "flex", flexDirection: "row", gap: "16px", alignItems: "center", background: "#444", padding: "5px", borderRadius: "5px" }}>
                <AvatarFromSource TextIfPhotoNull={email[0]} PhotoUrl={photoUrl} />
                
                <div style={{ display: "flex", flexDirection: "column", gap: "1px"}}>
                    {email}
                    <strong style={{ fontSize: "1.2em" }}>Role: <span>{state.data.getRoles().toString()}</span></strong>
                </div>
            </Link>
        </div>;
    }

    if(state.status === "unauthorized")
    {
        return <div style={{ display: "flex", flexDirection: "row", gap: "16px", alignItems: "center" }}>
            <Link to="/login" >
                Login
            </Link>
        </div>;
    }
    
    return <>
        NONE
    </>
}