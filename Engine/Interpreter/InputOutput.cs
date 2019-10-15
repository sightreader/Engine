using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Interpreter
{
    public interface IInputOutput
    {
        event Action<IPianoEvent> Received;

        void Send(IPianoEvent e);
    }

    public class Input : IInputOutput
    {

        public event Action<IPianoEvent> Received = delegate { };

        public void Send(IPianoEvent e)
        {
            Received(e);
        }
    }

    public class Output : IInputOutput
    {

        public event Action<IPianoEvent> Received = delegate { };

        public void Send(IPianoEvent e)
        {
            Received(e);
        }
    }
}
