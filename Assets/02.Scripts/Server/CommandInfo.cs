using System.Data;
using System;

namespace JaehyeokSong0.Tacidto.Server
{
    public enum CommandType
    {
        Global,
        PreServer,
        Running
    }

    public class CommandInfo
    {
        public string Usage { get; }
        public string Description { get; }
        public CommandType Type { get; }
        public Action<string[]> Handler { get; }

        public CommandInfo(string usage, string description, CommandType type, Action<string[]> handler)
        {
            Usage = usage;
            Description = description;
            Type = type;
            Handler = handler;
        }
    }
}