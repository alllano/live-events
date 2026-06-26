import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { ResponseDTO } from '../common/models/response-dto.model';

/** Centralizes HTTP error handling, turning the raw HttpErrorResponse into a friendly Error built from the backend's ResponseDTO message. */
export const httpErrorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const responseBody = error.error as ResponseDTO<unknown> | undefined;
      const message = responseBody?.message ?? 'An unexpected error occurred. Please try again.';

      return throwError(() => new Error(message));
    })
  );
};
