using System.Collections.Generic;

namespace JRMP.RVTool.Core.Models.App
{
    public class SearchRouteResult
    {
        public SearchRouteResult()
        {
            this.Status = Enums.SearchRouteStatus.NotOK;
            this.OutputMessage = "NOT OK: Default Result";
            this.BGPUniquePrefixes = new List<BGPUniquePrefix>();
        }

        public string Prefix { get; set; }
        public byte CIDRLength { get; set; }
        public Enums.SearchRouteStatus Status { get; set; }
        public string OutputMessage { get; set; }
        public List<BGPUniquePrefix> BGPUniquePrefixes { get; set; }
    }
}
