using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Fancyauth.Model.UserAttribute;

namespace Fancyauth.Model
{
    public class User
    {
        public int Id { get; set; }

        [Required, Index(IsUnique = true)]
        public string Name { get; set; }

        [Required, Index(IsUnique = true)]
        public string CertFingerprint { get; set; }

        [Required]
        public long CertSerial { get; set; }

        // Modelling as optional components because (as always) OO sucks:
        /// <summary>
        /// Real members with CA-signed certificate
        /// </summary>
        public virtual Membership Membership { get; set; }
        /// <summary>
        /// persistent guest properties
        /// </summary>
        public virtual PersistentGuest PersistentGuest { get; set; }
        /// <summary>
        /// Invite code - only set if user is not member and not PersistentGuest;
        /// only if he is actually a Guest with an Invite code
        /// </summary>
        public virtual Invite GuestInvite { get; set; }
    }
}

