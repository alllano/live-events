import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../environments/environment';
import { ResponseDTO } from '../common/models/response-dto.model';

@Injectable({ providedIn: 'root' })
export class HttpBaseService {
  constructor(private readonly httpClient: HttpClient) {}

  /** Sends a GET request to the API and returns the unwrapped response data. */
  get<T>(endpoint: string, params?: Record<string, unknown>): Observable<T> {
    return this.httpClient
      .get<ResponseDTO<T>>(this.buildUrl(endpoint), { params: this.buildParams(params) })
      .pipe(map((response) => response.data as T));
  }

  /** Sends a POST request to the API and returns the unwrapped response data. */
  post<T>(endpoint: string, body: unknown): Observable<T> {
    return this.httpClient
      .post<ResponseDTO<T>>(this.buildUrl(endpoint), body)
      .pipe(map((response) => response.data as T));
  }

  /** Sends a PUT request to the API and returns the unwrapped response data. */
  put<T>(endpoint: string, body: unknown): Observable<T> {
    return this.httpClient
      .put<ResponseDTO<T>>(this.buildUrl(endpoint), body)
      .pipe(map((response) => response.data as T));
  }

  /** Sends a PATCH request to the API and returns the unwrapped response data. */
  patch<T>(endpoint: string, body?: unknown): Observable<T> {
    return this.httpClient
      .patch<ResponseDTO<T>>(this.buildUrl(endpoint), body)
      .pipe(map((response) => response.data as T));
  }

  private buildUrl(endpoint: string): string {
    const baseUrl = environment.apiUrl.replace(/\/+$/, '');
    const normalizedEndpoint = endpoint.replace(/^\/+/, '');
    return `${baseUrl}/${normalizedEndpoint}`;
  }

  private buildParams(params?: Record<string, unknown>): HttpParams {
    let httpParams = new HttpParams();

    if (!params) {
      return httpParams;
    }

    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined && value !== null) {
        httpParams = httpParams.set(key, String(value));
      }
    }

    return httpParams;
  }
}
