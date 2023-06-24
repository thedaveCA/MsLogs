using System.Text.RegularExpressions;

namespace MailStoreLogFeatures;
internal static partial class LogModificationHelpers
{
    #region Internal Methods
    internal static void AddLineNumber(TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        AddLineNumber(streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }
    /// <summary>Add line numbers to significant lines</summary>
    internal static void AddLineNumber(TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush) {
        // I know this isn't used, but it might be and it's a pain to change
        _ = TextWriterWrite;
        const string RegexPattern = @"^ {0,11}([\.:|0-9]{10,14})";
        const int LineNumberWidth = 7;
        string NoNumberPrepend = new(' ', LineNumberWidth + 2);
        int LineCount = 0;
        while (streamReader.Peek() != -1) {
            string v = (streamReader.ReadLine() ?? string.Empty);
            Match? regexTimeParse = Regex.Match(v, RegexPattern);
            if (regexTimeParse.Success) {
                TextWriterWriteLine($"{LineCount,LineNumberWidth}: " + v);
                LineCount++;
            } else {
                TextWriterWriteLine(NoNumberPrepend + v);
            }
        }
        TextWriterFlush();
    }
    // Take a TextReader in and TextWriter out, set up TextWriter delegates so that this method can
    // be used easily in some cases
    internal static void CensorPrivates(TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        CensorPrivates(streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }
    internal static void CensorPrivates(TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush) {
        // I know this isn't used, but it might be and it's a pain to change
        _ = TextWriterWrite;
        while (streamReader.Peek() != -1) {
            TextWriterWriteLine(MailStoreLogFeatures.TextTools.ObfuscateLogLine(streamReader.ReadLine() ?? string.Empty));
        }
        TextWriterFlush();
    }
    // Goes through the motions of parsing text, but doesn't change anything
    internal static void TestTextModification(TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        TestTextModification(streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }
    internal static void TestTextModification(TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush) {
        // I know this isn't used, but it might be and it's a pain to change
        _ = TextWriterWrite;
        while (streamReader.Peek() != -1) {
            TextWriterWriteLine(streamReader.ReadLine() ?? string.Empty);
        }
        TextWriterFlush();
    }
    #endregion Internal Methods

}