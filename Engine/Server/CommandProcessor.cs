using Commons.Music.Midi;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MessagePack;
using System.IO;
using System.Text.RegularExpressions;

namespace SightReader.Engine.Server
{
    public class CommandProcessor
    {
        private IEngineContext Engine;

        public CommandProcessor(IEngineContext engineContext)
        {
            Engine = engineContext;
        }

        private void SendReply(ICommand command, Client client)
        {
            client.Socket.Send(MessagePackSerializer.Serialize(command));
        }

        public void Process(ICommand rawCommand, Client client)
        {
            switch (rawCommand)
            {
                case EnumerateMidiDevicesRequest command:
                    SendReply(ProcessEnumerateMidiDevicesRequest(command), client);
                    break;
                case SelectMidiDevicesRequest command:
                    SendReply(ProcessSelectMidiDevicesRequest(command), client);
                    break;
                case EnumerateScoresRequest command:
                    SendReply(ProcessEnumerateScoresRequest(command), client);
                    break;
                case LoadScoreRequest command:
                    SendReply(ProcessLoadScoreRequest(command), client);
                    break;
                case SetPlaybackPositionRequest command:
                    SendReply(ProcessSetPlaybackPositionRequest(command), client);
                    break;
                default:
                    Log.Debug($"{client.Id}: [CommandProcessor] Unrecognized command.");
                    break;
            }
        }

        private EnumerateMidiDevicesResponse ProcessEnumerateMidiDevicesRequest(EnumerateMidiDevicesRequest command)
        {
            var inputs = new string[] { };
            var outputs = new string[] { };
            var error = "";

            try
            {
                inputs = MidiAccessManager.Default.Inputs.Select(x => x.Name).ToArray();
                outputs = MidiAccessManager.Default.Outputs.Select(x => x.Name).ToArray();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return new EnumerateMidiDevicesResponse()
            {
                InputDeviceNames = inputs,
                OutputDeviceNames = outputs,
                Error = error
            };
        }

        private SelectMidiDevicesResponse ProcessSelectMidiDevicesRequest(SelectMidiDevicesRequest command)
        {
            var inputs = new string[] { };
            var outputs = new string[] { };
            var error = "";

            var midiAccess = MidiAccessManager.Default;

            foreach (var inputDeviceName in command.InputDeviceNames)
            {
                try
                {
                    var inputDevice = midiAccess.OpenInputAsync(midiAccess.Inputs.Where(x => x.Name == inputDeviceName).First().Id).Result;
                    Engine.MidiInputs.Add(inputDevice);
                }
                catch (Exception ex)
                {
                    error += $"Error opening input device '{inputDeviceName}': {ex.Message}\n";
                }
            }

            foreach (var outputDeviceName in command.OutputDeviceNames)
            {
                try
                {
                    var outputDevice = midiAccess.OpenOutputAsync(midiAccess.Inputs.Where(x => x.Name == outputDeviceName).First().Id).Result;
                    Engine.MidiOutputs.Add(outputDevice);
                }
                catch (Exception ex)
                {
                    error += $"Error opening output device '{outputDeviceName}': {ex.Message}\n";
                }
            }

            return new SelectMidiDevicesResponse()
            {
                Error = error
            };
        }

        private EnumerateScoresResponse ProcessEnumerateScoresRequest(EnumerateScoresRequest command)
        {
            var filePaths = new string[] { };
            var error = "";

            try
            {
                filePaths = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "scores"), "*.musicxml", new EnumerationOptions()
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    MatchType = MatchType.Simple,
                    RecurseSubdirectories = true,
                    ReturnSpecialDirectories = false
                });
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return new EnumerateScoresResponse()
            {
                FilePaths = filePaths,
                Error = error
            };
        }

        private LoadScoreResponse ProcessLoadScoreRequest(LoadScoreRequest command)
        {
            var error = "";

            try
            {
                var isPathJailedToScoresDir = Path.GetFullPath(command.FilePath).StartsWith(Path.Combine(Environment.CurrentDirectory, "scores"), StringComparison.OrdinalIgnoreCase);
                if (isPathJailedToScoresDir && Path.GetExtension(command.FilePath) == ".musicxml" && File.Exists(command.FilePath))
                {
                    var fileStream = new FileStream(command.FilePath, FileMode.Open, FileAccess.Read);
                    var scoreBuilder = new ScoreBuilder.ScoreBuilder(fileStream);
                    var score = scoreBuilder.Build();
                    Engine.Interpreter.SetScore(score);
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return new LoadScoreResponse()
            {
                Error = error
            };
        }

        private SetPlaybackPositionResponse ProcessSetPlaybackPositionRequest(SetPlaybackPositionRequest command)
        {
            var error = "";

            try
            {
                Engine.Interpreter.SeekMeasure(command.MeasureNumber);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return new SetPlaybackPositionResponse()
            {
                Error = error
            };
        }
    }
}
