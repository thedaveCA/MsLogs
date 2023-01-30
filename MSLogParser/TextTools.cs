using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;

namespace MailStoreLogFeatures;

public static partial class TextTools
{
    #region Fields
    private static readonly Helpers _charSwapper;

    private static readonly List<string> _linesStartingWith;

    private static readonly SortedList<string, string> _regexPatternList;
    #endregion Fields

    #region Public Constructors
    static TextTools() {
        _charSwapper = new();
        // A list of strings that themselves will remain, but will cause everything on the line
        // afterward to be censored. Matches against the start of the string and after initial
        // "HH:MM:ss.### [thread] " for a variable number of possible threads
        _linesStartingWith = new() {
                    "App Build Date",
                "INFO: Processing messages in folder",
                "INFO: Notify:WriteLine 'Current folder is",
                "INFO: Notify:SetCurrentFolder",
                "INFO:     Source Mailbox Name:",
                "INFO:     Target User Name:",
                "INFO: Authenticating user ",
                "INFO: POP3 command sent: USER ",
                "INFO:     Source Mailbox Name:             example. User",
                "INFO:     Target User Name:                example.user@example.com",

                "MailStore.Common.Interfaces.ServerUnmappableException: MailStore is unable to determine where to store this email. Please ensure that e-mail addresses are specified in the users' settings. Senders and recipients:"
            };
        // _regexPatternList requires a key, and an optional value. The key will be used to
        // matchlines, and if supplied the value will decide what is censored. If the value is not
        // supplied, the matching key regex will be censored instead.
        // NOTE: Maximum one hit per line from each type
        _regexPatternList = new() {
                {"INFO: Processing message: [/0-9]{10} [:0-9]{8} UTC '.*', UID","'.*'" },
                { "License ID:          [0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}","[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}" },
            {@"Creating authentication session for user .* \(.*"," [^ ]+@[^ ]+ " },
            {@"INFO: User [^ ]+@[^ ]+ \(.*\) logged in successfully.",@"[^ ]+@[^ ]+ \(.*\)" },
            {"INFO: Searching folder [^ ]+@[^ ]+ in archive store 39..."," [^ ]+@[^ ]+ " },
            {"INFO: EWS Settings: MailboxEmailAddress=[^ ]+@[^ ]+, ", "=[^ ]+@[^ ]+," },
            {"INFO: Trying to find an entry in the mailbox cache. Key: EwsMailbox.*",@"\$[^\$]+\$[^@]+@[^\$]+\$" },
            {"INFO: Notify:SetTitle '.*@.* <.*@.*>'","'.*'" }
            };
    }
    #endregion Public Constructors

    #region Public Methods
    public static string ObfuscateLogLine(String Line) {
        StringBuilder ThisLine = new();
        _ = ThisLine.Clear().Append(Line);
        int CensorStartingAt;
        // BracketPosition + 2 to count the bracket and following space
        int BracketPosition = ThisLine.ToString().IndexOf(']') + 2;
        // Simple string matching
        foreach (string StringToMatch in _linesStartingWith) {
            if (StringToMatch.Length > ThisLine.Length) {
                // If StringToMatch is longer than the line, it can't match so bail out now
                continue;
            } else if (ThisLine.ToString().Substring(0, StringToMatch.Length).Equals(StringToMatch)) {
                // See if the string is at the start of the line
                _charSwapper.CensorshipMaker(ThisLine, StringToMatch.Length);
            } else if (char.IsWhiteSpace(ThisLine[0])) {
                // Ignore the initial "HH:MM:ss.### [thread] " for a variable number of possible
                // threads Cut off CutThisPart, check the rest of the string
                for (int i = 0; i < ThisLine.Length; i++) {
                    // Skip over any whitespace
                    if (char.IsWhiteSpace(ThisLine[i])) { continue; }
                    // Found the first non-whitespace, so check for a match
                    if (StringToMatch.Length <= ThisLine.ToString().Substring(i).Length && ThisLine.ToString().Substring(i).Substring(0, StringToMatch.Length).Equals(StringToMatch)) {
                        CensorStartingAt = i + StringToMatch.Length;
                        _charSwapper.CensorshipMaker(ThisLine, CensorStartingAt);
                    }
                    // Break the loop to avoid checking the rest of the line
                    break;
                }
            } else if (BracketPosition > 0) {
                // Ignore the initial "HH:MM:ss.### [thread] " for a variable number of possible
                // threads Cut off CutThisPart, check the rest of the string
                string myString = ThisLine.ToString().Substring(BracketPosition);
                if (StringToMatch.Length <= myString.Length && myString.Substring(0, StringToMatch.Length).Equals(StringToMatch)) {
                    CensorStartingAt = BracketPosition + StringToMatch.Length;
                    _charSwapper.CensorshipMaker(ThisLine, CensorStartingAt);
                }
            }
        }
        // Complex regex parsing
        foreach (KeyValuePair<string, string> RegexPattern in _regexPatternList) {
            // If we don't match the key, no need to continue
            Match? RegexMatchResult = Regex.Match(ThisLine.ToString(), RegexPattern.Key);
            if (!RegexMatchResult.Success) {
                continue;
            }

            // If we match the key, RegEx the value to figure out what to remove...
            RegexMatchResult = Regex.Match(ThisLine.ToString(), String.IsNullOrWhiteSpace(RegexPattern.Value) ? RegexPattern.Key : RegexPattern.Value);
            if (!RegexMatchResult.Success) {
                Debug.WriteLine("WARNING: RegEx 'key' matched, but RegEx 'value' didn't find anything to remove.");
                Debug.WriteLine($"         Key:   {RegexPattern.Key}");
                Debug.WriteLine($"         Value: {RegexPattern.Value}");
                //Debug.WriteLine($"         Line: {Line}");
                throw new Exception("RegEx 'key' matched, but RegEx 'value' didn't find anything to remove.");
            }

            _charSwapper.CensorshipMaker(ThisLine, RegexMatchResult.Index, RegexMatchResult.Length);
        }
        return ThisLine.ToString();
    }
    #endregion Public Methods

}