import {BaseApiClient, type Result} from "./BaseApiClient.ts";

export interface OfficeDto {
    id: string;
    photoId: string | null;
    city: string;
    street: string;
    houseNumber: string;
    officeNumber: string | null;
    registryPhoneNumber: string;
    isActive: boolean;
}

export interface CreateOfficeCommand {
    photoId?: string | null;
    city: string;
    street: string;
    houseNumber: string;
    officeNumber?: string | null;
    registryPhoneNumber: string;
    isActive: boolean;
}

export interface UpdateOfficeCommand {
    photoId?: string | null;
    city?: string | null;
    street?: string | null;
    houseNumber?: string | null;
    officeNumber?: string | null;
    registryPhoneNumber?: string | null;
    isActive?: boolean | null;
}

export interface GetOfficesResponse {
    offices: OfficeDto[];
}

export class OfficesApi extends BaseApiClient {
    protected override readonly middlewarePath = "/offices";

    public async getOffices(page?: number, pageSize?: number): Promise<Result> {
        return this.get("api/v1/offices", {
            params: { Page: page, PageSize: pageSize },
        });
    }

    public async getOffice(id: string): Promise<Result> {
        return this.get(`api/v1/offices/${id}`);
    }

    public async createOffice(command: CreateOfficeCommand): Promise<Result> {
        return this.post("api/v1/offices", command);
    }

    public async updateOffice(id: string, command: UpdateOfficeCommand): Promise<Result> {
        return this.put(`api/v1/offices/${id}`, command);
    }

    public async patchOffice(id: string, command: UpdateOfficeCommand): Promise<Result> {
        return this.patch(`api/v1/offices/${id}`, command);
    }
}

export const officesApi = new OfficesApi();