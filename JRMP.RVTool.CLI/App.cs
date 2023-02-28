using JRMP.RVTool.Core;
using JRMP.RVTool.Core.Models.App;
using JRMP.RVTool.Core.Models.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace JRMP.RVTool.CLI
{
    public class App
    {
        private readonly IConfiguration _configuration;
        private readonly string _dbConnectionString;
        private bool _isDebug = false;
        private RVToolContext _context;

        public App(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._dbConnectionString = this._configuration.GetConnectionString("RVTool");
            this._isDebug = this._configuration.GetValue<bool>("RVTool:IsDebug");
            this._context = new RVToolContext(this._dbConnectionString);
        }

        public void Run(string[] args)
        {
            string commandName = null;
            string countryCode = null;
            string filePath = null;
            string prefix = null;
            int? sourceID = null;
            byte searchRouteResultStatus = 0;
            string taskName = null;

            if (args.Length > 0)
                commandName = args[0].Trim().ToLower();

            if (args.Length > 1)
                taskName = args[1].Trim().ToLower();

            switch (commandName)
            {
                case "/address-delegation":
                    switch (taskName)
                    {
                        case "/load-ipv4":
                            if (args.Length > 2) filePath = args[2];

                            if (!string.IsNullOrWhiteSpace(filePath))
                            {
                                this.LoadAddressDelegations(Core.Enums.AddressFamily.IPv4, filePath);
                            }
                            else
                            {
                                this.ShowInvalidCommandMessage();
                            }
                            break;
                        case "/load-ipv6":
                            if (args.Length > 2) filePath = args[2];

                            if (!string.IsNullOrWhiteSpace(filePath))
                            {
                                this.LoadAddressDelegations(Core.Enums.AddressFamily.IPv6, filePath);
                            }
                            else
                            {
                                this.ShowInvalidCommandMessage();
                            }
                            break;
                        default:
                            this.ShowInvalidCommandMessage();
                            break;
                    }
                    break;
                case "/bgp-prefix":
                    switch (taskName)
                    {
                        case "/load-ipv4":
                            if (args.Length > 2) sourceID = Functions.ToNullableInteger(args[2]);

                            if (sourceID.HasValue)
                            {
                                this.LoadBGPPrefixes(Core.Enums.AddressFamily.IPv4, sourceID.Value);
                            }
                            else
                            {
                                this.ShowInvalidCommandMessage();
                            }
                            break;
                        case "/load-ipv6":
                            if (args.Length > 2) sourceID = Functions.ToNullableInteger(args[2]);

                            if (sourceID.HasValue)
                            {
                                this.LoadBGPPrefixes(Core.Enums.AddressFamily.IPv6, sourceID.Value);
                            }
                            else
                            {
                                this.ShowInvalidCommandMessage();
                            }
                            break;
                        default:
                            this.ShowInvalidCommandMessage();
                            break;
                    }
                    break;
                case "/search-route":
                    switch (taskName)
                    {
                        case "/ipv4-by-country":
                            if (args.Length > 2) countryCode = args[2];
                            if (args.Length > 3) searchRouteResultStatus = Functions.ForceByte(args[3], 0);

                            if (!string.IsNullOrWhiteSpace(countryCode))
                            {
                                this.SearchRoutesByCountry(
                                    Core.Enums.AddressFamily.IPv4,
                                    countryCode.ToUpper(),
                                    (Core.Enums.SearchRouteStatus)searchRouteResultStatus);
                            }
                            else
                            {
                                this.ShowInvalidCommandMessage();
                            }
                            break;
                        case "/ipv4-by-prefix":
                            if (args.Length > 2) prefix = args[2];
                            if (args.Length > 3) searchRouteResultStatus = Functions.ForceByte(args[3], 0);

                            if (!string.IsNullOrWhiteSpace(prefix))
                            {
                                this.SearchRoutesByPrefix(
                                    Core.Enums.AddressFamily.IPv4,
                                    prefix.ToLower(),
                                    (Core.Enums.SearchRouteStatus)searchRouteResultStatus);
                            }
                            else
                            {
                                this.ShowInvalidCommandMessage();
                            }
                            break;
                        case "/ipv6-by-country":
                            if (args.Length > 2) countryCode = args[2];
                            if (args.Length > 3) searchRouteResultStatus = Functions.ForceByte(args[3], 0);

                            if (!string.IsNullOrWhiteSpace(countryCode))
                            {
                                this.SearchRoutesByCountry(
                                    Core.Enums.AddressFamily.IPv6,
                                    countryCode.ToUpper(),
                                    (Core.Enums.SearchRouteStatus)searchRouteResultStatus);
                            }
                            else
                            {
                                this.ShowInvalidCommandMessage();
                            }
                            break;
                        case "/ipv6-by-prefix":
                            if (args.Length > 2) prefix = args[2];
                            if (args.Length > 3) searchRouteResultStatus = Functions.ForceByte(args[3], 0);

                            if (!string.IsNullOrWhiteSpace(prefix))
                            {
                                this.SearchRoutesByPrefix(
                                    Core.Enums.AddressFamily.IPv6,
                                    prefix.ToLower(),
                                    (Core.Enums.SearchRouteStatus)searchRouteResultStatus);
                            }
                            else
                            {
                                this.ShowInvalidCommandMessage();
                            }
                            break;
                        default:
                            this.ShowInvalidCommandMessage();
                            break;
                    }
                    break;
                default:
                    this.ShowInvalidCommandMessage();
                    break;
            }
        }

        private int LoadAddressDelegations(Core.Enums.AddressFamily addressFamily, string filePath)
        {
            string strDelegationAddressFamily;

            switch (addressFamily)
            {
                case Core.Enums.AddressFamily.IPv4:
                    strDelegationAddressFamily = "ipv4";
                    break;
                case Core.Enums.AddressFamily.IPv6:
                    strDelegationAddressFamily = "ipv6";
                    break;
                default:
                    Console.WriteLine($"Unsupported Address Family ({addressFamily}). ");
                    Console.WriteLine();
                    return -1;
            };

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine($"Please specify file name. ");
                Console.WriteLine();
                return -1;
            }

            int ret = 0;

            using (StreamReader sr = new StreamReader(filePath))
            {
                int currentSize = 0;
                int batchSize = Functions.ForceInteger(this._configuration.GetValue<int>("LoadAddressDelegations:BatchSize"), 10000);

                string line = sr.ReadLine();

                while (!string.IsNullOrWhiteSpace(line))
                {
                    string[] sline = line.Split('|');

                    if (sline.Length == 7)
                    {
                        if ((sline[2] == strDelegationAddressFamily))
                        {
                            currentSize++;

                            string prefix = sline[3];
                            byte cidrLength = Functions.GetCIDRLength(addressFamily, sline[4]);
                            string prefixBinary = Functions.GetPrefixBinary(prefix, cidrLength);

                            Console.WriteLine($"[{DateTimeOffset.Now:s}] [{currentSize}/{batchSize}/{ret}] Adding Address Delegation {prefix}/{cidrLength}...\r\n");

                            AddressDelegation newAddressDelegation = new AddressDelegation()
                            {
                                RIRID = sline[0],
                                CountryCode = sline[1],
                                AddressFamilyID = (byte)addressFamily,
                                Prefix = prefix,
                                CIDRLength = cidrLength,
                                PrefixBinary = prefixBinary,
                                DelegationDate = Functions.GetDelegationDate(sline[5]),
                                DelegationTypeID = (byte)Functions.GetDelegationType(sline[6])
                            };

                            this._context.AddressDelegations.Add(newAddressDelegation);

                            Console.WriteLine($"[{DateTimeOffset.Now:s}] [{currentSize}/{batchSize}/{ret}] Added Address Delegation {prefix}/{cidrLength}: \r\n{JsonConvert.SerializeObject(newAddressDelegation, Formatting.Indented)}\r\n");
                        }

                        if (currentSize >= batchSize)
                        {
                            ret += this._context.SaveChanges();
                            currentSize = 0;
                        }
                    }

                    line = sr.ReadLine();
                }

                ret = this._context.SaveChanges();
            }

            return ret;
        }

        private int LoadBGPPrefixes(Core.Enums.AddressFamily addressFamily, int sourceID)
        {
            string commandText;

            switch (addressFamily)
            {
                case Core.Enums.AddressFamily.IPv4:
                    commandText = "/ip route print terse where bgp";
                    break;
                case Core.Enums.AddressFamily.IPv6:
                    commandText = "/ipv6 route print terse where bgp";
                    break;
                default:
                    Console.WriteLine($"Unsupported Address Family ({addressFamily}). ");
                    Console.WriteLine();
                    return -1;
            };

            BGPSource bgpSource = this._context.BGPSources.AsNoTracking().FirstOrDefault(x => x.SourceID == sourceID);

            if (bgpSource is null)
            {
                Console.WriteLine($"Source ID {sourceID} could not be found. ");
                Console.WriteLine();
                return -1;
            }

            if (string.IsNullOrWhiteSpace(bgpSource.RemoteAddress) ||
                !bgpSource.RemotePort.HasValue ||
                string.IsNullOrWhiteSpace(bgpSource.UserName) ||
                string.IsNullOrWhiteSpace(bgpSource.Password))
            {
                Console.WriteLine($"Source ID {sourceID} is not configured accordingly. ");
                Console.WriteLine();
                return -1;
            }

            int ret = 0;

            using (var sshClient = new SshClient(bgpSource.RemoteAddress, bgpSource.RemotePort.Value, bgpSource.UserName, bgpSource.Password))
            {
                int currentSize = 0;
                int batchSize = Functions.ForceInteger(this._configuration.GetValue<int>("LoadBGPPrefixes:BatchSize"), 10000);

                sshClient.Connect();
                SshCommand sshCommand = sshClient.CreateCommand(commandText);
                sshCommand.CommandTimeout = new TimeSpan(1, 0, 0);
                IAsyncResult asyncResult = sshCommand.BeginExecute();
                Stream outputStream = sshCommand.OutputStream;
                StreamReader streamReader = new StreamReader(outputStream);

                while (!asyncResult.IsCompleted || !streamReader.EndOfStream)
                {
                    string outputLine = streamReader.ReadLine();

                    if (!string.IsNullOrWhiteSpace(outputLine))
                    {
                        foreach (Match m in Regex.Matches(outputLine, @"^.+(dst-address=(?<DstAddress>[a-z0-9\.:\/]+))(.+(bgp-as-path=(?<BGPASPath>[a-z0-9,]+)))*(\s.+)?$"))
                        {
                            currentSize++;

                            string prefix = m.Groups["DstAddress"].Value.Split('/')[0];
                            byte cidrLength = Functions.ForceByte(m.Groups["DstAddress"].Value.Split('/')[1], 128);
                            string prefixBinary = Functions.GetPrefixBinary(prefix, cidrLength);
                            string asPath = m.Groups["BGPASPath"].Value.Replace(',', ' ');
                            long? originAS = Functions.ToNullableLong(asPath.Split(' ').LastOrDefault());

                            Console.WriteLine($"[{DateTimeOffset.Now:s}] [{currentSize}/{batchSize}/{ret}] Adding BGP Prefix {m.Groups["DstAddress"].Value}...\r\n");

                            BGPPrefix newBGPPrefix = new BGPPrefix()
                            {
                                AddressFamilyID = (byte)addressFamily,
                                Prefix = prefix,
                                CIDRLength = cidrLength,
                                PrefixBinary = prefixBinary,
                                ASPath = asPath,
                                OriginAS = originAS,
                                SourceID = bgpSource.SourceID
                            };

                            this._context.BGPPrefixes.Add(newBGPPrefix);

                            Console.WriteLine($"[{DateTimeOffset.Now:s}] [{currentSize}/{batchSize}/{ret}] Added BGP Prefix {m.Groups["DstAddress"].Value}: \r\n{JsonConvert.SerializeObject(newBGPPrefix, Formatting.Indented)}\r\n");
                        }
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }

                    if (currentSize >= batchSize)
                    {
                        ret += this._context.SaveChanges();
                        currentSize = 0;
                    }
                }

                sshCommand.EndExecute(asyncResult);
                sshClient.Disconnect();

                ret += this._context.SaveChanges();
            }

            return ret;
        }

        private void SearchRoutesByCountry(Core.Enums.AddressFamily addressFamily, string countryCode, Core.Enums.SearchRouteStatus showOnlyStatus)
        {
            string strRouteAddressFamily;

            switch (addressFamily)
            {
                case Core.Enums.AddressFamily.IPv4:
                    strRouteAddressFamily = "IPv4";
                    break;
                case Core.Enums.AddressFamily.IPv6:
                    strRouteAddressFamily = "IPv6";
                    break;
                default:
                    Console.WriteLine($"Unsupported Address Family ({addressFamily}). ");
                    Console.WriteLine();
                    return;
            };

            string strShowOnlyStatus;

            switch (showOnlyStatus)
            {
                case Core.Enums.SearchRouteStatus.Unspecified:
                    strShowOnlyStatus = "ALL";
                    break;
                case Core.Enums.SearchRouteStatus.OKExactMatch:
                    strShowOnlyStatus = "OK (Exact Match)";
                    break;
                case Core.Enums.SearchRouteStatus.OKSubPrefixCovered:
                    strShowOnlyStatus = "OK (Sub-prefix Covered)";
                    break;
                case Core.Enums.SearchRouteStatus.PartiallyOK:
                    strShowOnlyStatus = "PARTIALLY OK";
                    break;
                case Core.Enums.SearchRouteStatus.NotOK:
                    strShowOnlyStatus = "NOT OK";
                    break;
                default:
                    Console.WriteLine($"Unsupported Status ({showOnlyStatus}). ");
                    Console.WriteLine();
                    return;
            };

            Country country = this._context.Countries.AsNoTracking().FirstOrDefault(x => x.CountryCode == countryCode);

            if (country is null)
            {
                Console.WriteLine($"Could not find Country Code ({countryCode}). ");
                Console.WriteLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("############################################################");
            Console.WriteLine($"# Search {strRouteAddressFamily} Routes of Address Delegation to [{country.CountryCode}] {country.CountryName2}.");
            Console.WriteLine($"# Status: {strShowOnlyStatus}");
            Console.WriteLine("############################################################");
            Console.WriteLine();

            List<AddressDelegation> addressDelegations = Functions.GetAddressDelegationsByCountry(this._context, addressFamily, country.CountryCode);

            if (addressDelegations.Count == 0)
            {
                Console.WriteLine("No Address Delegation record found. ");
                Console.WriteLine();
                return;
            }

            int resultCount = 0;
            IEnumerable<SearchRouteResult> searchRouteResults = Functions.GetSearchRouteResult(this._context, addressDelegations);

            foreach (SearchRouteResult searchRouteResult in searchRouteResults)
            {
                resultCount++;

                if ((showOnlyStatus == Core.Enums.SearchRouteStatus.Unspecified) || (searchRouteResult.Status == showOnlyStatus))
                {
                    Console.WriteLine($"{searchRouteResult.Prefix}/{searchRouteResult.CIDRLength}");
                    Console.WriteLine($" --> {searchRouteResult.OutputMessage}");

                    foreach (BGPUniquePrefix bgpUniquePrefix in searchRouteResult.BGPUniquePrefixes)
                    {
                        Console.WriteLine($"\t{bgpUniquePrefix.Prefix}/{bgpUniquePrefix.CIDRLength}");
                        foreach (BGPPrefix path in bgpUniquePrefix.Paths)
                        {
                            Console.WriteLine($"\t\t - Origin AS: {path.OriginAS}, AS_PATH: {path.ASPath}");
                        }
                    }

                    Console.WriteLine();
                }
            }

            if (resultCount == 0)
            {
                Console.WriteLine("No result found. ");
                Console.WriteLine();
            }
        }

        private void SearchRoutesByPrefix(Core.Enums.AddressFamily addressFamily, string prefix, Core.Enums.SearchRouteStatus showOnlyStatus)
        {
            string strRouteAddressFamily;

            switch (addressFamily)
            {
                case Core.Enums.AddressFamily.IPv4:
                    strRouteAddressFamily = "IPv4";
                    break;
                case Core.Enums.AddressFamily.IPv6:
                    strRouteAddressFamily = "IPv6";
                    break;
                default:
                    Console.WriteLine($"Unsupported Address Family ({addressFamily}). ");
                    Console.WriteLine();
                    return;
            };

            string strShowOnlyStatus;

            switch (showOnlyStatus)
            {
                case Core.Enums.SearchRouteStatus.Unspecified:
                    strShowOnlyStatus = "ALL";
                    break;
                case Core.Enums.SearchRouteStatus.OKExactMatch:
                    strShowOnlyStatus = "OK (Exact Match)";
                    break;
                case Core.Enums.SearchRouteStatus.OKSubPrefixCovered:
                    strShowOnlyStatus = "OK (Sub-prefix Covered)";
                    break;
                case Core.Enums.SearchRouteStatus.PartiallyOK:
                    strShowOnlyStatus = "PARTIALLY OK";
                    break;
                case Core.Enums.SearchRouteStatus.NotOK:
                    strShowOnlyStatus = "NOT OK";
                    break;
                default:
                    Console.WriteLine($"Unsupported Status ({showOnlyStatus}). ");
                    Console.WriteLine();
                    return;
            };

            if (!IPNetwork.TryParse(prefix, out IPNetwork ipNetwork))
            {
                Console.WriteLine($"Cannot parse prefix ({prefix}). ");
                Console.WriteLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("############################################################");
            Console.WriteLine($"# Search {strRouteAddressFamily} Routes of Prefix {ipNetwork.Network}/{ipNetwork.Cidr}. ");
            Console.WriteLine($"# Status: {strShowOnlyStatus}");
            Console.WriteLine("############################################################");
            Console.WriteLine();

            List<AddressDelegation> addressDelegations = Functions.GetAddressDelegationsByPrefix(this._context, addressFamily, ipNetwork);

            if (addressDelegations.Count == 0)
            {
                Console.WriteLine("No Address Delegation record found. ");
                Console.WriteLine();
                return;
            }

            int resultCount = 0;
            IEnumerable<SearchRouteResult> searchRouteResults = Functions.GetSearchRouteResult(this._context, addressDelegations);

            foreach (SearchRouteResult searchRouteResult in searchRouteResults)
            {
                resultCount++;

                if ((showOnlyStatus == Core.Enums.SearchRouteStatus.Unspecified) || (searchRouteResult.Status == showOnlyStatus))
                {
                    Console.WriteLine($"{searchRouteResult.Prefix}/{searchRouteResult.CIDRLength}");
                    Console.WriteLine($" --> {searchRouteResult.OutputMessage}");

                    foreach (BGPUniquePrefix bgpUniquePrefix in searchRouteResult.BGPUniquePrefixes)
                    {
                        Console.WriteLine($"\t{bgpUniquePrefix.Prefix}/{bgpUniquePrefix.CIDRLength}");
                        foreach (BGPPrefix path in bgpUniquePrefix.Paths)
                        {
                            Console.WriteLine($"\t\t - Origin AS: {path.OriginAS}, AS_PATH: {path.ASPath}");
                        }
                    }

                    Console.WriteLine();
                }
            }

            if (resultCount == 0)
            {
                Console.WriteLine("No result found. ");
                Console.WriteLine();
            }
        }

        private void ShowInvalidCommandMessage(string taskName = null)
        {
            Console.WriteLine("Invalid command/task name, or required arguments are missing. ");
            Console.WriteLine();
            Console.WriteLine("Available commands and tasks: ");
            Console.WriteLine();
            Console.WriteLine("\t - /address-delegation");
            Console.WriteLine("\t\t - /load-ipv4 <file-path>");
            Console.WriteLine("\t\t - /load-ipv6 <file-path>");
            Console.WriteLine();
            Console.WriteLine("\t - /bgp-prefix");
            Console.WriteLine("\t\t - /load-ipv4 <source-id>");
            Console.WriteLine("\t\t - /load-ipv6 <source-id>");
            Console.WriteLine();
            Console.WriteLine("\t - /search-route");
            Console.WriteLine("\t\t - /ipv4-by-country <country-code> [*status]");
            Console.WriteLine("\t\t - /ipv4-by-prefix <prefix/len> [*status]");
            Console.WriteLine("\t\t - /ipv6-by-country <country-code> [*status]");
            Console.WriteLine("\t\t - /ipv6-by-prefix <prefix/len> [*status]");
            Console.WriteLine("\t\t * status:");
            Console.WriteLine("\t\t\tOKExactMatch = 1");
            Console.WriteLine("\t\t\tOKSubPrefixCovered = 2");
            Console.WriteLine("\t\t\tPartiallyOK = 3");
            Console.WriteLine("\t\t\tNotOK = 4");
            Console.WriteLine();
        }
    }
}
