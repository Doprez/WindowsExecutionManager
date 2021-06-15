using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsControlSystem.Controllers;
using WindowsControlSystem.Models;
using WindowsControlSystem.Settings;
using WindowsControlSystem.Utils;

namespace WindowsControlSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ApplicationInfoController appController = new ApplicationInfoController();
        FileTransferController _fileTransferController;
        public MainWindow()
        {
            InitializeComponent();
            txtCommandOutput.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            cmbApps.ItemsSource = appController.GetApplications().Result;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string selectedApp = string.IsNullOrWhiteSpace(cmbApps.Text) ? "cmd.exe" : cmbApps.Text;
            string selectedargument = appController.GetCommandArgs(cmbApps.SelectedIndex, cmbAppCommands.SelectedIndex).Result;

            var output = CreateCommand.BuildCommand(selectedApp, selectedargument + " " + txtCommand.Text).Result;
            if (checkBoxClearOnRun.IsChecked == true)
            {
                txtCommandOutput.Document.Blocks.Clear();
            }
            txtCommandOutput.AppendText(output.Output + "\nOutput Code: " + output.OutputCode);
            txtCommandOutput.ScrollToEnd();

        }

        private void cmbApps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbAppCommands.ItemsSource = appController.GetCommandNameList(cmbApps.SelectedIndex).Result;
            
            UpdateCommandText(true);
        }

        private void cmbAppCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCommandText();
        }

        public void UpdateCommandText(bool resetArgs = false)
        {
            cmbAppCommands.SelectedIndex = cmbAppCommands.SelectedIndex.Equals(-1) ? 0 : cmbAppCommands.SelectedIndex;
            textBoxCommand.Document.Blocks.Clear();
            textBoxCommand.AppendText(appController.GetApplication(cmbApps.SelectedIndex).Result + " " + appController.GetCommandArgs(cmbApps.SelectedIndex, cmbAppCommands.SelectedIndex).Result);
        }

        private void buttonClearOutput_Click(object sender, RoutedEventArgs e)
        {
            txtCommandOutput.Document.Blocks.Clear();
        }

        private void buttonConnect_Click(object sender, RoutedEventArgs e)
        {
            _fileTransferController = new FileTransferController(textBoxHost.Text, textBoxUserName.Text, textBoxPassword.Text, Convert.ToInt32(textBoxPort.Text));
            var sftpWindow = new SFTPWindow(_fileTransferController);
            sftpWindow.Show();
        }
    }
}
