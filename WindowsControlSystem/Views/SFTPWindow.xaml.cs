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
using System.Windows.Shapes;
using WindowsControlSystem.Controllers;

namespace WindowsControlSystem
{
    /// <summary>
    /// Interaction logic for SFTPWindow.xaml
    /// </summary>
    public partial class SFTPWindow : Window
    {
        FileTransferController _controller;

        public SFTPWindow(FileTransferController controller, string path = "/")
        {
            InitializeComponent();
            _controller = controller;
            listBoxDirectory.ItemsSource = SeparateFolderFromFile(path);
            textBoxPath.Text = _controller.GetDirectory(path).Result.Select(x => x.Name).FirstOrDefault();
        }

        private void listBoxDirectory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DirectoryDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string debug = listBoxDirectory.SelectedItem.ToString();
            var result = _controller.GetDirectory(debug).Result;
            if (result != null)
            {
                textBoxPath.Text = debug;
                //listBoxDirectory.ItemsSource = result.Select(x => x.FullName);
                listBoxDirectory.ItemsSource = SeparateFolderFromFile(debug);
            }
        }

        public IEnumerable<string> SeparateFolderFromFile(string path)
        {
            var result = _controller.GetDirectory(path).Result;
            List<string> add = new List<string>();

            foreach (var item in result)
            {
                if (item.IsDirectory)
                {
                    add.Add(item.FullName);
                }
                else
                {
                    add.Add(item.FullName.Remove(0, 1));
                }
            }

            return add;
        }
    }
}
