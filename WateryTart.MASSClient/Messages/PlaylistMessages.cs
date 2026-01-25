namespace WateryTart.MassClient.Messages;

public class PlaylistMessages : MessageFactoryBase
{
    public static MessageBase PlaylistGet(string playlistId, string provider_instance_id_or_domain = "library") =>
        IdAndProvider(Commands.PlaylistGet, playlistId, provider_instance_id_or_domain);

    public static MessageBase PlaylistTracksGet(string playlistId, string provider_instance_id_or_domain = "library") =>
            IdAndProvider(Commands.PlaylistTracksGet, playlistId, provider_instance_id_or_domain);
}