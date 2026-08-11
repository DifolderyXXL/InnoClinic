import {BaseApiClient, type Result} from "./BaseApiClient.ts";


interface UpdateAccountCommand {
    firstName?: string | null;
    lastName?: string | null;
    middleName?: string | null;
    phoneNumber?: string | null;
    photoId?: string | null;
}

export interface AccountRequest {
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
    officeId: string;
}

interface UpdateDoctorProfileCommand {
    id: number;
    dateOfBirth: string;
    careerStartYear: number;
    specializationId: number;
    status: number;
    officeId: string;
}

export interface AdminCreateAccountRequest {
    email: string;
    firstName: string;
    lastName: string;
    middleName?: string | null;
    phoneNumber?: string | null;
    roles: string[];
}

export class ProfilesApi extends BaseApiClient {
    protected override readonly middlewarePath = "/profiles";

    public async getMyProfiles(): Promise<Result> {
        return this.get("api/v1/profiles/me");
    }

    public async getProfiles(userId: string): Promise<Result> {
        return this.get(`api/v1/profiles/${userId}`);
    }


    public async createPatientMe(date: string): Promise<Result> {
        return this.post("api/v1/patients/me", { dateOfBirth: date });
    }

    public async updatePatientMe(date: string): Promise<Result> {
        return this.put("api/v1/patients/me", { dateOfBirth: date });
    }

    public async createPatient(userId: string,date: string): Promise<Result> {
        return this.post(`api/v1/patients/${userId}`, { dateOfBirth: date });
    }

    public async updatePatient(userId: string, date: string): Promise<Result> {
        return this.put(`api/v1/patients/${userId}`, { dateOfBirth: date });
    }

    public async createReceptionist(userId: string, officeId: string): Promise<Result> {
        return this.post(`api/v1/receptionists/${userId}`, { officeId: officeId });
    }

    public async updateReceptionist(userId: string, officeId: string): Promise<Result> {
        return this.put(`api/v1/receptionists/${userId}`, { officeId: officeId });
    }
    
    
    public async createDoctorMe(date: string): Promise<Result> {
        return this.post("api/v1/doctors/me", { dateOfBirth: date });
    }

    public async updateRole(userId: string, role: string, action: string): Promise<Result> {
        return this.put("api/v1/accounts/role", { userId, role, action });
    }
    
    public async getReceptionists(page: number = 1, pageSize: number = 50): Promise<Result> {
        return this.get("api/v1/receptionists", { Page: page, PageSize: pageSize } );
    }

    public async getPatients(page: number = 1, pageSize: number = 50): Promise<Result> {
        return this.get("api/v1/patients", { Page: page, PageSize: pageSize });
    }

    public async getPatient(id: string): Promise<Result> {
        return this.get(`api/v1/patients/${id}`);
    }

    public async getDoctors(params?: {
        status?: string;
        officeIds?: string[];
        specializationIds?: number[];
        fullName?: string;
        page?: number;
        pageSize?: number;
    }): Promise<Result> {
        return this.get("api/v1/doctors", 
            {
                Page: params?.page ?? 1,
                PageSize: params?.pageSize ?? 50,
                SpecializationIds: params?.specializationIds,
                OfficeIds: params?.officeIds,
                FullName: params?.fullName,
                Status: params?.status,
            }
        );
    }
    public async getDoctorById(id: string): Promise<Result> {
        return this.get(`api/v1/doctors/${id}`);
    }

    public async createDoctor(id: string, data: CreateDoctorCommand): Promise<Result> {
        return this.post(`api/v1/doctors/${id}`, data);
    }
    
    public async updateDoctor(id: string, data: UpdateDoctorProfileCommand): Promise<Result> {
        return this.put(`api/v1/doctors/${id}`, data);
    }

    public async updateDoctorMe(data: UpdateDoctorProfileCommand): Promise<Result> {
        return this.put("api/v1/doctors/me", data);
    }
    
    public async getAccounts(page: number = 1, pageSize: number = 50): Promise<Result> {
        return this.get("api/v1/accounts", { Page: page, PageSize: pageSize });
    }
    public async getAccount(userId: string): Promise<Result> {
        return this.get(`api/v1/accounts/${userId}`);
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

    public async updateAccount(userId: string, data: UpdateAccountCommand): Promise<Result> {
        return this.put(`api/v1/accounts/${userId}`, data);
    }

    public async createAccount(data: AdminCreateAccountRequest): Promise<Result> {
        return this.post("api/v1/accounts", data);
    }
}

export const profilesApi = new ProfilesApi();