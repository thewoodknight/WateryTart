namespace WateryTart.Service.MassClient.Models;

public class Streamdetails
{
    public string? provider { get; set; }
    public string? item_id { get; set; }
    public AudioFormat? audio_format { get; set; }
    public string? media_type { get; set; }
    public string? stream_type { get; set; }
    public int duration { get; set; }
    public object? size { get; set; }
    public object? stream_metadata { get; set; }
    public object? loudness { get; set; }
    public object? loudness_album { get; set; }
    public bool prefer_album_loudness { get; set; }
    public string? volume_normalization_mode { get; set; }
    public object? volume_normalization_gain_correct { get; set; }
    public double target_loudness { get; set; }

    // public Dsp dsp { get; set; }
    public object? stream_title { get; set; }
}