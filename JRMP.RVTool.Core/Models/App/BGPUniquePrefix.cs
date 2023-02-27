using JRMP.RVTool.Core.Models.DB;
using System.Collections.Generic;

namespace JRMP.RVTool.Core.Models.App
{
    public class BGPUniquePrefix
    {
        public string Prefix { get; set; }
        public byte CIDRLength { get; set; }
        public string PrefixBinary { get; set; }
        public bool IsCoveredBySupernet { get; set; }
        public List<BGPPrefix> Paths { get; set; }
    }
}
