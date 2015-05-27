using System;
using System.ComponentModel.DataAnnotations;

namespace Fancyauth.Model
{
    public class GuestAssociation
    {
        [Key, Required]
        public string Name { get; set; }

        public int? Session { get; set; }

        [Required]
        public virtual Invite Invite { get; set; }
    }
}

