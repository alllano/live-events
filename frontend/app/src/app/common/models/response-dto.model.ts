export interface ResponseDTO<T> {
  success: boolean;
  data?: T;
  statusCode: number;
  message?: string;
}
