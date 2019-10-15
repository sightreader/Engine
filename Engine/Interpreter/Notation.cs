using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public interface INotation
    {

    }

    public class Tie : INotation
    {
        public StartStopContinue Type { get; set; }
        /**
         * The number attribute is rarely needed to disambiguate ties, since note pitches will usually suffice. The attribute is implied rather than defaulting to 1 as with most elements. It is available for use in more complex tied notation situations.
         */
        public byte Number { get; set; }
    }

    public class Slur : INotation
    {
        public StartStopContinue Type { get; set; }
        /**
         * 	When a number-level value is implied, the value is 1 by default.
         */
        public byte Number { get; set; } 
    }

    public class Arpeggiate : INotation
    {
        public bool IsDownwards { get; set; }
        public byte Number { get; set; }
    }

    public class Dynamics : INotation
    {
        public string Marking { get; set; } = "";
    }

    public class Fermata : INotation
    {
        public bool IsInverted { get; set; }
    }

    public class Glissando : INotation
    {
        public bool IsStarting { get; set; }
        public byte Number { get; set; }
    }

    /**
     * The non-arpeggiate type indicates that this note is at the top or bottom of a bracket indicating to not arpeggiate these notes. Since this does not involve playback, it is only used on the top or bottom notes, not on each note as for the arpeggiate type.
     */
        public class NonArpeggiate : INotation
    {
        public bool IsTop { get; set; }
        public byte Number { get; set; }
    }

    public class Slide : INotation
    {
        public bool IsStarting { get; set; }
        public byte Number { get; set; }
    }

    public class Articulation : INotation
    {
        /**
         * The accent element indicates a regular horizontal accent mark.
         */
        public bool IsAccent { get; set; }
        /**
         * The caesura element indicates a slight pause. It is notated using a "railroad tracks" symbol.
         */
        public bool IsCaesura { get; set; }
        /**
         * The detached-legato element indicates the combination of a tenuto line and staccato dot symbol.
         */
        public bool IsDetachedLegato { get; set; }
        /**
         * The doit element is an indeterminate slide attached to a single note. The doit element appears after the main note and goes above the main pitch.
         */
        public bool IsDoit { get; set; }
        /**
         * The falloff element is an indeterminate slide attached to a single note. The falloff element appears before the main note and goes below the main pitch.
         */
        public bool IsFalloff { get; set; }
        /**
         * The other-articulation element is used to define any articulations not yet in the MusicXML format. This allows extended representation, though without application interoperability.
         */
        public string? Other { get; set; }
        /**
         * The plop element is an indeterminate slide attached to a single note. The plop element appears before the main note and comes from above the main pitch.
         */
        public bool IsPlop { get; set; }
        /**
         * The scoop element is an indeterminate slide attached to a single note. The scoop element appears before the main note and comes from below the main pitch.
         */
        public bool IsScoop { get; set; }
        /**
         * The spiccato element is used for a stroke articulation, as opposed to a dot or a wedge.
         */
        public bool IsSpiccato { get; set; }
        /**
         * The staccatissimo element is used for a wedge articulation, as opposed to a dot or a stroke.
         */
        public bool IsStaccatissimo { get; set; }
        /**
         * The staccato element is used for a dot articulation, as opposed to a stroke or a wedge.
         */
        public bool IsStaccato { get; set; }
        /**
         * The stress element indicates a stressed note.
         */
        public bool IsStressed { get; set; }
        /**
         * The strong-accent element indicates a vertical accent mark.
         */
        public bool IsStrongAccent { get; set; }
        /**
         * The tenuto element indicates a tenuto line symbol.
         */
        public bool IsTenuto { get; set; }
        /**
         * The unstress element indicates an unstressed note. It is often notated using a u-shaped symbol.
         */
        public bool IsUnstresed { get; set; }
    }

    public class Ornament : INotation
    {
        public bool IsDelayedInvertedTurn { get; set; }
        public bool IsDelayedTurn { get; set; }
    }

    public interface IOrnament
    {

    }

    public class Turn : INotation, IOrnament
    {
        /**
         * The delayed-inverted-turn element indicates an inverted turn that is delayed until the end of the current note.
         */
        public bool IsDelayed { get; set; }
        public bool IsInverted { get; set; }
        public StartNote StartNote { get; set; }
    }

    public class Mordent : INotation, IOrnament
    {
        public bool IsInverted { get; set; }
    }

    /**
     * The name for this ornament is based on the German, to avoid confusion with the more common slide element defined earlier.
     */
    public class Schleifer : INotation, IOrnament
    {

    }

    /**
     * The shake element has a similar appearance to an inverted-mordent element
     */
    public class Shake : INotation, IOrnament
    {

    }

    public enum TremoloType
    {
        Start,
        Stop,
        Single
    }

    public class Tremolo : INotation, IOrnament
    {
        public TremoloType Type { get; set; } = TremoloType.Single;
    }

    /**
     * The trill-step attribute describes the alternating note of trills and mordents for playback, relative to the current note.
     */
    public enum TrillStep
    {
        Whole,
        Half,
        Unison
    }

    public enum StartNote
    {
        Upper,
        Main,
        Below
    }

    /**
     * The two-note-turn type describes the ending notes of trills and mordents for playback, relative to the current note.
     */
    public enum TwoNoteTurn
    {
        Whole,
        Half,
        None
    }

    public class Trill : INotation, IOrnament
    {
        public StartNote StartNote { get; set; }
        public TrillStep TrillStep { get; set; }
        public TwoNoteTurn TwoNoteTurn { get; set; }
    }

    public enum StartStopContinue
    {
        Start,
        Stop,
        Continue
    }

    public class WavyLine : INotation, IOrnament
    {
        public StartStopContinue Type { get; set; }
        public byte Number { get; set; }
        public StartNote StartNote { get; set; }
        public TrillStep TrillStep { get; set; }
        public TwoNoteTurn TwoNoteTurn { get; set; }
    }
}
