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
        public int[] LastProcessedElementIndices { get; set; } = new int[] { 0, 0 };
        public int[] ElementIndices { get; set; } = new int[] { 0, 0 };
        public Action<IPianoEvent> Output { get; set; } = delegate { };
        public Action Processed { get; set; } = delegate { };
    }
    public class Interpreter
    {
        private PlaybackContext context;
        private PlaybackProcessor processor;

        public event Action<IPianoEvent> Output = delegate { };
        public event Action Processed = delegate { };

        public Interpreter()
        {
            context = new PlaybackContext()
            {
                Output = SendOutput,
                Processed = SendProcessed 
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

        public int[] GetMeasureNumbers()
        {
            var measureNumbers = new int[] { -1, -1 };

            for (var i = 0; i < context.LastProcessedElementIndices.Length; i++)
            {
                var elementIndex = context.LastProcessedElementIndices[i];
                var staffIndex = i;

                var elements = context.Score.Parts[0].Staves[staffIndex].Elements[elementIndex];
                if (elements.Length > 0)
                {
                    measureNumbers[staffIndex] = Math.Max(measureNumbers[staffIndex], elements.First().Measure);
                }
            }

            return measureNumbers;
        }

        public int[] GetMeasureGroupIndices()
        {
            var currentMeasureNumbers = GetMeasureNumbers();
            var groupIndices = new int[] { -1, -1 };

            for (var i = 0; i < context.LastProcessedElementIndices.Length; i++)
            {
                var elementIndex = context.LastProcessedElementIndices[i];
                var staffIndex = i;

                var elementGroups = context.Score.Parts[0].Staves[staffIndex].Elements;

                var currentMeasureNumber = currentMeasureNumbers[staffIndex];
                if (currentMeasureNumber <= 2)
                {
                    var lowestMeasureNumber = elementGroups[0][0].Measure;
                    if (currentMeasureNumber == lowestMeasureNumber)
                    {
                        groupIndices[staffIndex] = elementIndex;
                        continue;
                    }
                }

                for (var lastElementIndexOfPreviousMeasure = elementIndex; lastElementIndexOfPreviousMeasure >= 0; lastElementIndexOfPreviousMeasure--)
                {
                    var elementGroup = elementGroups[lastElementIndexOfPreviousMeasure];
                    if (elementGroup.Length > 0 && elementGroup.First().Measure < currentMeasureNumber)
                    {
                        groupIndices[staffIndex] = elementIndex - (lastElementIndexOfPreviousMeasure + 1);
                        break;
                    }
                }
            }

            return groupIndices;
        }

        public void SetScore(Score score, string scoreFilePath)
        {
            context.Score = score;
            context.ScoreFilePath = scoreFilePath;
            ResetPlayback();
        }

        public void ResetPlayback()
        {
            context.ElementIndices = new int[] { 0, 0 };
        }

        public void SeekMeasure(int measureNumber)
        {
            if (measureNumber == 0)
            {
                ResetPlayback();
                return;
            }

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
            Output?.Invoke(e);
        }

        internal void SendProcessed()
        {
            Processed?.Invoke();
        }

        public void Input(IPianoEvent e)
        {
            processor.Process(e);
        }
    }
}
