using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tcp_chat_server.DatabaseModels;

namespace tcp_chat_server.DAOs
{
    class MessagesDAO
    {
        /**
         * Returns list of last 10 (max) messages in chatroom room
         */ 
        public static List<Message> GetLastTenMessagesInRoom(Room room)
        {
            using (HistoryDb context = new HistoryDb())
            {
                String roomName = room.GetName();
                return context.Messages.Where(x => x.Room == roomName)
                    .OrderByDescending(x => x.Timestamp)
                    .Take(10)
                    .OrderBy(x => x.Timestamp)
                    .Select(x => new Message()
                    {
                        Content = x.Content,
                        Timestamp = x.Timestamp,
                        Username = x.Username,
                        Type = Message.MessageType.normal
                    }).ToList();
            }
        }


        /**
         * Save message to history in database
         */ 
        public static void Add(Message message, Room room)
        {
            DatabaseModels.Message databaseMessage = new DatabaseModels.Message();
            databaseMessage.Room = room.GetName();
            databaseMessage.Content = message.Content.ToString();
            databaseMessage.Timestamp = message.Timestamp;
            databaseMessage.Username = message.Username;
            databaseMessage.Id = Guid.NewGuid();

            using (HistoryDb context = new HistoryDb())
            {
                context.Messages.Add(databaseMessage);
                context.SaveChanges();
            }
        }
    }
}
