using System.IO;
using IRCBot.Model;

namespace Module
{
    /* Example */
    class JoinInfo : IEventModule
    {
        public void EventResult(string RawMessage, StreamWriter w)
        {
            if (RawMessage.Split(' ')[1] == "JOIN")
            {
                w.WriteLine("WHOIS " + RawMessage.Substring(1, RawMessage.IndexOf('!') - 1));
            }
        }
    }
}