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
    officeId: string;
    serviceId: number;
    specializationId: number;
    doctorFullName: string;
    patientFullName: string;
    serviceName: string;
    reservationId: number | null;
    date: string;
    startSlotIndex: number;
    beginTime: string | null;
    endTime: string | null;
    state: string;
}

export interface PagedResponseOfAppointmentDto {
    items: AppointmentDto[];
    page: number;
    pageSize: number;
    totalCount: number;
}

export const AppointmentState = {
    Created: 0,
    PendingReservation: 1,
    PendingApproval: 2,
    Approved: 3,
    Failed: 4,
    Confirmed: 5
} as const;

export type AppointmentState = typeof AppointmentState[keyof typeof AppointmentState];

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
        patientId?: string,
        page?: number,
        pageSize?: number
    ): Promise<Result> {
        return this.get("api/v1/Appointments", 
            { state, Page: page, PageSize: pageSize, patientId },
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

    public async getMyClientAppointmentById(id: string): Promise<Result> {
        return this.get(`api/v1/Appointments/${id}/me/client`);
    }

    public async getMyDoctorAppointmentById(id: string): Promise<Result> {
        return this.get(`api/v1/Appointments/${id}/me/doctor`);
    }

    // --------------------- Schedules ---------------------
    public async getScheduleTodayMe(): Promise<Result> {
        return this.get("api/v1/Schedule/today/me");
    }

    public async getScheduleMe(date: string): Promise<Result> {
        return this.get("api/v1/Schedule/me", { date });
    }

    public async getScheduleById(id: string, date: string): Promise<Result> {
        return this.get(`api/v1/Schedule/${id}`, { date });
    }

    public async getScheduleTodayById(id: string): Promise<Result> {
        return this.get(`api/v1/Schedule/today/${id}`);
    }

    public async getClinicAppointments(
        filter: ClinicAppointmentsFilterParameters
    ): Promise<Result<PagedResponseOfAppointmentDto>> {
        return this.get("api/v1/Appointments/clinic", {
            Date: filter.date,
            DoctorFullName: filter.doctorFullName,
            ServiceName: filter.serviceName,
            Status: filter.status,
            OfficeId: filter.officeId,
            Page: filter.page,
            PageSize: filter.pageSize,
        });
    }
}

export const appointmentsApi = new AppointmentsApi();

export interface ClinicAppointmentsFilterParameters {
    date?: string | null;            // Формат YYYY-MM-DD
    doctorFullName?: string | null;
    serviceName?: string | null;
    status?: AppointmentState | null;
    officeId?: string | null;
    page?: number;
    pageSize?: number;
}