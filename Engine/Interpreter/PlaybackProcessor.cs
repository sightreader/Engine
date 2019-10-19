using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public class PlaybackProcessor
    {
        private Range[] STAFF_PLAY_RANGES = new Range[]
        {
            new Range(21, 60),
            new Range(60, 108)
        };

        private PlaybackContext context;

        public PlaybackProcessor(PlaybackContext context) {
            this.context = context;
        }

        public byte GetStaffForEvent(IPianoEvent e)
        {
            switch (e)
            {
                case NotePress press:
                    for (int i = 0; i < STAFF_PLAY_RANGES.Length; i++)
                    {
                        var range = STAFF_PLAY_RANGES[i];
                        if (press.Pitch >= range.Start.Value && press.Pitch < range.End.Value)
                        {
                            return (byte)(i + 1);
                        }
                    }
                    return 0;
                case NoteRelease release:
                    for (int i = 0; i < STAFF_PLAY_RANGES.Length; i++)
                    {
                        var range = STAFF_PLAY_RANGES[i];
                        if (release.Pitch >= range.Start.Value && release.Pitch < range.End.Value)
                        {
                            return (byte)(i + 1);
                        }
                    }
                    return 0;
            }

            return 0;
        }

        private int Clock
        {
            get
            {
                return context.ElementIndex;
            }
            set
            {
                context.ElementIndex = value;
            }
        }

        public void Process(IPianoEvent e)
        {
            var staff = GetStaffForEvent(e);
            var elements = context.Score.Parts[0].Staves[staff].Elements;

            var previousGroup = Clock > 0 ? elements[Clock - 1] : null;
            var currentGroup = Clock < elements.Length ? elements[Clock] : null;
            var nextGroup = Clock < elements.Length - 1 ? elements[Clock + 1] : null;
        }
    }
}
