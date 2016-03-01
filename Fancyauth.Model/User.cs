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

        // Modelling as optional components because (as always) OO sucks:
        public virtual Membership Membership { get; set; }
        public virtual CertificateCredentials CertCredentials { get; set; }
        public virtual PersistentGuest PersistentGuest { get; set; }
        public virtual Invite GuestInvite { get; set; }
    }
}

