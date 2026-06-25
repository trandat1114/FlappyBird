namespace FlappyBird.Rendering;

public static class DiffRenderer
{
    public static void Flush(char[,] newBuf, char[,] prevBuf, int originY = 0, int originX = 0,
        Func<char, ConsoleColor>? colorFn = null)
    {
        int rows = newBuf.GetLength(0);
        int cols = newBuf.GetLength(1);
        ConsoleColor cur = Console.ForegroundColor;

        // Snapshot buffer bounds once — prevents crash when terminal is smaller than game area.
        int bufW = Console.BufferWidth;
        int bufH = Console.BufferHeight;

        for (int y = 0; y < rows; y++)
        {
            int absY = originY + y;
            if ((uint)absY >= (uint)bufH) continue; // row is outside the terminal buffer

            int x = 0;
            while (x < cols)
            {
                int absX = originX + x;
                if (absX >= bufW) break; // rest of this row is off-screen — skip

                if (newBuf[y, x] == prevBuf[y, x]) { x++; continue; }

                Console.SetCursorPosition(absX, absY);
                while (x < cols && newBuf[y, x] != prevBuf[y, x])
                {
                    if (originX + x >= bufW) break; // would overflow terminal row

                    if (colorFn is not null)
                    {
                        ConsoleColor c = colorFn(newBuf[y, x]);
                        if (c != cur) { Console.ForegroundColor = c; cur = c; }
                    }
                    Console.Write(newBuf[y, x]);
                    prevBuf[y, x] = newBuf[y, x];
                    x++;
                }
            }
        }
    }
}
