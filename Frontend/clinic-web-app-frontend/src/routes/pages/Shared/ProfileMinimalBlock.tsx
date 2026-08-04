import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../../services/states/userState.tsx";
import { AvatarFromSource } from "./Avatar.tsx";
import { profilesApi } from "../../../services/api/ProfilesApi.ts";
import { AccountActions } from "./AccountActions.tsx";
import "./ProfileMinimalBlock.css";

export const ProfileMinimalBlock: React.FC = () => {
    const { state } = useAuth();
    const [photoUrl, setPhotoUrl] = useState<string | null>(null);

    useEffect(() => {
        if (state.status === "authorized") {
            profilesApi
                .getAccountMe()
                .then((result) => {
                    if (result.type === "ok") {
                        setPhotoUrl(result.value.photoUrl);
                    }
                })
                .catch(() => setPhotoUrl(null));
        } else {
            setPhotoUrl(null);
        }
    }, [state.status]);

    if (state.status === "loading") {
        return <div className="profile-minimal-skeleton" />;
    }

    if (state.status === "authorized") {
        const email = state.data.getEmail();
        const roles = state.data.getRoles();
        const rolesString = Array.isArray(roles) ? roles.join(", ") : String(roles);

        return (
            <div className="profile-minimal-block">
                <Link to="/profile" className="profile-link">
                    <AvatarFromSource
                        TextIfPhotoNull={email?.[0]?.toUpperCase() ?? "?"}
                        PhotoUrl={photoUrl}
                    />
                    <div className="profile-user-info">
                        <span className="profile-email">{email}</span>
                        <span className="profile-role">
                            Role: <strong>{rolesString}</strong>
                        </span>
                    </div>
                </Link>

                <AccountActions className="profile-actions-wrapper" />
            </div>
        );
    }

    if (state.status === "unauthorized") {
        return (
            <div className="profile-minimal-unauthorized">
                <AccountActions>
                    <span className="login-btn-text">LOGIN</span>
                </AccountActions>
            </div>
        );
    }

    return null;
};