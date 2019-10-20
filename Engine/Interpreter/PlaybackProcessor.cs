using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SightReader.Engine.ScoreBuilder;
using SightReader.Engine.Interpreter;
using System.Linq;

namespace SightReader.Engine.Interpreter
{
    public enum PlayMode
    {
        Chord,
        Trill,
        Turn,
        Mordent,
    }
    public class PlaybackProcessor
    {
        private List<List<NotePress>> StavesEventQueue = new List<List<NotePress>>(2)
        {
            new List<NotePress>(10),
            new List<NotePress>(10)
        };
        private List<Dictionary<byte, byte>> PressedNotes = new List<Dictionary<byte, byte>>(2)
        {
            new Dictionary<byte, byte>(88),
            new Dictionary<byte, byte>(88),
        };
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
                    for (int i = STAFF_PLAY_RANGES.Length - 1; i >= 0; i--)
                    {
                        var range = STAFF_PLAY_RANGES[i];
                        if (press.Pitch >= range.Start.Value && press.Pitch < range.End.Value)
                        {
                            return (byte)(i);
                        }
                    }
                    return 0;
                case NoteRelease release:
                    for (int i = STAFF_PLAY_RANGES.Length - 1; i >= 0; i--)
                    {
                        var range = STAFF_PLAY_RANGES[i];
                        if (release.Pitch >= range.Start.Value && release.Pitch < range.End.Value)
                        {
                            return (byte)(i);
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
            var elements = context.Score.Parts[0].Staves[staff - 1].Elements;
            var eventQueue = StavesEventQueue[staff - 1];
            var pressedNotes = PressedNotes[staff - 1];

            var previousGroup = Clock > 0 ? elements[Clock - 1] : null;
            var currentGroup = Clock < elements.Length ? elements[Clock] : null;
            var nextGroup = Clock < elements.Length - 1 ? elements[Clock + 1] : null;

            var isFinished = currentGroup == null;
            var isStarting = previousGroup == null;

            if (isFinished)
            {
                return;
            }

            switch (e)
            {
                case PedalChange pedal:
                    if (context.Output != null)
                    {
                        context.Output(pedal);
                    }
                    break;
                case NoteRelease release:
                    var physicalPitch = release.Pitch;
                    var wasMappedPitchFound = pressedNotes.ContainsKey(physicalPitch);
                    
                    if (!wasMappedPitchFound) {
                        Trace.WriteLine($"No pitch mapping found for {physicalPitch}, but it was a previously pressed note.");
                        return;
                    }

                    var mappedPitch = pressedNotes[physicalPitch];
                    if (context.Output != null)
                    {
                        context.Output(new NoteRelease()
                        {
                           Pitch = mappedPitch
                        });
                    }
                    break;
                case NotePress press:
                    eventQueue.Add(press);

                    var areEnoughNotesPressed = eventQueue.Count >= currentGroup.Length;

                    if (areEnoughNotesPressed)
                    {
                        foreach (NotePress notePress in eventQueue)
                        {
                            ProcessChord(targetNotePress: notePress, notePresses: eventQueue, previousGroup, currentGroup, nextGroup);
                        }
                        eventQueue.Clear();
                    }
                    break;
            }
        }

        private void ProcessChord(NotePress targetNotePress, List<NotePress> notePresses, IElement[] previousGroup, IElement[] currentGroup, IElement[] nextGroup)
        {
            // Zero-based index of which note out of the chord's notes is being processed
            var noteIndex = notePresses.OrderBy(x => x.Pitch).ToList().FindIndex(x => x.Pitch == targetNotePress.Pitch);

            var correctedNote = currentGroup[noteIndex];

            if (correctedNote == null)
            {
                Trace.WriteLine($"No corrected note found in current group {currentGroup}.");
                return;
            }

            if (context.Output != null)
            {
                context.Output(new NotePress()
                {
                    Pitch = correctedNote.Pitch,
                    Velocity = targetNotePress.Velocity
                });
            }
        }
    }
}
