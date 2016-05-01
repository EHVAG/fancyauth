using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fancyauth.Model.UserAttribute
{
    public class CertificateCredentials
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }
        [Required]
        public virtual User User { get; set; }

        [Required, Index(IsUnique = true)]
        public string Fingerprint { get; set; }

        [Required]
        public long CertSerial { get; set; }
    }
}
