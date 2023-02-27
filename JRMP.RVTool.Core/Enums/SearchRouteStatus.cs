namespace JRMP.RVTool.Core.Enums
{
    public enum SearchRouteStatus : byte
    {
        Unspecified = 0,
        OKExactMatch = 1,
        OKSubPrefixCovered = 2,
        PartiallyOK = 3,
        NotOK = 4
    }
}
