using System;
using System.ComponentModel.DataAnnotations;

namespace Fancyauth.Model
{
    public class Rude
    {
        public int Id { get; set; }

        [Required]
        public User Target { get; set; }

        public User ActualTarget { get; set; }

        [Required]
        public User Actor { get; set; }

        public TimeSpan? Duration { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}

