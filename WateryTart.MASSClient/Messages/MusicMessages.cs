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

}