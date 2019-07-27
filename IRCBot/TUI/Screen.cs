using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace IRCBot.Settings
{
    /*  NOTE: THIS PROGRAM IS *NOT* FOR "LIBRARY".                    */
    /*  NOT RECOMMENDED FOR OTHER PURPOSE WITHOUT EDITING THE CODE    */
    class Screen
    {
        public static readonly List<Input> Inputs = new List<Input>();
        private int CurrentIndex { get; set; } = 0;
        public Input Current { get; private set; }
        private static Screen screen;
        public bool IfExit { get; set; } = false;
        public static readonly ConsoleColor BgDefault = Console.BackgroundColor;
        public const ConsoleColor BgColor = ConsoleColor.DarkBlue;
        public const ConsoleColor ButtonColor = ConsoleColor.DarkMagenta;
        public const ConsoleColor TitleColor = ConsoleColor.DarkRed;

        private Screen()
        {
            Console.BackgroundColor = BgColor;
            Console.Clear();
            Console.WriteLine("\n\n       If text is broken, check if you have right encoding! (command: IRCBot [encoding])");
            Title(":: Welcome to the IRCBotCore! ::");
            Console.BackgroundColor = BgDefault;
        }
        public void Start()
        {
            SetCurrent(CurrentIndex);
            GetKey();
        }
        internal static Screen Get()
        {
            if (screen == null)
            {
                screen = new Screen();
            }
            return screen;
        }
        private void Title(string text)
        {
            Console.BackgroundColor = TitleColor;
            Console.SetCursorPosition(0, 0);
            Console.Write(new String(' ', (Console.WindowWidth - text.Length) / 2));
            Console.Write(text);
            Console.Write(new String(' ', Console.WindowWidth - Console.CursorLeft));
        }
        internal void SetCurrent(int index)
        {
            if (index < 0) { index = Inputs.Count - 1; }
            else if (index > Inputs.Count - 1) { index = 0; }
            Current = Inputs[index];
            Current.SetPosition();
            CurrentIndex = index;
        }
        private void GetKey()
        {
            while (!IfExit)
            {
                ConsoleKeyInfo inputKey = Console.ReadKey(true);
                Current.OnKeyGet(inputKey);
                switch (inputKey.Key)
                {
                    case ConsoleKey.UpArrow:
                        SetCurrent(--CurrentIndex);
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.Tab:
                        SetCurrent(++CurrentIndex);
                        break;
                }
            }
        }
        public static void SelectBefore() => screen.SetCurrent(screen.CurrentIndex - 1);
        public static void SelectAfter() => screen.SetCurrent(screen.CurrentIndex + 1);
    }
    internal abstract class Input
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Length { get; private set; }
        public Input(int x, int y, int length, ConsoleColor color)
        {
            X = x;
            Y = y;
            Length = length;
            Draw(x, y, length, color);
            Screen.Inputs.Add(this);
        }
        protected void Draw(int x, int y, int length, ConsoleColor color)
        {
            Console.SetCursorPosition(x, y);
            WriteColor(new string(' ', length), color);
        }
        public virtual void SetPosition() => Console.SetCursorPosition(X, Y);
        public virtual void OnKeyGet(ConsoleKeyInfo info)
        {
        }
        public void WriteColor(string text, ConsoleColor color)
        {
            Console.BackgroundColor = color;
            Console.Write(text);
            Console.BackgroundColor = Screen.BgDefault;
        }
    }
    internal class InputButton : Input
    {
        public string Label { get; private set; }
        private Action ButtonAction { get; set; }
        public InputButton(string label, int x, int y, int length, Action action) : base(x, y, length, Screen.ButtonColor)
        {
            Label = label;
            int half = ((length - label.Length) / 2) - 1;
            string result;
            if (half < 0) result = $"[{label}]";
            else
            {
                string whitespace = new string('_', half);
                result = '[' + whitespace + label + whitespace + ']';
            }
            Console.SetCursorPosition(x, y);
            WriteColor(result, Screen.ButtonColor);
            ButtonAction = action;
        }
        public override void OnKeyGet(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    ButtonAction();
                    break;
            }
        }
    }
    internal class InputText : Input
    {
        public string Label { get; private set; }
        private int LabelX { get; set; }
        public readonly StringBuilder Value = new StringBuilder();
        public InputText(string label, int labelx, int x, int y, int length) : base(x, y, length, Console.BackgroundColor)
        {
            Label = label;
            LabelX = labelx;
            WriteLabel();
        }
        internal void WriteLabel()
        {
            Console.SetCursorPosition(LabelX, Y);
            WriteColor(Label, Screen.BgColor);
        }
        public override void SetPosition() => Console.SetCursorPosition(X + Value.Length, Y);
        protected virtual bool IsValid(char data) => !char.IsControl(data) && !char.IsSeparator(data) && Value.Length < Length;
        public override void OnKeyGet(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.LeftArrow:
                    if (Console.CursorLeft > X) Console.CursorLeft--;
                    break;
                case ConsoleKey.RightArrow:
                    if (Console.CursorLeft < X + Value.Length) Console.CursorLeft++;
                    break;
                case ConsoleKey.Backspace:
                    if (Console.CursorLeft > X)
                    {
                        int leftBuffer = Console.CursorLeft;
                        int stringPos = leftBuffer - X;
                        Console.CursorLeft--;
                        Console.Write(" ");
                        Console.MoveBufferArea(leftBuffer, Y, Value.Length - stringPos, 1, leftBuffer - 1, Y);
                        Value.Remove(stringPos - 1, 1);
                        Console.CursorLeft = leftBuffer - 1;
                    }
                    break;
                case ConsoleKey.Delete:
                    if (Console.CursorLeft < X + Value.Length)
                    {
                        int leftBuffer = Console.CursorLeft;
                        int stringPos = leftBuffer - X;
                        Console.Write(" ");
                        Console.MoveBufferArea(leftBuffer + 1, Y, Value.Length - stringPos - 1, 1, leftBuffer, Y);
                        Console.CursorLeft = leftBuffer;
                        Value.Remove(stringPos, 1);
                    }
                    break;
                default:
                    char data = keyInfo.KeyChar;
                    if (IsValid(data))
                    {
                        int leftBuffer = Console.CursorLeft;
                        int stringPos = leftBuffer - X;
                        Console.MoveBufferArea(leftBuffer, Y, Value.Length - stringPos, 1, leftBuffer + 1, Y);
                        Console.Write(data);
                        Value.Insert(stringPos, data);
                    }
                    break;
            }
        }
        public virtual void SetValue(string value)
        {
            Value.Clear();
            if (value.Length > Length) {
                value = value.Substring(0, Length);
            }
            Value.Append(value);
            Console.SetCursorPosition(X, Y);
            Console.Write(value);
        }
        public virtual void Clear()
        {
            Value.Clear();
            Console.SetCursorPosition(X, Y);
            Console.Write(new String(' ', Length));
            Console.SetCursorPosition(X, Y);
        }
    }
    internal class InputNumber : InputText
    {
        public InputNumber(string label, int labelx, int x, int y, int length) : base(label, labelx, x, y, length) { }
        protected override bool IsValid(char data) => char.IsNumber(data) && Value.Length < Length;
        public int GetNumber()
        {
            int num;
            int.TryParse(Value.ToString(), out num);
            return num;
        }
        public override void SetValue(string number) {
            int num;
            int.TryParse(number, out num);
            SetValue(num);
        }
        public void SetValue(int num)
        {
            if(num>0)
                base.SetValue(num.ToString());
        }
    }
    internal class InputChar : InputText
    {
        public new char Value { get; private set; }
        public InputChar(string label, int labelx, int x, int y) : base(label, labelx, x, y, 1) { }
        public override void OnKeyGet(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Delete:
                case ConsoleKey.Backspace:
                    Console.SetCursorPosition(X, Y);
                    Console.Write(" ");
                    Console.CursorLeft--;
                    Value = '\0';
                    break;
                default:
                    char keychar = keyInfo.KeyChar;
                    if (IsValid(keychar))
                    {
                        Console.Write(keychar);
                        Value = keychar;
                    }
                    break;
            }
        }
        protected override bool IsValid(char data) => char.IsPunctuation(data) || char.IsSymbol(data);
        public override void SetValue(string value)
        {
            SetValue(value[0]);
        }
        public void SetValue(char value)
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(value);
            Value = value;
        }
        public override void Clear()
        {
            Value = '\0';
            Console.SetCursorPosition(X, Y);
            Console.Write(' ');
            Console.SetCursorPosition(X, Y);
        }
    }
    internal class ListInput
    {
        public InputText TextField { get; private set; }
        private ListValueSet listSet;
        public ListInput(string label, int labelx, int x, int y, int length)
        {
            TextField = new InputText(label, labelx, x, y, length);
            new InputButton("+", x + length, y, 3, Add);
            listSet = new ListValueSet(x, y + 2);
        }
        internal void Add()
        {
            string GetValue = TextField.Value?.ToString();
            if (!string.IsNullOrEmpty(GetValue) && listSet.Values.Where(val => val.Value == GetValue).Count() == 0)
            {
                Add(GetValue);
            }
        }
        internal void Add(string channel) {
            int line = listSet.itemPosition / Console.WindowWidth;
            listSet.Values.Add(new ListValue(this, channel, listSet.itemPosition % Console.WindowWidth, listSet.Y + line));
            listSet.itemPosition += channel.Length + 5;
            int lineState = listSet.itemPosition / Console.WindowWidth;
            if (lineState < 2) return;
            sbyte lineChange = (sbyte)(lineState - line);
            if (lineChange != 0)
            {
                listSet.OnLineChange(lineChange);
            }
        }
        public IEnumerable<string> Result() => listSet.Values.Select(lvalue => lvalue.Value);
        internal class ListValue
        {
            private ListInput parent;
            internal int X { get; set; }
            internal int Y { get; set; }
            public string Value { get; private set; }
            internal ListValue(ListInput parent, string value, int x, int y)
            {
                this.parent = parent;
                Value = value;
                X = x;
                Y = y;
                Console.SetCursorPosition(x, y);
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write("[X]");
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.Write(value);
                Console.BackgroundColor = Screen.BgDefault;
                parent.TextField.Clear();
                Screen.SelectBefore();
            }
            public void Select()
            {
                Console.SetCursorPosition(X, Y);
            }
        }
        internal class ListValueSet : Input
        {
            public readonly List<ListValue> Values = new List<ListValue>();
            private ListValue Current { get; set; }
            private int index { get; set; } = 0;
            internal int itemPosition { get; set; }
            internal ListValueSet(int x, int y) : base(x, y, 0, Screen.BgDefault)
            {
                itemPosition = x;
            }
            public override void OnKeyGet(ConsoleKeyInfo keyInfo)
            {
                if (Values.Count == 0) return;
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        SetCurrent(--index);
                        break;
                    case ConsoleKey.RightArrow:
                        SetCurrent(++index);
                        break;
                    case ConsoleKey.Enter:
                        Remove();
                        break;
                }
            }
            public override void SetPosition()
            {
                base.SetPosition();
                SetCurrent(0);
            }
            //safer than Values[insert some calculation here].Select();
            internal void SetCurrent(int index)
            {
                if (Values.Count == 0) return;
                if (index < 0) { index = Values.Count - 1; }
                else if (index > Values.Count - 1) { index = 0; }
                Current = Values[index];
                Current.Select();
                this.index = index;
            }
            //meh, constructor.
            private void Remove()
            {
                int listIndex = Values.IndexOf(Current);
                int cutLength = Current.Value.Length + 5;
                int leftOffset = Console.CursorLeft;
                int topOffset = Console.CursorTop;
                int screenWidth = Console.WindowWidth;

                Console.BackgroundColor = Screen.BgColor;

                int line = itemPosition / screenWidth;

                itemPosition -= cutLength;
                if (leftOffset + cutLength < screenWidth)
                {
                    Console.MoveBufferArea(leftOffset + cutLength, topOffset, screenWidth - cutLength - leftOffset, 1, leftOffset, topOffset);
                }
                else
                {
                    int part1 = screenWidth - leftOffset;
                    int part2 = leftOffset + cutLength - screenWidth;
                    Console.MoveBufferArea(part2, topOffset + 1, part1, 1, leftOffset, topOffset);
                    Console.MoveBufferArea(cutLength, topOffset + 1, screenWidth - cutLength, 1, 0, topOffset + 1);
                    //leftOffset = part2;
                    topOffset++;
                }
                for (int i = 1; i <= line + 1; i++)
                {
                    Console.MoveBufferArea(0, topOffset + i, cutLength, 1, screenWidth - cutLength, topOffset + i - 1);
                    Console.MoveBufferArea(cutLength, topOffset + i, screenWidth - cutLength, 1, 0, topOffset + i);
                }
                Values.Remove(Current);
                IEnumerable<ListValue> afters = Values.TakeLast(Values.Count - listIndex);
                foreach (ListValue after in afters)
                {
                    after.X -= cutLength;
                    if (after.X < 0)
                    {
                        after.X += screenWidth;
                        after.Y--;
                    }
                }
                sbyte change = (sbyte)(itemPosition / screenWidth - line);
                if (change != 0 && line >= 2)
                {
                    OnLineChange(change);
                }
                if (Values.Count != 0)
                {
                    SetCurrent(listIndex + 1);
                }
                else
                {
                    Current = null;
                }
                Console.BackgroundColor = Screen.BgDefault;
            }
            protected new void Draw(int x, int y, int length, ConsoleColor color) { }
            //if too long, moves the next line
            //one of the feature that not makes suitable for real library.
            public void OnLineChange(sbyte lineChange)
            {
                int line = itemPosition / Console.WindowWidth;
                int moveTarget = Y + line + 2;
                if (Console.WindowHeight - moveTarget <= 0)
                {
                    Console.WindowHeight += 2;
                }
                Console.MoveBufferArea(0, moveTarget, Console.WindowWidth, Console.WindowHeight - moveTarget, 0, moveTarget + lineChange, ' ', Console.ForegroundColor, Screen.BgColor);
                List<Input> inputs = Screen.Inputs;
                IEnumerable<Input> afters = inputs.TakeLast(inputs.Count() - inputs.IndexOf(this) - 1);
                foreach (Input input in afters)
                {
                    input.Y += lineChange;
                }
            }
        }
        public void SetValue(IEnumerable<string> channels) {
            foreach (string channel in channels) {
                Add(channel);
            }
        }
    }
}