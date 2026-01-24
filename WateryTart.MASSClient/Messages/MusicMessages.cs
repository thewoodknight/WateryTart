using System.Collections;

namespace WateryTart.MassClient.Messages;

public class MusicMessages : MessageFactoryBase
{
    public static MessageBase MusicAlbumGet => JustCommand(Commands.MusicAlbumGet);
    public static MessageBase MusicAlbumLibraryItems => JustCommand(Commands.MusicAlbumLibraryItems);

    public static MessageBase MusicAlbumTracks(string albumId, string provider_instance_id_or_domain = "library")
    {
        var m = new Message(Commands.MusicAlbumTracks)
        {
            args = new Hashtable
            {
                { "item_id", albumId },
                { "provider_instance_id_or_domain", provider_instance_id_or_domain }
            }
        };

        return m;
    }
}