using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace SightReader.Engine.Interpreter
{
    public class PlaybackContext {
        public Score Score { get; set; } = new Score();
        public string ScoreFilePath { get; set; } = "";
        public int[] ElementIndices { get; set; } = new int[] { 0, 0 };
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

        public string ScoreFilePath
        {
            get
            {
                return context.ScoreFilePath;
            }
        }

        public void SetScore(Score score, string scoreFilePath)
        {
            context.Score = score;
            context.ScoreFilePath = scoreFilePath;
        }

        public void ResetPlayback()
        {
            context.ElementIndices = new int[] { 0, 0 };
        }

        public void SeekMeasure(int measureNumber)
        {
            var lowestElementIndices = new List<int>(2)
            {
                int.MaxValue,
                int.MaxValue
            };

            foreach (var part in context.Score.Parts)
            {
                foreach (var staff in part.Staves)
                {
                    var lowestElementIndex = lowestElementIndices[staff.Number - 1];
                    int indexOfFirstElementForMeasure = staff.Elements.Select(x => x).ToList().FindIndex(x => x.Where(y => y.Measure == measureNumber).Count() > 0);
                    lowestElementIndices[staff.Number - 1] = Math.Min(indexOfFirstElementForMeasure, lowestElementIndex);
                }
            }

            for (int i = 0; i < lowestElementIndices.Count; i++)
            {
                var lowestElementIndex = lowestElementIndices[i];
                var foundMeasureNumber = (lowestElementIndex != -1 && lowestElementIndex != int.MaxValue);

                if (foundMeasureNumber)
                {
                    context.ElementIndices[i] = lowestElementIndex;
                }
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
