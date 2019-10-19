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
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Bare_Score.musicxml", FileMode.Open));
            var score = builder.Build();

            AssertScoreIsEmpty(score);
        }

        [Fact]
        public void CanBuildBareScoreWithSkeletonMusicXml()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Bare_Score_With_Skeleton.musicxml", FileMode.Open));
            var score = builder.Build();

            AssertScoreIsEmpty(score);
        }

        [Fact]
        public void CanBuildFullIdentificationMusicXml()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Full_Identification.musicxml", FileMode.Open));
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
    }
}
