# Modify command

### Modify

This is a set of features to modify logs to make them a bit easier to work with. The internal help describes which features can be combined with other features.

| Feature | Notes |
| --- | --- |
| privacy <mailstore.log> | Remove (some) potentially personal/private information. |
| maketimesrelative | Timestamp from the start of execution, and individual line timing. |
| linenumbers | Add line numbers to each (significant) line

#### privacy

This is a first attempt at obscuring/removing private information from MailStore's logs. It's a proof of concept and only matches a some of the entries that potentially contain personal information, as time permits I'll add more patterns. Regardless, the log should be reviewed before being published as there will likely be many log entries that are not obscured. This is obfscuation, not encryption.

For anyone that didn't catch that, I'll say it again: *This is obfscuation, not encryption*.

Each letter will be swapped for another letter, each number for another number, the swapping is done consistently throughout a single log but different session to session.

Here's an example of MailStore's logs:

    16:50:11.261 [5] INFO: Processing messages in folder abby.hernandez@example.com/Exchange abby.hernandez/Inbox
    16:50:11.261 [5] INFO: Notify:SetCurrentFolder 'abby.hernandez@example.com/Exchange abby.hernandez/Inbox'
    16:50:11.261 [5] INFO: Notify:WriteLine 'Current folder is abby.hernandez@example.com/Exchange abby.hernandez/Inbox'
    16:50:11.261 [5] INFO: Notify:WriteLine 'Reading folder contents...'
    16:50:11.792 [5] INFO: Notify:WriteLine 'Processing...'
    16:50:11.808 [5] INFO: Processing message: 27/05/2010 04:35:03 UTC 'Google Alert - mailstore', UID 1: 000e0cd1b66e07099104878bea9e@google.com, UID 2: 202b6e05852f1c1d6a4101b0e4050937f10ad069

And here is the private version:

    16:50:11.261 [5] INFO: Processing messages in folder dzzm.khlqdqvhp@hxdourh.jyo/Hxjkdqah dzzm.khlqdqvhp/Fqzyx
    16:50:11.261 [5] INFO: Notify:SetCurrentFolder 'dzzm.khlqdqvhp@hxdourh.jyo/Hxjkdqah dzzm.khlqdqvhp/Fqzyx'
    16:50:11.261 [5] INFO: Notify:WriteLine 'Current folder is dzzm.khlqdqvhp@hxdourh.jyo/Hxjkdqah dzzm.khlqdqvhp/Fqzyx'
    16:50:11.261 [5] INFO: Notify:WriteLine 'Reading folder contents...'
    16:50:11.792 [5] INFO: Notify:WriteLine 'Processing...'
    16:50:11.808 [5] INFO: Processing message: 27/05/2010 04:35:03 UTC 'Ayyarh Drhli - odfrciylh', UID 1: 000e0cd1b66e07099104878bea9e@google.com, UID 2: 202b6e05852f1c1d6a4101b0e4050937f10ad069

And a few lines from another run:

    16:50:11.261 [5] INFO: Processing messages in folder mccf.sdetmtbdi@dumrxqd.hpr/Duhsmtvd mccf.sdetmtbdi/Ztcpu
    16:50:11.261 [5] INFO: Notify:SetCurrentFolder 'mccf.sdetmtbdi@dumrxqd.hpr/Duhsmtvd mccf.sdetmtbdi/Ztcpu'
    16:50:11.261 [5] INFO: Notify:WriteLine 'Current folder is mccf.sdetmtbdi@dumrxqd.hpr/Duhsmtvd mccf.sdetmtbdi/Ztcpu'
    16:50:11.261 [5] INFO: Notify:WriteLine 'Reading folder contents...'
    16:50:11.792 [5] INFO: Notify:WriteLine 'Processing...'
    16:50:11.808 [5] INFO: Processing message: 27/05/2010 04:35:03 UTC 'Vppvqd Mqdew - rmzqnwped', UID 1: 000e0cd1b66e07099104878bea9e@google.com, UID 2: 202b6e05852f1c1d6a4101b0e4050937f10ad069

MailStore's timestamps and message timestamp information will be left in the logs untouched, my intention is to obscure account information, server informatino, e-mail addresses, folder names and subject lines.

Because the obfucastion is consistent throughout the log if you spot a problem with `dzzm.khlqdqvhp@hxdourh.jyo`'s account you can find the other related log entries without knowing the user's actual name, and a MailStore Server administrator with the original version of the log should be able to match up the lines based on timestamps. Can you spot that `dzzm.khlqdqvhp@hxdourh.jyo` and `mccf.sdetmtbdi@dumrxqd.hpr` are the same? Probably.

Could this be reversed? With some effort and knowledge of the environment, almost definitely. 

#### maketimesrelative

    0:00:00.344   0:00:00.000  [5] INFO: Processing messages in folder mggf.xydwmwvyi@yumrcky.ehr/Yuexmwty mggf.xydwmwvyi/Owghu
    0:00:00.344   0:00:00.000  [5] INFO: Notify:SetCurrentFolder 'mggf.xydwmwvyi@yumrcky.ehr/Yuexmwty mggf.xydwmwvyi/Owghu'
    0:00:00.344   0:00:00.000  [5] INFO: Notify:WriteLine 'Current folder is mggf.xydwmwvyi@yumrcky.ehr/Yuexmwty mggf.xydwmwvyi/Owghu'
    0:00:00.344   0:00:00.531  [5] INFO: Notify:WriteLine 'Reading folder contents...'
    0:00:00.875   0:00:00.016  [5] INFO: Notify:WriteLine 'Processing...'
    0:00:00.891   0:00:00.047  [5] INFO: Processing message: 27/05/2010 04:35:03 UTC 'Thhtky Mkyds - rmokpshdy', UID 1: 000e0cd1b66e07099104878bea9e@google.com, UID 2: 202b6e05852f1c1d6a4101b0e405093
    0:00:00.938   0:00:00.078  [5] INFO: Retrieving message...

MailStore's timestamps are replaced with two timestamps, one indicating the time elapsed since the first line of the log, the other the amount of time the current log entry took to complete.

This should make it easy to spot patterns and outliars when trying to track down a performance issue. It should be clever enough to avoid mangling multi-line log entries such as exceptions: 

    0:00:00.891   0:00:00.047  [5] INFO: Processing message: 27/05/2010 04:35:03 UTC 'Lttlzr Izrnf - jiuzhftnr', UID 1: 000e0cd1b66e07099104878bea9e@google.com, UID 2: 202b6e05852f1c1d6a4101b0e4050937f10ad069
    0:00:00.938   0:00:00.078  [5] INFO: Retrieving message...
    0:00:01.016   0:00:00.265  [5] INFO: Message retrieved successfully.
    0:00:01.281   0:00:00.265  [5] EXCEPTION: MailboxImportWorker.ProcessMailboxMessage
    MailStore.Common.Interfaces.ServerUnmappableException: MailStore is unable to determine where to store this email. Please ensure that e-mail addresses are specified in the users' settings. Senders and recipients: xuissr.a.jkkzqnr@htjrktjbise.rvijbzr, iwwe.yrnsisxro@rvijbzr.ktj
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessMailboxMessageMultidrop(22_4_0_21151_CLIENT_ndbkhcjeuf mailboxFolder, 22_4_0_21151_CLIENT_ndbkhcjfgk msg, Nullable`1 arcDateUtc)
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessMailboxMessage(22_4_0_21151_CLIENT_ndbkhcjeuf mailboxFolder, 22_4_0_21151_CLIENT_ndbkhcjfgk msg, Nullable`1 arcDateUtc, 22_4_0_21151_CLIENT_wefdfnlqyj& skipReason)
        at 22_4_0_21151_CLIENT_ndbkhcjfbb.ProcessSourceFolder(22_4_0_21151_CLIENT_ndbkhcjeuf sourceFolder)
    0:00:01.281   0:00:00.047  [5] INFO: Item Status: skipped_unmappable

#### linenumbers

     56: 16:50:11.261 [5] INFO: Processing messages in folder lyyg.jeodldsen@exlbrie.fqb/Exfjldwe lyyg.jeodldsen/Tdyqx
     57: 16:50:11.261 [5] INFO: Notify:SetCurrentFolder 'lyyg.jeodldsen@exlbrie.fqb/Exfjldwe lyyg.jeodldsen/Tdyqx'
     58: 16:50:11.261 [5] INFO: Notify:WriteLine 'Current folder is lyyg.jeodldsen@exlbrie.fqb/Exfjldwe lyyg.jeodldsen/Tdyqx'
     59: 16:50:11.261 [5] INFO: Notify:WriteLine 'Reading folder contents...'
     60: 16:50:11.792 [5] INFO: Notify:WriteLine 'Processing...'
     61: 16:50:11.808 [5] INFO: Processing message: 27/05/2010 04:35:03 UTC 'Wqqwie Lieou - bltizuqoe', UID 1: 000e0cd1b66e07099104878bea9e@google.com, UID 2: 202b6e05852f1c1d6a4101b0e4050937f10ad0
     62: 16:50:11.855 [5] INFO: Retrieving message...

This one is pretty self-explanatory, it prepends line numbers to every line in the log.

