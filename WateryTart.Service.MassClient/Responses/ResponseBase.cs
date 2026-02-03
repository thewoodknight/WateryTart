namespace WateryTart.Service.MassClient.Responses;

public abstract class ResponseBase<T>
{
    public string? message_id { get; set; }

    public bool Partial { get; set; }

    public T? Result { get; set; }
}