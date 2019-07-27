using IRCBot.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IRCBot
{
    static internal class MainController
    {
        internal const string ConfigPath = "config.json";
        internal static ModuleLoader Modules { get; private set; } = null;
        public static Encoding IRCEncoding { get; private set; }
        internal static bool SetEncoding(string encodingName)
        {
            try
            {
                IRCEncoding = Encoding.GetEncoding(encodingName);
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"Encoding {encodingName} is not available.");
                Console.WriteLine("Default encoding is UTF-8.");
                Console.WriteLine("Available encodings on your computer:");
                EncodingInfo[] test = Encoding.GetEncodings();
                Console.WriteLine(string.Join(", ", test.Select(enc => enc.Name)));
                return false;
            }
            Console.InputEncoding = IRCEncoding;
            Console.OutputEncoding = IRCEncoding;
            return true;
        }
        //SkpValidation is "Already Checked" state, for example, ScreenController already checkes if validate.
        internal static void Load(Setting setting, bool SkipValidation = true)
        {
            if (!SkipValidation) {
                IEnumerable<Setting.ValueCheck> CheckResult=setting.CheckValid();
                if (CheckResult.Count()!=0) {
                    Console.WriteLine($"!! Invalid value(s) found in your config file : {String.Join(", ", CheckResult)}. Please check your {ConfigPath} file.");
                    Console.WriteLine("Failed to start.");
                }
            }
            Modules = ModuleLoader.Call("Modules");//Module Directory
            Modules.LoadAll();

            IRCBot Bot = new IRCBot(setting);
            Bot.Start();
        }
        internal async static Task<Setting> Start() {
            if (!File.Exists(ConfigPath)){
                Console.WriteLine("You have no setting file. Please start WITHOUT start parameter.");
                return null;
            }
            return await ReadFile();
        }
        internal static async Task<Setting> ReadFile()
        {
            string file;
            Setting s;
            if (!File.Exists(ConfigPath)) return new Setting();
            try
            {
                file = await File.ReadAllTextAsync(ConfigPath, MainController.IRCEncoding);
                s = JsonSerializer.Parse<Setting>(file);
                return s;
            }
            catch (JsonException) {
                Console.WriteLine("Bad setting file format from "+ConfigPath+". Remove to make a new setting or edit carefully.");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}