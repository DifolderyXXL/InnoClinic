import { useState, useRef, useEffect, type ReactNode, type CSSProperties } from "react";
import { FaEllipsisV } from "react-icons/fa";
import { useNavigate } from "react-router";
import { useAuth } from "../../../services/states/userState.tsx";
import { RequireRole } from "../../../components/common/RequireRole.tsx";
import "./AccountActions.css";

interface AccountActionsProps {
    style?: CSSProperties;
    className?: string;
    children?: ReactNode;
}

export function AccountActions({ style, className = "", children = <FaEllipsisV /> }: AccountActionsProps) {
    const navigate = useNavigate();
    const { logout } = useAuth();
    const [isOpen, setIsOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    const toggleDropdown = () => setIsOpen((prev) => !prev);
    const closeDropdown = () => setIsOpen(false);

    // Close menu on click outside
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                closeDropdown();
            }
        };

        if (isOpen) {
            document.addEventListener("mousedown", handleClickOutside);
        }

        return () => {
            document.removeEventListener("mousedown", handleClickOutside);
        };
    }, [isOpen]);

    const handleNavigation = (path: string) => {
        closeDropdown();
        navigate(path);
    };

    const handleLogout = () => {
        closeDropdown();
        logout();
    };

    return (
        <div className={`account-actions-wrapper ${className}`.trim()} style={style} ref={dropdownRef}>
            <button
                type="button"
                className="dropdown-trigger-btn"
                onClick={toggleDropdown}
                aria-expanded={isOpen}
            >
                {children}
            </button>

            {isOpen && (
                <ul className="dropdown-menu">
                    <RequireRole>
                        <li>
                            <button
                                type="button"
                                className="option-btn logout-btn"
                                onClick={handleLogout}
                            >
                                Logout
                            </button>
                        </li>
                    </RequireRole>

                    <li>
                        <button
                            type="button"
                            className="option-btn"
                            onClick={() => handleNavigation("/login-patient")}
                        >
                            Login Patient
                        </button>
                    </li>
                    <li>
                        <button
                            type="button"
                            className="option-btn"
                            onClick={() => handleNavigation("/login-doctor")}
                        >
                            Login Doctor
                        </button>
                    </li>
                    <li>
                        <button
                            type="button"
                            className="option-btn"
                            onClick={() => handleNavigation("/login-receptionist")}
                        >
                            Login Receptionist
                        </button>
                    </li>
                </ul>
            )}
        </div>
    );
}