using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.Model.MusiG
{
    public class SongSuggestion
    {
        public int Id { get; set; }

        [Required]
        public string YouTubeURL { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Interpret { get; set; }

        [Required]
        public string Album { get; set; }

        [Required]
        public string AlbumInterpret { get; set; }

        [Required]
        public string Genre { get; set; }

        public bool Decided { get; set; }
    }
}
