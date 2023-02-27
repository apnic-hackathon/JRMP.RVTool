using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JRMP.RVTool.Core.Models.DB
{
    [Serializable()]
    [Table("AddressFamilies")]
    public partial class AddressFamily
    {
        public AddressFamily()
        {
            this.AddressDelegations = new List<AddressDelegation>();
        }

        [Key]
        [Column("AddressFamilyID", TypeName = "tinyint")]
        [Required]
        public byte AddressFamilyID { get; set; } // tinyint, not null

        [Column("AddressFamilyName", TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [StringLength(50)]
        [Required]
        public string AddressFamilyName { get; set; } // nvarchar(50), not null

        // dbo.AddressDelegations.AddressFamilyID -> dbo.AddressFamilies.AddressFamilyID (FK_AddressDelegations_AddressFamilies)
        public virtual List<AddressDelegation> AddressDelegations { get; set; }
    }
}
