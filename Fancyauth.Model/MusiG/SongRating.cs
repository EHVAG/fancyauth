using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.Model.MusiG
{
    public class SongRating
    {
        [Key, Column(Order = 0)]
        public int SongId { get; set; }
        [Key, Column(Order = 1)]
        public int UserId { get; set; }

        public int QueueModifier { get; set; }

        [Required]
        public virtual User User { get; set; }

        [Required]
        public virtual Song Song { get; set; }
    }
}
