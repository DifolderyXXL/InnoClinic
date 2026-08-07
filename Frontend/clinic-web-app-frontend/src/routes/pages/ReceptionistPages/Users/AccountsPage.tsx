import { useCallback } from "react";
import { Link } from "react-router";
import { PaginatedListView, type PaginatedResult } from "../../common/PaginatedListView.tsx";
import { profilesApi } from "../../../../services/api/ProfilesApi.ts";
import { AvatarFromSource } from "../../Shared/Avatar.tsx";
import "./AccountsPage.css";
import {RouteLinkCard} from "../../../../components/RouteLinkCard.tsx";

const PAGE_SIZE = 50;

export interface AccountDto {
    id: string;
    email: string;
    phoneNumber?: string | null;
    isEmailVerified: boolean;
    firstName?: string | null;
    lastName?: string | null;
    middleName?: string | null;
    photoId?: string | null;
    photoUrl?: string | null;
    createdAt: string;
    updatedAt: string;
}

export function AccountsPage() {
    const fetchAccounts = useCallback(
        async (page: number): Promise<PaginatedResult<AccountDto>> => {
            try {
                const result = await profilesApi.getAccounts(page, PAGE_SIZE);

                if (result.type === "ok") {
                    const data = result.value;
                    const items = data?.items ?? (Array.isArray(data) ? data : []);
                    const total = data?.total ?? items.length;

                    return {
                        items: Array.isArray(items) ? items : [],
                        total: typeof total === "number" ? total : 0,
                    };
                }

                return {
                    items: [],
                    total: 0,
                    error: result.error?.title || result.error?.message || "Failed to load accounts",
                };
            } catch (err: any) {
                return {
                    items: [],
                    total: 0,
                    error: err?.message || "An unexpected error occurred while loading accounts",
                };
            }
        },
        []
    );

    return (
        <div className="accounts-page">
            <header className="page-header">
                <h1>Accounts</h1>
            </header>
            
            <RouteLinkCard to={"create"}>Create user</RouteLinkCard>

            <PaginatedListView<AccountDto>
                pageSize={PAGE_SIZE}
                fetchRequest={fetchAccounts}
                renderItems={(items) => (
                    <div className="accounts-grid">
                        {items.map((account) => (
                            <AccountCard key={account.id} account={account} />
                        ))}
                    </div>
                )}
            />
        </div>
    );
}

interface AccountCardProps {
    account: AccountDto;
}

export function AccountCard({ account }: AccountCardProps) {
    const fullName = [account.lastName, account.firstName, account.middleName]
        .filter(Boolean)
        .join(" ") || "No Name";

    const initial = (account.firstName?.[0] || account.email?.[0] || "U").toUpperCase();

    return (
        <Link to={`/accounts/details?id=${account.id}`} className="account-card-link">
            <div className="account-card">
                <div className="account-card-header">
                    <AvatarFromSource
                        PhotoUrl={account.photoUrl}
                        TextIfPhotoNull={initial}
                    />
                    <div className="account-header-info">
                        <h3 className="account-name" title={fullName}>{fullName}</h3>
                        <span className="account-email" title={account.email}>{account.email}</span>
                    </div>
                </div>

                <div className="account-card-body">
                    <div className="account-info-row">
                        <span className="label">Phone:</span>
                        <span className="value">{account.phoneNumber || "—"}</span>
                    </div>

                    <div className="account-info-row">
                        <span className="label">Status:</span>
                        <span className={`status-badge ${account.isEmailVerified ? "verified" : "unverified"}`}>
                            {account.isEmailVerified ? "Verified" : "Unverified"}
                        </span>
                    </div>
                </div>
            </div>
        </Link>
    );
}