using MusicXmlSchema;
using System;
using Xunit;
using SightReader.Engine.ScoreBuilder;
using FluentAssertions;
using System.IO;
using SightReader.Engine.Interpreter;

namespace SightReader.Engine.Tests
{
    public class ScoreBuilderTests
    {
        [Fact]
        public void CanBuildValidMusicXml()
        {
            var builder = new ScoreBuilder.ScoreBuilder(new FileStream(@"Assets\Etude_No._1.musicxml", FileMode.Open));
            var score = builder.Build();
        }
    }
}
