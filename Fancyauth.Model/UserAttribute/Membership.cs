using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.Model.UserAttribute
{
    public class Membership
    {
        public Membership()
        {
            Texture = new byte[0];
            Comment = String.Empty;
            Godfatherships = new List<PersistentGuest>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Required]
        public virtual User User { get; set; }

        [Required]
        public byte[] Texture { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Comment { get; set; }

        public long? SteamId { get; set; }

        public virtual ICollection<PersistentGuest> Godfatherships { get; set; }
    }
}
