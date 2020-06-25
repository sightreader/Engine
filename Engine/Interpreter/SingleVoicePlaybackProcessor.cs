using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SightReader.Engine.ScoreBuilder;
using SightReader.Engine.Interpreter;
using System.Linq;

namespace SightReader.Engine.Interpreter
{
    public enum SingleVoicePlayMode
    {
        Chord,
        Trill,
        Turn,
        Mordent,
    }
    public class SingleVoicePlaybackProcessor
    {
        private List<List<NotePress>> StavesUpcomingNotePressQueue = new List<List<NotePress>>(2)
        {
            new List<NotePress>(10),
            new List<NotePress>(10)
        };
        private List<List<NoteRelease>> StavesUpcomingNoteReleaseQueue = new List<List<NoteRelease>>(2)
        {
            new List<NoteRelease>(10),
            new List<NoteRelease>(10)
        };
        private List<Dictionary<byte, List<byte>>> StavesPressedNotes = new List<Dictionary<byte, List<byte>>>(2)
        {
            new Dictionary<byte, List<byte>>(88),
            new Dictionary<byte, List<byte>>(88),
        };
        private Range[] STAFF_PLAY_RANGES = new Range[]
        {
            new Range(60, 108),
            new Range(21, 60)
        };

        private PlaybackContext context;

        public SingleVoicePlaybackProcessor(PlaybackContext context)
        {
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

        public void Process(IPianoEvent e)
        {
            if (e is PedalChange pedal)
            {
                if (context.Output != null)
                {
                    context.Output(pedal);
                }
                return;
            }

            var staff = GetStaffForEvent(e);
            var elements = context.Score.Parts[0].Staves[staff - 1].Elements;
            var noteReleaseQueue = StavesUpcomingNoteReleaseQueue[staff - 1];
            var notePressQueue = StavesUpcomingNotePressQueue[staff - 1];
            var pressedNotes = StavesPressedNotes[staff - 1];
            var clock = context.ElementIndices[staff - 1];

            var previousGroup = clock > 0 ? elements[clock - 1] : null;
            var currentGroup = clock < elements.Length ? elements[clock] : null;
            var nextGroup = clock < elements.Length - 1 ? elements[clock + 1] : null;

            var isFinished = currentGroup == null;
            var isStarting = previousGroup == null;


            switch (e)
            {
                case NoteRelease release:
                    var physicalPitch = release.Pitch;

                    var wasMappedPitchesFound = pressedNotes.ContainsKey(physicalPitch);

                    if (!wasMappedPitchesFound)
                    {
                        Trace.WriteLine($"{physicalPitch} was physically pressed and should correspond to the virtual playing of a notated note or chord, but the key press was not found to be linked to any virtually pressed notes.");
                        return;
                    }

                    var mappedPitches = pressedNotes[physicalPitch];
                    if (context.Output != null)
                    {
                        pressedNotes.Remove(physicalPitch);
                        foreach (var mappedPitch in mappedPitches)
                        {
                            context.Output(new NoteRelease()
                            {
                                Pitch = mappedPitch
                            });
                        }
                    }
                    break;
                case NotePress press:
                    if (isFinished)
                    {
                        return;
                    }

                    ProcessChord(physicalNotePressed: press, previousGroup!, currentGroup!, nextGroup!, pressedNotes);

                    context.LastProcessedElementIndices[staff - 1] = context.ElementIndices[staff - 1];
                    context.ElementIndices[staff - 1] += 1;

                    context.Processed?.Invoke();
                    break;
            }
        }

        private void ProcessChord(NotePress physicalNotePressed, IElement[] previousGroup, IElement[] currentGroup, IElement[] nextGroup, Dictionary<byte, List<byte>> pressedNotes)
        {
            foreach (var element in currentGroup)
            {
                if (context.Output != null)
                {
                    if (!pressedNotes.ContainsKey(physicalNotePressed.Pitch))
                    {
                        pressedNotes[physicalNotePressed.Pitch] = new List<byte>(currentGroup.Length);
                    }
                    pressedNotes[physicalNotePressed.Pitch].Add(element.Pitch);
                    context.Output(new NotePress()
                    {
                        Pitch = element.Pitch,
                        Velocity = physicalNotePressed.Velocity
                    });
                }
            }
        }
    }
}
