using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRIEAdvanced.Models
{
    public class Users
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(20)]
        [Column(TypeName = "varchar(20)")]
        public String UserID { get; set; } = null!;

        [MaxLength(20)]
        [Column(TypeName = "varchar(20)")]
        public String UserPassword { get; set; } = null!;
    }
}
