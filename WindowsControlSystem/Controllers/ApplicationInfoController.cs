using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsControlSystem.Models;
using WindowsControlSystem.Settings;

namespace WindowsControlSystem.Controllers
{
    public class ApplicationInfoController
    {
        List<ApplicationCommand> appSettings;

        public ApplicationInfoController()
        {
            appSettings = Applications.GetApps();
        }
        public Task<IEnumerable<string>> GetApplications()
        {
            var result = appSettings.Select(x => x.ApplicationExe);
            return Task.FromResult(result);
        }

        public Task<string> GetApplication(int index)
        {
            return Task.FromResult(appSettings[index].ApplicationExe);
        }

        public Task<IEnumerable<string>> GetCommandNameList(int index1)
        {
            return Task.FromResult(appSettings[index1].CommandOptions.Select(x => x.Name));
        }

        public Task<string> GetCommandName(int index1, int index2)
        {
            return Task.FromResult(appSettings[index1].CommandOptions[index2].Name);
        }

        public Task<string> GetCommandArgs(int index1, int index2)
        {
            return Task.FromResult(appSettings[index1].CommandOptions[index2].Arguments);
        }
    }
}
