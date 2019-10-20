using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public interface IPianoEvent { }

    public struct NotePress : IPianoEvent
    {
        public byte Pitch { get; set; }
        public byte Velocity { get; set; }
    }

    public struct NoteRelease : IPianoEvent
    {
        public byte Pitch { get; set; }
    }
    public enum PedalKind
    {
        UnaCorda,
        Sostenuto,
        Sustain
    }

    public struct PedalChange : IPianoEvent
    {
        public PedalKind Pedal { get; set; }
        public byte Position { get; set; }
    }
}
