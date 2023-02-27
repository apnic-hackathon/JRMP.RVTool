using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JRMP.RVTool.Core.Models.DB
{
    [Serializable()]
    [Table("BGPPrefixes")]
    public partial class BGPPrefix
    {
        public BGPPrefix()
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

        [Column("AddressFamilyID", TypeName = "tinyint")]
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

        [Column("ASPath", TypeName = "varchar(1000)")]
        [MaxLength(1000)]
        [StringLength(1000)]
        public string ASPath { get; set; } // varchar(1000), null

        [Column("OriginAS", TypeName = "bigint")]
        public long? OriginAS { get; set; } // bigint, null

        [Column("SourceID", TypeName = "int")]
        [Required]
        public int SourceID { get; set; } // int, not null

        [Column("IsActive", TypeName = "bit")]
        [Required]
        public bool IsActive { get; set; } // bit, not null

        [Column("CreateDate", TypeName = "datetimeoffset(2)")]
        [Required]
        public DateTimeOffset CreateDate { get; set; } // datetimeoffset(2), not null

        [Column("LastUpdated", TypeName = "datetimeoffset(2)")]
        [Required]
        public DateTimeOffset LastUpdated { get; set; } // datetimeoffset(2), not null

        // dbo.BGPPrefixes.SourceID -> dbo.BGPSources.SourceID (FK_BGPPrefixes_BGPSources)
        [ForeignKey("SourceID")]
        public virtual BGPSource BGPSource { get; set; }
    }
}
