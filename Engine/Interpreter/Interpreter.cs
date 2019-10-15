using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public class Interpreter
    {
        public event Action<IPianoEvent> Received = delegate { };

        public void Play(IPianoEvent e)
        {
            throw new NotImplementedException();
        }
    }
}
