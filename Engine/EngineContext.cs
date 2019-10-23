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
        Interpreter.Interpreter Interpreter { get; set; }
        Introducer.IntroducerClient Introducer { get; set; }
        Server.CommandServer Server { get; set; }
    }
}
