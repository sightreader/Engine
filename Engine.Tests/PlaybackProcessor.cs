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
        private static string EtudeNo1ScoreFilePath = @"Assets\Etude_No._1.musicxml";

        [Fact]
        public void CanPlayAndReleaseNote()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();
            var outputs = new List<IPianoEvent>();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
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

            var noteRelease = new NoteRelease()
            {
                Pitch = "C5".ToPitch()
            };
            interpreter.Input(noteRelease);
            outputs.Should().Contain(new NoteRelease()
            {
                Pitch = "C4".ToPitch()
            });
        }
        [Fact]
        public void CanPlaySequenceOfThreeNotes()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();
            var outputs = new List<IPianoEvent>();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
            interpreter.SeekMeasure(1);
            interpreter.Output += (IPianoEvent e) =>
            {
                outputs.Add(e);
            };
            var notePress1 = new NotePress()
            {
                Pitch = "C5".ToPitch(),
                Velocity = 65
            };
            var notePress2 = new NotePress()
            {
                Pitch = "D5".ToPitch(),
                Velocity = 65
            };
            var notePress3 = new NotePress()
            {
                Pitch = "E5".ToPitch(),
                Velocity = 65
            };

            interpreter.Input(notePress1);
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "C4".ToPitch(),
                Velocity = notePress1.Velocity
            });

            interpreter.Input(notePress2);
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "D4".ToPitch(),
                Velocity = notePress2.Velocity
            });

            interpreter.Input(notePress3);
            outputs.Should().Contain(new NotePress()
            {
                Pitch = "E4".ToPitch(),
                Velocity = notePress3.Velocity
            });
        }

        [Fact]
        public void CanPlayChord()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();
            var outputs = new List<IPianoEvent>();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
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
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();
            var outputs = new List<IPianoEvent>();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
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

        [Fact]
        public void CanPlaySingleNoteTies()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
            interpreter.SeekMeasure(19);

            var playedPitches = new List<byte>();
            interpreter.Output += (IPianoEvent e) =>
            {
                if (e is NotePress press)
                {
                    playedPitches.Add(press.Pitch);
                }
            };

            playedPitches.Should().BeEmpty();

            interpreter.Input(new NotePress()
            {
                Pitch = "C5".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "D5".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch(), "E4".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "E5".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch(), "E4".ToPitch(), "G4".ToPitch() });
        }

        [Fact]
        public void CanPlayTiesAndNonTiesInChords()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
            interpreter.SeekMeasure(20);

            var playedPitches = new List<byte>();
            interpreter.Output += (IPianoEvent e) =>
            {
                if (e is NotePress press)
                {
                    playedPitches.Add(press.Pitch);
                }
            };

            playedPitches.Should().BeEmpty();

            interpreter.Input(new NotePress()
            {
                Pitch = "C5".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEmpty();

            interpreter.Input(new NotePress()
            {
                Pitch = "D5".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch(), "E4".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "E5".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch(), "E4".ToPitch(), "G4".ToPitch() });
        }

        [Fact]
        public void CanPlayGraceNote()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
            interpreter.SeekMeasure(18);

            var playedPitches = new List<byte>();
            interpreter.Output += (IPianoEvent e) =>
            {
                if (e is NotePress press)
                {
                    playedPitches.Add(press.Pitch);
                }
            };

            playedPitches.Should().BeEmpty();

            interpreter.Input(new NotePress()
            {
                Pitch = "C5".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "D4".ToPitch() });


            interpreter.Input(new NotePress()
            {
                Pitch = "D5".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "D4".ToPitch(), "E4".ToPitch() });
        }

        [Fact]
        public void CanPlayArpeggiatedDefault()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
            interpreter.SeekMeasure(21);

            var playedPitches = new List<byte>();
            interpreter.Output += (IPianoEvent e) =>
            {
                if (e is NotePress press)
                {
                    playedPitches.Add(press.Pitch);
                }
            };

            playedPitches.Should().BeEmpty();


            interpreter.Input(new NotePress()
            {
                Pitch = "D6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch() });


            interpreter.Input(new NotePress()
            {
                Pitch = "E6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch(), "E4".ToPitch() });


            interpreter.Input(new NotePress()
            {
                Pitch = "F6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch(), "E4".ToPitch(), "G4".ToPitch() });


            interpreter.Input(new NotePress()
            {
                Pitch = "G6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "C4".ToPitch(), "E4".ToPitch(), "G4".ToPitch(), "C5".ToPitch() });
        }

        [Fact]
        public void CanPlayArpeggiatedUp()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
            interpreter.SeekMeasure(22);

            var playedPitches = new List<byte>();
            interpreter.Output += (IPianoEvent e) =>
            {
                if (e is NotePress press)
                {
                    playedPitches.Add(press.Pitch);
                }
            };

            playedPitches.Should().BeEmpty();

            interpreter.Input(new NotePress()
            {
                Pitch = "C6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "D4".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "D6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "D4".ToPitch(), "F4".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "E6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "D4".ToPitch(), "F4".ToPitch(), "A4".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "F6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "D4".ToPitch(), "F4".ToPitch(), "A4".ToPitch(), "D5".ToPitch() });
        }

        [Fact]
        public void CanPlayArpeggiatedDown()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
            interpreter.SeekMeasure(23);

            var playedPitches = new List<byte>();
            interpreter.Output += (IPianoEvent e) =>
            {
                if (e is NotePress press)
                {
                    playedPitches.Add(press.Pitch);
                }
            };

            playedPitches.Should().BeEmpty();

            interpreter.Input(new NotePress()
            {
                Pitch = "C6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "E5".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "D6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "E5".ToPitch(), "B4".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "E6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "E5".ToPitch(), "B4".ToPitch(), "G4".ToPitch() });

            interpreter.Input(new NotePress()
            {
                Pitch = "F6".ToPitch(),
                Velocity = 100
            });
            playedPitches.Should().BeEquivalentTo(new byte[] { "E5".ToPitch(), "B4".ToPitch(), "G4".ToPitch(), "E4".ToPitch() });
        }

        [Fact]
        public void CanPlayPedals()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(EtudeNo1ScoreFilePath, FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            var interpreter = new Interpreter.Interpreter();
            interpreter.SetScore(score, EtudeNo1ScoreFilePath);
            interpreter.SeekMeasure(0);

            var pedalChanges = new List<PedalChange>();
            interpreter.Output += (IPianoEvent e) =>
            {
                if (e is PedalChange pedal)
                {
                    pedalChanges.Add(pedal);
                }
            };

            pedalChanges.Should().BeEmpty();

            var sustainPedalChange = new PedalChange()
            {
                Pedal = PedalKind.Sustain,
                Position = 28
            };

            var sostenutoPedalChange = new PedalChange()
            {
                Pedal = PedalKind.Sostenuto,
                Position = 28
            };

            var unaCordaPedalChange = new PedalChange()
            {
                Pedal = PedalKind.UnaCorda,
                Position = 28
            };

            interpreter.Input(sustainPedalChange);
            pedalChanges.Should().BeEquivalentTo(new PedalChange[] { sustainPedalChange });

            interpreter.Input(sostenutoPedalChange);
            pedalChanges.Should().BeEquivalentTo(new PedalChange[] { sustainPedalChange, sostenutoPedalChange });

            interpreter.Input(unaCordaPedalChange);
            pedalChanges.Should().BeEquivalentTo(new PedalChange[] { sustainPedalChange, unaCordaPedalChange });
        }
    }
}
