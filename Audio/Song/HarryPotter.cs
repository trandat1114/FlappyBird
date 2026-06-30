using System;
using FlappyBird.Audio.Enum;

namespace FlappyBird.Audio.Song;

public static class HarryPotter
{
    public static int Tempo { get; } = 300; // Tempo in BPM
    public static int ShortPause { get; } = 50; // Short pause duration in milliseconds
    public static int LongPause { get; } = 100; // Long pause duration in

    public static (Note note, int duration)[] Melody { get; } =
     [
        // Phần 1: B4 E5 G5 F#5 E5 _ B5 A5 _ F#5
        (Note.B4, Tempo),
        (Note.E5, Tempo),
        (Note.G5, Tempo),
        (Note.FSharp5, Tempo),
        (Note.E5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.B5, Tempo),
        (Note.A5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.FSharp5, Tempo),
        (Note.Rest, LongPause),

        // Phần 2: E5 G5 F#5 D#5 _ E5 B4 _ G4 B4
        (Note.E5, Tempo),
        (Note.G5, Tempo),
        (Note.FSharp5, Tempo),
        (Note.DSharp5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.E5, Tempo),
        (Note.B4, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.G4, Tempo),
        (Note.B4, Tempo),
        (Note.Rest, LongPause * 2),

        // Phần 3: B4 E5 G5 F#5 E5 _ B5 D6 _ C#6 C6
        (Note.B4, Tempo),
        (Note.E5, Tempo),
        (Note.G5, Tempo),
        (Note.FSharp5, Tempo),
        (Note.E5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.B5, Tempo),
        (Note.D6, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.CSharp6, Tempo),
        (Note.C6, Tempo),
        (Note.Rest, LongPause),

        // Phần 4: G#5 C6 B5 A#5 A#4 _ G5 E5 _ G4 B4
        (Note.GSharp5, Tempo),
        (Note.C6, Tempo),
        (Note.B5, Tempo),
        (Note.ASharp5, Tempo),
        (Note.ASharp4, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.G5, Tempo),
        (Note.E5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.G4, Tempo),
        (Note.B4, Tempo),
        (Note.Rest, LongPause * 2),

        // Phần 5: G5 B5 _ G5 B5 _ G5 C6 _ B5 A#5
        (Note.G5, Tempo),
        (Note.B5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.G5, Tempo),
        (Note.B5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.G5, Tempo),
        (Note.C6, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.B5, Tempo),
        (Note.ASharp5, Tempo),
        (Note.Rest, LongPause),

        // Phần 6: F#5 G5 B5 A#5 A#4 _ B4 B5 _ G4 B4
        (Note.FSharp5, Tempo),
        (Note.G5, Tempo),
        (Note.B5, Tempo),
        (Note.ASharp5, Tempo),
        (Note.ASharp4, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.B4, Tempo),
        (Note.B5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.G4, Tempo),
        (Note.B4, Tempo),
        (Note.Rest, LongPause * 2),

        // Phần 7: G5 B5 _ G5 B5 _ G5 D6 _ C#6 C6
        (Note.G5, Tempo),
        (Note.B5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.G5, Tempo),
        (Note.B5, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.G5, Tempo),
        (Note.D6, Tempo * 2),
        (Note.Rest, ShortPause),
        (Note.CSharp6, Tempo),
        (Note.C6, Tempo),
        (Note.Rest, LongPause),

        // Phần 8: G#5 C6 B5 A#5 A#4 __ G5 E5 (kết thúc)
        (Note.GSharp5, Tempo),
        (Note.C6, Tempo),
        (Note.B5, Tempo),
        (Note.ASharp5, Tempo),
        (Note.ASharp4, Tempo * 3),
        (Note.Rest, LongPause * 2),
        (Note.G5, Tempo),
        (Note.E5, Tempo * 3),

        // Kết thúc
        (Note.Rest, 1000)
    ];
}
