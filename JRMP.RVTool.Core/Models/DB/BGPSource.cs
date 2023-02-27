using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JRMP.RVTool.Core.Models.DB
{
    [Serializable()]
    [Table("BGPSources")]
    public partial class BGPSource
    {
        public BGPSource()
        {
            this.BGPPrefixes = new List<BGPPrefix>();
        }

        [Key]
        [Column("SourceID", TypeName = "int")]
        [Required]
        public int SourceID { get; set; } // int, not null

        [Column("SourceName", TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [StringLength(50)]
        [Required]
        public string SourceName { get; set; } // nvarchar(50), not null

        [Column("RemoteAddress", TypeName = "nvarchar(200)")]
        [MaxLength(200)]
        [StringLength(200)]
        public string RemoteAddress { get; set; } // nvarchar(200), null

        [Column("RemotePort", TypeName = "int")]
        public int? RemotePort { get; set; } // nvarchar(50), null

        [Column("UserName", TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [StringLength(50)]
        public string UserName { get; set; } // nvarchar(50), null

        [Column("Password", TypeName = "nvarchar(50)")]
        [MaxLength(50)]
        [StringLength(50)]
        public string Password { get; set; } // nvarchar(50), null

        // dbo.BGPPrefixes.SourceID -> dbo.BGPSources.SourceID (FK_BGPPrefixes_BGPSources)
        public virtual List<BGPPrefix> BGPPrefixes { get; set; }
    }
}
