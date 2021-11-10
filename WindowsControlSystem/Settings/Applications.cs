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
        public static List<ApplicationCommandsSettings> GetApps()
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
                    List<ApplicationCommandsSettings> apps = JsonSerializer.Deserialize<List<ApplicationCommandsSettings>>(json, options);
                    return apps;
                }
            }
            else
            {
                using(StreamWriter write = new StreamWriter(file))
                {
                    List<ApplicationCommandsSettings> apps = new List<ApplicationCommandsSettings> 
                    {
                        new ApplicationCommandsSettings
                        { ApplicationExe = "cmd.exe", CommandOptions = new List<CommandOptions>
                            {
                                new CommandOptions
                                {
                                    Name = "Get IP",
                                    Arguments = "ipconfig"
                                },
                                new CommandOptions
                                {
                                    Name = "help command",
                                    Arguments = "help"
                                },
                                new CommandOptions
                                {
                                    Name = "test",
                                    Arguments = "Get-NetIPAddress"
                                }
                            }
                        },
                        new ApplicationCommandsSettings
                        { ApplicationExe = "powershell.exe", CommandOptions = new List<CommandOptions>
                            {
                                new CommandOptions 
                                {
                                    Name = "Get IP",
                                    Arguments = "Get-NetIPAddress"
                                },
                                new CommandOptions
                                {
                                    Name = "Get IP",
                                    Arguments = "Get-NetIPAddress"
                                },
                                new CommandOptions
                                {
                                    Name = "Get IP",
                                    Arguments = "Get-NetIPAddress"
                                }
                            }
                        },
                        new ApplicationCommandsSettings
                        {
                            ApplicationExe = "example.ps1",
                            CommandOptions = new List<CommandOptions>
                            {
                                new CommandOptions
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
