using SightReader.Engine.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.ScoreBuilder
{
    public class StaffBuilder
    {
        public int StaffNumber { get; }
        private decimal clock = 0;
        private Dictionary<decimal, Dictionary<byte, Note>> notes = new Dictionary<decimal, Dictionary<byte, Note>>();

        public StaffBuilder(int staffNumber)
        {
            StaffNumber = staffNumber;
        }

        /**
         * Adds a note and advances the clock time if the note isn't part of a chord.
         */
        public void AddNote(Note note)
        {
            var existingNotes = notes[clock] ?? new Dictionary<byte, Note>();
            existingNotes[note.Pitch] = note;

            var shouldNoteAdvanceClock = !note.IsChordNote && note.Staff == StaffNumber;
            if (shouldNoteAdvanceClock)
            {
                AdvanceClock(note.Duration);
            }
        }

        public void AdvanceClock(decimal forwardBy)
        {
            clock += forwardBy;
        }

        public void RewindClock(decimal rewindBy)
        {
            clock -= rewindBy;
        }
    }
}
