using System.Collections;
using System.Text;

//TODO convert everything to use StringBuilders in and out, so that it is possible to perform multiple operations (e.g. Scramble the log, then find e-mail addresses, with everything scrambled in the same way)

namespace MailStoreLogFeatures;

public static partial class LogModification
{
    #region Classes
    public class ServerUnmappableExceptionParser
    {
        #region Fields
        private const string _serverUnmappableExceptionText = "\tMailStore.Common.Interfaces.ServerUnmappableException: MailStore is unable to determine where to store this email. Please ensure that e-mail addresses are specified in the users' settings. Senders and recipients: ";

        // I think(?) a Hashtable is faster than a dictionary here, but _hashtable can be swapped
        // 1:1 for Dictionary<string, HashSet<string>>
        private Hashtable _hashtable;

        private bool _reportAllDomains;
        #endregion Fields

        #region Public Constructors
        public ServerUnmappableExceptionParser() {
            _hashtable = new();
            ReportStatus = new();
            _reportAllDomains = true;
        }
        #endregion Public Constructors

        #region Enums
        internal enum SaveAddressOptions
        {
            DomainAndAddressNew,
            AddressNew,
            AlreadyListed,
            Dragons
        }
        #endregion Enums

        #region Properties
        public StringBuilder ReportStatus { get; }
        #endregion Properties

        #region Public Methods
        public void AddLogfile(TextReader streamReader, string FileName = "") {
            _ = ReportStatus.Append(DateTime.Now).Append(' ').Append(FileName).AppendLine(": Opening file...");
            int CountLines = 0, UniqueDomains = 0, CountEmailAddress = 0, CountValidLogLines = 0, UniqueAddresses = 0;
            try {
                while (streamReader.Peek() != -1) {
                    // Read a line of the file at a time
                    string Line = streamReader.ReadLine() ?? string.Empty;
                    CountLines++;
                    if (Line.StartsWith(_serverUnmappableExceptionText)) {
                        CountValidLogLines++;
                        // Loop through the e-mail addresses
                        string ThisLine = Line.Substring(_serverUnmappableExceptionText.Length);
                        foreach (string EmailAddress in RetrieveEmailAddresses(ThisLine)) {
                            CountEmailAddress++;
                            switch (SaveAddress(EmailAddress)) {
                                case SaveAddressOptions.DomainAndAddressNew:
                                    UniqueDomains++;
                                    UniqueAddresses++;
                                    break;

                                case SaveAddressOptions.AddressNew:
                                    UniqueAddresses++;
                                    break;

                                case SaveAddressOptions.AlreadyListed:
                                    break;

                                case SaveAddressOptions.Dragons:
                                    break;
                            }
                        }
                    }
                }
            } catch (IOException e) {
                Console.WriteLine("An error occurred: '{0}'", e);
            }

            _ = ReportStatus.Append(DateTime.Now).Append(' ').Append(FileName).Append(": Parsed ").AppendFormat("{0:N0}", CountLines).Append(" lines, found ").AppendFormat("{0:N0}", CountValidLogLines).AppendLine(" valid log lines.");
            _ = ReportStatus.Append(DateTime.Now).Append(' ').Append(FileName).Append(": Retrieved ").AppendFormat("{0:N0}", CountEmailAddress).Append(" e-mail ").Append(CountEmailAddress == 1 ? "address" : "addresses").Append(", of which ").AppendFormat("{0:N0}", UniqueAddresses).Append(" were unique across ").AppendFormat("{0:N0}", UniqueDomains).Append(' ').Append(CountEmailAddress == 1 ? "domain" : "domains").AppendLine(".");
        }
        /// <summary>
        /// Select domains to include or exclude. Once "Add" is called, only these domains are
        /// reported on until Reset is triggered
        /// </summary>
        /// <param name="Add">string[] of domains to be added</param>
        /// <param name="Block">string[] of domains to be blocked</param>
        /// <param name="Unblock">string[] of domains to be unblocked</param>
        /// <param name="Reset">Reset (Added/Block can be combined)</param>
        public void DomainsToReport(string[]? Add = null, string[]? Block = null, string[]? Unblock = null, bool Reset = false) {
            // Reset removes all domains, all add/remove state, and sets _reportAllDomains to
            // default so the next parse operates in a fresh state.
            //
            // "Add" adds a domain to _hashtable to accept future entries, and sets
            // _reportAllDomains to false to prevent domains from being added automatically during
            // log parsing.
            //
            // Remove removes a domain, removing explicit Add and Blocks too.
            //
            // Unblock removes a block (null record) but does not change active data
            //
            // _reportAllDomains *cannot* be removed without doing a reset. There's no technical
            // reason for this, but I struggle to understand how/when/why it would be useful and
            // this is enough of a mess to test. A lot of this will never be used, user-facing UI
            // will probably never have more than Add and Block.

            if (Reset) {
                _hashtable = new();
                _reportAllDomains = true;
            }
            if (Unblock is not null) {
                foreach (string domain in Unblock) {
                    // If the entry exists and is null, remove it
                    if (_hashtable.ContainsKey(domain) && _hashtable[domain] is null) {
                        _hashtable.Remove(domain);
                    }
                }
            }
            if (Add is not null) {
                _reportAllDomains = false;
                foreach (string domain in Add) {
                    // If the entry exists and is null, remove it
                    if (_hashtable.ContainsKey(domain) && _hashtable[domain] is null) {
                        _hashtable.Remove(domain);
                    }
                    // If the entry doesn't exist, add it with an empty HashSet
                    if (!_hashtable.ContainsKey(domain)) {
                        _hashtable.Add(domain, new HashSet<string>());
                    }
                }
            }
            if (Block is not null) {
                foreach (string domain in Block) {
                    if (_hashtable.ContainsKey(domain)) {
                        _hashtable.Remove(domain);
                    }
                    _hashtable.Add(domain, null);
                }
            }
        }
        /// <summary>DomainsToReport - Add a single domain</summary>
        /// <param name="Add">The domain</param>
        public void DomainsToReportAdd(string Add) => DomainsToReport(Add: new string[] { Add });
        /// <summary>DomainsToReport - Add a single domain</summary>
        /// <param name="Block">The domain</param>
        public void DomainsToReportBlock(string Block) => DomainsToReport(Block: new string[] { Block });
        /// <summary>Reset the report, removing all entries and all allow/block entries</summary>
        public void DomainsToReportReset() => DomainsToReport(Reset: true);
        /// <summary>DomainsToReport - Unblock a domain without explicitly adding it</summary>
        /// <param name="Unblock">The domain to unblock</param>
        public void DomainsToReportUnblock(string Unblock) => DomainsToReport(Unblock: new string[] { Unblock });
        /// <summary>General the final output into a TextWriter</summary>
        /// <param name="OutputString"></param>
        public void GenerateReport(TextWriter OutputString) {
            int domainCount = 0, uniqueAddressCount = 0;
            foreach (object domain in _hashtable.Keys) {
                if (domain is null) { continue; }
                if (_hashtable[key: (string)domain] is null) { continue; }
                int uniqueAddressCountInDomain = 0;
                domainCount++;
                string DomainString = domain.ToString() ?? string.Empty;
                OutputString.Write('@');
                OutputString.WriteLine(DomainString);
                foreach (string user in (IEnumerable?)_hashtable[(string)domain]!) {
                    if (user is null) { continue; }
                    uniqueAddressCount++;
                    uniqueAddressCountInDomain++;
                    OutputString.Write(new string(' ', DomainString.Length + 1));
                    OutputString.WriteLine(user);
                }
            }
            _ = ReportStatus.Append(DateTime.Now).Append(" {Summary}: ").AppendFormat("{0:N0}", uniqueAddressCount).Append(" unique ").Append(uniqueAddressCount == 1 ? "address" : "addresses").Append(" across ").AppendFormat("{0:N0}", domainCount).Append(' ').Append(domainCount == 1 ? "domain" : "domains").Append(" were added in this run").AppendLine(".");
            OutputString.Flush();
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Extract the e-mail addresses from the incoming text -- From a MailStore log line, not generalized!!!
        /// </summary>
        /// <param name="LogLine"></param>
        /// <returns></returns>
        private static List<string> RetrieveEmailAddresses(string LogLine) {
            List<string> Results = new();
            foreach (string lineSplit in LogLine.Split(new char[] { ' ', ',' })) {
                if (lineSplit.Contains('@')) {
                    Results.Add(lineSplit.Trim());
                }
            }
            return Results;
        }
        /// <summary>Save a discovered address into _hashtable, building the structure as needed</summary>
        /// <param name="EmailAddress"></param>
        /// <returns></returns>
        private SaveAddressOptions SaveAddress(string EmailAddress) {
            SaveAddressOptions? saveAddressResults = null;
            // Split the user/domain at the @
            string user = EmailAddress.Substring(0, EmailAddress.IndexOf("@")).Trim();
            string domain = EmailAddress.Substring(EmailAddress.IndexOf("@") + 1).Trim();
            // Check if the domain is in _hashtable, and if not, add it with a fresh hashset Blocked
            // domain will be in _hashtable as null.
            if (!_hashtable.ContainsKey(domain)) {
                if (_reportAllDomains) {
                    _hashtable.Add(domain, new HashSet<string>());
                }
                saveAddressResults ??= SaveAddressOptions.DomainAndAddressNew;
            }
            // If the domain is in the _hashtable, address the address to the hashset
            if (_hashtable[domain] is HashSet<string> current) {
                if (current.Add(user)) {
                    saveAddressResults ??= SaveAddressOptions.AddressNew;
                } else {
                    saveAddressResults ??= SaveAddressOptions.AlreadyListed;
                }
            }
            return saveAddressResults ?? SaveAddressOptions.Dragons;
        }
        #endregion Private Methods

    }
    #endregion Classes

}