import {useAuth} from "../../../services/states/userState.tsx";
import {Link} from "react-router-dom";

export const ProfileMinimalBlock: React.FC = () => {
    const { state } = useAuth();

    if(state.status === "loading")
    {
        return ( <div style={{minHeight:"60px"}}>
                
            </div>
        );
    }

    if(state.status === "authorized")
    {
        return <div >
            <Link to="/profile" style={{ display: "flex", flexDirection: "row", gap: "16px", alignItems: "center", background: "#444", padding: "5px", borderRadius: "5px" }}>
                <svg width="50" height="50">
                    <circle cx="25" cy="25" r="25" fill="#fff" />
                </svg>
                {state.data.getEmail()}
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