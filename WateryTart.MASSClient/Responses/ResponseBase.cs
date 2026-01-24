namespace WateryTart.MassClient.Responses;

public abstract class ResponseBase
{
    public string message_id { get; set; }
    //public ResultBase result { get; set; }
    public bool partial { get; set; }

}