using System.Text;
using static MailStoreLogFeatures.LogModificationHelpers;

namespace MailStoreLogFeatures;

public static partial class LogModification
{
    #region Fields
    private static readonly Dictionary<TextModifications, string> _textModificationsDescriptions;
    #endregion Fields

    #region Public Constructors
    static LogModification() {
        _textModificationsDescriptions = new() {
            {TextModifications.None, "Description not defined" },
            {TextModifications.privacy, "Remove (some) potentially personal/private information."},
            {TextModifications.linenumbers,"Add line numbers to each (significant) line."},
            {TextModifications.maketimesrelative,"Timestamp from the start of execution, and individual line timing." },
            //{TextModifications.uniqueexceptions,"" },
        };
    }
    #endregion Public Constructors

    #region Enums
    // The numerical order of the flags controls the order in which they're processed
    [Flags]
    public enum TextModifications
    {
        None = 0,
        privacy = 1 << 1,
        maketimesrelative = 1 << 2,
        linenumbers = 1 << 3,
        //uniqueexceptions = 1 << 4,
    }
    #endregion Enums

    #region Public Methods
    public static string GetDescription(TextModifications InfoRequest) {
        return _textModificationsDescriptions.TryGetValue(InfoRequest, out string DescriptionText)
            ? DescriptionText
            : $"{_textModificationsDescriptions[TextModifications.None]} {{{InfoRequest}}}";
    }

    public static void TextModification(TextModifications Modifications, TextReader streamReader, TextWriter OutputString) {
        TwDelgateString? TextWriterWrite = OutputString.Write;
        TwDelgateString? TextWriterWriteLine = OutputString.WriteLine;
        TwDelegateEmpty? TextWriterFlush = OutputString.Flush;
        TextModification(Modifications, streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
    }
    // Run multiple text modifications on one log without each having to have a (bool
    // andSomethingElse and associated logic)
    public static void TextModification(TextModifications Modifications, TextReader streamReader, TwDelgateString TextWriterWrite, TwDelgateString TextWriterWriteLine, TwDelegateEmpty TextWriterFlush) {
        // I know this isn't used, but it might be and it's a pain to change
        _ = TextWriterWriteLine;
        // Set up some internal variables, default to null as initialized, but null-tolerant because
        // I want to Exception if I screw this up
        TextReader internalStreamReader = null!;
        StringBuilder internalStringBuilder = null!;
        StringWriter internalOutputString = null!;

        // If we have no modifications then... Maybe this shouldn't have been called? But just in
        // case, we'll call TestTextModification as we need something here and it's useful for
        // testing without adding anything to the command line parameters
        if (Modifications == TextModifications.None) {
            internalStreamReader = internalStreamReader is null ? streamReader : new StringReader(internalStringBuilder.ToString());
            internalStringBuilder = new StringBuilder();
            internalOutputString = new(sb: internalStringBuilder);
            TestTextModification(internalStreamReader, internalOutputString);
        } else {
            // Loop through each of the enums and take action
            foreach (TextModifications value in Enum.GetValues(typeof(TextModifications))) {
                // None gets called every time, but we don't want to set up new objects. Otherwise,
                // if the foreach loop gave us a value that is set, get ready
                if (value != TextModifications.None && (Modifications & value) == value) {
                    // Each pass needs a internalStreamReader, either from outside or the last pass
                    internalStreamReader = internalStreamReader is null ? streamReader : new StringReader(internalStringBuilder.ToString());

                    // Each pass needs a fresh StringBuilder
                    internalStringBuilder = new StringBuilder();
                    internalOutputString = new(sb: internalStringBuilder);

                    // Switch through the possibilities -- NOTE order is controlled by the enums,
                    // not the switch statements
                    switch (value) {
                        case TextModifications.privacy:
                            Modifications &= ~TextModifications.privacy;
                            CensorPrivates(internalStreamReader, internalOutputString);
                            break;
                        case TextModifications.linenumbers:
                            Modifications &= ~TextModifications.linenumbers;
                            AddLineNumber(internalStreamReader, internalOutputString);
                            break;
                        case TextModifications.maketimesrelative:
                            Modifications &= ~TextModifications.maketimesrelative;
                            MakeTimesRelative(internalStreamReader, internalOutputString);
                            break;
                        //case TextModifications.uniqueexceptions:
                        //    Modifications &= ~TextModifications.uniqueexceptions;
                        //    LogFilterHelpers.FindUniqueExceptions(internalStreamReader, internalOutputString);
                        //    break;
                        default:
                            // flags that are not implemented can be in the enum, but they can't
                            // actually be called
                            throw new NotImplementedException($"TextModification \"{value}\" has not been implemented");
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