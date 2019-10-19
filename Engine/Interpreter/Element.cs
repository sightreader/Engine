using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SightReader.Engine.ScoreBuilder;

namespace SightReader.Engine.Interpreter
{
    public static class StringExtensions
    {
        public static byte ToPitch(this string s)
        {
            byte OCTAVE_SEMITONES = 12;
            byte C0_VALUE = 12; /* MIDI defines C0 as 12 */
            byte octave = s.Last().ToString().ToByte();

            byte valueForOctave = (byte)(C0_VALUE + octave * OCTAVE_SEMITONES);

            byte stepSemitone = s.First() switch
            {
                'C' => 0,
                'D' => 2,
                'E' => 4,
                'F' => 5,
                'G' => 7,
                'A' => 9,
                'B' => 11,
                _ => throw new NotSupportedException($"Received a string pitch to parse with a step of {s.First().ToString()}")
            };

            var isAlterSpecified = s.Length > 2;

            var numberOfSharps = s.Count(x => x == '#');
            var numberOfFlats = s.Count(x => x == 'b');

            var alterSemitoneForSharps = numberOfSharps;
            var alterSemitoneForFlats = -numberOfFlats;

            return (byte)(valueForOctave + stepSemitone + alterSemitoneForSharps + alterSemitoneForFlats);
        }
    }

    public interface IElement
    {
        decimal Duration { get; set; }
        byte Voice { get; set; }
        byte Staff { get; set; }
        ushort Measure { get; set; }
        byte Pitch { get; set; }
        bool IsChordChild { get; set; }
        bool IsGraceNote { get; set; }
        bool IsRest { get; set; }
        INotation[] Notations { get; set; }
    }

    public class Element : IElement
    {
        private byte staff = 1;
        private byte voice = 1;

        /**
         * Duration is a positive number specified in division units. This is the intended duration vs. notated duration (for instance, swing eighths vs. even eighths, or differences in dotted notes in Baroque-era music). Differences in duration specific to an interpretation or performance should use the note element's attack and release attributes.
         */
        public decimal Duration { get; set; }
        public byte Voice
        {
            get
            {
                return voice;
            }
            set
            {
                voice = value == 0 ? (byte)1 : value;
            }
        }

        public byte Staff
        {
            get
            {
                return staff;
            }
            set
            {
                staff = value == 0 ? (byte)1 : value;
            }
        }
        public ushort Measure { get; set; }
        public byte Pitch { get; set; }
        public bool IsChordChild { get; set; }
        public bool IsGraceNote { get; set; }
        public bool IsRest { get; set; }
        public INotation[] Notations { get; set; } = new INotation[] { };
    }
}
