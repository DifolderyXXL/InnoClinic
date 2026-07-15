import {BaseApiClient, type Result} from "./BaseApiClient.ts";

export class ProfilesApi extends BaseApiClient {
    protected override readonly middlewarePath = "/profiles";
    
    public async getMyProfiles(): Promise<Result>{
        return this.request("api/v1/profiles/me", { method: 'GET' })
    }

    public async createMyProfiles(date: string): Promise<Result>{
        return this.request("api/v1/patients/me", { method: 'POST', headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ dateOfBirth: date }) })
    }
}

export const profilesApi = new ProfilesApi();