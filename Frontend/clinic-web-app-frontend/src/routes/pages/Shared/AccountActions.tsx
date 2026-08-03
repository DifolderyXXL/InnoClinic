import {useState} from "react";
import {FaEllipsisV} from "react-icons/fa";
import "./AccountActions.css"
import {useNavigate} from "react-router";
import {useAuth} from "../../../services/states/userState.tsx";
import {RequireRole} from "../../../components/common/RequireRole.tsx";

export function AccountActions({ style = {}, children = <FaEllipsisV /> }) {
    const navigate = useNavigate();
    const [isOpen, setIsOpen] = useState(false);
    const toggleDropdown = () => setIsOpen(!isOpen);

    const { logout } = useAuth();
    
    return (
        <div className="account-actions-wrapper" style={style}>
            <button className="dropdown-trigger-btn" onClick={toggleDropdown}>
                {children}
            </button>

            {isOpen && (
                <ul className="dropdown-menu">
                    <RequireRole>
                        <li>
                            <button className="option-btn"
                                    onClick={logout}>
                                Logout
                            </button>
                        </li>
                    </RequireRole>

                    <li>
                        <button className="option-btn" 
                                onClick={_ => navigate("/login-patient")}>
                            Login Patient
                        </button>
                    </li>
                    <li>
                        <button className="option-btn" 
                                onClick={_ => navigate("/login-doctor")}>
                            Login Doctor
                        </button>
                    </li>
                    <li>
                        <button className="option-btn" 
                                onClick={_ => navigate("/login-receptionist")}>
                            Login Receptionist
                        </button>
                    </li>
                </ul>
            )}
        </div>
    );
}
