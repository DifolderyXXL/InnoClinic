import {useAuth} from "../../../services/states/userState.tsx";
import {Link} from "react-router-dom";

export const ProfileMinimalBlock: React.FC = () => {
    const { state } = useAuth();

    if(state.status === "loading")
    {
        
    }

    if(state.status === "authorized")
    {
        return <div style={{ display: "flex", flexDirection: "row", gap: "16px", alignItems: "center" }}>
            <Link to="/profile" >
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