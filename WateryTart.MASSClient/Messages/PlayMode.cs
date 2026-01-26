using System.Runtime.Serialization;

namespace WateryTart.MassClient.Messages;

public enum PlayMode
{
    [EnumMember(Value = "play")]Play, 
    [EnumMember(Value = "replace")]Replace,
    [EnumMember(Value = "next")] Next,
    [EnumMember(Value = "replace_next")] ReplaceNext, 
    [EnumMember(Value = "add")]Add,
    [EnumMember(Value = "unknown")] Unknown
}