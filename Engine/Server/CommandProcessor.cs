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

        private void SendReply<T>(T command, Client client)
        {
            client.Socket.Send(MessagePackSerializer.Serialize(command));
        }

        public void Process(Command rawCommand, RequestResponse requestResponse, byte[] bytes, Client client)
        {
            var json = MessagePackSerializer.ToJson(bytes);
            Log.Debug($"[CommandProcessor] Message Received: {json}");

            switch (rawCommand)
            {
                case Command.EnumerateMidiDevices when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<EnumerateMidiDevicesRequest>(bytes);
                        SendReply<EnumerateMidiDevicesResponse>(ProcessEnumerateMidiDevicesRequest(command), client);
                    }
                    break;
                case Command.SelectMidiDevices when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<SelectMidiDevicesRequest>(bytes);
                        SendReply<SelectMidiDevicesResponse>(ProcessSelectMidiDevicesRequest(command), client);
                    }
                    break;
                case Command.EnumerateScores when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<EnumerateScoresRequest>(bytes);
                        SendReply<EnumerateScoresResponse>(ProcessEnumerateScoresRequest(command), client);
                    }
                    break;
                case Command.LoadScore when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<LoadScoreRequest>(bytes);
                        SendReply<LoadScoreResponse>(ProcessLoadScoreRequest(command), client);
                    }
                    break;
                case Command.SetPlaybackPosition when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<SetPlaybackPositionRequest>(bytes);
                        SendReply<SetPlaybackPositionResponse>(ProcessSetPlaybackPositionRequest(command), client);
                    }
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
                EnabledInputDeviceNames = inputs.Select(x => Engine.MidiInputs.Where(y => x == y.Details.Name).Count() > 0).ToArray(),
                EnabledOutputDeviceNames = outputs.Select(x => Engine.MidiOutputs.Where(y => x == y.Details.Name).Count() > 0).ToArray(),
                Error = error
            };
        }

        private SelectMidiDevicesResponse ProcessSelectMidiDevicesRequest(SelectMidiDevicesRequest command)
        {
            var midiAccess = MidiAccessManager.Default;

            var inputs = new string[] { };
            var outputs = new string[] { };
            var error = "";

            try
            {
                inputs = midiAccess.Inputs.Select(x => x.Name).ToArray();
                outputs = midiAccess.Outputs.Select(x => x.Name).ToArray();
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            foreach (var inputDeviceName in command.InputDeviceNames)
            {
                try
                {
                    if (Engine.MidiInputs.Exists(x => x.Details.Name == inputDeviceName))
                    {
                        var input = Engine.MidiInputs.Find(x => x.Details.Name == inputDeviceName);
                        input.CloseAsync().Wait();
                        Engine.MidiInputs.Remove(input);
                    }
                    else
                    {
                        var inputDevice = midiAccess.OpenInputAsync(midiAccess.Inputs.Where(x => x.Name == inputDeviceName).First().Id).Result;
                        Engine.MidiInputs.Add(inputDevice);
                    }
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
                    if (Engine.MidiOutputs.Exists(x => x.Details.Name == outputDeviceName))
                    {
                        var output = Engine.MidiOutputs.Find(x => x.Details.Name == outputDeviceName);
                        output.CloseAsync().Wait();
                        Engine.MidiOutputs.Remove(output);
                    }
                    else
                    {
                        var outputDevice = midiAccess.OpenOutputAsync(midiAccess.Outputs.Where(x => x.Name == outputDeviceName).First().Id).Result;
                        Engine.MidiOutputs.Add(outputDevice);
                    }
                }
                catch (Exception ex)
                {
                    error += $"Error opening output device '{outputDeviceName}': {ex.Message}\n";
                }
            }

            return new SelectMidiDevicesResponse()
            {
                Error = error,
                InputDeviceNames = inputs,
                OutputDeviceNames = outputs,
                EnabledInputDeviceNames = inputs.Select(x => Engine.MidiInputs.Exists(y => x == y.Details.Name)).ToArray(),
                EnabledOutputDeviceNames = outputs.Select(x => Engine.MidiOutputs.Exists(y => x == y.Details.Name)).ToArray(),
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
            var scoreBytes = new byte[] { };

            try
            {
                var isPathJailedToScoresDir = Path.GetFullPath(command.FilePath).StartsWith(Path.Combine(Environment.CurrentDirectory, "scores"), StringComparison.OrdinalIgnoreCase);
                if (isPathJailedToScoresDir && Path.GetExtension(command.FilePath) == ".musicxml" && File.Exists(command.FilePath))
                {
                    var fileStream = new FileStream(command.FilePath, FileMode.Open, FileAccess.Read);
                    var scoreBuilder = new ScoreBuilder.ScoreBuilder(fileStream);
                    var score = scoreBuilder.Build();
                    Engine.Interpreter.SetScore(score);
                    Engine.Interpreter.ResetPlayback();

                    using var memoryStream = new MemoryStream();
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.CopyTo(memoryStream);
                    scoreBytes = memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return new LoadScoreResponse()
            {
                Score = scoreBytes,
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
