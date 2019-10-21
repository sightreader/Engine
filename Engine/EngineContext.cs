using System;
using System.Collections.Generic;
using System.Text;
using Commons.Music.Midi;
using System.Linq;

namespace SightReader.Engine
{
    public interface IEngineContext
    {
        List<IMidiInput> MidiInputs { get; set; }
        List<IMidiOutput> MidiOutputs { get; set; }
        public Interpreter.Interpreter Interpreter { get; set; }
        public Introducer.IntroducerClient Introducer { get; set; }
        public Server.CommandServer Server { get; set; }
    }
}
