import {BaseApiClient, type Result} from "./BaseApiClient.ts";


interface UpdateAccountCommand {
    firstName?: string | null;
    lastName?: string | null;
    middleName?: string | null;
    phoneNumber?: string | null;
    photoId?: string | null;
}

interface AccountRequest {
    firstName: string;
    lastName: string;
    middleName: string | null;
    phoneNumber: string | null;
}


interface CreateDoctorCommand {
    accountId: string;
    dateOfBirth: string;
    careerStartYear: number;
    specializationId: number;
    status: number;
    officeId: number;
}

interface UpdateDoctorProfileCommand {
    id: number;
    firstName: string;
    lastName: string;
    middleName: string | null;
    dateOfBirth: string;
    careerStartYear: number;
    specializationId: number;
    status: number;
    officeId: number;
}

export class ProfilesApi extends BaseApiClient {
    protected override readonly middlewarePath = "/profiles";

    public async getMyProfiles(): Promise<Result> {
        return this.get("api/v1/profiles/me");
    }

    public async createPatientMe(date: string): Promise<Result> {
        return this.post("api/v1/patients/me", { dateOfBirth: date });
    }
    
    public async createDoctorMe(date: string): Promise<Result> {
        return this.post("api/v1/doctors/me", { dateOfBirth: date });
    }
    
    public async updateRole(userId: string, role: string): Promise<Result> {
        return this.put("api/v1/accounts/role", { userId, role });
    }
    
    public async getReceptionists(page: number = 1, pageSize: number = 50): Promise<Result> {
        return this.get("api/v1/receptionists", { params: { Page: page, PageSize: pageSize } });
    }

    public async getPatients(page: number = 1, pageSize: number = 50): Promise<Result> {
        return this.get("api/v1/patients", { params: { Page: page, PageSize: pageSize } });
    }
    
    public async getDoctors(page: number = 1, pageSize: number = 50): Promise<Result> {
        return this.get("api/v1/doctors", { params: { Page: page, PageSize: pageSize } });
    }

    public async getDoctorById(id: string): Promise<Result> {
        return this.get(`api/v1/doctors/${id}`);
    }

    public async createDoctor(id: string, data: CreateDoctorCommand): Promise<Result> {
        return this.post(`api/v1/doctors/${id}`, data);
    }
    
    public async updateDoctor(id: number, data: UpdateDoctorProfileCommand): Promise<Result> {
        return this.put(`api/v1/doctors/${id}`, data);
    }

    public async updateDoctorMe(data: UpdateDoctorProfileCommand): Promise<Result> {
        return this.put("api/v1/doctors/me", data);
    }
    
    public async getAccounts(page: number = 1, pageSize: number = 50): Promise<Result> {
        return this.get("api/v1/accounts", { params: { Page: page, PageSize: pageSize } });
    }

    public async createAccountMe(data: AccountRequest): Promise<Result> {
        return this.post("api/v1/accounts/me", data);
    }

    public async getAccountMe(): Promise<Result> {
        return this.get("api/v1/accounts/me");
    }

    public async updateAccountMe(data: UpdateAccountCommand): Promise<Result> {
        return this.put("api/v1/accounts/me", data);
    }
}

export const profilesApi = new ProfilesApi();