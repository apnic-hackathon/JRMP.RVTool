using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JRMP.RVTool.Core.Models.DB
{
    [Serializable()]
    [Table("AddressDelegations")]
    public partial class AddressDelegation
    {
        public AddressDelegation()
        {
            this.IsActive = true;
            this.CreateDate = DateTimeOffset.UtcNow;
            this.LastUpdated = DateTimeOffset.UtcNow;
        }

        [Key]
        [Column("ID", TypeName = "int")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int ID { get; set; } // int, not null

        [Column("RIRID", TypeName = "varchar(10)")]
        [MaxLength(10)]
        [StringLength(10)]
        [Required]
        public string RIRID { get; set; } // varchar(10), not null

        [Column("CountryCode", TypeName = "char(2)")]
        [MaxLength(2)]
        [StringLength(2)]
        [Required]
        public string CountryCode { get; set; } // char(2), not null

        [Column("AddressFamilyID", TypeName = "tinyint(2)")]
        [Required]
        public byte AddressFamilyID { get; set; } // tinyint, not null

        [Column("Prefix", TypeName = "varchar(36)")]
        [MaxLength(36)]
        [StringLength(36)]
        [Required]
        public string Prefix { get; set; } // varchar(36), not null

        [Column("CIDRLength", TypeName = "tinyint")]
        [Required]
        public byte CIDRLength { get; set; } // tinyint, not null

        [Column("PrefixBinary", TypeName = "varchar(128)")]
        [MaxLength(128)]
        [StringLength(128)]
        [Required]
        public string PrefixBinary { get; set; } // varchar(128), not null

        [Column("DelegationDate", TypeName = "date")]
        [Required]
        public DateTime DelegationDate { get; set; } // date, not null

        [Column("DelegationTypeID", TypeName = "byte")]
        [Required]
        public byte DelegationTypeID { get; set; } // tinyint, not null

        [Column("IsActive", TypeName = "bit")]
        [Required]
        public bool IsActive { get; set; } // bit, not null

        [Column("CreateDate", TypeName = "datetimeoffset(2)")]
        [Required]
        public DateTimeOffset CreateDate { get; set; } // datetimeoffset(2), not null

        [Column("LastUpdated", TypeName = "datetimeoffset(2)")]
        [Required]
        public DateTimeOffset LastUpdated { get; set; } // datetimeoffset(2), not null

        // dbo.AddressDelegations.RIRID -> dbo.RIRs.RIRID (FK_AddressDelegations_RIRs)
        [ForeignKey("RIRID")]
        public virtual RIR RIR { get; set; }

        // dbo.AddressDelegations.CountryCode -> dbo.Countries.CountryCode (FK_AddressDelegations_Countries)
        [ForeignKey("CountryCode")]
        public virtual Country Country { get; set; }

        // dbo.AddressDelegations.AddressFamilyID -> dbo.AddressFamilies.AddressFamilyID (FK_AddressDelegations_AddressFamilies)
        [ForeignKey("AddressFamilyID")]
        public virtual AddressFamily AddressFamily { get; set; }

        // dbo.AddressDelegations.DelegationTypeID -> dbo.DelegationTypes.DelegationTypeID (FK_AddressDelegations_DelegationTypes)
        [ForeignKey("DelegationTypeID")]
        public virtual DelegationType DelegationType { get; set; }
    }
}
