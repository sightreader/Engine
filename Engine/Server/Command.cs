using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace SightReader.Engine.Server
{
    public enum Command
    {
        EnumerateMidiDevices,
        SelectMidiDevices,
        EnumerateScores,
        LoadScore,
        SetScoreDisplayPosition,
        SetPlaybackPosition
    }

    public enum RequestResponse
    {
        Request,
        Response
    }

    public interface ICommand
    {
    }

    [MessagePackObject]
    public class GenericCommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.SetPlaybackPosition;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
    }

    [MessagePackObject]
    public class EnumerateMidiDevicesRequest : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.EnumerateMidiDevices;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
    }

    [MessagePackObject]
    public class EnumerateMidiDevicesResponse : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.EnumerateMidiDevices;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key("Error")]
        public string Error { get; set; } = "";
        [Key("InputDeviceNames")]
        public string[] InputDeviceNames { get; set; } = new string[] { };
        [Key("OutputDeviceNames")]
        public string[] OutputDeviceNames { get; set; } = new string[] { };
        [Key("EnabledInputDeviceNames")]
        public bool[] EnabledInputDeviceNames { get; set; } = new bool[] { };
        [Key("EnabledOutputDeviceNames")]
        public bool[] EnabledOutputDeviceNames { get; set; } = new bool[] { };
    }

    [MessagePackObject]
    public class SelectMidiDevicesRequest : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.SelectMidiDevices;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
        [Key("InputDeviceNames")]
        public string[] InputDeviceNames { get; set; } = new string[] { };
        [Key("OutputDeviceNames")]
        public string[] OutputDeviceNames { get; set; } = new string[] { };
    }

    [MessagePackObject]
    public class SelectMidiDevicesResponse : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.SelectMidiDevices;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key("Error")]
        public string Error { get; set; } = "";
        [Key("InputDeviceNames")]
        public string[] InputDeviceNames { get; set; } = new string[] { };
        [Key("OutputDeviceNames")]
        public string[] OutputDeviceNames { get; set; } = new string[] { };
        [Key("EnabledInputDeviceNames")]
        public bool[] EnabledInputDeviceNames { get; set; } = new bool[] { };
        [Key("EnabledOutputDeviceNames")]
        public bool[] EnabledOutputDeviceNames { get; set; } = new bool[] { };
    }

    [MessagePackObject]
    public class EnumerateScoresRequest: ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.EnumerateScores;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
    }

    [MessagePackObject]
    public class EnumerateScoresResponse : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.EnumerateScores;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key("Error")]
        public string Error { get; set; } = "";
        [Key("FilePaths")]
        public string[] FilePaths { get; set; } = new string[] { };
    }

    [MessagePackObject]
    public class LoadScoreRequest : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.LoadScore;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
        [Key("FilePaths")]
        public string FilePath { get; set; } = "";
    }

    [MessagePackObject]
    public class LoadScoreResponse : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.LoadScore;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key("Score")]
        public byte[] Score { get; set; } = new byte[] { };
        [Key("Error")]
        public string Error { get; set; } = "";
    }

    [MessagePackObject]
    public class SetScoreDisplayPositionRequest : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.SetScoreDisplayPosition;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
        [Key("MeasureNumber")]
        public int MeasureNumber { get; set; }
        [Key("GroupIndex")]
        public int GroupIndex { get; set; }
    }

    [MessagePackObject]
    public class SetScoreDisplayPositionResponse : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.SetScoreDisplayPosition;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key("Error")]
        public string Error { get; set; } = "";
    }

    [MessagePackObject]
    public class SetPlaybackPositionRequest : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.SetPlaybackPosition;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
        [Key("MeasureNumber")]
        public int MeasureNumber { get; set; }
    }

    [MessagePackObject]
    public class SetPlaybackPositionResponse : ICommand
    {
        [Key("Command")]
        public Command Command { get; set; } = Command.SetPlaybackPosition;
        [Key("Kind")]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key("Error")]
        public string Error { get; set; } = "";
    }
}
