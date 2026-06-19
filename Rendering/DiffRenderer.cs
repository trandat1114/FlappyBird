namespace FlappyBird.Rendering;

/// <summary>
/// Utility that flushes a <see cref="ConsoleBuffer"/> from a legacy char[,] source array —
/// bridges existing GameState.PreviousScreen diff logic until Phase 3 migration is complete.
/// </summary>
public static class DiffRenderer
{
    /// <summary>
    /// Writes only cells that differ between <paramref name="newBuf"/> and
    /// <paramref name="prevBuf"/>, updating <paramref name="prevBuf"/> in-place.
    /// </summary>
    /// <param name="newBuf">Current frame content.</param>
    /// <param name="prevBuf">Previous frame — mutated to match newBuf for changed cells.</param>
    /// <param name="originY">Console row where row 0 of the buffer is drawn.</param>
    public static void Flush(char[,] newBuf, char[,] prevBuf, int originY = 0)
    {
        int rows = newBuf.GetLength(0);
        int cols = newBuf.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            int x = 0;
            while (x < cols)
            {
                if (newBuf[y, x] == prevBuf[y, x]) { x++; continue; }

                Console.SetCursorPosition(x, originY + y);
                while (x < cols && newBuf[y, x] != prevBuf[y, x])
                {
                    Console.Write(newBuf[y, x]);
                    prevBuf[y, x] = newBuf[y, x];
                    x++;
                }
            }
        }
    }
}
