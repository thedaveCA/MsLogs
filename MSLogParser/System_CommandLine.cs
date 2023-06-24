using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Text;
using static MailStoreLogFeatures.LogFilter;
using static MailStoreLogFeatures.LogModification;

namespace MailStoreLogFeatures;
public delegate void TwDelegateEmpty();

public delegate void TwDelgateString(string text);

internal static class System_CommandLine
{
    #region Fields
    private static Command? _allHelp = null;
    private static TextFilters _logTextFilterOptionFlags;
    private static TextModifications _logTextModificationOptionFlags;
    private static RootCommand? _rootCommand = null;
    #endregion Fields

    #region Public Methods
    public static int ShowMenu(string[] args) {
        // Build a couple special commands
        _rootCommand = new("A tool to assist with MailStore log files.") { };
        _allHelp = new("AllHelp", description: "Return all available help at once.") {
            IsHidden = true
        };
        _logTextModificationOptionFlags = TextModifications.None;
        _logTextFilterOptionFlags = TextFilters.None;

        // Build the primary set of Commands
        Command ReportLogCommand = new("report", "Analyze logfile and generate reports");
        Command ModifyLogCommand = new("modify", description: "Modify the log to add, modify or obscure some information.");
        Command FilterLogCommand = new("filter", description: "Filter the log");
        Command GenerateLogCommand = new("generate", description: "Generate sample data to test the other features");
        Command ServerUnmappableExceptionCommand = new("ServerUnmappableException", description: "Generate a report showing unique e-mail addresses from \"ServerUnmappableException\" log entries.");

        // Pick an argument - source log file(s)
        Argument<FileSystemInfo[]> mailstorelogs = new("mailstorelogs", "One or more logs to parse.") {
            Arity = ArgumentArity.OneOrMore,
            HelpName = "mailstore.log"
        };

        // Build the set of Options, these can be added to one or more commands as needed
        Option<string[]> DomainsToReportAdd = new("--domain", "Limit report to one or more domains (default is all).") {
            AllowMultipleArgumentsPerToken = true
        };
        Option<string[]> DomainsToReportBlock = new("--remove", "Remove one or more domains from the report.") {
            AllowMultipleArgumentsPerToken = true
        };
        Option<FileInfo> LogOutput = new("--output", description: "Write the results to this file.");
        LogOutput.AddAlias("-o");
        LogOutput.ArgumentHelpName = "report.txt | report.log";
        LogOutput.Arity = ArgumentArity.ExactlyOne;
        Option<bool> AllowOverwriting = new("--overwrite", "Allow the report to overwrite an existing file.");
        AllowOverwriting.AddAlias("-O");
        Option<bool>? console = new("--console", "Print the report to the console.");
        Option<bool>? interactive = new("--interactive", "Log the output to a slightly buggy interactive window.");

        // Okay, I'm feeling clever. Hopefully not too clever... Dynamically create menu commands
        // for each text modification, and options for each (other) option
        foreach (TextModifications EachDynamicCommand in Enum.GetValues(typeof(TextModifications))) {
            if (EachDynamicCommand == TextModifications.None) {
                continue;
            }

            Command DynamicCommand = new(EachDynamicCommand.ToString()) {
                Handler = CommandHandler.Create<TextModifications, FileSystemInfo[]?, FileSystemInfo?, bool, bool, bool>(MagicHandlerModification)
            };
            DynamicCommand.AddValidator(_ => _logTextModificationOptionFlags |= EachDynamicCommand);
            DynamicCommand.Description = GetDescription(EachDynamicCommand);
            DynamicCommand.Add(mailstorelogs);
            DynamicCommand.AddOption(LogOutput);
            DynamicCommand.AddOption(AllowOverwriting);
            DynamicCommand.AddOption(console);
            DynamicCommand.AddOption(interactive);
            foreach (TextModifications EachDynamicOption in Enum.GetValues(typeof(TextModifications))) {
                if (EachDynamicOption== TextModifications.None || EachDynamicCommand == EachDynamicOption) {
                    continue;
                }

                Option<bool>? DynamicOption = new("--" + EachDynamicOption) {
                    Description = GetDescription(EachDynamicOption)
                };
                DynamicCommand.AddOption(DynamicOption);
                DynamicCommand.AddValidator(result => {
                    if (result.GetValueForOption(DynamicOption)) {
                        _logTextModificationOptionFlags |= EachDynamicOption;
                    }
                });
            }
            ModifyLogCommand.Add(DynamicCommand);
        }
        foreach (TextFilters EachDynamicCommand in Enum.GetValues(typeof(TextFilters))) {
            if (EachDynamicCommand == TextFilters.None) {
                continue;
            }

            Command DynamicCommand = new(EachDynamicCommand.ToString()) {
                Handler = CommandHandler.Create<TextFilters, FileSystemInfo[]?, FileSystemInfo?, bool, bool, bool>(MagicHandlerFilter)
            };
            DynamicCommand.AddValidator(_ => _logTextFilterOptionFlags |= EachDynamicCommand);
            DynamicCommand.Description = GetDescription(EachDynamicCommand);
            DynamicCommand.Add(mailstorelogs);
            DynamicCommand.AddOption(LogOutput);
            DynamicCommand.AddOption(AllowOverwriting);
            DynamicCommand.AddOption(console);
            DynamicCommand.AddOption(interactive);
            foreach (TextFilters EachDynamicOption in Enum.GetValues(typeof(TextFilters))) {
                if (EachDynamicOption == TextFilters.None || EachDynamicCommand == EachDynamicOption) {
                    continue;
                }

                Option<bool>? DynamicOption = new("--" + EachDynamicOption) {
                    Description = GetDescription(EachDynamicOption)
                };
                DynamicCommand.AddOption(DynamicOption);
                DynamicCommand.AddValidator(result => {
                    if (result.GetValueForOption(DynamicOption)) {
                        _logTextFilterOptionFlags |= EachDynamicOption;
                    }
                });
            }
            FilterLogCommand.Add(DynamicCommand);
        }
        Option<IEnumerable<string>> itemsOption = new("--items") { AllowMultipleArgumentsPerToken = true };

        // Build the validation logic for the Arguments/Options
        mailstorelogs.AddValidator(result => {
            if (result.GetValueForArgument(mailstorelogs).Length == 0) {
                result.ErrorMessage = "No filename provided";
            }

            {
                string? ErrorMessage = FileTools.CanFileBeCreated(result.GetValueForArgument(mailstorelogs), true);
                if (ErrorMessage is not null && result.ErrorMessage is not null) {
                    result.ErrorMessage += Environment.NewLine;
                }
                result.ErrorMessage += ErrorMessage;
            }
        });
        // Some output method must be selected, adding validation to the required argument as I
        // don't see a general validation and you can't validate options that were not supplied.
        // Maybe I could assign a default=false somewhere? This works, the only catch is you only
        // get one validation reply from the default argument at a time.
        mailstorelogs.AddValidator(result => {
            if (!result.GetValueForOption(console) && !result.GetValueForOption(interactive) && result.GetValueForOption(LogOutput) is null) {
                result.ErrorMessage += "--output or --console or --interactive required";
            }
        });
        LogOutput.AddValidator(result => {
            bool? AllowExisting = result.GetValueForOption(AllowOverwriting) ? null : false;
            result.ErrorMessage = FileTools.CanFileBeCreated(result.GetValueForOption(LogOutput), AllowExisting);
        });
        AllowOverwriting.AddValidator(result => {
            if (result.GetValueForOption(LogOutput) is null) {
                result.ErrorMessage = "Cannot specify --overwrite without an --output filename.";
            }
        });
        interactive.AddValidator(result => {
            if (result.GetValueForOption(console) && result.GetValueForOption(interactive)) {
                result.ErrorMessage = "Cannot specify both --console and --interactive.";
            }
        });
        // Add the Options to the Commands
        ServerUnmappableExceptionCommand.Add(mailstorelogs);
        ServerUnmappableExceptionCommand.AddOption(LogOutput);
        ServerUnmappableExceptionCommand.AddOption(AllowOverwriting);
        ServerUnmappableExceptionCommand.AddOption(console);
        ServerUnmappableExceptionCommand.AddOption(interactive);
        ServerUnmappableExceptionCommand.AddOption(DomainsToReportAdd);
        ServerUnmappableExceptionCommand.AddOption(DomainsToReportBlock);

        // Add the Handler (actions) to the statically defined commands
        ServerUnmappableExceptionCommand.Handler = CommandHandler.Create<FileSystemInfo[]?, FileSystemInfo?, bool, bool, bool, string[], string[]>(ServerUnmappableExceptionCommandHandler);
        _allHelp.Handler = CommandHandler.Create(TraverseTreeToGetAllHelp);

        // Add the Commands to the rootCommand TODO remove ServerUnmappableExceptionCommand from
        // root and add ReportLogCommand instead when more options are available
        _rootCommand.AddCommand(ModifyLogCommand);
        _rootCommand.AddCommand(FilterLogCommand);
        _rootCommand.AddCommand(ReportLogCommand);
        _rootCommand.AddCommand(ServerUnmappableExceptionCommand);
        ReportLogCommand.AddCommand(ServerUnmappableExceptionCommand);
        //_rootCommand.AddCommand(GenerateLogCommand);
        //rootCommand.AddCommand(JunkGeneratorCommand);
        _rootCommand.AddCommand(_allHelp);

        //// Add any global options (via the _rootCommand)
        //Option<bool> verbose = new("-v", "Provide more detailed output");
        //verbose.AddAlias("--verbose");
        //_rootCommand.AddGlobalOption(verbose);
        //Option<bool> performance = new("-p", "Provide details about how long operations took to complete");
        //performance.AddAlias("--performance");
        //_rootCommand.AddGlobalOption(performance);

        // And finally, invoke the menu
        return _rootCommand.Invoke(args);
        /*
        // Also possible to build an object and Do More Things
        //
        //var parser = new CommandLineBuilder(rootCommand)
        //    .UseDefaults()
        //    .UseHelp(ctx => {
        //        ctx.HelpBuilder.CustomizeSymbol(overwrite,
        //            firstColumnText: "--color <Black, White, Red, or Yellow>",
        //            secondColumnText: "Specifies the foreground color. " +
        //                "Choose a color that provides enough contrast " +
        //                "with the background color. " +
        //                "For example, a yellow foreground can't be read " +
        //                "against a light mode background.");
        //    })
        //.Build();
        //return parser.Invoke(args);
        */
    }
    #endregion Public Methods

    #region Private Methods
    private static void MagicHandlerFilter(LogFilter.TextFilters textFilters, FileSystemInfo[]? mailstorelogs, FileSystemInfo? output, bool overwrite, bool console, bool interactive) {
        // The idea here is that the other generic handlers just call this method, adding the needed
        // flags, and we'll figure it out.
        StringBuilder stringBuilder = null!;
        TwDelgateString TextWriterWrite = null!;
        TwDelgateString TextWriterWriteLine = null!;
        TwDelegateEmpty TextWriterFlush = null!;
        TwDelegateEmpty TextWriterDispose = null!;
        textFilters |= _logTextFilterOptionFlags;
            // This was interesting to write. I create a delegate so that it is possible to have
            // writes hit multiple TextWriters at the same time, then pass the delegate to the
            // backend method.
            List<TextWriter> TextWriters = new();

            if (output != null) {
                TextWriters.Add(new StreamWriter(output.FullName));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }

            if (console) {
                TextWriters.Add(new StreamWriter(Console.OpenStandardOutput()));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }
            if (interactive) {
                stringBuilder = new();
                TextWriters.Add(new StringWriter(stringBuilder));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }

            foreach (FileSystemInfo LogFile in mailstorelogs!) {
                Console.WriteLine($"Processing {LogFile.Name}");
                StreamReader streamReader = new(LogFile.FullName);
                MailStoreLogFeatures.LogFilter.TextFilter(textFilters, streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
            }
            TextWriterFlush();
        if (interactive) {
            TheView.TheView theView = new();
            theView.SetText(stringBuilder!.ToString());
            theView.Show();
        }
    }
    private static void MagicHandlerModification(LogModification.TextModifications textModifications, FileSystemInfo[]? mailstorelogs, FileSystemInfo? output, bool overwrite, bool console, bool interactive) {
        // The idea here is that the other generic handlers just call this method, adding the needed
        // flags, and we'll figure it out.
        StringBuilder stringBuilder = null!;
        TwDelgateString TextWriterWrite = null!;
        TwDelgateString TextWriterWriteLine = null!;
        TwDelegateEmpty TextWriterFlush = null!;
        TwDelegateEmpty TextWriterDispose = null!;
        textModifications |= _logTextModificationOptionFlags;
            // This was interesting to write. I create a delegate so that it is possible to have
            // writes hit multiple TextWriters at the same time, then pass the delegate to the
            // backend method.
            List<TextWriter> TextWriters = new();

            if (output != null) {
                TextWriters.Add(new StreamWriter(output.FullName));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }

            if (console) {
                TextWriters.Add(new StreamWriter(Console.OpenStandardOutput()));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }
            if (interactive) {
                stringBuilder = new();
                TextWriters.Add(new StringWriter(stringBuilder));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }

            foreach (FileSystemInfo LogFile in mailstorelogs!) {
                Console.WriteLine($"Processing {LogFile.Name}");
                StreamReader streamReader = new(LogFile.FullName);
                MailStoreLogFeatures.LogModification.TextModification(textModifications, streamReader, TextWriterWrite, TextWriterWriteLine, TextWriterFlush);
            }
            TextWriterFlush();
        if (interactive) {
            TheView.TheView theView = new();
            theView.SetText(stringBuilder!.ToString());
            theView.Show();
        }
    }
    private static void ServerUnmappableExceptionCommandHandler(FileSystemInfo[]? mailstorelogs, FileSystemInfo? output, bool overwrite, bool console, bool interactive, string[] domain, string[] remove) {
        StreamReader streamReader = null!;
        StringBuilder stringBuilder = null!;
        TwDelgateString TextWriterWrite = null!;
        TwDelgateString TextWriterWriteLine = null!;
        TwDelegateEmpty TextWriterFlush = null!;
        TwDelegateEmpty TextWriterDispose = null!;
        try {
            List<TextWriter> TextWriters = new();

            if (output != null) {
                TextWriters.Add(new StreamWriter(output.FullName));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }

            if (console) {
                TextWriters.Add(new StreamWriter(Console.OpenStandardOutput()));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }
            if (interactive) {
                stringBuilder = new();
                TextWriters.Add(new StringWriter(stringBuilder));
                TextWriterWrite += TextWriters[TextWriters.Count - 1].Write;
                TextWriterWriteLine += TextWriters[TextWriters.Count - 1].WriteLine;
                TextWriterFlush += TextWriters[TextWriters.Count - 1].Flush;
                TextWriterDispose += TextWriters[TextWriters.Count - 1].Dispose;
            }
            MailStoreLogFeatures.LogModification.ServerUnmappableExceptionParser serverUnmappableExceptionParser = new();
            serverUnmappableExceptionParser.DomainsToReport(domain.Length > 0 ? domain : null, remove.Length > 0 ? remove : null);
            foreach (FileSystemInfo LogFile in mailstorelogs!) {
                Console.WriteLine($"Processing {LogFile.Name}");
                streamReader = new(LogFile.FullName);
                serverUnmappableExceptionParser.AddLogfile(streamReader);
            }
            StringBuilder stringBuilderOut = new();
            StringWriter MySW = new(stringBuilderOut);
            serverUnmappableExceptionParser.GenerateReport(MySW);

            TextWriterWrite(stringBuilderOut.ToString());

            TextWriterFlush();
        } finally {
            TextWriterDispose();
            streamReader?.Dispose();
        }
        if (interactive) {
            TheView.TheView theView = new();
            theView.SetText(stringBuilder!.ToString());
            theView.Show();
        }
    }
    // This loops through the commands and subcommand, invoking "help" on each to provide one dump
    // of the entire helpfile at once
    private static void TraverseTreeToGetAllHelp(Command? command = null) {
        command ??= _rootCommand;
        if (command is not null) {
            if (command != _allHelp) {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(command.Name);
                Console.ResetColor();
                _ = command.Invoke(new string[] { command.Name, "--help" });
            }
            foreach (Command subCommand in command.Subcommands) {
                TraverseTreeToGetAllHelp(subCommand);
            }
        }
    }
    #endregion Private Methods

}