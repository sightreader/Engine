using MusicXmlSchema;
using System;
using Xunit;
using SightReader.Engine.ScoreBuilder;
using SightReader.Engine.Interpreter;
using FluentAssertions;

namespace SightReader.Engine.Tests
{
    public class MidiPitchFromString
    {
        [Fact]
        public void LowestNote()
        {
            "A0".ToPitch().Should().Be(21);
        }

        [Fact]
        public void MiddleC()
        {
            "C4".ToPitch().Should().Be(60);
        }

        [Fact]
        public void MiddleD()
        {
            "D4".ToPitch().Should().Be(62);
        }

        [Fact]
        public void MiddleE()
        {
            "E4".ToPitch().Should().Be(64);
        }

        [Fact]
        public void MiddleF()
        {
            "F4".ToPitch().Should().Be(65);
        }

        [Fact]
        public void MiddleG()
        {
            "G4".ToPitch().Should().Be(67);
        }

        [Fact]
        public void MiddleA()
        {
            "A4".ToPitch().Should().Be(69);
        }

        [Fact]
        public void MiddleB()
        {
            "B4".ToPitch().Should().Be(71);
        }

        [Fact]
        public void C5()
        {
            "C5".ToPitch().Should().Be(72);
        }

        [Fact]
        public void MiddleCSharp()
        {
            "C#4".ToPitch().Should().Be(61);
        }

        [Fact]
        public void MiddleDFlat()
        {
            "Db4".ToPitch().Should().Be(61);
        }

        [Fact]
        public void MiddleDDoubleFlat()
        {
            "Dbb4".ToPitch().Should().Be(60);
        }

        [Fact]
        public void MiddleCDoubleSharp()
        {
            "C##4".ToPitch().Should().Be(62);
        }

        [Fact]
        public void HighestNote()
        {
            "C8".ToPitch().Should().Be(108);
        }
    }
    public class MidiPitchFromMusicXml
    {
        [Fact]
        public void LowestNote()
        {
            new pitch()
            {
                octave = "0",
                step = step.A
            }.ToByte().Should().Be(21);
        }

        [Fact]
        public void MiddleC()
        {
            new pitch()
            {
                octave = "4",
                step = step.C
            }.ToByte().Should().Be(60);
        }

        [Fact]
        public void MiddleD()
        {
            new pitch()
            {
                octave = "4",
                step = step.D
            }.ToByte().Should().Be(62);
        }

        [Fact]
        public void MiddleE()
        {
            new pitch()
            {
                octave = "4",
                step = step.E
            }.ToByte().Should().Be(64);
        }

        [Fact]
        public void MiddleF()
        {
            new pitch()
            {
                octave = "4",
                step = step.F
            }.ToByte().Should().Be(65);
        }

        [Fact]
        public void MiddleG()
        {
            new pitch()
            {
                octave = "4",
                step = step.G
            }.ToByte().Should().Be(67);
        }

        [Fact]
        public void MiddleA()
        {
            new pitch()
            {
                octave = "4",
                step = step.A
            }.ToByte().Should().Be(69);
        }

        [Fact]
        public void MiddleB()
        {
            new pitch()
            {
                octave = "4",
                step = step.B
            }.ToByte().Should().Be(71);
        }

        [Fact]
        public void C5()
        {
            new pitch()
            {
                octave = "5",
                step = step.C
            }.ToByte().Should().Be(72);
        }

        [Fact]
        public void MiddleCSharp()
        {
            new pitch()
            {
                octave = "4",
                step = step.C,
                alter = 1
            }.ToByte().Should().Be(61);
        }

        [Fact]
        public void MiddleDFlat()
        {
            new pitch()
            {
                octave = "4",
                step = step.D,
                alter = -1
            }.ToByte().Should().Be(61);
        }

        [Fact]
        public void MiddleDDoubleFlat()
        {
            new pitch()
            {
                octave = "4",
                step = step.D,
                alter = -2
            }.ToByte().Should().Be(60);
        }

        [Fact]
        public void MiddleCDoubleSharp()
        {
            new pitch()
            {
                octave = "4",
                step = step.C,
                alter = 2
            }.ToByte().Should().Be(62);
        }

        [Fact]
        public void HighestNote()
        {
            new pitch()
            {
                octave = "8",
                step = step.C
            }.ToByte().Should().Be(108);
        }
    }
}
