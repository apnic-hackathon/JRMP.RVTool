using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JRMP.RVTool.Core.Models.DB
{
    [Serializable()]
    [Table("RIRs")]
    public partial class RIR
    {
        public RIR()
        {
            this.AddressDelegations = new List<AddressDelegation>();
        }

        [Key]
        [Column("RIRID", TypeName = "varchar(10)")]
        [MaxLength(10)]
        [StringLength(10)]
        [Required]
        public string RIRID { get; set; } // varchar(10), not null

        [Column("RIRName", TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [StringLength(50)]
        [Required]
        public string RIRName { get; set; } // nvarchar(50), not null

        // dbo.AddressDelegations.RIRID -> dbo.RIRs.RIRID (FK_AddressDelegations_RIRs)
        public virtual List<AddressDelegation> AddressDelegations { get; set; }
    }
}
