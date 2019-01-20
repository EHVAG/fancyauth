using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Fancyauth.Model.UserAttribute;
using System.Collections.Generic;

namespace Fancyauth.Model
{
    public class User
    {
        public int Id { get; set; }

        [Required, Index(IsUnique = true)]
        public string Name { get; set; }

        // Modelling as optional components because (as always) OO sucks:
        /// <summary>
        /// Full members with an EHVAG CA-signed certificate.
        /// </summary>
        public virtual Membership Membership { get; set; }
        /// <summary>
        /// Certificates the user can log in with.
        /// MAY be signed by ehvag mumble root ca, but that one is sunsetting so we
        /// </summary>
        public virtual ICollection<CertificateCredentials> CertCredentials { get; set; }
        public virtual PersistentGuest PersistentGuest { get; set; }
        /// <summary>
        /// Guest invite - only set for guests that joined with an invite code.
        /// </summary>
        public virtual Invite GuestInvite { get; set; }
    }
}

