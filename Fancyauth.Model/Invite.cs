using Fancyauth.Model.UserAttribute;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fancyauth.Model
{
    public class Invite
    {
        public int Id { get; set; }

        [Required, Index(IsUnique = true)]
        public string Code { get; set; }

        [Required]
        public virtual User Inviter { get; set; }

        public int InviterId { get; set; }

        [Required]
        public DateTimeOffset ExpirationDate { get; set; }

        [Required, DefaultValue(0)]
        public int UseCount { get; set; }
    }
}

