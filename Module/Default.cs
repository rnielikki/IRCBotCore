using System;
using System.IO;
using System.Threading.Tasks;
using IRCBot.Model;

namespace Module
{
    class Ping : IModule
    {
        public string command { get; private set; }
        public string description { get; private set; }
        public double version { get; private set; }
        //constructor.
        //the name should be *SAME AS CLASS NAME*
        public Ping()
        {
            /* Set your command and description */
            command = "ping";
            description = "Sends ping message";
            version = 1.0;
        }
        /* Make the action here */
        public string CmdResult(ChatMsg c)
        {
            return c.caller + ", pong!";
        }
    }
    class Time : IModule
    {
        public string command { get; private set; }
        public string description { get; private set; }
        public double version { get; private set; }
        //constructor.
        //the name should be *SAME AS CLASS NAME*
        public Time()
        {
            /* Set your command and description */
            command = "time";
            description = "Tells the time.";
            version = 1.0;
        }
        /* Make the action here */
        public string CmdResult(ChatMsg c)
        {
            return "time is now " + DateTime.Now.ToString();
        }
    }
    class Echo : IModule
    {
        public string command { get; private set; }
        public string description { get; private set; }
        public double version { get; private set; }
        //constructor.
        //the name should be *SAME AS CLASS NAME*
        public Echo()
        {
            /* Set your command and description */
            command = "echo";
            description = "I say what You say.";
            version = 1.0;
        }
        /* Make the action here */
        public string CmdResult(ChatMsg c)
        {
            if (c.parameter.Length != 0)
            {
                return c.parameter;
            }
            else
            {
                return "I have nothing to say.";
            }
        }
    }
    class Timer : IModule
    {
        public string command { get; private set; }
        public string description { get; private set; }
        public double version { get; private set; }
        //constructor.
        //the name should be *SAME AS CLASS NAME*
        public Timer()
        {
            /* Set your command and description */
            command = "timer";
            description = "Wait after some seconds. This checks if the bot works asynchronously.";
            version = 1.0;
        }
        /* Make the action here */
        public string CmdResult(ChatMsg c)
        {
            int secs;
            Int32.TryParse(c.parameter, out secs);
            if (secs < 1)
            {
                return "Invalid seconds. Usage: .timer (Seconds to wait)";
            }
            Task.Delay(secs * 1000).Wait();
            return $"{secs} seconds wasted, {c.caller}!";
        }
    }
}