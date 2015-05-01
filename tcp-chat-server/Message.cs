using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


[Serializable]
public class Message
{
    public enum MessageType { normal, system }

    public Message()
    {
        this.Timestamp = new DateTime();
    }

    public String Username { get; set; }
    public Object Content { get; set; }
    public DateTime Timestamp { get; set; } 
    public MessageType Type { get; set; }
}
