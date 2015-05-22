using System;
using System.ComponentModel.DataAnnotations;

namespace Fancyauth.Model
{
    public class OfflineNotification
    {
        public int Id { get; set; }

        [Required]
        public virtual User Sender { get; set; }

        [Required]
        public virtual User Recipient { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTimeOffset When { get; set; }
    }
}

