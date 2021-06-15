using SFTP.FileTransfer;
using SFTP.Models;
using SFTP.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsControlSystem.Controllers
{
    public class FileTransferController
    {
        readonly SftpFileTransfer _sftp;

        public FileTransferController(string hostName, string userName, string password, int port = 22)
        {
            _sftp = new SftpFileTransfer(new FileTransferSettings
            {
                HostName = hostName,
                UserName = userName,
                Password = password,
                PortNumber = port
            }) ;
        }

        public async Task<IEnumerable<FileTransferObject>> GetDirectory(string path)
        {
            return await _sftp.GetDirectoryListingAsync(path);
        }

        public async Task<bool> CreateFolder(string path)
        {
            return await _sftp.CreateRemoteFolder(path);
        }
        public async Task<bool> DeleteFolder(string path)
        {
            return await _sftp.DeleteEntireDirectoryAsync(path);
        }
    }
}
