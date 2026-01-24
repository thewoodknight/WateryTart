namespace WateryTart.MassClient.Models;

public class Album
{
    public string item_id { get; set; }
    public string provider { get; set; }
    public string name { get; set; }
    public string version { get; set; }
    public string sort_name { get; set; }
    public string uri { get; set; }
    public List<List<string>> external_ids { get; set; }
    public bool is_playable { get; set; }
    public object translation_key { get; set; }
    public string media_type { get; set; }
    public List<ProviderMapping> provider_mappings { get; set; }
    public Metadata metadata { get; set; }
    public bool favorite { get; set; }
    public object position { get; set; }
    public int? year { get; set; }
    public List<Artist> artists { get; set; }
    public string album_type { get; set; }
}