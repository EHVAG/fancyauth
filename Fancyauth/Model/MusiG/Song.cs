using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.Model.MusiG
{
    public class Song
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime AdditionDate { get; set; }

        public virtual SongSuggestion SourceSuggestion { get; set; }

        [Required]
        public virtual Album Album { get; set; }

        [Required]
        public virtual Genre Genre { get; set; }

        [Required]
        public virtual Interpret Interpret { get; set; }

        public virtual IEnumerable<Interpret> AdditionalInterprets { get; set; }
    }
}
