using System.Collections;

namespace WateryTart.MassClient.Messages;

public class ArtistMessages : MessageFactoryBase
{

    public static MessageBase ArtistCount()
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

    public static MessageBase ArtistsGet => JustCommand(Commands.ArtistsGet);

    public static MessageBase ArtistGet(string artistid, string provider_instance_id_or_domain = "library") =>
        IdAndProvider(Commands.ArtistGet, artistid, provider_instance_id_or_domain);

    public static MessageBase ArtistAlbums(string artistid, string provider_instance_id_or_domain = "library") =>
        IdAndProvider(Commands.ArtistAlbums, artistid, provider_instance_id_or_domain);

}