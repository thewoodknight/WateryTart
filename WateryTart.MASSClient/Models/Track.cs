namespace WateryTart.MassClient.Models
{    public class Track
    {
        public string item_id { get; set; }
        public string provider { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public string sort_name { get; set; }
        public string uri { get; set; }
        public List<object> external_ids { get; set; }
        public bool is_playable { get; set; }
        public object translation_key { get; set; }
        public string media_type { get; set; }
        public List<ProviderMapping> provider_mappings { get; set; }
        public Metadata metadata { get; set; }
        public bool favorite { get; set; }
        public object position { get; set; }
        public int duration { get; set; }
        public List<Artist> artists { get; set; }
        public int last_played { get; set; }
        public Album album { get; set; }
        public int disc_number { get; set; }
        public int track_number { get; set; }
    }
}
