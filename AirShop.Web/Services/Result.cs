namespace AirShop.Web.Services;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsNotFound { get; set; }

    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static Result<T> Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
    public static Result<T> NotFound(string message = "Не найдено") => new() { IsSuccess = false, IsNotFound = true, ErrorMessage = message };
}