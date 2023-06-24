using System.Globalization;
using System.Text.RegularExpressions;

namespace MailStoreLogFeatures;

internal static partial class LogModificationHelpers
{
    #region Internal Methods
    // Take a TextReader in and TextWriter out, set up TextWriter delegates so that this method can
    // be used easily in some cases
    internal static void MakeTimesRelative(TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        MakeTimesRelative(streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }

    internal static void MakeTimesRelative(TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush, bool AndCensor = false) {
        // NOTE: There is a bunch of null forgiveness here. I think that's okay, it is due to how
        // the looping works, I could probably handle it by setting some dummy values during the
        // first pass, but at the moment I'm leaving it as-is. TODO research: Is there a better way?
        // I think C# might do this internally somehow
        DateTime referenceDate = new(2022, 1, 1);
        const string RegexPattern = @"^([\.:|0-9]{12})";
        DateTime? LogStartedAt = null;
        DateTime? PreviousLineStartedAt = null;
        int dayCount = 0;
        bool? LastLineHadTime = null;

        string? LastCommandStartedAt = null, LastRemainderOfLogLine = null;
        string? ThisCommandLasted = null, ThisCommandStartedAt = null;
        // Read a line of the file at a time
        while (streamReader.Peek() != -1) {
            string Line = AndCensor
                ? MailStoreLogFeatures.TextTools.ObfuscateLogLine(streamReader.ReadLine() ?? string.Empty)
                : streamReader.ReadLine() ?? string.Empty;
            Match? regexTimeParse = Regex.Match(Line, RegexPattern);
            if (regexTimeParse.Success) {
                string RemainderOfLogLine = Line.Substring(12);
                string LogStartedAtString = regexTimeParse.Value;
                if (TimeOnly.TryParse(LogStartedAtString, null, DateTimeStyles.None, out TimeOnly timeOnly)) {
                    // If this is the first time we've encountered a time, set this as the start time
                    if (LogStartedAt is null) {
                        LogStartedAt ??= referenceDate + timeOnly.ToTimeSpan();
                        PreviousLineStartedAt ??= (DateTime)LogStartedAt;
                        LastCommandStartedAt = LogStartedAtString + '*';
                        LastRemainderOfLogLine = Line;
                        //WriteTheLineToTheLog(LastLineHadTime, TextWriterWrite, TextWriterWriteLine, LastCommandStartedAt, RemainderOfLogLine, ThisCommandLasted!);
                    } else {
                        DateTime CurrentLineStartedAt = referenceDate + timeOnly.ToTimeSpan();

                        bool RollOverToday;
                        // If PreviousLineStartedAt is greater than CurrentLineStartedAt we have
                        // rolled over to a new day
                        if ((PreviousLineStartedAt ?? CurrentLineStartedAt) > CurrentLineStartedAt) {
                            // If the current time is less than the previous time, it's a brand new
                            // day. No way to tell if we've crossed multiple days, but that
                            // shouldn't be too realistic
                            RollOverToday = true;
                            dayCount++;
                        } else {
                            RollOverToday = false;
                        }
                        // Calculate time between CurrentLineStartedAt and PreviousLineStartedAt
                        TimeSpan TimingOfThisCommandDelta = (TimeSpan)(CurrentLineStartedAt + (RollOverToday ? TimeSpan.FromDays(1) : TimeSpan.Zero) - PreviousLineStartedAt!);
                        ThisCommandLasted = $"{(int)TimingOfThisCommandDelta.TotalHours,3:D1}:{TimingOfThisCommandDelta.Minutes,2:D2}:{TimingOfThisCommandDelta.Seconds,2:D2}.{TimingOfThisCommandDelta.Milliseconds,0:D3}";
                        // Calculate time between CurrentLineStartedAt and LogStartedAt
                        TimeSpan StartOfThisCommandDelta = (TimeSpan)(CurrentLineStartedAt + TimeSpan.FromDays(dayCount) - LogStartedAt);
                        ThisCommandStartedAt = $"{(int)StartOfThisCommandDelta.TotalHours,3:D1}:{StartOfThisCommandDelta.Minutes,2:D2}:{StartOfThisCommandDelta.Seconds,2:D2}.{StartOfThisCommandDelta.Milliseconds,0:D3}";
                        WriteTheLineToTheLog(LastLineHadTime, TextWriterWrite, TextWriterWriteLine, LastCommandStartedAt!, LastRemainderOfLogLine!, ThisCommandLasted);
                        PreviousLineStartedAt = CurrentLineStartedAt;

                        LastCommandStartedAt = ThisCommandStartedAt;
                        LastRemainderOfLogLine = RemainderOfLogLine;
                        LastLineHadTime = true;
                    }
                } else {
                    throw new Exception();
                }
            } else {
                // Write the line to the log
                WriteTheLineToTheLog(LastLineHadTime, TextWriterWrite, TextWriterWriteLine, LastCommandStartedAt!, LastRemainderOfLogLine!, ThisCommandLasted!);
                // Else no time found, so just add the line
                LastLineHadTime = false;
                LastRemainderOfLogLine = Line;
            }
        }
        // And write the very last line
        WriteTheLineToTheLog(LastLineHadTime, TextWriterWrite, TextWriterWriteLine, LastCommandStartedAt!, LastRemainderOfLogLine!, ThisCommandLasted!);
        TextWriterFlush();
        return;
    }

    // Split off the code that writes the log entry for easier customization as it gets called at
    // least 4 times
    internal static void WriteTheLineToTheLog(bool? LastLineHadTime, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, string LastCommandStartedAt, string LastRemainderOfLogLine, string ThisCommandLasted) {
        if (LastRemainderOfLogLine is not null) {
            if (LastLineHadTime == true) {
                TextWriterWrite(LastCommandStartedAt + ' ');
                TextWriterWrite(ThisCommandLasted + ' ');
            }
            TextWriterWriteLine(LastRemainderOfLogLine);
        }
    }
    #endregion Internal Methods

}