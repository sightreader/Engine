using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public class PlaybackContext {
        public Score Score { get; set; } = new Score();
        public int ElementIndex { get; set; }
        public Action<IPianoEvent> Output { get; set; }
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
                Output = Output
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

        public void Input(IPianoEvent e)
        {
            processor.Process(e);
        }
    }
}
