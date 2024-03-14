using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRIEAdvanced.Models
{
    public class UserRefreshTokens
    {
        [Key]
        public int Id { get; set; }
        public string UserIP { get; set; } = null!;
        public int UserId { get; set; }
        public string RefreshToken { get; set; } = null!;
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreateDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime LimitDate { get; set; }
        public DateTime? ResetDate { get; set; }
    }
}
