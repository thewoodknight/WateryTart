namespace WateryTart.Service.MassClient.Models;

public class AudioFormat
{
    public string? content_type { get; set; }
    public string? codec_type { get; set; }
    public int sample_rate { get; set; }
    public int bit_depth { get; set; }
    public int channels { get; set; }
    public string? output_format_str { get; set; }
    public int bit_rate { get; set; }
}