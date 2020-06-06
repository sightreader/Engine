using Commons.Music.Midi;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MessagePack;
using System.IO;
using System.Text.RegularExpressions;
using SightReader.Engine.Interpreter;

namespace SightReader.Engine.Server
{
    public class CommandProcessor
    {
        private IConfig Config;
        private IEngineContext Engine;
        private ClientManager ClientManager;

        public CommandProcessor(IConfig config, IEngineContext engineContext, ClientManager clientManager)
        {
            Config = config;
            Engine = engineContext;
            ClientManager = clientManager;
            Engine.Interpreter.Output += OnInterpreterOutput;
            Engine.Interpreter.Processed += OnInterpreterProcessed;
        }

        private void OnInterpreterOutput(IPianoEvent e)
        {
            foreach (var output in Engine.MidiOutputs)
            {
                switch (e)
                {
                    case PedalChange pedal:
                        output.Send(new byte[]
                        {
                            MidiEvent.CC,
                            pedal.Pedal switch
                            {
                               PedalKind.UnaCorda => 67,
                               PedalKind.Sostenuto => 66,
                               PedalKind.Sustain => 64,
                               _ => 64
                            },
                            pedal.Position
                        }, 0, 3, 0);
                        break;
                    case NoteRelease release:
                        output.Send(new byte[]
                        {
                            MidiEvent.NoteOff,
                            release.Pitch,
                            64 /* Default release velocity */
                        }, 0, 3, 0);
                        break;
                    case NotePress press:
                        output.Send(new byte[]
                        {
                            MidiEvent.NoteOn,
                            press.Pitch,
                            press.Velocity
                        }, 0, 3, 0);
                        break;
                }
            }
        }

        private void OnInterpreterProcessed()
        {
            foreach (var client in ClientManager.Clients)
            {
                SendReply(new SetScoreDisplayPositionRequest()
                {
                    MeasureNumbers = Engine.Interpreter.GetMeasureNumbers(),
                    GroupIndices = Engine.Interpreter.GetMeasureGroupIndices()
                }, client);
            }
        }

        private void SendReply<T>(T command, Client client)
        {
            if (client.Socket == null)
            {
                return;
            }

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
                        SendReply(ProcessEnumerateMidiDevicesRequest(command), client);
                    }
                    break;
                case Command.SelectMidiDevices when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<SelectMidiDevicesRequest>(bytes);
                        SendReply(ProcessSelectMidiDevicesRequest(command), client);
                    }
                    break;
                case Command.EnumerateScores when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<EnumerateScoresRequest>(bytes);
                        SendReply(ProcessEnumerateScoresRequest(command), client);
                    }
                    break;
                case Command.LoadScore when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<LoadScoreRequest>(bytes);
                        SendReply(ProcessLoadScoreRequest(command), client);
                    }
                    break;
                case Command.SetPlaybackPosition when requestResponse == RequestResponse.Request:
                    {
                        var command = MessagePackSerializer.Deserialize<SetPlaybackPositionRequest>(bytes);
                        SendReply(ProcessSetPlaybackPositionRequest(command), client);
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

            if (command.InputDeviceNames != null)
            {
                UnregisterMidiInputs();
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
                RegisterMidiInputs();
            }

            if (command.OutputDeviceNames != null)
            {
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

        private void UnregisterMidiInputs()
        {
            foreach (var midiInput in Engine.MidiInputs)
            {
                midiInput.MessageReceived -= OnMidiInputMessageReceived;
            }
        }

        private void RegisterMidiInputs()
        {
            foreach (var midiInput in Engine.MidiInputs)
            {
                midiInput.MessageReceived += OnMidiInputMessageReceived;
            }
        }

        private void OnMidiInputMessageReceived(object sender, MidiReceivedEventArgs e)
        {
            switch (e.Data[0])
            {
                case MidiEvent.NoteOff:
                    {
                        var pitch = e.Data[1];
                        Engine.Interpreter.Input(new NoteRelease()
                        {
                            Pitch = pitch
                        });
                    }
                    break;
                case MidiEvent.NoteOn:
                    {
                        var pitch = e.Data[1];
                        var velocity = e.Data[2];

                        /** The Yamaha P-45 sends Note Off messages as Note On 
                         * messages with zero velocity. */
                        var isNoteOnActuallyNoteOff = velocity == 0;

                        if (isNoteOnActuallyNoteOff)
                        {
                            Engine.Interpreter.Input(new NoteRelease()
                            {
                                Pitch = pitch
                            });
                        }
                        else
                        {
                            Engine.Interpreter.Input(new NotePress()
                            {
                                Pitch = pitch,
                                Velocity = velocity
                            });
                        }
                    }
                    break;
                case MidiEvent.CC:
                    {
                        var pedalKind = e.Data[1];
                        var position = e.Data[2];
                        Engine.Interpreter.Input(new PedalChange()
                        {
                            Pedal = (PedalKind)pedalKind,
                            Position = position
                        });
                    }
                    break;
            }
        }

        private EnumerateScoresResponse ProcessEnumerateScoresRequest(EnumerateScoresRequest command)
        {
            var filePaths = new List<string>();
            var error = "";

            var scorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "scores");

            if (Config.ScoresPath != "default")
            {
                if (Directory.Exists(Config.ScoresPath))
                {
                    scorePath = Config.ScoresPath;
                }
                else
                {
                    error = $"Custom score path {Config.ScoresPath} does not exist.";
                }
            }

            try
            {
                filePaths.AddRange(Directory.GetFiles(scorePath, "*.musicxml", new EnumerationOptions()
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    MatchType = MatchType.Simple,
                    RecurseSubdirectories = true,
                    ReturnSpecialDirectories = false
                }));
                filePaths.AddRange(Directory.GetFiles(scorePath, "*.xml", new EnumerationOptions()
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    MatchType = MatchType.Simple,
                    RecurseSubdirectories = true,
                    ReturnSpecialDirectories = false
                }));
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return new EnumerateScoresResponse()
            {
                FilePaths = filePaths.ToArray(),
                ActiveScoreFilePath = Engine.Interpreter.ScoreFilePath,
                Error = error
            };
        }

        private LoadScoreResponse ProcessLoadScoreRequest(LoadScoreRequest command)
        {
            var error = "";
            var scoreBytes = new byte[] { };
            var scorePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "scores");

            if (Config.ScoresPath != "default")
            {
                if (Directory.Exists(Config.ScoresPath))
                {
                    scorePath = Config.ScoresPath;
                }
                else
                {
                    error = $"Custom score path {Config.ScoresPath} does not exist.";
                }
            }

            try
            {
                var isPathJailedToScoresDir = Path.GetFullPath(command.FilePath).StartsWith(scorePath, StringComparison.OrdinalIgnoreCase);
                if (isPathJailedToScoresDir && (Path.GetExtension(command.FilePath) == ".musicxml" || Path.GetExtension(command.FilePath) == ".xml") && File.Exists(command.FilePath))
                {
                    var fileStream = new FileStream(command.FilePath, FileMode.Open, FileAccess.Read);
                    var scoreBuilder = new ScoreBuilder.ScoreBuilder(fileStream);
                    var score = scoreBuilder.Build();

                    if (score.Parts.Length == 0)
                    {
                        error = $"Score {command.FilePath} does not have any MusicXML parts.";
                    }

                    Engine.Interpreter.SetScore(score, command.FilePath);
                    Engine.Interpreter.ResetPlayback();

                    using var memoryStream = new MemoryStream();
                    fileStream.Seek(0, SeekOrigin.Begin);
                    fileStream.CopyTo(memoryStream);
                    scoreBytes = memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message + "\n" + ex.StackTrace;
            }

            return new LoadScoreResponse()
            {
                Score = scoreBytes,
                ActiveScoreFilePath = Engine.Interpreter.ScoreFilePath,
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
