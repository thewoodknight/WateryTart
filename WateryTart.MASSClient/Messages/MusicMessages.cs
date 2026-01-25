using System.Collections;

namespace WateryTart.MassClient.Messages;

public class MusicMessages : MessageFactoryBase
{
    public static MessageBase MusicAlbumLibraryItems => JustCommand(Commands.MusicAlbumLibraryItems);
    public static MessageBase MusicRecommendations => JustCommand(Commands.MusicRecommendations);

    public static MessageBase MusicAlbumTracks(string albumId, string provider_instance_id_or_domain = "library") =>
        IdAndProvider(Commands.MusicAlbumTracks, albumId, provider_instance_id_or_domain);


    public static MessageBase MusicAlbumGet(string albumId, string provider_instance_id_or_domain = "library") =>
        IdAndProvider(Commands.MusicAlbumGet, albumId, provider_instance_id_or_domain);

    public static MessageBase AlbumsCount()
    {
        var m = new Message("music/albums/count")
        {
            args = new Hashtable
            {
                { "favorite_only", "false" },
                { "album_types", "[\"album\", \"single\", \"live\", \"soundtrack\", \"compilation\", \"ep\", \"unknown\"]" }
            }
        };

        return m;
    }

    public static MessageBase TrackCount()
    {
        var m = new Message("music/tracks/count")
        {
            args = new Hashtable
            {
                { "favorite_only", "false" },
            }
        };

        return m;
    }

    public static MessageBase PlaylistCount()
    {
        var m = new Message(Commands.ArtistsCount)
        {
            args = new Hashtable
            {
                { "favorite_only", "false" },
                { "album_artists_only", "true" }
            }
        };

        return m;
    }

}