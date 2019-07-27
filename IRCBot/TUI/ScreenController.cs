using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;

namespace IRCBot.Settings
{
    public class ScreenController
    {
        private readonly Screen screen = Screen.Get();
        private Setting setting = null;
        // -- input values -- //
        private InputText Server;
        private InputNumber Port;
        private InputText Nickname;
        private InputText AltNickname;
        private InputChar CommandPrefix;
        private InputText User;
        private ListInput AutoJoin;
        private static ScreenController screenController;
        private IEnumerable<InputText> InputFields;
        private int InputLength { get; set; } = 30; //not const because can be changed with height or server length in future
        private ScreenController()
        {
            Task<Setting> settingTask=MainController.ReadFile();
            Server = new InputText("Server", 3, 20, 5, InputLength);
            Port = new InputNumber("Port", 3, 20, 7, InputLength);
            Nickname = new InputText("Nickname", 3, 20, 9, InputLength);
            AltNickname = new InputText("Alternative Nick", 3, 20, 11, InputLength);
            CommandPrefix = new InputChar("Command Prefix", 3, 20, 13);
            //new InputChar("PrivMsg Prefix", 30, 49, 13);
            User = new InputText("User", 3, 20, 15, InputLength);
            AutoJoin = new ListInput("Autojoin channel", 3, 20, 17, InputLength);
            new InputButton("Connect", 12, 23, 11, Send);
            new InputButton("Cancel", 30, 23, 10, Exit);

            //Note: Call Load After Every Field was made
            settingTask.ContinueWith((configTask) => {
                if (configTask.IsCanceled || configTask.IsFaulted) {
                    Console.Clear();
                    Console.WriteLine("Failed to load setting files.");
                    Environment.Exit(-1);
                }
                setting = configTask.Result;
                LoadConfig(setting);
            });
            //"validation checkable" input fields.
            InputFields = new InputText[] { Port, Nickname, AltNickname, User, CommandPrefix, AutoJoin.TextField };
            screen.Start();
        }
        public static ScreenController Get()
        {
            if (screenController==null) screenController=new ScreenController();
            return screenController;
        }
        private void LoadConfig(Setting Config)
        {
            //read files.
            Server.SetValue(Config.Server);
            Port.SetValue(Config.Port);
            Nickname.SetValue(Config.Nickname);
            AltNickname.SetValue(Config.AltNickname);
            CommandPrefix.SetValue(Config.CommandPrefix);
            User.SetValue(Config.User);
            AutoJoin.SetValue(Config.AutoJoin);

        }
        private void Send()
        {
            if (!BindData()) return;
            Console.Clear();
            string ConfigJson=JsonSerializer.ToString(setting);
            try
            {
                File.WriteAllTextAsync(MainController.ConfigPath, ConfigJson, MainController.IRCEncoding);
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to save the file.");
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
            // add some setting saves here.
            // and...
            screen.IfExit = true;
            MainController.Load(setting);
        }
        private bool BindData()
        {
            setting.Server = Server.Value.ToString();
            setting.Port = Port.GetNumber();
            setting.Nickname = Nickname.Value.ToString();
            setting.AltNickname = AltNickname.Value.ToString();
            setting.CommandPrefix = CommandPrefix.Value;
            setting.User = User.Value.ToString();
            setting.AutoJoin = AutoJoin.Result();
            IEnumerable<InputText> result = setting.CheckValid().Select(target => InputFromEnum(target));
            if (result.Count() != 0)
            {
                Console.BackgroundColor = Screen.BgColor;
                Console.SetCursorPosition(0, 2);
                Console.WriteLine("       Some of the values are not valid for IRC. Check the red labels, fix and try again.");
                Console.BackgroundColor = Screen.BgDefault;
                ConsoleColor normalColor = Console.ForegroundColor;
                ConsoleColor errorColor = ConsoleColor.Red;
                //Make normal if valid, or red if not valid! 
                foreach (InputText input in InputFields)
                {
                    if (result.Contains(input))
                    {
                        Console.ForegroundColor = errorColor;
                    }
                    else
                    {
                        Console.ForegroundColor = normalColor;
                    }
                    input.WriteLabel();
                }
                Console.ForegroundColor = normalColor;
                screen.SetCurrent(Screen.Inputs.IndexOf(result.First()));
                return false;
            }
            return true;
        }
        private InputText InputFromEnum(Setting.ValueCheck ValueEnum)
        {
            switch (ValueEnum)
            {
                case Setting.ValueCheck.Port:
                    return Port;
                case Setting.ValueCheck.Nickname:
                    return Nickname;
                case Setting.ValueCheck.AltNickname:
                    return AltNickname;
                case Setting.ValueCheck.CommandPrefix:
                    return CommandPrefix;
                case Setting.ValueCheck.User:
                    return User;
                case Setting.ValueCheck.AutoJoin:
                    return AutoJoin.TextField;
                default:
                    return null;
            }
        }
        private void Exit()
        {
            screen.IfExit = true;
            Console.Clear();
            Environment.Exit(0);
        }
    }
}

