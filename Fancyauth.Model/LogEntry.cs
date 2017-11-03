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
        public DateTimeOffset When { get; set; }

        [Required]
        public virtual User Who { get; set; }

        [Required]
        public virtual Channel Where { get; set; }

        public enum Discriminator : int
        {
            Connected,
            Disconnected,
            ChatMessage,
            ChannelSwitched,
        }

        public class Connected : LogEntry
        {
        }

        public class ChannelSwitched : LogEntry
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

