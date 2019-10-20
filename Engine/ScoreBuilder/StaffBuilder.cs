using SightReader.Engine.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SightReader.Engine.ScoreBuilder
{
    public class StaffBuilder
    {
        public int StaffNumber { get; }
        private decimal clock = 0;
        private SortedDictionary<decimal, SortedDictionary<byte, Element>> notes = new SortedDictionary<decimal, SortedDictionary<byte, Element>>();
        private Element previousEl = new Element();

        public StaffBuilder(int staffNumber)
        {
            StaffNumber = staffNumber;
        }

        /**
         * Adds a note and advances the clock time if the note isn't part of a chord.
         */
        public void ProcessNote(Element el)
        {
            var isStartingChord = el.IsChordChild && !previousEl.IsChordChild;
            var wasChordEnded = previousEl.IsChordChild && !el.IsChordChild;
            var shouldRewindLastAppliedDuration = isStartingChord;
            var shouldForwardLastUnappliedDuration = wasChordEnded;
            var shouldAdvanceClock = !el.IsChordChild;

            if (shouldRewindLastAppliedDuration)
            {
                RewindClock(previousEl.Duration);
            }
            if (shouldForwardLastUnappliedDuration)
            {
                AdvanceClock(previousEl.Duration);
            }

            if (el.Staff == StaffNumber)
            {
                AddElement(el);
            }

            if (shouldAdvanceClock) {
                AdvanceClock(el.Duration);
            }

            previousEl = el;
        }

        private void AddElement(Element el)
        {
            if (!notes.ContainsKey(clock))
            {
                notes[clock] = new SortedDictionary<byte, Element>();
            }
            notes[clock][el.Pitch] = el;
        }

        public void AdvanceClock(decimal forwardBy)
        {
            clock += forwardBy;
        }

        public void RewindClock(decimal rewindBy)
        {
            clock -= rewindBy;
        }

        public IEnumerable<List<IElement>> GetElements()
        {
            return notes.Values.Select(x => x.Values.Where(y => !y.IsRest).ToList<IElement>()).Where(x => x.Count > 0);
        }
    }
}
