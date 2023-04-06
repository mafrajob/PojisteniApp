using System.ComponentModel.DataAnnotations;

namespace PojisteniApp2.Models
{
    public class InsuranceType
    {
        public int InsuranceTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string InsuranceTypeName { get; set; } = string.Empty;
    }
}
