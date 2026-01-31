using Newtonsoft.Json;
using System.Collections;

namespace WateryTart.Service.MassClient.Messages
{
    public abstract class MessageBase
    {
        public MessageBase(string _command)
        {
            message_id = Guid.NewGuid().ToString();
            command = _command;
        }

        public Hashtable args { get; set; }
        public string message_id { get; set; }
        public string command { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}