using System.Text.RegularExpressions;

namespace MailStoreLogFeatures;
internal static partial class LogFilterHelpers
{
    #region Internal Methods
    // Take a TextReader in and TextWriter out, set up TextWriter delegates so that this method can
    // be used easily in some cases
    internal static void FindExceptions(TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        FindExceptions(streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }
    internal static void FindExceptions(TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush) {
        // I know this isn't used, but it might be and it's a pain to change
        bool WriteLines = false;
        string Line;
        const string RegexPattern = "^([\\.:|0-9]{12})";
        _ = TextWriterWrite;
        while (streamReader.Peek() != -1) {
            Line = (streamReader.ReadLine() ?? string.Empty);
            if (Regex.Match(Line, RegexPattern).Success) {
                WriteLines = false;
            }

            if (Line.Contains("EXCEPTION")) {
                WriteLines = true;
            }

            if (WriteLines) {
                TextWriterWriteLine(Line);
            }
        }
        TextWriterFlush();
    }
    internal static void FindUniqueExceptions(TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        FindUniqueExceptions(streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }
    internal static void FindUniqueExceptions(TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush) {
        // I know this isn't used, but it might be and it's a pain to change
        bool WriteLines = false;
        string Line;
        HashSet<string> ExceptionHashList = new();
        // RegEx matches the timestamp, plus [thread] number
//        const string RegexPattern = "^([\\.:|0-9]{12}) +\\[[0-9]+\\] ";
        const string RegexPattern = @"^([\.:|0-9]{12})\s+\[[0-9]+\]\s+";
        _ = TextWriterWrite;
        while (streamReader.Peek() != -1) {
            Line = (streamReader.ReadLine() ?? string.Empty);
            Match RegexMatch = Regex.Match(Line, RegexPattern);
            // If this line has a timestamp, stop writing lines (otherwise it might be allowed through as exceptions can be multi-line)
            if (RegexMatch.Success) {
                WriteLines = false;
            }
            // If this line is an exception, check if it is new
            if (Line.Contains("EXCEPTION")) {
                // Add to the hashlist, if it is new then we need to start writing here
                WriteLines = ExceptionHashList.Add(Line.Substring(RegexMatch.Length));
            }

            // Write the text based on the decision above. 
            if (WriteLines) {
                TextWriterWriteLine(Line);
            }
        }
        TextWriterFlush();
    }
    // Goes through the motions of parsing text, but doesn't change anything
    internal static void TestTextFilter(TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        TestTextFilter(streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }
    internal static void TestTextFilter(TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush) {
        // I know this isn't used, but it might be and it's a pain to change
        _ = TextWriterWrite;
        while (streamReader.Peek() != -1) {
            TextWriterWriteLine(streamReader.ReadLine() ?? string.Empty);
        }
        TextWriterFlush();
    }
    #endregion Internal Methods

}