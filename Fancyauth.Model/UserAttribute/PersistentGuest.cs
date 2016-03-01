using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.Model.UserAttribute
{
    public class PersistentGuest
    {
        public PersistentGuest()
        {
            Godfathers = new List<Membership>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Required]
        public virtual User User { get; set; }

        public virtual ICollection<Membership> Godfathers { get; set; }

        [Required]
        public virtual Invite OriginalInvite { get; set; }
    }
}
