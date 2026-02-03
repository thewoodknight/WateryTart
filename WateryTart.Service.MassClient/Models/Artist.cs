namespace WateryTart.Service.MassClient.Models
{
    public class Artist : MediaItemBase
    {
        public bool Available { get; set; }
        public Image? Image { get; set; }
    }
}