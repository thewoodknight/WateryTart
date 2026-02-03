namespace WateryTart.Service.MassClient.Models
{
    public class Item : MediaItemBase
    {
        public object? position { get; set; }
        public string? owner { get; set; }
        public bool? is_editable { get; set; }
        public bool? available { get; set; }

        public int? duration { get; set; }
        public List<Artist>? artists { get; set; }
        public int? last_played { get; set; }
        public Album? album { get; set; }
        public int? disc_number { get; set; }
        public int? track_number { get; set; }
        public string? album_type { get; set; }
    }
}