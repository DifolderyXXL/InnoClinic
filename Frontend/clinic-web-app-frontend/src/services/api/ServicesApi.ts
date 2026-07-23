// ============ Request/Response DTOs ============
// (These types mirror the OpenAPI schemas; adjust as needed for your project)

import {BaseApiClient, type Result} from "./BaseApiClient";

export interface CreateCategoryCommand {
    categoryName: string;
    timeSlotSize: number;
}

export interface UpdateCategoryCommand {
    id: number; // int64
    categoryName: string;
    timeSlotSize: number; // uint32
}

export interface CreateSpecializationCommand {
    specializationName: string;
    isActive: boolean;
}

export interface UpdateSpecializationCommand {
    id: number; // int64
    specializationName: string;
    isActive: boolean;
}

export interface CreateServiceCommand {
    serviceName: string;
    price: number; // double
    isActive: boolean;
    categoryId: number; // int64
    specializationId: number; // int64
}

export interface UpdateServiceCommand {
    id: number; // int64
    serviceName: string;
    price: number; // double
    isActive: boolean;
    categoryId: number; // int64
    specializationId: number; // int64
}

export interface CategoryDto {
    id: number | string;
    categoryName: string;
    timeSlotSize: number | string;
}

export interface GetCategoriesResponse {
    categories: CategoryDto[];
}

export interface SpecializationDto {
    id: number | string;
    specializationName: string;
    isActive: boolean;
}

export interface GetSpecializationsResponse {
    specializations: SpecializationDto[];
}

export interface ServiceDto {
    id: number | string;
    serviceName: string;
    price: number | string;
    isActive: boolean;
    slotLength: number;
    categoryId: number | string;
    categoryName: string;
    specializationId: number | string;
    specializationName: string;
}

export interface GetServicesResponse {
    services: ServiceDto[];
}

export interface ScheduleDto {
    appointmentId: string;
    beginTime: string;
    endTime: string; 
}

export interface GetScheduleResponse {
    schedule: ScheduleDto[];
}

// ============ Services API Client ============

export class ServicesApi extends BaseApiClient {
    protected override readonly middlewarePath = "/services";

    // --------------------- Specializations ---------------------
    public async getSpecializations(onlyActive?: boolean): Promise<Result> {
        return this.get("api/v1/specializations", { onlyActive });
    }

    public async createSpecialization(
        command: CreateSpecializationCommand
    ): Promise<Result> {
        return this.post("api/v1/specializations", command);
    }

    public async updateSpecialization(
        id: number,
        command: UpdateSpecializationCommand
    ): Promise<Result> {
        return this.put(`api/v1/specializations/${id}`, command);
    }

    public async deleteSpecialization(id: number): Promise<Result> {
        return this.delete(`api/v1/specializations/${id}`);
    }

    // --------------------- Services ---------------------
    public async getServices(
        categoryId?: number | string,
        specializationId?: number | string
    ): Promise<Result> {
        return this.get("api/v1/services", { categoryId, specializationId } );
    }

    public async createService(command: CreateServiceCommand): Promise<Result> {
        return this.post("api/v1/services", command);
    }

    public async updateService(
        id: number,
        command: UpdateServiceCommand
    ): Promise<Result> {
        return this.put(`api/v1/services/${id}`, command);
    }

    public async deleteService(id: number): Promise<Result> {
        return this.delete(`api/v1/services/${id}`);
    }

    // --------------------- Categories ---------------------
    public async getCategories(): Promise<Result> {
        return this.get("api/v1/categories");
    }

    public async createCategory(command: CreateCategoryCommand): Promise<Result> {
        return this.post("api/v1/categories", command);
    }

    public async updateCategory(
        id: number,
        command: UpdateCategoryCommand
    ): Promise<Result> {
        return this.put(`api/v1/categories/${id}`, command);
    }

    public async deleteCategory(id: number): Promise<Result> {
        return this.delete(`api/v1/categories/${id}`);
    }

    // --------------------- Schedules ---------------------
    public async getScheduleTodayMe(): Promise<Result> {
        return this.get("api/v1/schedules/today/me");
    }

    public async getScheduleMe(date: string): Promise<Result> {
        return this.get("api/v1/schedules/me", { date });
    }

    public async getScheduleById(id: string, date: string): Promise<Result> {
        return this.get(`api/v1/schedules/${id}`, { date });
    }

    public async getScheduleTodayById(id: string): Promise<Result> {
        return this.get(`api/v1/schedules/today/${id}`);
    }

    public async getAvailableDoctorSlots(doctorId: string, date: Date): Promise<Result> {

        const dateStr = dateToDateOnly(date);
        return this.get(`api/v1/schedules/doctor/${doctorId}`, { dateOnly: dateStr });
    }
}

export function dateToDateOnly(date: Date) : string{
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

export const servicesApi = new ServicesApi();


export class DateOnly {
    private readonly value: string;

    private constructor(dateStr: string) {
        this.value = dateStr;
    }

    static fromString(dateStr: string): DateOnly {
        if (!/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) {
            throw new Error(`Invalid DateOnly format: "${dateStr}". Must be YYYY-MM-DD.`);
        }
        return new DateOnly(dateStr);
    }
    static fromNativeDate(date: Date): DateOnly {
        const yyyy = date.getFullYear();
        const mm = String(date.getMonth() + 1).padStart(2, '0');
        const dd = String(date.getDate()).padStart(2, '0');
        return new DateOnly(`${yyyy}-${mm}-${dd}`);
    }

    toString(): string {
        return this.value;
    }
}

export interface AvailablePositionsOnDay{
    dayBeginTime: TimeSpan;
    dayEndTime: TimeSpan;
    timeSlotLength: TimeSpan;
    slotAmount: number;
    availableTimeWindows: TimeSlotWindow[];
}

export interface TimeSlotWindow{
    timeSlotStart: number;
    timeSlotSize: number;
    beginTime: TimeSpan;
    endTime: TimeSpan;
}


export type TimeSpan = string & { readonly __brand: unique symbol };

export function toTimeSpan(timeStr: string): TimeSpan {
    if (!/^\d{2}:\d{2}(:\d{2})?$/.test(timeStr)) {
        throw new Error(`Invalid TimeSpan format: "${timeStr}". Expected HH:mm:ss or HH:mm`);
    }
    return timeStr as TimeSpan;
}
export function timeSpanToMinutes(ts: TimeSpan): number {
    const parts = ts.split(':').map(Number);
    if (parts.length === 2) return parts[0] * 60 + parts[1];
    return parts[0] * 60 + parts[1] + parts[2] / 60;
}

export function minutesToTimeSpan(minutes: number): TimeSpan {
    const h = Math.floor(minutes / 60);
    const m = Math.floor(minutes % 60);
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}` as TimeSpan;
}

export function getHourFromMinutes(minutes: number): number {
    return Math.floor(minutes / 60);
}
export function getSlotsInHour(slotLength: TimeSpan): number {
    const lengthMinutes = timeSpanToMinutes(slotLength);
    
    return Math.floor(60 / lengthMinutes);
}