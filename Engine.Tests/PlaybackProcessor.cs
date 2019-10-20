using MusicXmlSchema;
using System;
using Xunit;
using SightReader.Engine.ScoreBuilder;
using FluentAssertions;
using System.IO;
using SightReader.Engine.Interpreter;
using System.Linq;
using System.Collections.Generic;

namespace SightReader.Engine.Tests
{
    public class PlaybackProcessorTests
    {
        [Fact]
        public void CanPlayNote()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Etude_No._1.musicxml", FileMode.Open, FileAccess.Read));
            var score = builder.Build();
            var outputs = new List<IPianoEvent>();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score);
            interpreter.SeekMeasure(1);
            interpreter.Output += (IPianoEvent e) =>
            {
                outputs.Add(e);
            };
            var notePress = new NotePress()
            {
                Pitch = "C5".ToPitch(),
                Velocity = 65
            };

            interpreter.Input(notePress);
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "C4".ToPitch(),
                Velocity = notePress.Velocity
            });
        }

        [Fact]
        public void CanPlayChord()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Etude_No._1.musicxml", FileMode.Open, FileAccess.Read));
            var score = builder.Build();
            var outputs = new List<IPianoEvent>();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score);
            interpreter.SeekMeasure(3);
            interpreter.Output += (IPianoEvent e) =>
            {
                outputs.Add(e);
            };

            var notePress1 = new NotePress()
            {
                Pitch = "C5".ToPitch(),
                Velocity = 100
            };
            var notePress2 = new NotePress()
            {
                Pitch = "D5".ToPitch(),
                Velocity = 101
            };
            var notePress3 = new NotePress()
            {
                Pitch = "E5".ToPitch(),
                Velocity = 102
            };
            var notePress4 = new NotePress()
            {
                Pitch = "F5".ToPitch(),
                Velocity = 103
            };

            interpreter.Input(notePress1);
            outputs.Should().BeEmpty();

            interpreter.Input(notePress2);
            outputs.Should().BeEmpty();

            interpreter.Input(notePress3);
            outputs.Should().BeEmpty();

            interpreter.Input(notePress4);
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "C3".ToPitch(),
                Velocity = notePress1.Velocity
            });
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "E3".ToPitch(),
                Velocity = notePress2.Velocity
            });
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "G3".ToPitch(),
                Velocity = notePress3.Velocity
            });
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "C4".ToPitch(),
                Velocity = notePress4.Velocity
            });
        }

        [Fact]
        public void CanPlayChordOutOfOrder()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Etude_No._1.musicxml", FileMode.Open, FileAccess.Read));
            var score = builder.Build();
            var outputs = new List<IPianoEvent>();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score);
            interpreter.SeekMeasure(3);
            interpreter.Output += (IPianoEvent e) =>
            {
                outputs.Add(e);
            };

            var notePress1 = new NotePress()
            {
                Pitch = "C5".ToPitch(),
                Velocity = 100
            };
            var notePress2 = new NotePress()
            {
                Pitch = "D5".ToPitch(),
                Velocity = 101
            };
            var notePress3 = new NotePress()
            {
                Pitch = "E5".ToPitch(),
                Velocity = 102
            };
            var notePress4 = new NotePress()
            {
                Pitch = "F5".ToPitch(),
                Velocity = 103
            };

            interpreter.Input(notePress3);
            outputs.Should().BeEmpty();

            interpreter.Input(notePress2);
            outputs.Should().BeEmpty();

            interpreter.Input(notePress4);
            outputs.Should().BeEmpty();

            interpreter.Input(notePress1);
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "G3".ToPitch(),
                Velocity = notePress3.Velocity
            });
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "C4".ToPitch(),
                Velocity = notePress4.Velocity
            });
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "E3".ToPitch(),
                Velocity = notePress2.Velocity
            });
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "C3".ToPitch(),
                Velocity = notePress1.Velocity
            });
        }
    }
}
