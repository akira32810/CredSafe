using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CredSafeDotnetCore.BusinessClass;

namespace CredSafeDotnetCore.Model
{
    public class CRModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CRID { get; set; }

        [Required]
        public string CRService { get; set; }

        [Required]
        public string CRUser { get; set; }

        [Required]
        public string CRPass { get; set; }


        [Required]
        public string Owner { get; set; }

        public DateTime CRUpdatedDate { get; set; }


        [NotMapped] //prevent from adding to DB
        public string TokenStr { get; set; }


        public CRModel()
        {
            CRUpdatedDate = DateTime.Now;

           // CRPass = Cryptography.EncryptAESManaged("blobsa");
        }

    }
}
