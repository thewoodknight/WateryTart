namespace WateryTart.MassClient.Models;

public class ProviderMapping
{
    public string item_id { get; set; }
    public string provider_domain { get; set; }
    public string provider_instance { get; set; }
    public bool? available { get; set; }
    public bool? in_library { get; set; }
    public object is_unique { get; set; }
    public AudioFormat audio_format { get; set; }
    public string url { get; set; }
    public object details { get; set; }
}