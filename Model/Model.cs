using System.IO;

namespace IRCBot.Model
{
    //If you want to mention the author(s), leave them to description;
    public delegate void MessageEvents(string RawMessage, StreamWriter w);
    public interface IModule
    {
        string command { get; }
        string description { get; }
        double version { get; }
        string CmdResult(ChatMsg c);
    }
    public interface IEventModule
    {
        void EventResult(string RawMessage, StreamWriter w);
    }
    public struct ChatMsg
    {
        public string host;
        public string caller;
        public string channel;
        public string text;
        public string parameter;
        public ChatMsg(string host, string caller, string channel, string text)
        {
            this.host = host;
            this.caller = caller;
            this.channel = channel;
            this.text = text.Substring(2);
            int index = this.text.IndexOf(' ') + 1;
            parameter = (index == 0) ? "" : this.text.Substring(index);
        }
    }

}