using System;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IRCBot.Model;
using IRCBot.Settings;

namespace IRCBot
{
    internal class IRCBot
    {
        const int LineMax = 3;
        static char[] Space = new char[1] { ' ' };
        private Setting setting;
        public string Nickname; //can be changed
        //Events
        public static event MessageEvents messageEvents;
        internal IRCBot(Setting setting)
        {
            this.setting = setting;
            Nickname = setting.Nickname;
        }
        private void Write(StreamWriter w, string text)
        {
            //not safe, but "IRC writing is limited" over it. we can lock the streamwriter, but can't gurarantee what happens.
            w.WriteLine(text);
            w.Flush();
            ColorMessage(text, ConsoleColor.Yellow);
        }
        private void WriteMsg(StreamWriter w, string channel, string text)
        {
            if (text.Contains('\n')) //seperate lines with \n
            {
                string[] splitText = text.Split('\n');
                int lines = 0;
                foreach (string str in splitText)
                {
                    Write(w, "PRIVMSG " + channel + " :" + str);
                    if (++lines >= LineMax) break;
                }
            }
            else
            {
                Write(w, "PRIVMSG " + channel + " :" + text);
            }
        }
        private void RetriveBotCall(StreamWriter w, ChatMsg chat)
        {
            string[] splitText = chat.text.Split(Space, 2);
            if (chat.channel == Nickname) { chat.channel = chat.caller; }
            //commandlist, help, and version.
            switch (splitText[0])
            {
                case "help":
                    if (splitText.Length == 1)
                    {
                        WriteMsg(w, chat.channel, "Available commands: " + String.Join(", ", MainController.Modules.ModuleList.Keys));
                    }
                    else
                    {
                        var description = MainController.Modules.ModuleList.SingleOrDefault(a => a.Key == splitText[1]).Value?.description;
                        if (description != null)
                        {
                            WriteMsg(w, chat.channel, description);
                        }
                        else
                        {
                            WriteMsg(w, chat.channel, "Oops, 404 command not found!");
                        }
                    }
                    break;
                case "version":
                    if (splitText.Length == 1)
                    {
                        WriteMsg(w, chat.channel, "See the version of the file.");
                    }
                    else
                    {
                        var version = MainController.Modules.ModuleList.SingleOrDefault(a => a.Key == splitText[1]).Value?.version;
                        if (version != null)
                        {
                            WriteMsg(w, chat.channel, $"Command {splitText[1]}, Version {version.ToString()}");
                        }
                        else
                        {
                            WriteMsg(w, chat.channel, "Oops, 404 command not found!");
                        }

                    }
                    break;
                default:
                    var Cmd = MainController.Modules?.ModuleList?.SingleOrDefault(a => a.Key == splitText[0]).Value;
                    if (Cmd != null)
                    {
                        Task<string> t = Task.Run(() => Cmd.CmdResult(chat));
                        t.ContinueWith((CmdResult) => {
                            if (t.IsFaulted)
                            {
                                WriteMsg(w, chat.channel, "Module Error: " + t.Exception.Message);
                            }
                            else if (t.IsCanceled)
                            {
                                WriteMsg(w, chat.channel, "Module Error: The operation is canceled");
                            }
                            else
                            {
                                WriteMsg(w, chat.channel, CmdResult.Result);
                            }

                        });
                    }
                    break;
            }
        }
        public void Start()
        {
            TcpClient Connection = null;
            try
            {
                Connection = new TcpClient(setting.Server, setting.Port);
                using (Stream stream = Connection.GetStream())
                using (StreamReader reader = new StreamReader(stream))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.AutoFlush = true;
                    Write(writer, "NICK " + Nickname);
                    Write(writer, "USER " + setting.Host);
                    while (true)
                    {
                        string Line = reader.ReadLine();
                        if (Line != null)
                        {
                            Console.WriteLine(Line);
                            messageEvents?.Invoke(Line, writer);
                            string[] Value = Line.Split(Space, 4);
                            string[] NickSplit = Value[0].Substring(1).Split('!');
                            string CallerNick = "";
                            string CallerHost = "";
                            if (NickSplit.Length > 1)
                            {
                                CallerNick = NickSplit[0];
                                CallerHost = NickSplit[1];
                            }
                            if (Value[0] == "PING")
                            {
                                Write(writer, "PONG " + Value[1]);
                                continue;
                            }
                            else
                            {
                                switch (Value[1])
                                {
                                    //see RFC2812 IRC standard: https://tools.ietf.org/html/rfc2812
                                    case "001": //welcome
                                        foreach (string channel in setting.AutoJoin) {
                                            Write(writer, "JOIN " + channel);
                                        }
                                        break;
                                    case "433": //nickname already in use
                                        if (Nickname == setting.Nickname)
                                        {
                                            Nickname = setting.AltNickname;
                                        }
                                        else
                                        {
                                            Nickname = setting.Nickname + "_";
                                        }
                                        Write(writer, "NICK " + Nickname);
                                        break;
                                    case "PRIVMSG":
                                        if (Value[3][1] == setting.CommandPrefix)
                                        {
                                            ChatMsg msg = new ChatMsg(host: CallerHost, caller: CallerNick, channel: Value[2], text: Value[3]);
                                            RetriveBotCall(writer, msg);
                                        }
                                        break;
                                    case "INVITE":
                                        string Chan = Value[3].Substring(1);
                                        Write(writer, "JOIN " + Chan);
                                        WriteMsg(writer, Chan, $"Thank you for inviting! Write {setting.CommandPrefix}help to check usages.");
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException) {
                ErrorMessage($"Port {setting.Port} is invalid.");
            }
            catch (SocketException) {
                ErrorMessage($"Failed to connect Server {setting.Server}:{setting.Port}.\nThis can be occured when either server has problem or you wrote wrong server.\nIf server or port has some typo, please restart and change Server/Port.");
                Console.WriteLine("Retrying...");
                Task.Delay(5000).Wait();
                Start();

            }
            finally
            {
                Connection?.Close();
            }
        }
        private void ColorMessage(string message, ConsoleColor color) {
                ConsoleColor Default=Console.ForegroundColor;
                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor=Default;
        }
        private void ErrorMessage(string message) {
            ColorMessage(message, ConsoleColor.Red);
        }
    }
}
