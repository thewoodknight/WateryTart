namespace WateryTart.MassClient.Models
{
    public class Artist
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
        public bool available { get; set; }
        public object image { get; set; }
    }
}
