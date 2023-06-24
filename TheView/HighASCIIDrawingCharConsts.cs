namespace TheView;

public static class HighASCIIDrawingCharConsts
{
    public const char SPACE = ' ';

    // Single Lines
    public const char SINGLE_VERTICAL_BAR = '│'; // vertical bar
    public const char SINGLE_HORIZONTAL_BAR = '─'; // horizontal bar
    public const char SINGLE_TOP_LEFT_CORNER = '┌'; // top left corner
    public const char SINGLE_TOP_RIGHT_CORNER = '┐'; // top right corner
    public const char SINGLE_BOTTOM_LEFT_CORNER = '└'; // bottom left corner
    public const char SINGLE_BOTTOM_RIGHT_CORNER = '┘'; // bottom right corner
    public const char SINGLE_TOP_RIGHT_BOTTOM_LEFT_CORNER = '├'; // top right-bottom left corner
    public const char SINGLE_TOP_LEFT_BOTTOM_RIGHT_CORNER = '┤'; // top left-bottom right corner
    public const char SINGLE_TOP_LEFT_TOP_RIGHT_BOTTOM_LEFT = '┬'; // top left-top right-bottom left
    public const char SINGLE_TOP_LEFT_BOTTOM_LEFT_BOTTOM_RIGHT = '┴'; // top left-bottom left-bottom right
    public const char SINGLE_TOP_LEFT_TOP_RIGHT_BOTTOM_LEFT_BOTTOM_RIGHT = '┼'; // top left-top right-bottom left-bottom right

    // Double lines
    public const char DOUBLE_VERTICAL_BAR = '║'; // vertical bar
    public const char DOUBLE_HORIZONTAL_BAR = '═'; // horizontal bar
    public const char DOUBLE_TOP_LEFT_CORNER = '╔'; // top left corner
    public const char DOUBLE_TOP_RIGHT_CORNER = '╗'; // top right corner
    public const char DOUBLE_BOTTOM_LEFT_CORNER = '╚'; // bottom left corner
    public const char DOUBLE_BOTTOM_RIGHT_CORNER = '╝'; // bottom right corner
    public const char DOUBLE_TOP_RIGHT_BOTTOM_LEFT_CORNER = '╠'; // top right-bottom left corner
    public const char DOUBLE_TOP_LEFT_BOTTOM_RIGHT_CORNER = '╣'; // top left-bottom right corner
    public const char DOUBLE_TOP_LEFT_TOP_RIGHT_BOTTOM_LEFT = '╦'; // top left-top right-bottom left
    public const char DOUBLE_TOP_LEFT_BOTTOM_LEFT_BOTTOM_RIGHT = '╩'; // top left-bottom left

    public const char BOXDRAWING_LIGHTSHADE = '\u2591';             // ░ Light shade
    public const char BOXDRAWING_MEDIUMSHADE = '\u2592';            // ▒ Medium shade
    public const char BOXDRAWING_DARKSHADE = '\u2593';              // ▓ Dark shade

    public const char BLOCK_FULL = '\u2588';   // █ Full block
    public const char BLOCK_LEFTHALF = '\u258C'; // ▌ Left half block
    public const char BLOCK_RIGHTHALF = '\u2590'; // ▐ Right half block
    public const char BLOCK_TOPHALF = '\u2580'; // ▀ Top half block
    public const char BLOCK_BOTTOMHALF = '\u2584'; // ▄ Bottom half block
    public const char BLOCK_SOLIDSQUARE = '\u25A0'; // ▪ Solid square

    public const char ARROW_LEFT = '\u25C4';   // ◄ Black left-pointing pointer
    public const char ARROW_RIGHT = '\u25BA';  // ► Black right-pointing pointer
    public const char FROWN = '\u263B';        // ☻ Black smiling face
    public const char SMILE = '\u263A';        // ☺ White smiling face
    public const char TRIANGLE_DOWN = '\u25BC'; // ▼ Black down-pointing triangle
    public const char TRIANGLE_UP = '\u25B2'; // ▲ Black up-pointing triangle
}