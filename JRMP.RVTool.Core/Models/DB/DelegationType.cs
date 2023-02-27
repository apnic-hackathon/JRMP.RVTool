using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JRMP.RVTool.Core.Models.DB
{
    [Serializable()]
    [Table("DelegationTypes")]
    public partial class DelegationType
    {
        public DelegationType()
        {
            this.AddressDelegations = new List<AddressDelegation>();
        }

        [Key]
        [Column("DelegationTypeID", TypeName = "tinyint")]
        [Required]
        public byte DelegationTypeID { get; set; } // tinyint, not null

        [Column("DelegationTypeName", TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [StringLength(50)]
        [Required]
        public string DelegationTypeName { get; set; } // nvarchar(50), not null

        // dbo.AddressDelegations.DelegationTypeID -> dbo.DelegationTypes.DelegationTypeID (FK_AddressDelegations_DelegationTypes)
        public virtual List<AddressDelegation> AddressDelegations { get; set; }
    }
}
