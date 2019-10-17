using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public interface IElement
    {
    }

    public enum NoteType
    {
        /**
         * A regular note.
         */
        Default,
        /**
         * A grace note.
         */
        Grace,

    }

    /**
     * https://usermanuals.musicxml.com/MusicXML/Content/EL-MusicXML-note.htm
     */
    public class Note : IElement
    {
        public NoteType Type { get; set; } = NoteType.Default;
        public byte Pitch { get; set; }
        /**
         * Duration is a positive number specified in division units. This is the intended duration vs. notated duration (for instance, swing eighths vs. even eighths, or differences in dotted notes in Baroque-era music). Differences in duration specific to an interpretation or performance should use the note element's attack and release attributes.
         */
        public decimal Duration { get; set; }
        public byte Voice { get; set; }
        public byte Staff { get; set; }
        public bool IsChordNote { get; set; }
        public ushort Measure { get; set; }
        public INotation[] Notations { get; set; } = new INotation[] { };
    }

    public class Chord : IElement
    {
        public Note[] Notes { get; set; } = new Note[] { };
    }
}
