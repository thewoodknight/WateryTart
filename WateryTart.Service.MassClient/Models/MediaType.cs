using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace WateryTart.Service.MassClient.Models;

[JsonConverter(typeof(StringEnumConverter))]
public enum MediaType
{
    Artist,
    Album,
    Track,
    Radio,
    Playlist,
    Audiobook,
    Podcast,
    PodcastEpisode,
    Genre,
    Folder
}

[JsonConverter(typeof(StringEnumConverter))]
public enum EventType
{
    [EnumMember(Value = "player_added")] PlayerAdded,
    [EnumMember(Value = "player_updated")] PlayerUpdated,
    [EnumMember(Value = "player_removed")] PlayerRemoved,
    [EnumMember(Value = "player_config_updated")] PlayerConfigUpdated,
    [EnumMember(Value = "queue_added")] QueueAdded,
    [EnumMember(Value = "queue_updated")] QueueUpdated,
    [EnumMember(Value = "queue_items_updated")] QueueItemsUpdated,
    [EnumMember(Value = "queue_time_updated")] QueueTimeUpdated,
    [EnumMember(Value = "media_item_added")] MediaItemAdded,
    [EnumMember(Value = "media_item_updated")] MediaItemUpdated,
    [EnumMember(Value = "media_item_deleted")] MediaItemDeleted,
    [EnumMember(Value = "media_item_played")] MediaItemPlayed,
    [EnumMember(Value = "providers_updated")] ProvidersUpdated,
    [EnumMember(Value = "sync_tasks_updated")] SyncTasksUpdated,
    [EnumMember(Value = "application_shutdown")] ApplicationShutdown
}