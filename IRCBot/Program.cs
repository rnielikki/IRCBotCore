﻿using IRCBot.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRCBot
{
    class Program
    {
        static void Main(string[] args)
        {
            bool directStart = false;
            string encodingName = null;
            string ThisName = Environment.GetCommandLineArgs()[0];
            IEnumerator<string> Args = args.Cast<string>().GetEnumerator();
            while (Args.MoveNext())
            {
                switch (Args.Current.ToLower())
                {
                    case "-encoding":
                        if (!Args.MoveNext())
                        {
                            Console.WriteLine($"usage: {ThisName} [-encoding Encoding] [-start]");
                            Console.WriteLine($"use {ThisName} -help to see help.");
                            return;
                        }
                        else
                        {
                            encodingName = Args.Current;
                        }
                        break;
                    case "-start":
                        directStart = true;
                        break;
                    case "-help":
                        Console.WriteLine($"usage: {ThisName} [-encoding Encoding] [-start]");
                        Console.WriteLine("-start : directly start from configuration without UI. (Needs already done setting file)");
                        Console.WriteLine("-encoding : Set encoding for the IRCBot (default is UTF-8)");
                        return;
                }
            }
            if (encodingName == null) encodingName = "utf-8";
            if (!MainController.SetEncoding(encodingName)) return;
            if (!directStart)
            {
                ScreenController.Get();
            }
            else
            {
                Task<Setting> task = MainController.Start();
                if (task?.Result == null)
                {
                    Console.WriteLine("An error occured while reading or parsing the file. The IRCBot cannot be started.");
                }
                else if (task.IsCanceled || task.IsFaulted)
                {
                    Console.WriteLine("Reading task is failed or canceled. The IRCBot cannot be started.");
                }
                else
                {
                    MainController.Load(task.Result, false);
                }
            }
        }
    }
}