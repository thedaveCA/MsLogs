using System.Diagnostics;
using System.Text;
using Humanizer;

namespace MailStoreLogFeatures;

public static class JunkGenerator
{
    #region Fields
    private static readonly Random _rand;
    #endregion Fields

    #region Public Constructors
    // TODO Fixed seed, oh yes! Very Random!
    static JunkGenerator() => _rand = new(0);
    #endregion Public Constructors

    #region Public Methods
    public static void ServerUnmappableException(string FakeLogToWrite, int TargetNumberOfLines, bool MakeEveryUserAndDomainUnique = false, bool MakeEveryNameUnique = false, int PercentageOfJibberishBeingAdded = 0) {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        const string EmailAddressSource = "C:\\Users\\Dave\\source\\ScratchPad\\LogParser\\LogParser\\addresslist.txt";
        int lineCount = 0;
        int addressCount = 0;
        List<string> addressList = new();
        const string _serverUnmappableExceptionText = "\tMailStore.Common.Interfaces.ServerUnmappableException: MailStore is unable to determine where to store this email. Please ensure that e-mail addresses are specified in the users' settings. Senders and recipients: ";
        const string OutputSep = ", ";
        int[] addressCounts = new int[6];
        Console.WriteLine("!!! Generating fake data !!!");
        try {
            using StreamReader streamReader = new(EmailAddressSource);
            while (!streamReader.EndOfStream) {
                string LineIn = streamReader.ReadLine() ?? string.Empty;
                foreach (string Line in LineIn.Split(new char[] { ' ', ',' })) {
                    if (Line.IndexOf("@") > 0) {
                        if (!addressList.Contains(Line)) {
                            addressList.Add(Line);
                        }
                        addressCount++;
                    }
                }
                lineCount++;
            }
        } catch (IOException e) {
            Console.WriteLine("An error occurred: '{0}'", e);
        }
        TimeSpan TimeReading = stopwatch.Elapsed;
        stopwatch.Restart();

        Console.WriteLine($"Imported {addressList.Count} unique addresses (from {lineCount} lines). Generating {TargetNumberOfLines} lines of log entries.");
        StringBuilder OutputString = new(capacity: 512);
        for (int i = 0; i < TargetNumberOfLines; i++) {
            _ = OutputString.Append(_serverUnmappableExceptionText);
            int numAddresses = JunkGenerator.WeightedRandom();
            addressCounts[numAddresses]++;
            for (int j = 0; j < numAddresses + 1; j++) {
                if (j > 0) {
                    _ = OutputString.Append(OutputSep);
                }
                // Making every domain unique uses a *ton* of memory.
                if (!MakeEveryUserAndDomainUnique && !MakeEveryNameUnique) {
                    _ = OutputString.Append(addressList[_rand.Next(addressList.Count)]);
                } else if (MakeEveryUserAndDomainUnique) {
                    _ = OutputString.Append(addressList[_rand.Next(addressList.Count)]).Append(_rand.Next());
                } else if (MakeEveryNameUnique) {
                    _ = OutputString.Append(_rand.Next()).Append(addressList[_rand.Next(addressList.Count)]);
                }
            }
            _ = OutputString.AppendLine();
            if (PercentageOfJibberishBeingAdded > _rand.Next(100)) {
                // TODO write some quality jibberish similar to MailStore's actual logs
                switch (_rand.Next(3)) {
                    case 0:
                        _ = OutputString.AppendLine("jibberish-0");
                        break;
                    case 1:
                        _ = OutputString.AppendLine("jibberish-1");
                        break;
                    case 2:
                        _ = OutputString.AppendLine("jibberish-2");
                        break;
                }
            }
        }

        //Console.WriteLine(OutputString.ToString());
        File.WriteAllText(FakeLogToWrite, OutputString.ToString());

        TimeSpan TimeWriting = stopwatch.Elapsed;
        stopwatch.Restart();
        Console.Write("Wrote ");
        for (int i = 0; i < addressCounts.Length; i++) { Console.Write($"{(i == 5 ? "and " : "")}{addressCounts[i]} {(i == 1 ? "lines" : "lines")} with {i + 1} {(i > 0 ? "addresses" : "address")}{(i == 5 ? "." : ", ")}"); }
        Console.Write($"{Environment.NewLine}Read the source file in {TimeReading.Humanize(5)}, wrote the target in {TimeWriting.Humanize(5)}, with a buffer of {OutputString.Capacity}.{Environment.NewLine}{Environment.NewLine}");
    }
    // "Weighted Dice" to pick how many email addresses each logline should have
    public static int WeightedRandom() {
        double[] cdf = { 0.10, 0.80, 0.90, 0.95, 0.98, 1.0 };
        double randValue = _rand.NextDouble(); // 0-1
        int result = Array.FindIndex(cdf, p => p >= randValue);
        return result; // adding 1 since array starts from 0
    }
    #endregion Public Methods

}