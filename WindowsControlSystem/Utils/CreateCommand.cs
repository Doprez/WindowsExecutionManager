using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using WindowsControlSystem.Models;

namespace WindowsControlSystem.Utils
{
    class CreateCommand
    {
        public static async Task<ProcessOutput> BuildCommand(string application, string args)
        {
            try
            {
                ProcessOutput output = new ProcessOutput();

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName = application;
                startInfo.Arguments = "/C " + args;

                process.StartInfo = startInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    output.Output +=process.StandardOutput.ReadLineAsync().Result + "\n";
                }
                output.OutputCode = process.ExitCode;

                return await Task.FromResult(output);
            }
            catch(Exception exception)
            {
                return new ProcessOutput { Error = exception.Message, OutputCode = 1};
                //logging needed
            }
        }

        public static async Task<ProcessOutput> BuildCommandAdmin(string application, string args)
        {
            try
            {
                ProcessOutput output = new ProcessOutput();

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName = application;
                startInfo.Arguments = "/C " + args;
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
                //startInfo.UserName = userName;
                //startInfo.Password = password;
                process.StartInfo = startInfo;
                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    output.Output += "\n" + process.StandardOutput.ReadLineAsync().Result;
                }
                output.OutputCode = process.ExitCode;

                return await Task.FromResult(output);
            }
            catch(Exception exception)
            {
                return new ProcessOutput { Error = exception.Message, OutputCode = 1 }; 
                //logging needed
            }
        }
    }
}
