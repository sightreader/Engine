using MusicXmlSchema;
using System;
using Xunit;
using SightReader.Engine.ScoreBuilder;
using FluentAssertions;
using System.IO;
using SightReader.Engine.Interpreter;
using System.Linq;

namespace SightReader.Engine.Tests
{
    public class ScoreBuilderTests
    {
        private void AssertScoreIsEmpty(Score score)
        {
            score.Info.WorkTitle.Should().BeEmpty();
            score.Info.WorkNumber.Should().BeEmpty();
            score.Info.MovementTitle.Should().BeEmpty();
            score.Info.MovementNumber.Should().BeEmpty();
            score.Info.Creators.Should().BeEmpty();
            score.Info.Credits.Should().BeEmpty();
            score.Info.EncodingDates.Should().BeEmpty();
            score.Info.EncodingSoftware.Should().BeEmpty();
            score.Info.Misc.Should().BeEmpty();
            score.Info.Rights.Should().BeEmpty();
            score.Info.Source.Should().BeEmpty();

            score.Parts.Should().ContainSingle();
            score.Parts.First().Staves.Should().BeEmpty();
        }

        [Fact]
        public void CanBuildBareScoreMusicXml()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Bare_Score.musicxml", FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            AssertScoreIsEmpty(score);
        }

        [Fact]
        public void CanBuildBareScoreWithSkeletonMusicXml()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Bare_Score_With_Skeleton.musicxml", FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            AssertScoreIsEmpty(score);
        }

        [Fact]
        public void CanBuildFullIdentificationMusicXml()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Full_Identification.musicxml", FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            score.Info.WorkTitle.Should().Be("Work Title");
            score.Info.WorkNumber.Should().Be("Work Number");
            score.Info.MovementTitle.Should().Be("Movement Title");
            score.Info.MovementNumber.Should().Be("Movement Number");
            score.Info.Source.Should().Be("Source Description");
            score.Info.Creators.Should().BeEquivalentTo(new ScoreCreator[]{
                new ScoreCreator()
                {
                    Type= "creator-key-a",
                    Name = "creator value a",
                },
                new ScoreCreator()
                {
                    Type= "creator-key-b",
                    Name = "creator value b",
                },
                new ScoreCreator()
                {
                    Type= "creator-key-c",
                    Name = "", /* No XML value */
                },
                new ScoreCreator()
                {
                    Type = "", /* No creator type specified */
                    Name = "Ludwig van Beethoven",
                }
            });
            score.Info.Rights.Should().BeEquivalentTo(new ScoreRights[]{
                new ScoreRights()
                {
                    Type= "",
                    Content = "Sample Copyright",
                },
                new ScoreRights()
                {
                    Type= "rights-key-a",
                    Content = "rights value a",
                },
                new ScoreRights()
                {
                    Type= "rights-key-b",
                    Content = "rights value b",
                },
            });
            score.Info.EncodingSoftware.Should().BeEquivalentTo(new string[]
            {
                "Finale 2012 for Windows",
                "Dolet Light for Finale 2012"
            });
            score.Info.EncodingDates.Should().BeEquivalentTo(new DateTime[]
            {
                DateTime.Parse("2012-09-06"),
                DateTime.Parse("2012-09-09"),
            });
            score.Info.Credits.Should().BeEquivalentTo(new string[]
            {
                "Etude No. 1",
                "Alan Turing",
            });
            score.Info.Misc.Should().BeEquivalentTo(new ScoreMisc[]{
                new ScoreMisc()
                {
                    Key = "",
                    Value = "Misc value without key"
                },
                new ScoreMisc()
                {
                    Key = "misc-key-1",
                    Value = "Misc value 1"
                },
                new ScoreMisc()
                {
                    Key = "misc-key-2",
                    Value = "Misc value 2"
                },
            });
        }

        [Fact]
        public void CanBuildFullFeaturedSingleStaffMusicXml()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Etude_No._1.musicxml", FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            score.Parts.Should().ContainSingle();
            score.Parts.First().Staves.Should().HaveCount(1);

            var staff = score.Parts.First().Staves.First();

            staff.Elements[0].Should().BeEquivalentTo(new Element()
            {
                Pitch = "C4".ToPitch(),
                IsChordChild = false,
                Measure = 1,
                Staff = 1,
                Voice = 1,
                Duration = 384m,
                NotatedPitch = "C4"
            });

            var noteWithTrill = staff.Elements.SelectMany(x => x.Where(y => y.Notations.OfType<Trill>().Count() > 0)
            ).First();
            noteWithTrill.Measure.Should().Be(13);
            noteWithTrill.Pitch.Should().Be("G4".ToPitch());

            var noteWithTurn = staff.Elements.SelectMany(x => x.Where(y => y.Notations.OfType<Turn>().Count() > 0)
            ).First();
            noteWithTurn.Measure.Should().Be(14);
            noteWithTurn.Notations[0].As<Turn>().IsInverted = false;
            noteWithTurn.Pitch.Should().Be("A4".ToPitch());

            var noteWithInvertedTurn = staff.Elements.SelectMany(x => x.Where(y => y.Notations.OfType<Turn>().Count() > 0)
            ).ElementAt(1);
            noteWithInvertedTurn.Measure.Should().Be(15);
            noteWithInvertedTurn.Notations[0].As<Turn>().IsInverted = true;
            noteWithInvertedTurn.Pitch.Should().Be("B4".ToPitch());

            var noteWithMordent = staff.Elements.SelectMany(x => x.Where(y => y.Notations.OfType<Mordent>().Count() > 0)
            ).First();
            noteWithMordent.Measure.Should().Be(16);
            noteWithMordent.Notations[0].As<Mordent>().IsInverted = false;
            noteWithMordent.Pitch.Should().Be("C5".ToPitch());

            var noteWithInvertedMordent = staff.Elements.SelectMany(x => x.Where(y => y.Notations.OfType<Mordent>().Count() > 0)
            ).ElementAt(1);
            noteWithInvertedMordent.Measure.Should().Be(17);
            noteWithInvertedMordent.Notations[0].As<Mordent>().IsInverted = true;
            noteWithInvertedMordent.Pitch.Should().Be("D5".ToPitch());

            var graceNote = staff.Elements.SelectMany(x => x.Where(y => y.IsGraceNote)).First();
            graceNote.Measure.Should().Be(18);
            graceNote.Pitch.Should().Be("D4".ToPitch());

            var arpeggiatedGroups = staff.Elements.Where(x => x.Where(y => y.Notations.OfType<Arpeggiate>().Count() > 0).Count() > 0
            ).ToArray();


            var arpeggiatedDefault = arpeggiatedGroups[0].ToArray();
            arpeggiatedGroups.Should().HaveCount(3);

            arpeggiatedDefault[0].Measure.Should().Be(21);
            arpeggiatedDefault[0].Pitch.Should().Be("C4".ToPitch());
            arpeggiatedDefault[1].Pitch.Should().Be("E4".ToPitch());
            arpeggiatedDefault[2].Pitch.Should().Be("G4".ToPitch());
            arpeggiatedDefault[3].Pitch.Should().Be("C5".ToPitch());

            var arpeggiatedUp = arpeggiatedGroups[1].ToArray();
            arpeggiatedUp[0].Measure.Should().Be(22);
            arpeggiatedUp[0].Pitch.Should().Be("D4".ToPitch());
            arpeggiatedUp[1].Pitch.Should().Be("F4".ToPitch());
            arpeggiatedUp[2].Pitch.Should().Be("A4".ToPitch());
            arpeggiatedUp[3].Pitch.Should().Be("D5".ToPitch());

            var arpeggiatedDown = arpeggiatedGroups[2].ToArray();
            arpeggiatedDown[0].Measure.Should().Be(23);
            arpeggiatedDown[0].Pitch.Should().Be("E4".ToPitch());
            arpeggiatedDown[1].Pitch.Should().Be("G4".ToPitch());
            arpeggiatedDown[2].Pitch.Should().Be("B4".ToPitch());
            arpeggiatedDown[3].Pitch.Should().Be("E5".ToPitch());


            var measureWithVoices = staff.Elements.Where(x => x.Where(y => y.Measure == 26).Count() > 0
            ).ToArray();
            measureWithVoices.Should().HaveCount(4);

            measureWithVoices[0][0].Pitch.Should().Be("D4".ToPitch());
            measureWithVoices[0][0].Voice.Should().Be(3);
            measureWithVoices[0][1].Pitch.Should().Be("A4".ToPitch());
            measureWithVoices[0][1].Voice.Should().Be(2);
            measureWithVoices[0][2].Pitch.Should().Be("E5".ToPitch());
            measureWithVoices[0][2].Voice.Should().Be(1);

            measureWithVoices[1][0].Pitch.Should().Be("F4".ToPitch());
            measureWithVoices[1][0].Voice.Should().Be(3);

            measureWithVoices[2][0].Pitch.Should().Be("D4".ToPitch());
            measureWithVoices[2][0].Voice.Should().Be(3);
            measureWithVoices[2][1].Pitch.Should().Be("C5".ToPitch());
            measureWithVoices[2][1].Voice.Should().Be(2);

            measureWithVoices[3][0].Pitch.Should().Be("F4".ToPitch());
            measureWithVoices[3][0].Voice.Should().Be(3);

        }

        [Fact]
        public void CanBuildMultiVoiceGrandStaffExcerptMusicXml()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Multi_Voice_Grand_Staff_Excerpt.musicxml", FileMode.Open, FileAccess.Read));
            var score = builder.Build();

            score.Parts.Should().ContainSingle();
            score.Parts.First().Staves.Should().HaveCount(2);

            var treble = score.Parts.First().Staves.First().Elements;
            var bass = score.Parts.First().Staves[1].Elements;

            treble.Should().HaveCount(6);
            treble[0][0].Pitch.Should().Be("C5".ToPitch());
            treble[0][0].Voice.Should().Be(2);
            treble[0][1].Pitch.Should().Be("F5".ToPitch());
            treble[0][1].Voice.Should().Be(2);
            treble[0][2].Pitch.Should().Be("Bb5".ToPitch());
            treble[0][2].Voice.Should().Be(1);

            treble[1][0].Pitch.Should().Be("Ab5".ToPitch());
            treble[1][0].Voice.Should().Be(1);

            treble[2][0].Pitch.Should().Be("Bb4".ToPitch());
            treble[2][0].Voice.Should().Be(2);
            treble[2][1].Pitch.Should().Be("F5".ToPitch());
            treble[2][1].Voice.Should().Be(1);

            treble[3][0].Pitch.Should().Be("Bb4".ToPitch());
            treble[3][0].Voice.Should().Be(2);
            treble[3][1].Pitch.Should().Be("Ab5".ToPitch());
            treble[3][1].Voice.Should().Be(1);

            treble[4][0].Pitch.Should().Be("G5".ToPitch());
            treble[4][0].Voice.Should().Be(1);

            treble[5][0].Pitch.Should().Be("Gb4".ToPitch());
            treble[5][0].Voice.Should().Be(2);
            treble[5][1].Pitch.Should().Be("C5".ToPitch());
            treble[5][1].Voice.Should().Be(2);
            treble[5][2].Pitch.Should().Be("Eb5".ToPitch());
            treble[5][2].Voice.Should().Be(1);

            bass.Should().HaveCount(8);
            bass[0][0].Pitch.Should().Be("F3".ToPitch());
            bass[0][0].Voice.Should().Be(5);
            bass[1][0].Pitch.Should().Be("Eb3".ToPitch());
            bass[1][0].Voice.Should().Be(5);
            bass[2][0].Pitch.Should().Be("D3".ToPitch());
            bass[2][0].Voice.Should().Be(5);
            bass[3][0].Pitch.Should().Be("Bb2".ToPitch());
            bass[3][0].Voice.Should().Be(5);
            bass[4][0].Pitch.Should().Be("Eb3".ToPitch());
            bass[4][0].Voice.Should().Be(5);
            bass[5][0].Pitch.Should().Be("D3".ToPitch());
            bass[5][0].Voice.Should().Be(5);
            bass[6][0].Pitch.Should().Be("Eb3".ToPitch());
            bass[6][0].Voice.Should().Be(5);
            bass[7][0].Pitch.Should().Be("A2".ToPitch());
            bass[7][0].Voice.Should().Be(5);
        }
    }
}
