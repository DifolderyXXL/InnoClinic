import { BaseApiClient, type Result } from "./BaseApiClient";

// ============ Request/Response DTOs ============

export interface UserFullName {
    firstName: string;
    lastName: string;
    middleName?: string | null;
}

export interface CreateMedicalResultRequest {
    userId?: string;
    doctorName?: UserFullName;
    specialization?: string;
    serviceName?: string;
    patientName?: UserFullName;
    patientDateOfBirth?: string; // YYYY-MM-DD
    complaints?: string;
    conclusion?: string;
    diagnosis?: string;
    recommendations?: string;
}

export interface MedicalResultBody {
    complaints?: string;
    conclusion?: string;
    diagnosis?: string;
    recommendations?: string;
}

export interface PhotoCreatedResponse {
    photoId: string;
}

// ============ Documents API Client ============

export class DocumentsApi extends BaseApiClient {
    protected override readonly middlewarePath = "/documents";

    // --------------------- Medical Results ---------------------

    /**
     * Export medical result for current user by appointment ID
     */
    public async exportMyMedicalResult(appointmentId: string): Promise<Result> {
        return this.get(`api/v1/MedicalResults/appointments/${appointmentId}/me/export`);
    }

    public async exportUserMedicalResult(appointmentId: string, userId: string): Promise<Result> {
        return this.get(`api/v1/MedicalResults/appointments/${appointmentId}/users/${userId}/export`);
    }
    

    /**
     * Get medical result for current user by appointment ID
     */
    public async getMyMedicalResult(appointmentId: string): Promise<Result> {
        return this.get(`api/v1/MedicalResults/appointments/${appointmentId}/me`);
    }

    public async getUserMedicalResult(appointmentId: string, userId: string): Promise<Result> {
        return this.get(`api/v1/MedicalResults/appointments/${appointmentId}/users/${userId}`);
    }

    /**
     * Create medical result for an appointment
     */
    public async createMedicalResult(
        appointmentId: string,
        request: CreateMedicalResultRequest
    ): Promise<Result> {
        return this.post(`api/v1/MedicalResults/appointments/${appointmentId}`, request);
    }

    /**
     * Update medical result for an appointment
     */
    public async updateMedicalResult(
        appointmentId: string,
        body: MedicalResultBody
    ): Promise<Result> {
        return this.put(`api/v1/MedicalResults/appointments/${appointmentId}`, body);
    }

    // --------------------- Photos ---------------------

    /**
     * Get office avatar photo
     */
    public async getOfficeAvatar(officeId: string, photoId: string): Promise<Result> {
        return this.get(`api/v1/Photos/offices/${officeId}/avatar/${photoId}`);
    }

    /**
     * Upload office avatar photo
     */
    public async uploadOfficeAvatar(officeId: string, file: File | Blob): Promise<Result> {
        const formData = new FormData();
        formData.append("file", file);
        return this.post(`api/v1/Photos/offices/${officeId}/avatar`, formData);
    }

    /**
     * Confirm office avatar change
     */
    public async confirmOfficeAvatar(
        officeId: string,
        photoId?: string,
        oldPhotoId?: string
    ): Promise<Result> {
        return this.post(`api/v1/Photos/offices/${officeId}/avatar/confirm`, null, {
            photoId,
            oldPhotoId,
        });
    }

    /**
     * Get doctor avatar photo
     */
    public async getDoctorAvatar(doctorId: string, photoId: string): Promise<Result> {
        return this.get(`api/v1/Photos/doctors/${doctorId}/avatar/${photoId}`);
    }

    /**
     * Get user avatar photo
     */
    public async getUserAvatar(photoId: string): Promise<Result> {
        return this.get(`api/v1/Photos/users/avatar/${photoId}`);
    }

    /**
     * Upload user avatar photo
     */
    public async uploadUserAvatar(file: File | Blob): Promise<Result> {
        const formData = new FormData();
        formData.append("file", file);
        return this.post(`api/v1/Photos/users/avatar`, formData);
    }
}

export const documentsApi = new DocumentsApi();