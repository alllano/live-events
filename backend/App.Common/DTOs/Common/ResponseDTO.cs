namespace App.Common.DTOs.Common;

public class ResponseDTO<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }

    public static ResponseDTO<T> AsResponseDTO(T? data, int statusCode, bool success = true, string? message = null)
    {
        return new ResponseDTO<T>
        {
            Success = success,
            Data = data,
            StatusCode = statusCode,
            Message = message
        };
    }
}
