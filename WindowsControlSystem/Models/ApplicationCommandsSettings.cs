using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsControlSystem.Models
{
    public class ApplicationCommandsSettings
    {
        public string ApplicationExe { get; set; }
        public List<CommandOptions> CommandOptions { get; set; }
        
    }

    public class CommandOptions
    {
        public string Name { get; set; }
        public string Arguments { get; set; }
    }
}
