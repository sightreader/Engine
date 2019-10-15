using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public interface IPianoEvent { }

    public readonly struct NotePress : IPianoEvent
    {
        public byte Pitch { get; }
        public byte Velocity { get; }

        public NotePress(byte pitch, byte velocity)
        {
            Pitch = pitch;
            Velocity = velocity;
        }
    }

    public readonly struct NoteRelease : IPianoEvent
    {
        public byte Pitch { get; }

        public NoteRelease(byte pitch)
        {
            Pitch = pitch;
        }
    }
    public enum PedalKind
    {
        UnaCorda,
        Sostenuto,
        Sustain
    }

    public readonly struct PedalChange : IPianoEvent
    {
        public PedalKind Pedal { get; }
        public byte Position { get; }

        public PedalChange(PedalKind pedal, byte position)
        {
            Pedal = pedal;
            Position = position;
        }
    }
}
