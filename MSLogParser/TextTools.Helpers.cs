using System.Text;

//TODO convert everything to use StringBuilders in and out, so that it is possible to perform multiple operations (e.g. Scramble the log, then find e-mail addresses, with everything scrambled in the same way)

namespace MailStoreLogFeatures;

public static class FileTools
{
    #region Public Methods
    public static string? CanFileBeCreated(string path, bool? AllowExisting = null) => CanFileBeCreated(new FileInfo(path), AllowExisting);

    public static string? CanFileBeCreated(string[] path, bool? AllowExisting = null) {
        string? result = null;
        foreach (string File in path) {
            string? ErrorMessage = CanFileBeCreated(File, AllowExisting);
            if (result is not null && ErrorMessage is not null) {
                result += Environment.NewLine;
            }
            result += ErrorMessage;
        }
        return result;
    }

    public static string? CanFileBeCreated(FileSystemInfo[] Files, bool? AllowExisting = null) {
        string? result = null;
        foreach (FileSystemInfo File in Files) {
            string? ErrorMessage = FileTools.CanFileBeCreated(File, AllowExisting);
            if (result is not null && ErrorMessage is not null) {
                result += Environment.NewLine;
            }
            result += ErrorMessage;
        }
        return result;
    }

    public static string? CanFileBeCreated(FileSystemInfo File, bool? AllowExisting = null) {
        string? ErrorMessage = null;
        string RelFileName = Path.GetRelativePath(Directory.GetCurrentDirectory(), File.FullName);
        if (File == null) {
            ErrorMessage = "Filename is missing.";
        } else if (File.Name[0] == '-') {
            ErrorMessage = $"\"{File.Name}\" cannot start with '-' (or is missing).";
        } else if (IsPathValidRootedLocal(RelFileName)) {
            ErrorMessage = $"\"{File.FullName}\" is not valid.";
        } else if (AllowExisting == false && File.Exists && (File.Attributes & FileAttributes.Archive) == FileAttributes.Archive) {
            ErrorMessage += $"\"{RelFileName}\" exists.";
        } else if (AllowExisting == true && !File.Exists) {
            ErrorMessage += $"\"{RelFileName}\" does not exist.";
        } else if (File.Exists && (File.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
            ErrorMessage += $"\"{RelFileName}\" is a directory.";
        } else if (new string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "COM0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "LPT0" }.Contains(File.Name.ToUpper())) {
            ErrorMessage += $"\"{File.Name}\" is not a valid filename.";
        } else if (!Directory.Exists(Path.GetDirectoryName(File.FullName))) {
            ErrorMessage = $"\"{Path.GetDirectoryName(File.FullName)}\" is not a valid directory.";
        }
        return ErrorMessage;
    }
    #endregion Public Methods

    #region Private Methods
    private static bool IsPathValidRootedLocal(string pathString) {
        bool isValidUri = Uri.TryCreate(pathString, UriKind.Absolute, out Uri? pathUri);
        return isValidUri && pathUri?.IsLoopback == true;
    }
    #endregion Private Methods

}

public static partial class TextTools
{
    #region Classes
    internal class Helpers
    {
        #region Fields
        private readonly Random _rand;
        private readonly Dictionary<char, char> _swapping;
        private readonly Dictionary<char, char> _unswapping;
        #endregion Fields

        #region Public Constructors
        internal Helpers(int? seed = null) {
            _swapping = new();
            _unswapping = new();
            // TODO Fixed seed, oh yes! Very Random!
            _rand = seed is null ? new Random() : new Random((int)seed);

            // Build a scrambler table -- There might be a better way to do this, I don't care.
            // Every character will be swapped for another character of the same class (number,
            // alpha), but consistently throughout all logs obfuscated at the same time. Encryption
            // this is not, but it means a string in the log can be searched for other instances of
            // that string, and strings look/feel normal (although jibberished). Maybe there is a
            // better way to generate the table than trial and error, I can't think of one that
            // wouldn't involve creating more structures that then need to be garbage collected. If
            // this is too slow on your 386 let me know, otherwise it'll be fine.

            char r;
            for (char i = '0'; i <= '9'; i++) {
                do {
                    do {
                        // Yes, that's plus '0' not 0
                        r = (char)(_rand.Next(10) + '0');
                    } while (_swapping.ContainsValue(r));
                    _swapping.Add(i, r);
                    _unswapping.Add(r, i);
                } while (!_swapping.ContainsKey(i));
            }
            for (char i = 'a'; i <= 'z'; i++) {
                do {
                    do {
                        r = (char)(_rand.Next(26) + 'a');
                    } while (_swapping.ContainsValue(r));
                    _swapping.Add(i, r);
                    _swapping.Add(Char.ToUpper(i), Char.ToUpper(r));
                    _unswapping.Add(r, i);
                    _unswapping.Add(Char.ToUpper(r), Char.ToUpper(i));
                } while (!_swapping.ContainsKey(i));
            }
        }
        #endregion Public Constructors

        #region Public Methods
        internal void CensorshipMaker(StringBuilder InputString, int Index) => CensorshipMaker(InputString, Index, InputString.Length - Index);

        /// <summary>Censor characters from a provided StringBuilder...</summary>
        /// <param name="InputString"></param>
        /// <param name="Index"></param>
        /// <param name="Length"></param>
        internal void CensorshipMaker(StringBuilder InputString, int Index, int Length) {
            for (int i = Index; i < Index + Length; i++) {
                _ = InputString.Replace(InputString[i], Swap(InputString[i]), i, 1);
            }
        }

        /// <summary>Swap a character in a consistent way</summary>
        /// <param name="c1"></param>
        /// <returns></returns>
        internal char Swap(char c1) => _swapping.ContainsKey(c1) ? _swapping[c1] : c1;

        internal char Unswap(char c1) => _unswapping.ContainsKey(c1) ? _unswapping[c1] : c1;
        #endregion Public Methods

    }
    #endregion Classes

}