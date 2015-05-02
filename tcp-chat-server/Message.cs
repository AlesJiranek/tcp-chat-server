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

    /**
     * Constructor
     */ 
    public Message()
    {
        this.Timestamp = new DateTime();
    }


    /**
     * Name of user who sent message
     */ 
    public String Username { get; set; }


    /**
     * Messaeg Content
     */ 
    public Object Content { get; set; }


    /**
     * Timestamp when message was created
     */ 
    public DateTime Timestamp { get; set; } 


    /**
     * Message type
     */ 
    public MessageType Type { get; set; }
}
