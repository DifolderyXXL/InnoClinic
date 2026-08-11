import { BaseApiClient, type Result } from "./BaseApiClient";

export class IdentityServerApi extends BaseApiClient {
    protected override readonly middlewarePath = "/identity";

    /**
     * Назначает роль пользователю
     * POST /users/{userId}/{role}
     */
    public async assignRole(userId: string, role: string): Promise<Result<void>> {
        return this.post(`api/v1/users/${encodeURIComponent(userId)}/${encodeURIComponent(role)}`);
    }

    /**
     * Удаляет роль у пользователя
     * DELETE /users/{userId}/{role}
     */
    public async removeRole(userId: string, role: string): Promise<Result<void>> {
        return this.delete(`api/v1/users/${encodeURIComponent(userId)}/${encodeURIComponent(role)}`);
    }

    /**
     * Получает список ролей конкретного пользователя
     * GET /users/{userId}/roles
     */
    public async getUserRoles(userId: string): Promise<Result<string[]>> {
        return this.get(`api/v1/users/${encodeURIComponent(userId)}/roles`);
    }
}

export const identityServerApi = new IdentityServerApi();