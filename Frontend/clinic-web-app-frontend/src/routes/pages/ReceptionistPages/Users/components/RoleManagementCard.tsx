import { useState, useEffect } from "react";
import { identityServerApi } from "../../../../../services/api/IdentityServerApi.ts";
import { AlertMessage } from "./AccountProfileCard";
import {ROLE_MAPPING, SERVER_ROLES} from "../hooks/useAccountDetails.ts";

export function RoleManagementCard({ accountId }: { accountId: string }) {
    const [roles, setRoles] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);
    const [selectedServerRole, setSelectedServerRole] = useState(SERVER_ROLES[0] || "");
    const [isProcessing, setIsProcessing] = useState(false);
    const [processingRole, setProcessingRole] = useState<string | null>(null);
    const [actionMessage, setActionMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);

    useEffect(() => {
        if (!accountId) return;
        const fetchRoles = async () => {
            setLoading(true);
            try {
                const result = await identityServerApi.getUserRoles(accountId);
                if (result.type === "ok") setRoles(result.value || []);
            } finally {
                setLoading(false);
            }
        };
        fetchRoles();
    }, [accountId]);

    const assignRole = async (role: string) => {
        if (!accountId) return { success: false, message: "No account ID" };
        try {
            const result = await identityServerApi.assignRole(accountId, role);
            if (result.type === "ok") {
                setRoles((prev) => [...prev, role]);
                return { success: true, message: `Role assigned successfully!` };
            }
            return { success: false, message: result.error?.title || "Failed to assign role" };
        } catch {
            return { success: false, message: "Error assigning role" };
        }
    };

    const removeRole = async (role: string) => {
        if (!accountId) return { success: false, message: "No account ID" };
        try {
            const result = await identityServerApi.removeRole(accountId, role);
            if (result.type === "ok") {
                setRoles((prev) => prev.filter((r) => r !== role));
                return { success: true, message: `Role removed successfully!` };
            }
            return { success: false, message: result.error?.title || "Failed to remove role" };
        } catch {
            return { success: false, message: "Error removing role" };
        }
    };

    const availableRolesToAssign = SERVER_ROLES.filter((role) => !roles.includes(role));

    useEffect(() => {
        if (availableRolesToAssign.length > 0 && !availableRolesToAssign.includes(selectedServerRole)) {
            setSelectedServerRole(availableRolesToAssign[0]);
        }
    }, [availableRolesToAssign, selectedServerRole]);

    const handleAssign = async () => {
        if (!selectedServerRole) return;
        setIsProcessing(true);
        setActionMessage(null);

        const result = await assignRole(selectedServerRole);
        setActionMessage({ type: result.success ? "success" : "error", text: result.message });

        setIsProcessing(false);
    };

    const handleRemove = async (roleToRemove: string) => {
        setProcessingRole(roleToRemove);
        setActionMessage(null);

        const result = await removeRole(roleToRemove);
        setActionMessage({ type: result.success ? "success" : "error", text: result.message });

        setProcessingRole(null);
    };

    if (loading) return <div className="account-role-card">Loading roles...</div>;

    return (
        <div className="account-role-card">
            <h3>Manage User Roles</h3>
            {actionMessage && <AlertMessage message={actionMessage.text} type={actionMessage.type} />}

            <div className="current-roles-section">
                <span className="label">Active Roles:</span>
                <div className="roles-list">
                    {roles.length > 0 ? (
                        roles.map((serverRole) => (
                            <span key={serverRole} className="role-chip">
                                {ROLE_MAPPING[serverRole] || serverRole}
                                <button
                                    type="button"
                                    className="remove-role-btn"
                                    onClick={() => handleRemove(serverRole)}
                                    disabled={processingRole === serverRole}
                                >
                                    {processingRole === serverRole ? "…" : "✕"}
                                </button>
                            </span>
                        ))
                    ) : (
                        <span className="no-roles">No assigned roles</span>
                    )}
                </div>
            </div>

            {availableRolesToAssign.length > 0 && (
                <div className="role-selector-row">
                    <select
                        value={selectedServerRole}
                        onChange={(e) => setSelectedServerRole(e.target.value)}
                        disabled={isProcessing}
                    >
                        {availableRolesToAssign.map((serverRole) => (
                            <option key={serverRole} value={serverRole}>
                                {ROLE_MAPPING[serverRole] || serverRole}
                            </option>
                        ))}
                    </select>
                    <button type="button" className="submit-btn" onClick={handleAssign} disabled={isProcessing || !selectedServerRole}>
                        {isProcessing ? "Assigning..." : "Assign Role"}
                    </button>
                </div>
            )}
        </div>
    );
}
