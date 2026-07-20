export interface ClaimItem {
    type: string;
    value: string | object;
    valueType?: string | null;
}

export class User {
    public claims: ClaimItem[];
    public profile: Record<string, any>;
    public logoutUrl: string | undefined;

    constructor(claims: ClaimItem[], profile: Record<string, string | object>, logoutUrl: string | undefined) {
        this.claims = claims;
        this.profile = profile;
        this.logoutUrl = logoutUrl;
    }

    public getEmail(): string{
        return this.profile["email"];
    }

    public getRoles(): string[] {
        const roles = this.profile["role"];

        if (!roles) return [];

        return Array.isArray(roles) ? roles : [roles];
    }
}

type LoadingState = {
    status: "loading";
};

type AuthorizedState = {
    status: "authorized";
    data: User;
};

type UnauthorizedState = {
    status: "unauthorized";
    error?: Error;
};

export type AuthState = LoadingState | AuthorizedState | UnauthorizedState;