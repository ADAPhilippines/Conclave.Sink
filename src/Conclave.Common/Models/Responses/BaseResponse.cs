namespace Conclave.Common.Models.Responses;

public class BaseResponse<T>
{
    public string Message { get; init; } = string.Empty;
    public bool IsSucess { get; init; }
    public T Result { get; init; } = default!;
}