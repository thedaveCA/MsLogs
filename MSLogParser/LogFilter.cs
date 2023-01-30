using System.Text;
using static MailStoreLogFeatures.LogFilterHelpers;

namespace MailStoreLogFeatures;

public static partial class LogFilter
{
    // TODO rewrite filters to run in one pass rather than multiples, and allow them to stack on top of all modifications as a result
    #region Fields
    private static readonly Dictionary<TextFilters, string> _textFiltersDescriptions;
    #endregion Fields

    #region Public Constructors
    static LogFilter() {
        _textFiltersDescriptions = new() {
            {TextFilters.None, "Description not defined" },
// {TextFilters.exceptions, "Find all exceptions."},
            {TextFilters.uniqueexceptions, "Find all unique exceptions."},
        };
    }
    #endregion Public Constructors

    #region Enums
    [Flags]
    public enum TextFilters
    {
        None = 0,
        exceptions = 1 << 1,
        uniqueexceptions = 1 << 2,
    }
    #endregion Enums

    #region Public Methods
    public static string GetDescription(TextFilters InfoRequest) {
        return _textFiltersDescriptions.TryGetValue(InfoRequest, out string? DescriptionText)
            ? DescriptionText
            : $"{_textFiltersDescriptions[TextFilters.None]} {{{InfoRequest}}}";
    }

    public static void TextFilter(TextFilters Filters, TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        TextFilter(Filters, streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }
    // Run multiple text filters on one log without each having to have a (bool andSomethingElse and
    // associated logic)
    public static void TextFilter(TextFilters Filters, TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush) {
        // I know this isn't used, but it might be and it's a pain to change
        _ = TextWriterWriteLine;
        // Set up some internal variables, default to null as initialized, but null-tolerant because
        // I want to Exception if I screw this up
        TextReader internalStreamReader = null!;
        StringBuilder internalStringBuilder = null!;
        StringWriter internalOutputString = null!;

        // If we have no filters then... Maybe this shouldn't have been called? But just in case,
        // we'll call TestTextFilter as we need something here and it's useful for testing without
        // adding anything to the command line parameters
        if (Filters == TextFilters.None) {
            internalStreamReader = internalStreamReader is null ? streamReader : new StringReader(internalStringBuilder.ToString());
            internalStringBuilder = new StringBuilder();
            internalOutputString = new(sb: internalStringBuilder);
            TestTextFilter(internalStreamReader, internalOutputString);
        } else {
            // Loop through each of the enums and take action
            foreach (TextFilters value in Enum.GetValues(typeof(TextFilters))) {
                // None gets called every time, but we don't want to set up new objects. Otherwise,
                // if the foreach loop gave us a value that is set, get ready
                if (value != TextFilters.None && (Filters & value) == value) {
                    // Each pass needs a internalStreamReader, either from outside or the last pass
                    internalStreamReader = internalStreamReader is null ? streamReader : new StringReader(internalStringBuilder.ToString());

                    // Each pass needs a fresh StringBuilder
                    internalStringBuilder = new StringBuilder();
                    internalOutputString = new(sb: internalStringBuilder);

                    // Switch through the possibilities -- NOTE order is controlled by the enums,
                    // not the switch statements
                    switch (value) {
                        case TextFilters.exceptions:
                            Filters &= ~TextFilters.exceptions;
                            FindExceptions(internalStreamReader, internalOutputString);
                            break;
                        case TextFilters.uniqueexceptions:
                            Filters &= ~TextFilters.uniqueexceptions;
                            FindUniqueExceptions(internalStreamReader, internalOutputString);
                            break;
                        //case TextFilters.linenumbers:
                        //    Filters &= ~TextFilters.linenumbers;
                        //    AddLineNumber(internalStreamReader, internalOutputString);
                        //    break;
                        default:
                            // flags that are not implemented can be in the enum, but they can't
                            // actually be called
                            throw new NotImplementedException($"TextFilter \"{value}\" has not been implemented");
                    }
                }
            }
        }

        // Finally write the internalOutputString to the external TextWriterWrite delegate
        TextWriterWrite(internalOutputString.ToString() ?? string.Empty);
        // Courtesy flush
        TextWriterFlush();
    }
    #endregion Public Methods

}