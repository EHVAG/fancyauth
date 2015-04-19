using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fancyauth.Model;

namespace Fancyauth.Model
{
    public abstract class LogEntry
    {
        public int Id { get; set; }

        [Required, Index]
        public DateTime When { get; set; }

        public virtual User WhoU { get; set; }
        public virtual Invite WhoI { get; set; }

        public int? WhoUId { get; set; }
        public int? WhoIId { get; set; }

        public enum Discriminator : int
        {
            Connected,
            Disconnected,
            ChatMessage,
        }

        public class Connected : LogEntry
        {
        }

        public class Disconnected : LogEntry
        {
        }

        public class ChatMessage : LogEntry
        {
            public string Message { get; set; }
        }
    }
}

