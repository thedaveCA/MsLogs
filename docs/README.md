# MailStore Server log parser

## What is this?

MailStore Server log parser is a console tool to make MailStore's logs a bit easier to work with, with a focus on larger logfiles.

All commands can take one or more logfiles on the command line, and write the results to a `--output` file on disk, `--console` to review directly, or `--interactive` to get a list you can scroll and review in real-time.

The interactive mode is very basic, but I may expand on it further in the future. Currently it holds the entire result file in memory, I have 64GB RAM on my workstation so memory usage is not my first concern but be aware when you're working with large logs. Eventually I want to read the version from disk when `--output` and `--interactive` are combined.

In `--output` and `--console` mode the inputs and outputs are streamed and buffered into reasonably size chunks to keep memory usage reasonable. `--output` and either the `--console` or `--interactive` can be used together.

## me

I've been programming in C# for a few weeks, learning and playing around, this is my first attempt at a tool that might be useful to anyone but me. Feedback, bugs, insight into better coding, etc, all quite welcome!

## What does it do?

### modify

Parse a logfile and return a modified version.

modification | notes
--- | ---
  privacy | Remove (some) potentially personal/private information.
  maketimesrelative | Timestamp from the start of execution, and individual line timing.
  linenumbers | Add line numbers to each (significant) line.

All of the modify commands can perform multiple modifications at once.

See the full [modify documentation](command_modify.md).

### filter

Parse a logfile and return a limited set of information.

| Feature | Notes |
| --- | --- |
exceptions | List exceptions.
uniqueexceptions | List all unique exceptions.

These are very basic proof-of-concept, *exceptions* will print all exceptions, while *uniqueexceptions* will list the first instance of each exception but skip duplicates. I'll be extending these as needed, in particular to cover cases where you need to work with a multi-GB logfile that contains misc daily operation entries intermixed with an Index Rebuild.

### report

#### ServerUnmappableException

Parse a MailStore log file containing `ServerUnmappableException` exceptions and return a deduplicated list of e-mail addresses to assist with importing journal mailboxes or dumps from other archiving products into MailStore.

See the full [documentation](command_report.md).

## Contact

Open an [issue](https://github.com/thedaveCA/MailStore-Log-Parser/issues) or [discussion](https://github.com/thedaveCA/MailStore-Log-Parser/discussions) on GitHub, or [mslogparser@thedave.dev](mailto:mslogparser@thedave.dev) or see [thedave.dev](https://thedave.dev/) for other places you can reach me.

## License

[GNU GPLv3](/LICENSE)
