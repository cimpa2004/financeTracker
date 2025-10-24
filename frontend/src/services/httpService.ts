import { enqueueSnackbar } from 'notistack';
import { z } from 'zod';

import { type CustomSnackbarOptions } from '../providers/ToastProvider.tsx';

import { handleHttpResponse, reduceZodError } from '../utils/ErrorHandling.ts';

function getErrorMessage(error: unknown, context?: { operation?: string }): { message: string; header: string } {
  if (error instanceof Error) {
    if (error.message.includes('401')) {
      return { message: 'Please check your credentials and try again', header: 'Invalid Credentials' };
    }
    if (error.message.includes('403')) {
      return { message: 'You are not authorized to access this resource', header: 'Access Denied' };
    }
    if (error.message.includes('404')) {
      return { message: 'The requested resource was not found', header: 'Not Found' };
    }
    if (error.message.includes('409')) {
      // Specific from backend
      if (error.message.includes('Cannot delete user profile with undelivered parcels')) {
        return {
          message: 'Cannot delete account while you have undelivered parcels',
          header: 'Account Deletion Blocked',
        };
      }
      return { message: 'Cannot complete this action due to a conflict', header: 'Action Blocked' };
    }
    if (error.message.includes('500')) {
      // Handle specific 500 errors based on context and message content
      if (context?.operation === 'deleteUser') {
        if (error.message.includes('An unexpected error occurred')) {
          return {
            message: 'Cannot delete account while you have undelivered parcels',
            header: 'Account Deletion Blocked',
          };
        }
      }
      return { message: 'Something went wrong on our end. Please try again later', header: 'Server Error' };
    }
    if (error.message.includes('Network')) {
      return { message: 'Please check your internet connection', header: 'Connection Error' };
    }

    return { message: error.message, header: 'Error' };
  }

  return { message: String(error), header: 'Error' };
}

function notifyError(error: unknown, context?: { operation?: string }): void {
  if (error instanceof z.ZodError) {
    const reduced = reduceZodError(error);
    enqueueSnackbar(reduced, {
      variant: 'error',
      headerMessage: 'Invalid Data',
    } as CustomSnackbarOptions);
  } else {
    const { message, header } = getErrorMessage(error, context);
    enqueueSnackbar(message, {
      variant: 'error',
      headerMessage: header,
    } as CustomSnackbarOptions);
  }
}

class HttpService {
  public baseUrl: string;
  private defaultHeaders: Record<string, string>;

  constructor(
    baseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5218/api/',
    defaultHeaders: Record<string, string> = {}
  ) {
    this.baseUrl = baseUrl;
    this.defaultHeaders = defaultHeaders;
  }

  setGlobalHeader(key: string, value: string) {
    this.defaultHeaders[key] = value;
  }

  removeGlobalHeader(key: string) {
    delete this.defaultHeaders[key];
  }

  private createRequestConfig(options: RequestInit = {}): RequestInit {
    const headers: Record<string, string> = {
      ...this.defaultHeaders,
      ...(options.headers as Record<string, string>),
    };
    return {
      ...options,
      headers,
    };
  }

  private async executeRequest(url: string, options: RequestInit): Promise<Response> {
    const config = this.createRequestConfig(options);
    return fetch(url, config);
  }

  async request<Z extends z.ZodTypeAny>(
    url: string,
    schema: Z,
    options: RequestInit = { method: 'GET' },
    context?: { operation?: string }
  ): Promise<z.infer<Z>> {
    const fullUrl = `${this.baseUrl}${url}`;

    try {
      const response = await this.executeRequest(fullUrl, options);
      return await handleHttpResponse(response, schema);
    } catch (error) {
      notifyError(error, context);
      throw error;
    }
  }

  private createBodyRequestOptions(method: string, body?: BodyInit | object): RequestInit {
    const options: RequestInit = { method };

    if (body instanceof FormData) {
      options.body = body;
    } else if (body !== undefined) {
      options.body = JSON.stringify(body);
      options.headers = { 'Content-Type': 'application/json' };
    }

    return options;
  }

  get<Z extends z.ZodTypeAny>(url: string, schema: Z, context?: { operation?: string }): Promise<z.infer<Z>> {
    return this.request(url, schema, { method: 'GET' }, context);
  }

  delete<Z extends z.ZodTypeAny>(url: string, schema: Z, context?: { operation?: string }): Promise<z.infer<Z>> {
    return this.request(url, schema, { method: 'DELETE' }, context);
  }

  post<Z extends z.ZodTypeAny>(
    url: string,
    schema: Z,
    body?: BodyInit | object,
    context?: { operation?: string }
  ): Promise<z.infer<Z>> {
    return this.request(url, schema, this.createBodyRequestOptions('POST', body), context);
  }

  put<Z extends z.ZodTypeAny>(
    url: string,
    schema: Z,
    body?: BodyInit | object,
    context?: { operation?: string }
  ): Promise<z.infer<Z>> {
    return this.request(url, schema, this.createBodyRequestOptions('PUT', body), context);
  }

  async download(path: string, options?: RequestInit, context?: { operation?: string }): Promise<Blob> {
    options = options ?? { method: 'GET' };
    const fullUrl = path.startsWith('http') ? path : `${this.baseUrl}${path}`;
    try {
      const resp = await this.executeRequest(fullUrl, options);
      if (!resp.ok) {
        const txt = await resp.text().catch(() => '');
        const err = new Error(`${resp.status} ${resp.statusText} ${txt}`);
        notifyError(err, context);
        throw err;
      }
      const blob = await resp.blob();
      return blob;
    } catch (error) {
      notifyError(error, context);
      throw error;
    }
  }
}

export const httpService = new HttpService();