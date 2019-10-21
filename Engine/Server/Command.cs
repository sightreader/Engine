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
    public class EnumerateMidiDevicesRequest : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.EnumerateMidiDevices;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
    }

    [MessagePackObject]
    public class EnumerateMidiDevicesResponse : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.EnumerateMidiDevices;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key(2)]
        public string[] InputDeviceNames { get; set; } = new string[] { };
        [Key(3)]
        public string[] OutputDeviceNames { get; set; } = new string[] { };
        [Key(4)]
        public string Error { get; set; } = "";
    }

    [MessagePackObject]
    public class SelectMidiDevicesRequest : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.SelectMidiDevices;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
        [Key(2)]
        public string[] InputDeviceNames { get; set; } = new string[] { };
        [Key(3)]
        public string[] OutputDeviceNames { get; set; } = new string[] { };
    }

    [MessagePackObject]
    public class SelectMidiDevicesResponse : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.SelectMidiDevices;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key(2)]
        public string Error { get; set; } = "";
    }

    [MessagePackObject]
    public class EnumerateScoresRequest: ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.EnumerateScores;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
    }

    [MessagePackObject]
    public class EnumerateScoresResponse : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.EnumerateScores;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key(2)]
        public string[] FilePaths { get; set; } = new string[] { };
        [Key(3)]
        public string Error { get; set; } = "";
    }

    [MessagePackObject]
    public class LoadScoreRequest : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.LoadScore;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
        [Key(2)]
        public string FilePath { get; set; } = "";
    }

    [MessagePackObject]
    public class LoadScoreResponse : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.LoadScore;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key(2)]
        public byte[] Score { get; set; } = new byte[] { };
        [Key(3)]
        public string Error { get; set; } = "";
    }

    [MessagePackObject]
    public class SetScoreDisplayPositionRequest : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.SetScoreDisplayPosition;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
        [Key(2)]
        public int MeasureNumber { get; set; }
        [Key(3)]
        public int GroupIndex { get; set; }
    }

    [MessagePackObject]
    public class SetScoreDisplayPositionResponse : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.SetScoreDisplayPosition;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key(2)]
        public string Error { get; set; } = "";
    }

    [MessagePackObject]
    public class SetPlaybackPositionRequest : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.SetPlaybackPosition;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Request;
        [Key(2)]
        public int MeasureNumber { get; set; }
    }

    [MessagePackObject]
    public class SetPlaybackPositionResponse : ICommand
    {
        [Key(0)]
        public Command Command { get; set; } = Command.SetPlaybackPosition;
        [Key(1)]
        public RequestResponse Kind { get; set; } = RequestResponse.Response;
        [Key(2)]
        public string Error { get; set; } = "";
    }
}
