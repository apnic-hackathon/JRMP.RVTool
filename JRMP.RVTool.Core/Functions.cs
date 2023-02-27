using JRMP.RVTool.Core.Models.App;
using JRMP.RVTool.Core.Models.DB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;

namespace JRMP.RVTool.Core
{
    public class Functions
    {
        public static byte ForceByte(object expression, byte defaultValue)
        {
            if (expression is null)
                return defaultValue;

            if (byte.TryParse(expression.ToString(), out byte ret))
                return ret;

            return defaultValue;
        }

        public static int ForceInteger(object expression, int defaultValue)
        {
            if (expression is null)
                return defaultValue;

            if (int.TryParse(expression.ToString(), out int ret))
                return ret;

            return defaultValue;
        }

        public static long ForceLong(object expression, long defaultValue)
        {
            if (expression is null)
                return defaultValue;

            if (long.TryParse(expression.ToString(), out long ret))
                return ret;

            return defaultValue;
        }

        public static List<AddressDelegation> GetAddressDelegationsByCountry(RVToolContext context, Enums.AddressFamily addressFamily, string countryCode)
        {
            if (context is null)
                throw new ArgumentException(nameof(context));

            return context.AddressDelegations.AsNoTracking()
                .Where(x => x.AddressFamilyID == (byte)addressFamily)
                .Where(x => x.CountryCode == countryCode)
                .OrderBy(x => x.PrefixBinary)
                .ThenBy(x => x.CIDRLength)
                .ToList();
        }

        public static List<AddressDelegation> GetAddressDelegationsByPrefix(RVToolContext context, Enums.AddressFamily addressFamily, IPNetwork ipNetwork)
        {
            if (context is null)
                throw new ArgumentException(nameof(context));

            string prefixBinary = Functions.GetPrefixBinary(ipNetwork);

            return context.AddressDelegations.AsNoTracking()
                .Where(x => x.AddressFamilyID == (byte)addressFamily)
                .Where(x => x.PrefixBinary.StartsWith(prefixBinary))
                .OrderBy(x => x.PrefixBinary)
                .ThenBy(x => x.CIDRLength)
                .ToList();
        }

        public static Enums.AddressFamily GetAddressFamily(string expression)
        {
            Enums.AddressFamily ret = Enums.AddressFamily.Unspecified;

            switch (expression)
            {
                case "ipv4":
                    ret = Enums.AddressFamily.IPv4;
                    break;
                case "ipv6":
                    ret = Enums.AddressFamily.IPv6;
                    break;
            }

            return ret;
        }

        public static byte GetCIDRLength(Enums.AddressFamily addressFamily, string expression)
        {
            byte ret = 0;

            switch (addressFamily)
            {
                case Enums.AddressFamily.IPv4:
                    ret = NumberOfIPv4ToCIDRLength(ForceInteger(expression, 0));
                    break;
                case Enums.AddressFamily.IPv6:
                    ret = ForceByte(expression, 128);
                    break;
            }

            return ret;
        }

        public static DateTime GetDelegationDate(string expression)
        {
            if (expression.Length != 8)
                throw new ArgumentException(nameof(expression));

            int year = int.Parse(expression.Substring(0, 4));
            int month = int.Parse(expression.Substring(4, 2));
            int day = int.Parse(expression.Substring(6, 2));

            return new DateTime(year, month, day);
        }

        public static Enums.DelegationType GetDelegationType(string expression)
        {
            Enums.DelegationType ret = Enums.DelegationType.Unspecified;

            switch (expression)
            {
                case "allocated":
                    ret = Enums.DelegationType.Allocation;
                    break;
                case "assigned":
                    ret = Enums.DelegationType.Assignment;
                    break;
            }

            return ret;
        }

        public static string GetPrefixBinary(IPNetwork ipNetwork)
        {
            return string.Join(string.Empty, ipNetwork.Network.GetAddressBytes().Select(x => Convert.ToString(x, 2).PadLeft(8, '0'))).Substring(0, ipNetwork.Cidr);
        }

        public static string GetPrefixBinary(string prefix, byte cidrLength)
        {
            return string.Join(string.Empty, IPNetwork.Parse($"{prefix}/{cidrLength}").Network.GetAddressBytes().Select(x => Convert.ToString(x, 2).PadLeft(8, '0'))).Substring(0, cidrLength);
        }

        public static IEnumerable<SearchRouteResult> GetSearchRouteResult(RVToolContext context, List<AddressDelegation> addressDelegations)
        {
            if (context is null)
                throw new ArgumentException(nameof(context));

            List<SearchRouteResult> ret = new List<SearchRouteResult>();

            foreach (AddressDelegation addressDelegation in addressDelegations)
            {
                IPNetwork delegatedNetwork = IPNetwork.Parse($"{addressDelegation.Prefix}/{addressDelegation.CIDRLength}");
                BigInteger delegatedAddresses = delegatedNetwork.Total;
                BGPUniquePrefix exactMatchPrefix = context.BGPPrefixes.AsNoTracking()
                    .Where(x => x.AddressFamilyID == addressDelegation.AddressFamilyID)
                    .Where(x => x.Prefix == addressDelegation.Prefix)
                    .Where(x => x.CIDRLength == addressDelegation.CIDRLength)
                    .ToList()
                    .GroupBy(x => new { x.Prefix, x.CIDRLength })
                    .Select(x => new BGPUniquePrefix()
                    {
                        Prefix = x.Key.Prefix,
                        CIDRLength = x.Key.CIDRLength,
                        PrefixBinary = Functions.GetPrefixBinary(x.Key.Prefix, x.Key.CIDRLength),
                        IsCoveredBySupernet = false,
                        Paths = x.ToList()
                    })
                    .FirstOrDefault();

                SearchRouteResult newSearchRouteResult = new SearchRouteResult()
                {
                    Prefix = addressDelegation.Prefix,
                    CIDRLength = addressDelegation.CIDRLength
                };

                if (!(exactMatchPrefix is null))
                {
                    newSearchRouteResult.Status = Enums.SearchRouteStatus.OKExactMatch;
                    newSearchRouteResult.OutputMessage = "OK: Exact match found. ";
                    newSearchRouteResult.BGPUniquePrefixes = new List<BGPUniquePrefix>() { exactMatchPrefix };
                }
                else
                {
                    List<BGPUniquePrefix> subPrefixes = context.BGPPrefixes.AsNoTracking()
                        .Where(x => x.AddressFamilyID == addressDelegation.AddressFamilyID)
                        .Where(x => x.CIDRLength > addressDelegation.CIDRLength)
                        .Where(x => x.PrefixBinary.StartsWith(addressDelegation.PrefixBinary))
                        .ToList()
                        .GroupBy(x => new { x.Prefix, x.CIDRLength })
                        .Select(x => new BGPUniquePrefix()
                        {
                            Prefix = x.Key.Prefix,
                            CIDRLength = x.Key.CIDRLength,
                            PrefixBinary = Functions.GetPrefixBinary(x.Key.Prefix, x.Key.CIDRLength),
                            IsCoveredBySupernet = false,
                            Paths = x.ToList()
                        })
                        .OrderBy(x => x.PrefixBinary)
                        .ThenBy(x => x.CIDRLength)
                        .ToList();

                    if (subPrefixes.Count > 0)
                    {
                        foreach (var subPrefix in subPrefixes)
                        {
                            IPNetwork subPrefixNetwork = IPNetwork.Parse($"{subPrefix.Prefix}/{subPrefix.CIDRLength}");
                            BigInteger subPrefixAddresses = subPrefixNetwork.Total;

                            if (!subPrefix.IsCoveredBySupernet)
                            {
                                delegatedAddresses -= subPrefixAddresses;
                                subPrefixes
                                    .Where(x => x.PrefixBinary.StartsWith(subPrefix.PrefixBinary))
                                    .ToList()
                                    .ForEach(x => x.IsCoveredBySupernet = true);
                            }
                        }
                        if (delegatedAddresses == 0)
                        {

                            newSearchRouteResult.Status = Enums.SearchRouteStatus.OKSubPrefixCovered;
                            newSearchRouteResult.OutputMessage = "OK: Sub-prefix(es) found and they can cover the entire delegation space.";
                            newSearchRouteResult.BGPUniquePrefixes = subPrefixes;
                        }
                        else
                        {
                            newSearchRouteResult.Status = Enums.SearchRouteStatus.PartiallyOK;
                            newSearchRouteResult.OutputMessage = $"PARTIALLY OK: Sub-prefix(es) found but they cannot cover the entire delegation space ({delegatedAddresses:N0} uncovered). ";
                            newSearchRouteResult.BGPUniquePrefixes = subPrefixes;
                        }

                    }
                    else
                    {
                        newSearchRouteResult.Status = Enums.SearchRouteStatus.NotOK;
                        newSearchRouteResult.OutputMessage = "NOT OK: Prefix and Sub-prefix Not Found";
                        newSearchRouteResult.BGPUniquePrefixes = new List<BGPUniquePrefix>();
                    }
                }

                yield return newSearchRouteResult;
            }
        }

        public static byte NumberOfIPv4ToCIDRLength(int numberOfIPv4)
        {
            byte hostBits = 0;
            int remainingIPv4 = numberOfIPv4;

            while (remainingIPv4 > 1)
            {
                remainingIPv4 /= 2;
                hostBits++;
            }

            return (byte)(32 - hostBits);
        }

        public static byte? ToNullableByte(object expression)
        {
            if (expression is null)
                return null;

            if (byte.TryParse(expression.ToString(), out byte ret))
                return ret;

            return null;
        }

        public static int? ToNullableInteger(object expression)
        {
            if (expression is null)
                return null;

            if (int.TryParse(expression.ToString(), out int ret))
                return ret;

            return null;
        }

        public static long? ToNullableLong(object expression)
        {
            if (expression is null)
                return null;

            if (long.TryParse(expression.ToString(), out long ret))
                return ret;

            return null;
        }
    }
}
