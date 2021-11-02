using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CredSafeDotnetCore.Model
{
    public class Token
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TokenID { get; set; }

        public string Tokenstr { get; set; }
        

        public DateTime tokenCreatedDate { get; set; }


        public Token()
        {
            tokenCreatedDate = DateTime.UtcNow;
        }
    }
}
