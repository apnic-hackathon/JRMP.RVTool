using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JRMP.RVTool.Core.Models.DB
{
    [Serializable()]
    [Table("Countries")]
    public partial class Country
    {
        public Country()
        {
            this.AddressDelegations = new List<AddressDelegation>();
        }

        [Key]
        [Column("CountryCode", TypeName = "char(2)")]
        [MaxLength(2)]
        [StringLength(2)]
        [Required]
        public string CountryCode { get; set; } // char(2), not null

        [Column("CountryCodeISO3", TypeName = "char(3)")]
        [MaxLength(3)]
        [StringLength(3)]
        public string CountryCodeISO3 { get; set; } // char(3), null

        [Column("CountryName1", TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [StringLength(50)]
        [Required]
        public string CountryName1 { get; set; } // nvarchar(50), not null

        [Column("CountryName2", TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [StringLength(50)]
        [Required]
        public string CountryName2 { get; set; } // nvarchar(50), not null

        [Column("PhoneCode", TypeName = "varchar(5)")]
        [MaxLength(5)]
        [StringLength(5)]
        public string PhoneCode { get; set; } // varchar(5), null

        // dbo.AddressDelegations.CountryCode -> dbo.Countries.CountryCode (FK_AddressDelegations_Countries)
        public virtual List<AddressDelegation> AddressDelegations { get; set; }
    }
}
