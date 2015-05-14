using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace Fancyauth.Model
{
    public class User
    {
        public User()
        {
            Texture = new byte[0];
            Comment = String.Empty;
        }

        [Required]
        public int Id { get; set; }

        [Required, Index(IsUnique = true)]
        public string Name { get; set; }

        [Required, Index(IsUnique = true)]
        public string Fingerprint { get; set; }

        [Required]
        public long CertSerial { get; set; }

        [Required]
        public byte[] Texture { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Comment { get; set; }

        public long? SteamId { get; set; }
    }
}

