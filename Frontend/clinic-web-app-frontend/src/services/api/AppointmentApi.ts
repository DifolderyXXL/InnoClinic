import {BaseApiClient, type Result} from "./BaseApiClient";

export interface BookAppointmentCommand {
    doctorAccountId: string;
    officeId: string;
    date: string;
    startSlotIndex: number;
    serviceId: string | number;
    specializationId: string | number;
}

export interface DeclineCommand {
    reason: string;
}

export interface AppointmentDto {
    id: string;
    patientAccountId: string;
    doctorAccountId: string;
    reservationId: number | null;
    date: string;
    startSlotIndex: number;
    serviceId: number;
    state: string;
}

export interface PagedResponseOfAppointmentDto {
    items: AppointmentDto[];
    page: number;
    pageSize: number;
    totalCount: number;
}

export type AppointmentState = number;

export class AppointmentsApi extends BaseApiClient {
    protected override readonly middlewarePath = "/appointments";

    public async bookAppointment(command: BookAppointmentCommand): Promise<Result> {
        return this.post("api/v1/Appointments/book", command);
    }

    public async approveAppointment(id: string): Promise<Result> {
        return this.post(`api/v1/Appointments/approve-book/${id}`);
    }

    public async declineAppointment(id: string, command: DeclineCommand): Promise<Result> {
        return this.post(`api/v1/Appointments/decline-book/${id}`, command);
    }

    public async getAppointments(
        state?: AppointmentState,
        page?: number,
        pageSize?: number,
        skip?: number
    ): Promise<Result> {
        return this.get("api/v1/Appointments", 
            { state, Page: page, PageSize: pageSize, Skip: skip },
        );
    }

    public async getMyDoctorAppointments(
        state?: AppointmentState,
        page?: number,
        pageSize?: number,
        skip?: number
    ): Promise<Result> {
        return this.get("api/v1/Appointments/me/doctor", 
            { state, Page: page, PageSize: pageSize, Skip: skip },
        );
    }

    public async getMyClientAppointments(
        state?: AppointmentState,
        page?: number,
        pageSize?: number,
        skip?: number
    ): Promise<Result> {
        return this.get("api/v1/Appointments/me/client",
            { state, Page: page, PageSize: pageSize, Skip: skip },
        );
    }
}

export const appointmentsApi = new AppointmentsApi();