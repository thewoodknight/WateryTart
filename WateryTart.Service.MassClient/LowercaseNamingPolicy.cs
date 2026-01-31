namespace WateryTart.Service.MassClient;

public class LowercaseNamingPolicy : Newtonsoft.Json.Serialization.NamingStrategy
{
    protected override string ResolvePropertyName(string name)
    {
        return name.ToLower();
    }
}