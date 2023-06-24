using System.Diagnostics;
using System.Text;

namespace TheView;
public class TheView
{
    private int _longestLine;
    #region Fields
    // The physical on-screen position of private 0,0
    private readonly int _internalLeft, _internalTop;
    // The longest line that can be written, and the number of lines that can be written
    private int _internalWidth, _internalHeight;
    private System.Threading.Timer? _mainLoopTimer;
    private readonly Queue<ActionsX> _mainQueue;
    private readonly List<string> _screenText;
    private int _viewportLeft, _viewportTop;
    #endregion Fields
    #region Public Constructors
    public TheView() {
        //_screenText = new();
        _mainQueue = new();
        _mainLoopTimer = null;
        _internalLeft = 0;
        _internalTop = 0;
        _longestLine = 0;
        _screenText= new List<string>();
    }
    #endregion Public Constructors

    public void SetText(string text) {
        _screenText.Clear();
        _screenText.AddRange(text.ReplaceLineEndings()
            .Replace("\t", "   ").Split(Environment.NewLine));
    }
    public void SetText(List<string> texts) {
        _screenText.Clear();
        foreach (string text in texts) {
            _screenText.AddRange(text.ReplaceLineEndings().Replace("\t", "   ").Split(Environment.NewLine));
        }
    }
    public void SetText(List<StringBuilder> texts) {
        _screenText.Clear();
        foreach (StringBuilder text in texts) {
            string textIn = text.ToString().ReplaceLineEndings();
            textIn = textIn.Replace("\t", "   ");
            _screenText.AddRange(textIn.Split(Environment.NewLine));
        }
    }

    #region Enums
    private enum ActionsX
    {
        Screen_Refresh, Screen_Clear,
        Arrow_Up, Arrow_Down, Arrow_Left, Arrow_Right, Page_Up, Page_Down, Move_Home, Move_End,
        Sleep
    }
    #endregion Enums

    #region Public Methods
    private void DrawText() {
        CalculateGeometry();
        for (int i = _viewportTop; i < +_viewportTop + _internalHeight; i++) {
            MoveToPosInWindow(_internalLeft, _internalTop + i - _viewportTop);
            string s;
            if (i < _screenText.Count) {
                try {
                    s = _screenText[i]
                    .Substring(startIndex: Math.Min(_viewportLeft, _screenText[i].Length),
                    Math.Max(0, Math.Min(_internalWidth, _screenText[i].Length - _viewportLeft)));
                } catch { s = string.Empty; }
            } else {
                s = string.Empty;
            }
            Console.Write(s.PadRight(_internalWidth));
        }
        //DrawScrollbars();
    }
    /*private void DrawScrollbars() {
        if (true || _longestLine > _internalWidth) {
            StringBuilder ScrollbarBottom;
            ScrollbarBottom = new();
            Console.Write(ScrollbarBottom.ToString());
            ScrollbarBottom.Clear();

            int LinesOfTextLeft = _viewportLeft;
            int LinesOfTextRight = _longestLine - (_viewportLeft + _internalWidth);

            int CurrentBarT = (int)Math.Floor((double)_internalWidth * _internalWidth / _longestLine);
            int BarLeft = (int)Math.Ceiling((double)(_internalWidth - CurrentBarT) * LinesOfTextLeft / _longestLine);
            int BarRight = (int)Math.Ceiling((double)(_internalWidth - CurrentBarT) * LinesOfTextRight / _longestLine);

            for (int i = 0; i < _internalWidth; i++) {
                MoveToPosInWindow(i, _internalHeight + 1);
                if (i < BarLeft) {
                    Console.Write(HighASCIIDrawingCharConsts.BLOCK_BOTTOMHALF);
                } else if (i > _internalWidth - BarRight) {
                    Console.Write(HighASCIIDrawingCharConsts.BLOCK_BOTTOMHALF);
                } else {
                    Console.Write(HighASCIIDrawingCharConsts.BLOCK_FULL);
                }
            }
            MoveToPosInWindow(_viewportLeft, _internalHeight + 1);
            Console.WriteLine(ScrollbarBottom);
        }
        if (true || _screenText.Count > _internalHeight) {
            int LinesOfTextAbove = _viewportTop;
            int LinesOfTextBelow = _screenText.Count - (_viewportTop + _internalHeight);

            int CurrentBarR = (int)Math.Floor((double)_internalHeight * _internalHeight / _screenText.Count);
            int BarAbove = (int)Math.Floor((double)(_internalHeight - CurrentBarR) * LinesOfTextAbove / _screenText.Count);
            int BarBelow = (int)Math.Ceiling((double)(_internalHeight - CurrentBarR) * LinesOfTextBelow / _screenText.Count);
            for (int i = 0; i < _internalHeight; i++) {
                MoveToPosInWindow(_internalWidth +1, i);
                if (i < BarAbove) {
                    Console.Write(HighASCIIDrawingCharConsts.BLOCK_RIGHTHALF);
                } else if (i > _internalHeight - BarBelow) {
                    Console.Write(HighASCIIDrawingCharConsts.BLOCK_RIGHTHALF);
                } else {
                    Console.Write(HighASCIIDrawingCharConsts.BLOCK_FULL);
                }
            }
        }
    }*/
    public void Show() {
        Console.CursorVisible = false;
        _mainQueue.Enqueue(ActionsX.Screen_Refresh);
        using (_mainLoopTimer = new Timer(MainLoop, null, 0, 0)) {
            // Wait for the user to hit <Enter>
            bool ExitNow = false;
            ConsoleKeyInfo KeyPress;
            while (!ExitNow) {
                KeyPress = Console.ReadKey(true);
                switch (KeyPress.Key) {
                    case ConsoleKey.Spacebar:
                        _mainQueue.Enqueue(ActionsX.Screen_Clear);
                        break;
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        _mainQueue.Enqueue(ActionsX.Arrow_Up);
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        _mainQueue.Enqueue(ActionsX.Arrow_Left);
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        _mainQueue.Enqueue(ActionsX.Arrow_Down);
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        _mainQueue.Enqueue(ActionsX.Arrow_Right);
                        break;
                    case ConsoleKey.Home:
                        _mainQueue.Enqueue(ActionsX.Move_Home);
                        break;
                    case ConsoleKey.End:
                        _mainQueue.Enqueue(ActionsX.Move_End);
                        break;
                    case ConsoleKey.PageUp:
                        _mainQueue.Enqueue(ActionsX.Page_Up);
                        break;
                    case ConsoleKey.PageDown:
                        _mainQueue.Enqueue(ActionsX.Page_Down);
                        break;
                    case ConsoleKey.Escape:
                    case ConsoleKey.Q:
                        ExitNow = true;
                        break;
                }
            }
        }
        Console.CursorVisible = true;
        Console.Clear();
    }
    #endregion Public Methods

    #region Internal Methods
    private void MoveToPosInWindow(int left, int top) {
        try {
            Console.SetCursorPosition(_internalLeft + left, _internalTop + top);
        } catch { }
    }
    #endregion Internal Methods
    #region Private Methods
    private void CalculateGeometry() {
        if (_screenText is null || _internalHeight != Console.WindowHeight - _internalTop - 1) {
            _internalHeight = Console.WindowHeight - _internalTop - 1;
            _mainQueue.Enqueue(ActionsX.Sleep);
        }
        if (_internalWidth != Console.WindowWidth - _internalLeft - 1) {
            _internalWidth = Console.WindowWidth - _internalLeft - 1;
            _mainQueue.Enqueue(ActionsX.Sleep);
        }
        if (_viewportLeft < 0) {
            _viewportLeft = 0;
        }

        if (_viewportLeft > _longestLine) {
            _viewportLeft = _longestLine;
        }

        if (_viewportLeft > _longestLine-_internalWidth) {
            _viewportLeft = Math.Max(0,_longestLine - _internalWidth);
        }

        if (_viewportTop < 0) {
            _viewportTop = 0;
        }

        if (_viewportTop >= _screenText!.Count) {
            _viewportTop = _screenText.Count - 1;
        }

        if (_viewportTop >= _screenText.Count - _internalHeight) {
            _viewportTop = _screenText.Count - _internalHeight;
        }
        if (_longestLine == 0) { foreach (string EachString in _screenText) {
                _longestLine = Math.Max(_longestLine, EachString.Length);
            }
        }

        if (_screenText.Count == 0) {
            throw new Exception("Text not found");
        }
    }
    private void MainLoop(Object? o = null) {
        //Console.WriteLine($"MainLoop _mainQueue.Count {_mainQueue.Count}");
        int TimerNextDueTime = 150;
        CalculateGeometry();
        while (_mainQueue.TryDequeue(out ActionsX NextAction)) {
            Debug.WriteLine($"NextAction {NextAction}");
            switch (NextAction) {
                case ActionsX.Sleep:
                    if (_mainQueue.Contains(ActionsX.Sleep)) {
                        continue;
                    }

                    _mainQueue.Enqueue(ActionsX.Screen_Clear);
                    TimerNextDueTime = 350;
                    break;
                case ActionsX.Screen_Refresh:
                    // If we have another pending Screen_Refresh, skip this one
                    if (_mainQueue.Contains(ActionsX.Screen_Refresh) || _mainQueue.Contains(ActionsX.Screen_Clear) || _mainQueue.Contains(ActionsX.Sleep)) {
                        continue;
                    }
                    DrawText();
                    break;
                case ActionsX.Page_Up:
                    _viewportTop -= _internalHeight;
                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                case ActionsX.Page_Down:
                    _viewportTop += _internalHeight;
                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                case ActionsX.Arrow_Up:
                    _viewportTop--;
                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                case ActionsX.Arrow_Down:
                    _viewportTop++;
                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                case ActionsX.Arrow_Left:
                    _viewportLeft--;
                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                case ActionsX.Arrow_Right:
                    _viewportLeft++;
                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                case ActionsX.Screen_Clear:
                    if (_mainQueue.Contains(ActionsX.Screen_Refresh) || _mainQueue.Contains(ActionsX.Screen_Clear) || _mainQueue.Contains(ActionsX.Sleep)) {
                        continue;
                    }
                    Console.Clear();
                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                case ActionsX.Move_Home:
                    if (_viewportLeft != 0) {
                        _viewportLeft = 0;
                    } else {
                        _viewportTop = 0;
                    }

                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                case ActionsX.Move_End:
                    if (_viewportTop + 1 != _screenText.Count) {
                        _viewportTop = _screenText.Count;
                    } else {
                        _viewportLeft = _longestLine;
                    }

                    _mainQueue.Enqueue(ActionsX.Screen_Refresh);
                    break;
                default:
                    throw new NotImplementedException($"{{{NextAction}}} not recognized");
            }
        }
        // And set the timer again
        _ = _mainLoopTimer!.Change(TimerNextDueTime, 0);
    }
    #endregion Private Methods
}