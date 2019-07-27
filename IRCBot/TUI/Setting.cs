using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IRCBot.Settings
{
    public sealed class Setting
    {
        //for validation "not valid" return
        public enum ValueCheck { Port, Nickname, AltNickname, User, CommandPrefix, AutoJoin }

        public string Server { get; set; } = "";
        public int Port { get; set; } = 0;
        public string Nickname { get; set; } = "";
        public string AltNickname { get; set; }
        // NOTE: Please keep the syntax, Name (Number) * :(Text)
        // If you don't know, please leave.
        public string Host { get => $"{Nickname} 1 * : {User}"; }
        public string User { get; set; }
        public char CommandPrefix { get; set; } = '\0';
        private IEnumerable<string> _AutoJoin { get; set; }
        public IEnumerable<string> AutoJoin
        {
            get => _AutoJoin;
            set => _AutoJoin = value.Distinct();
        }
        public IEnumerable<ValueCheck> CheckValid()
        {
            Regex NicknameRule = new Regex(@"^[\p{L}\p{Nd}]{1,15}$"); //not tested.
            Regex UserRule = new Regex(@"^[\x01-\x09\x0b-\x0c\x0e-\x1f\x21-\x3f\x41-\xff]{1,9}$");
            Regex ChannelRule = new Regex(@"^[&#\+!][\x01-\x07\x08-\x09\x0b-\x0c\x0e-\x1f\x21-\x2b\x2d-\x39\x3b-\xff]{1,49}$");
            List<ValueCheck> invalid = new List<ValueCheck>();
            if (Port <= 0) invalid.Add(ValueCheck.Port);
            if (!(char.IsPunctuation(CommandPrefix) || char.IsSymbol(CommandPrefix))) invalid.Add(ValueCheck.CommandPrefix);
            if (!NicknameRule.IsMatch(Nickname)) invalid.Add(ValueCheck.Nickname);
            if (!NicknameRule.IsMatch(AltNickname)) invalid.Add(ValueCheck.AltNickname);
            if (!NicknameRule.IsMatch(User)) invalid.Add(ValueCheck.User);
            if (AutoJoin.Where(channel => !ChannelRule.IsMatch(channel)).Count() != 0) invalid.Add(ValueCheck.AutoJoin);
            return invalid;
        }
    }
}
