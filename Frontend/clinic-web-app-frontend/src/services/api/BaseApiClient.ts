import bffFetch from "../bffFetch.tsx";


export abstract class BaseApiClient {
    protected abstract readonly middlewarePath: string;

    public async request<T = any>(path: string, options: RequestInit = {}): Promise<Result<T>> {

        const uri = `${this.middlewarePath}/${path}`;

        const response = await bffFetch(uri, options)
        

        if (!response.ok) {
            const errorData = await response.json().catch(() => null);

            return {
                type: "error",
                error: errorData ?? {
                    status: response.status,
                }
            };
        }

        if (response.status === 204 || response.headers.get("content-length") === "0") {
            return {type: "ok", value: {} as T};
        }

        return {
            type: "ok",
            value: await response.json()
        }
    }
}

type ErrorResult = {
    type: "error";
    error: any
};
type OkResult<T> = {
    type: "ok";
    value: T
};
export type Result<T=any> = ErrorResult | OkResult<T>;