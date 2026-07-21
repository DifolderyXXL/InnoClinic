import bffFetch from "../bffFetch.tsx";


export abstract class BaseApiClient {
    protected abstract readonly middlewarePath: string;

    private serialize<TRequest = null>(request: TRequest | null = null): string | null {
        return request === null ? null : JSON.stringify(request);
    }
    
    public async get<T = any>(path: string, params: Record<string, any> = {}) : Promise<Result<T>>{
        return this.request(path, { method: 'GET' }, params)
    }
    public async post<T = any, TRequest = null>(path: string, request: TRequest | null = null, params: Record<string, any> = {}) : Promise<Result<T>>{
        return this.request(path, { method: 'POST', headers: {
                "Content-Type": "application/json"
            },
            body: this.serialize(request)
        }, params)
    }

    public async put<T = any, TRequest = null>(path: string, request: TRequest | null = null, params: Record<string, any> = {}) : Promise<Result<T>>{
        return this.request(path, { method: 'PUT', headers: {
                "Content-Type": "application/json"
            },
            body: this.serialize(request)
        }, params)
    }

    public async patch<T = any, TRequest = null>(path: string, request: TRequest | null = null, params: Record<string, any> = {}) : Promise<Result<T>>{
        return this.request(path, { method: 'PATCH', headers: {
                "Content-Type": "application/json"
            },
            body: this.serialize(request)
        },params)
    }
    
    private completeUriParameters(uri: string, query: Record<string, any> = {}) : string
    {
        if(!query) return uri;
        const params = new URLSearchParams();
        for (const [key, value] of Object.entries(query))
        {
            if (value !== undefined && value !== null) {
                params.append(key, String(value));
            }
        }
        const queryString = params.toString();
        if (!queryString) return uri;
        const separator = uri.includes('?') ? '&' : '?';
        return `${uri}${separator}${queryString}`;
    }

    private async parseJsonSafe(response: Response): Promise<any | null> {
        const contentType = response.headers.get("content-type");
        if (contentType && contentType.includes("application/json")) {
            try {
                return await response.json();
            } catch {
                return null;
            }
        }
        return null;
    }
    
    public async request<T = any>(path: string, options: RequestInit = {}, params: Record<string, any> = {}): Promise<Result<T>> {
        const uri = this.completeUriParameters(`${this.middlewarePath}/${path}`, params);

        const response = await bffFetch(uri, options)

        const parseBody = async () => {
            if (response.status === 204 || response.headers.get("content-length") === "0") {
                return null;
            }
            return this.parseJsonSafe(response);
        };

        const body = await parseBody();
        
        if (!response.ok) {
            return {
                type: "error",
                error: body ?? { status: response.status },
            };
        }

        if (body === null) {
            return { type: "ok", value: {} as T };
        }

        return { type: "ok", value: body };
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