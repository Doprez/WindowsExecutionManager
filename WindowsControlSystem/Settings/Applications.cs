using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsControlSystem.Models;

namespace WindowsControlSystem.Settings
{
    public class Applications
    {      
        public static List<ApplicationCommand> GetApps()
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            string file = "applications.json";
            if (File.Exists(file))
            {
                using (StreamReader r = new StreamReader(file))
                {
                    string json = r.ReadToEnd();
                    List<ApplicationCommand> apps = JsonSerializer.Deserialize<List<ApplicationCommand>>(json, options);
                    return apps;
                }
            }
            else
            {
                using(StreamWriter write = new StreamWriter(file))
                {
                    List<ApplicationCommand> apps = new List<ApplicationCommand> 
                    {
                        new ApplicationCommand
                        { ApplicationExe = "cmd.exe", CommandOptions = new List<CommandOption>
                            {
                                new CommandOption
                                {
                                    Name = "Get IP",
                                    Arguments = "ipconfig"
                                },
                                new CommandOption
                                {
                                    Name = "help command",
                                    Arguments = "help"
                                },
                                new CommandOption
                                {
                                    Name = "test",
                                    Arguments = "Get-NetIPAddress"
                                }
                            }
                        },
                        new ApplicationCommand
                        { ApplicationExe = "powershell.exe", CommandOptions = new List<CommandOption>
                            {
                                new CommandOption 
                                {
                                    Name = "Get IP",
                                    Arguments = "Get-NetIPAddress"
                                },
                                new CommandOption
                                {
                                    Name = "Get IP",
                                    Arguments = "Get-NetIPAddress"
                                },
                                new CommandOption
                                {
                                    Name = "Get IP",
                                    Arguments = "Get-NetIPAddress"
                                }
                            }
                        },
                        new ApplicationCommand
                        {
                            ApplicationExe = "example.ps1",
                            CommandOptions = new List<CommandOption>
                            {
                                new CommandOption
                                {
                                    Name = "",
                                    Arguments = ""
                                }
                            }
                        }
                    };
                    var result = JsonSerializer.Serialize(apps, options);
                    write.Write(result);
                    return apps;
                }
            }
        }

    }
}
