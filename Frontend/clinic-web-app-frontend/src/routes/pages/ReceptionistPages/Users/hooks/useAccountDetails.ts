import { useState, useEffect } from "react";
import { profilesApi } from "../../../../../services/api/ProfilesApi.ts";
import { identityServerApi } from "../../../../..//services/api/IdentityServerApi.ts";
import type {AccountDto} from "../AccountsPage.tsx";

export const ROLE_MAPPING: Record<string, string> = {
    client: "Patient",
    doctor: "Doctor",
    receptionist: "Receptionist",
};

export const SERVER_ROLES = Object.keys(ROLE_MAPPING);

// --- HOOKS ---

export function useAccountDetails(accountId: string | null) {
    const [account, setAccount] = useState<AccountDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchAccount = async () => {
        if (!accountId) {
            setError("Account ID is missing");
            setLoading(false);
            return;
        }

        setLoading(true);
        try {
            const result = await profilesApi.getAccount(accountId);
            if (result.type === "ok") {
                setAccount(result.value);
            } else {
                setError(result.error?.title || "Failed to load account");
            }
        } catch {
            setError("An unexpected error occurred");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchAccount();
    }, [accountId]);

    const updateAccount = async (data: Partial<AccountDto>) => {
        if (!accountId) return { success: false, message: "No account ID" };
        try {
            const result = await profilesApi.updateAccount(accountId, data);
            if (result.type === "ok") {
                await fetchAccount();
                return { success: true, message: "Account details updated successfully!" };
            }
            return { success: false, message: result.error?.title || "Failed to update account" };
        } catch {
            return { success: false, message: "An unexpected error occurred" };
        }
    };

    return { account, loading, error, updateAccount, fetchAccount };
}

export function useAccountRoles(accountId: string | null) {
    const [roles, setRoles] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);

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

    return { roles, loading, assignRole, removeRole };
}

export interface UserProfilesDto {
    onlyPatient: { dateOfBirth: string } | null;
    onlyDoctor: { dateOfBirth: string; careerStartYear: number; specializationId: number; officeId: string; status: number } | null;
    onlyReceptionist: { officeId: string } | null;
}

export function useUserProfiles(accountId: string | null) {
    const [profiles, setProfiles] = useState<UserProfilesDto | null>(null);
    const [loading, setLoading] = useState(true);

    const fetchProfiles = async () => {
        if (!accountId) return;
        setLoading(true);
        try {
            const result = await profilesApi.getProfiles(accountId);
            if (result.type === "ok") {
                setProfiles(result.value);
            }
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchProfiles();
    }, [accountId]);

    return { profiles, loading, fetchProfiles };
}
