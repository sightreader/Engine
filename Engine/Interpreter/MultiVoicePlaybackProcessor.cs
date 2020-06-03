using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SightReader.Engine.ScoreBuilder;
using SightReader.Engine.Interpreter;
using System.Linq;

namespace SightReader.Engine.Interpreter
{
    public enum MultiVoicePlayMode
    {
        Chord,
        Trill,
        Turn,
        Mordent,
    }
    public class MultiVoicePlaybackProcessor
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
        private List<Dictionary<byte, byte>> StavesPressedNotes = new List<Dictionary<byte, byte>>(2)
        {
            new Dictionary<byte, byte>(88),
            new Dictionary<byte, byte>(88),
        };
        private Range[] STAFF_PLAY_RANGES = new Range[]
        {
            new Range(60, 108),
            new Range(21, 60)
        };

        private PlaybackContext context;

        public MultiVoicePlaybackProcessor(PlaybackContext context) {
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

                    /* Don't release notes before they are pressed */
                    if (notePressQueue.Exists(x => x.Pitch == physicalPitch))
                    {
                        noteReleaseQueue.Add(new NoteRelease()
                        {
                            Pitch = physicalPitch
                        });
                        return;
                    }

                    var wasMappedPitchFound = pressedNotes.ContainsKey(physicalPitch);

                    if (!wasMappedPitchFound) {
                        Trace.WriteLine($"No pitch mapping found for {physicalPitch}, but it was a previously pressed note.");
                        return;
                    }

                    var mappedPitch = pressedNotes[physicalPitch];
                    if (context.Output != null)
                    {
                        pressedNotes.Remove(physicalPitch);
                        context.Output(new NoteRelease()
                        {
                            Pitch = mappedPitch
                        });
                    }
                    break;
                case NotePress press:
                    if (pressedNotes.ContainsKey(press.Pitch) || notePressQueue.Exists(x => x.Pitch == press.Pitch))
                    {
                        Console.WriteLine($"Ignoring {press.Pitch} because it's already pressed.");
                        // You can't press the same key if it's already being held down
                        // You can only do it on Android devices while testing
                        return;
                    }

                    notePressQueue.Add(press);

                    var filteredCurrentGroup = GetFilteredCurrentGroup(currentGroup!);
                    var numRequiredNotesPressed = filteredCurrentGroup.Count();
                    var areEnoughNotesPressed = notePressQueue.Count >= numRequiredNotesPressed;

                    if (areEnoughNotesPressed)
                    {
                        foreach (NotePress notePress in notePressQueue)
                        {
                            ProcessChord(targetNotePress: notePress, notePresses: notePressQueue, previousGroup!, filteredCurrentGroup, nextGroup!, pressedNotes);
                        }
                        notePressQueue.Clear();

                        foreach (var noteRelease in noteReleaseQueue)
                        {
                            Process(noteRelease);
                        }
                        noteReleaseQueue.Clear();

                        context.LastProcessedElementIndices[staff - 1] = context.ElementIndices[staff - 1];
                        context.ElementIndices[staff - 1] += 1;

                        context.Processed?.Invoke();
                    }
                    break;
            }
        }

        private IElement[] GetFilteredCurrentGroup(IElement[] currentGroup)
        {
            /* Was originally used for ties, but modified to remove ties earlier at score building. */
            return currentGroup;
        }

        private void ProcessChord(NotePress targetNotePress, List<NotePress> notePresses, IElement[] previousGroup, IElement[] filteredCurrentGroup, IElement[] nextGroup, Dictionary<byte, byte> pressedNotes)
        {
            // Zero-based index of which note out of the chord's notes is being processed
            var noteIndex = notePresses.OrderBy(x => x.Pitch).ToList().FindIndex(x => x.Pitch == targetNotePress.Pitch);

            var correctedNote = filteredCurrentGroup[noteIndex];

            if (correctedNote == null)
            {
                Trace.WriteLine($"No corrected note found in filtered current group {filteredCurrentGroup}.");
                return;
            }

            if (context.Output != null)
            {
                pressedNotes[targetNotePress.Pitch] = correctedNote.Pitch;
                context.Output(new NotePress()
                {
                    Pitch = correctedNote.Pitch,
                    Velocity = targetNotePress.Velocity
                });
            }
        }
    }
}
