using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace SightReader.Engine.Interpreter
{
    public class PlaybackContext {
        public Score Score { get; set; } = new Score();
        public int ElementIndex { get; set; }
        public Action<IPianoEvent> Output { get; set; } = delegate { };
    }
    public class Interpreter
    {
        private PlaybackContext context;
        private PlaybackProcessor processor;

        public event Action<IPianoEvent> Output = delegate { };

        public Interpreter()
        {
            context = new PlaybackContext()
            {
                Output = SendOutput
            };
            processor = new PlaybackProcessor(context);
        }

        public void SetScore(Score score)
        {
            context.Score = score;
        }

        public void ResetPlayback()
        {
            context.ElementIndex = 0;
        }

        public void SeekMeasure(int measureNumber)
        {
            var lowestElementIndex = int.MaxValue;

            foreach (var part in context.Score.Parts)
            {
                foreach (var staff in part.Staves)
                {
                    int indexOfFirstElementForMeasure = staff.Elements.Select(x => x).ToList().FindIndex(x => x.Where(y => y.Measure == measureNumber).Count() > 0);
                    lowestElementIndex = Math.Min(indexOfFirstElementForMeasure, lowestElementIndex);
                }
            }

            var foundMeasureNumber = lowestElementIndex != -1 && lowestElementIndex != int.MaxValue;
            if (foundMeasureNumber)
            {
                context.ElementIndex = lowestElementIndex;
            }
        }

        internal void SendOutput(IPianoEvent e)
        {
            if (Output != null)
            {
                Output(e);
            }
        }

        public void Input(IPianoEvent e)
        {
            processor.Process(e);
        }
    }
}
