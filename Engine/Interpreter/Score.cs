using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public class ScoreCreator
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class ScoreRights
    {
        public string Type { get; set;  } = "";
        public string Content { get; set;  } = "";
    }

    public class ScoreMisc
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class ScoreInfo
    {
        public string WorkTitle { get; set; } = "";
        public string WorkNumber { get; set; } = "";
        public string MovementNumber { get; set; } = "";
        public string MovementTitle { get; set; } = "";
        public ScoreCreator[] Creators { get; set; } = new ScoreCreator[] { };
        public ScoreRights[] Rights { get; set; } = new ScoreRights[] { };
        public ScoreMisc[] Misc { get; set; } = new ScoreMisc[] { };
        public string[] EncodingSoftware { get; set; } = new string[] { };
        public DateTime[] EncodingDates { get; set; } = new DateTime[] { };
        public string Source { get; set; } = "";
        public string[] Credits { get; set; } = new string[] { };
    }

    public class Part
    {
        public string Id { get; set; } = "";
        public string Name { get; set;  } = "";

        public Staff[] Staves { get; set; } = new Staff[] { };
    }

    public class Staff
    {
        public IElement[] Elements { get; set; } = new IElement[] { };
        public IDirective[] Directives { get; set; } = new IDirective[] { };
        public int Number { get; set; } = 1;
    }

    public interface IDirective { }

    /**
     * Decribes the divisions per quarter note starting from a measure/note.
     * 
     * A cached list of beat duration directives can describe which notes should use which beat duration directive.
     */
    public class BeatDurationDirective : IDirective
    {
        /**
         * Musical notation duration is commonly represented as fractions. The divisions element indicates how many divisions per quarter note are used to indicate a note's duration. For example, if duration = 1 and divisions = 2, this is an eighth note duration. Duration and divisions are used directly for generating sound output, so they must be chosen to take tuplets into account. Using a divisions element lets us use just one number to represent a duration for each note in the score, while retaining the full power of a fractional representation.
         * 
         * Note: This can be changed mid-measure.
         */
        public decimal Divisions { get; set; }

        /**
         * Defines the element range in the staff in which this directive first applies.
         */
        public Range ElementRange { get; set; }
    }

    /**
     * Decribes the element range of a repeat.
     */
    public class RepeatDirective : IDirective
    {
        public Range ElementRange { get; set; }
    }


    public class Score
    {
        public ScoreInfo Info { get; set; } = new ScoreInfo();
        public Part[] Parts { get; set; } = new Part[] { };

        public static Score CreateFromMusicXml(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
