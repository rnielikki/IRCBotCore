using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using IRCBot.Model;

namespace IRCBot
{
    class ModuleLoader
    {
        public Dictionary<string, IModule> ModuleList { get; private set; }
        private string[] ReservedWords = new string[] { "help", "version" };
        private static ModuleLoader Instance = null;
        private string path;
        private ModuleLoader(string modpath)
        {
            ModuleList = new Dictionary<string, IModule>();
            path = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, modpath);
        }
        //Singleton
        internal static ModuleLoader Call(string modpath)
        {
            if (Instance == null)
            {
                Instance = new ModuleLoader(modpath);
            }
            return Instance;
        }
        internal void LoadAll()
        {
            Console.WriteLine("----------------Loading modules...-----------");
            foreach (string module in Directory.GetFiles(path, "*.dll"))
            {
                Assembly M = null;
                try
                {
                    M = Assembly.LoadFile(module);
                }
                catch (FileLoadException)
                {
                    Console.WriteLine($"Module {module} is already loaded");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"Module {module} is possibly deleted.");
                }
                catch (BadImageFormatException){
                    Console.WriteLine($"Module {module} has bad format");
                }
                var types = M?.GetTypes()?.Where(a => a.GetInterface("IModule") != null);
                var eventTypes = M?.GetTypes()?.Where(a => a.GetInterface("IEventModule") != null);
                if ((types == null && eventTypes == null) || types.Count() + eventTypes.Count() == 0)
                {
                    Console.WriteLine("Nothing can be loaded : " + module);
                }
                else
                {
                    Console.WriteLine("Starts to load module : " + module);
                    foreach (var type in types)
                    {
                        try
                        {
                            IModule imodule = Activator.CreateInstance(type) as IModule;
                            if (imodule != null)
                            {
                                AddModule(imodule);
                            }
                        }
                        catch (MissingMethodException)
                        {
                            //if constructor cannot found
                            Console.WriteLine($"Failed to load {type.Name}, Check the class constructor.");
                        }
                        //Not interested to load non-public module, because IModule is public.
                        catch (MethodAccessException) { }
                    }
                    foreach (var type in  eventTypes) {
                        try
                        {
                            IEventModule iemodule = Activator.CreateInstance(type) as IEventModule;
                            Console.WriteLine("Event Module Loaded: " + type.Name);
                            if (iemodule != null)
                            { //only eventmodule
                                IRCBot.messageEvents += iemodule.EventResult;
                            }
                        }
                        catch (MissingMethodException)
                        {
                            //if constructor cannot found
                            Console.WriteLine($"Failed to load {type.Name}, Check the class constructor.");
                        }
                        //Not interested to load non-public module, because IEventModule is public.
                        catch (MethodAccessException) { }
                    }
                    /*foreach (var eventType in eventTypes)
                    {
                        try
                        {
                            IEventModule iemodule = Activator.CreateInstance(eventType) as IEventModule;
                            if (iemodule != null)
                            {
                                IRCBot.messageEvents+=iemodule.EventResult;
                            }
                        }
                        catch (MissingMethodException)
                        {
                            //if constructor cannot found
                            Console.WriteLine($"Failed to load {eventType.Name}, Check the class constructor.");
                        }
                    }*/
                    Console.WriteLine("Module loaded : " + module);
                }
            }
            Console.WriteLine("-------------Loaded modules.--------------");
        }
        private void AddModule(IModule Instance)
        {
            if (ModuleList.Keys.Contains(Instance.command))
            {
                Console.WriteLine($":: The command {Instance.command} was already implemented. This module cannot be added.");
            }
            else if (ReservedWords.Contains(Instance.command))
            {
                Console.WriteLine($":: The command {Instance.command} is reserved. This module cannot be added.");
            }
            else
            {
                ModuleList.Add(Instance.command, Instance);
                IEventModule tryIE = Instance as IEventModule;
                if (tryIE != null)
                {
                    //If both methods are implemented
                    IRCBot.messageEvents += tryIE.EventResult;
                }
                Console.WriteLine(":: Command loaded : " + Instance.command);
            }
        }
    }
}
