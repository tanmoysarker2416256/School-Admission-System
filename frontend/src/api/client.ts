const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:8080';

export class ApiError extends Error {
    status: number;

    constructor(message: string, status: number) {
        super(message);
        this.status = status;
    }
}

interface RequestOptions {
    method?: string;
    body?: unknown;
    studentId?: string;
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const headers: Record<string, string> = {};
    if (options.body !== undefined) headers['Content-Type'] = 'application/json';
    if (options.studentId) headers['X-Student-Id'] = options.studentId;

    let response: Response;
    try {
        response = await fetch(`${API_URL}${path}`, {
            method: options.method ?? 'GET',
            headers,
            body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
        });
    } catch {
        throw new ApiError('Cannot reach the server. Please check your connection and try again.', 0);
    }

    if (!response.ok) {
        let message = 'Something went wrong. Please try again.';
        try {
            const data = await response.json();
            if (typeof data?.error === 'string') message = data.error;
        } catch {
            // the body was not JSON; keep the generic message
        }
        throw new ApiError(message, response.status);
    }

    if (response.status === 204) return undefined as T;
    return (await response.json()) as T;
}