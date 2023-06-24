# Report command

Currently there is only one type of report:

## ServerUnmappableException

What motivated me to start this project was to solve a somewhat uncommon but annoying-to-solve problem.

MailStore is great at picking through messages in a variety of formats (Exchange journal, Google-specific headers, plus the headers most common mail servers write) and sorting them into the correct archive. When migrating data into MailStore, whether from another archiving product, large existing journal mailbox, ton of unsorted PST exports or various other situations you run into a problem because MailStore will only archive messages for users that exist in MailStore's user list, but typically most companies do not have a single comprehensive list of former employees to work from.

MailStore can toss everything unrecognized into a `@catchall` archive but this is not recommended for a few reasons.

MailStore's logs do describe what is happening with the data needed to solve the problem, here's a snippet of two such messages:

    16:50:12.495 [5] INFO: Processing message: 24/07/2010 09:18:34 UTC 'Google Alert - mailstore', UID 1: 00163630f191bfa05d048c1ea2ca@google.com, UID 2: d6fe90deafeca8ac498a7befaaebe9420c1eedf0
    16:50:12.495 [5] INFO: Retrieving message...
    16:50:12.558 [5] INFO: Message retrieved successfully.
    16:50:12.558 [5] EXCEPTION: MailboxImportWorker.ProcessMailboxMessage
        MailStore.Common.Interfaces.ServerUnmappableException: MailStore is unable to determine where to store this email. Please ensure that e-mail addresses are specified in the users' settings. Senders and recipients: karen@somedomain.example, abby.hernandez@example.com
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessMailboxMessageMultidrop(22_4_0_21151_CLIENT_ndbkhcjeuf mailboxFolder, 22_4_0_21151_CLIENT_ndbkhcjfgk msg, Nullable`1 arcDateUtc)
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessMailboxMessage(22_4_0_21151_CLIENT_ndbkhcjeuf mailboxFolder, 22_4_0_21151_CLIENT_ndbkhcjfgk msg, Nullable`1 arcDateUtc, 22_4_0_21151_CLIENT_wefdfnlqyj& skipReason)
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessSourceFolder(22_4_0_21151_CLIENT_ndbkhcjeuf sourceFolder)
    16:50:12.558 [5] INFO: Item Status: skipped_unmappable
    16:50:12.636 [5] INFO: Processing message: 15/09/2010 18:12:59 UTC 'Google Alert - mailstore', UID 1: 0016364ed2cc8e1a4d0490504798@google.com, UID 2: b775bd5716b3c0245ec38213bd81e33c373f297e
    16:50:12.636 [5] INFO: Retrieving message...
    16:50:12.808 [5] INFO: Message retrieved successfully.
    16:50:12.808 [5] EXCEPTION: MailboxImportWorker.ProcessMailboxMessage
        MailStore.Common.Interfaces.ServerUnmappableException: MailStore is unable to determine where to store this email. Please ensure that e-mail addresses are specified in the users' settings. Senders and recipients: bob@somewhereelse.example, abby.hernandez@example.com
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessMailboxMessageMultidrop(22_4_0_21151_CLIENT_ndbkhcjeuf mailboxFolder, 22_4_0_21151_CLIENT_ndbkhcjfgk msg, Nullable`1 arcDateUtc)
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessMailboxMessage(22_4_0_21151_CLIENT_ndbkhcjeuf mailboxFolder, 22_4_0_21151_CLIENT_ndbkhcjfgk msg, Nullable`1 arcDateUtc, 22_4_0_21151_CLIENT_wefdfnlqyj& skipReason)
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessSourceFolder(22_4_0_21151_CLIENT_ndbkhcjeuf sourceFolder)
    16:50:12.808 [5] INFO: Item Status: skipped_unmappable

In particular, what you need is the *Senders and recipients: karen@somedomain.example.net, abby.hernandez@example.com*. There are a few ways to do it, if you have access to a Linux environment (WSL is perfect) then it's just a bit of [cleverness at the commandline](https://listed.to/p/JoNcBiUbbL) wiill get you a workable list. But in the Windows Server SMB world it isn't uncommon to find administrators with no Linux experience or access to the same. You can do it using the Windows Command Line and a bunch of steps in Excel ([documented here](https://listed.to/p/wBW5zgCwnw)), but that's a pain.

Or you can use my `ServerUnmappableException` report.

Currently the report looks like this:

    @example.com
        abby.hernandez
    @somewhereelse.example
        bob
        karen
    <...>

Each domain will have a list of the addresses found, deduplicated and ready for review. My intention is to either generate a script that will create the needed users, or just call MailStore's API to create the users, archive the mail and remove the users.

There are a few options available at the moment:

Option | Description
--- | ---
--domain | Restrict the report to a list of domains.
--remove | Remove/skip certain domains.

I have tested against logs up to around 700MB, and a sythentic data set of many GB (in a lot comtaining only the e-mail addresses, this would be a real-world log of around 100GB). The amount of memory is directly proportional to the number of domains and e-mail addresses found in the log and it can use a lot of memory for a short time as I optimized for speed at the expensive of consuming more memory than might be completely required. I may work on tuning this in the future, but with the `--domain` flag you can limit the report to just the information you want and memory utilization is quite reasonable.

This is the only *report* currently available so there is an alias at the main command line, but I have a few other ideas of things to add.
