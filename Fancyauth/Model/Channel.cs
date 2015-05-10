using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.Model
{
    public class Channel
    {
        public int Id { get; set; }

        public bool Temporary { get; set; }

        public virtual Channel Parent { get; set; }

        /// <summary>
        /// The channel ID that can be used for server RPC calls. It's null if the channel was deleted.
        /// </summary>
        public int? ServerId { get; set; }

        [Table("ChannelInfoChanges")]
        public class InfoChange
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public DateTime When { get; set; }

            [Required]
            public virtual Channel Channel { get; set; }
        }
    }
}
