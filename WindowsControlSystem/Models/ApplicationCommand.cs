using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsControlSystem.Models
{
    public class ApplicationCommand
    {
        public string ApplicationExe { get; set; }
        public List<CommandOption> CommandOptions { get; set; }
        
    }

    public class CommandOption
    {
        public string Name { get; set; }
        public string Arguments { get; set; }
    }
}
