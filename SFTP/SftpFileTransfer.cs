using Renci.SshNet;
using Renci.SshNet.Common;
using Serilog;
using SFTP.Enumerations;
using SFTP.Interfaces;
using SFTP.Models;
using SFTP.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SFTP.FileTransfer
{
    public class SftpFileTransfer : IFileTransfer
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(SftpFileTransfer));
        private readonly FileTransferSettings _settings;
        private List<FileTransferObject> _filesFound = new();

        public SftpFileTransfer(FileTransferSettings settings)
        {
            _settings = settings;
        }

        public async Task<IEnumerable<FileTransferObject>> GetDirectoryListingAsync(string remoteDirectoryPath, bool excludeDirectories = false)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();

                try
                {
                    var directoryListing = sftp.ListDirectory(remoteDirectoryPath).Select(file => new FileTransferObject(file));
                    _logger.Information($"Obtained directoryListing from remote diretory path {remoteDirectoryPath}.");

                    sftp.Disconnect();

                    return await Task.FromResult(excludeDirectories
                        ? directoryListing.Where(x => !x.IsDirectory)
                        : directoryListing);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error getting directory listing from {remoteDirectoryPath} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10004}");

                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }
                return null;
            }
        }

        public async Task<IEnumerable<FileTransferObject>> GetAllDirectoryListingsAsync(string remoteDirectoryPath, bool excludeDirectories = false)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();

                try
                {
                    var directoryListing = sftp.ListDirectory(remoteDirectoryPath).Select(file => new FileTransferObject(file));
                    _logger.Information($"Obtained directoryListing from remote diretory path {remoteDirectoryPath}.");

                    foreach (var file in directoryListing)
                    {
                        if (!file.IsDirectory)
                        {
                            _filesFound.Add(file);
                        }
                    }
                    foreach (var folder in directoryListing)
                    {
                        if (folder.Name != "." && folder.Name != ".." && folder.IsDirectory)
                        {
                            _filesFound.Add(folder);
                        }
                    }
                    foreach (var subfolder in directoryListing)
                    {
                        if (subfolder.Name != "." && subfolder.Name != ".." && subfolder.IsDirectory)
                        {
                            await GetAllDirectoryListingsAsync(subfolder.FullName);
                        }
                    }

                    return excludeDirectories
                        ? _filesFound.Where(x => !x.IsDirectory)
                        : _filesFound;
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error getting directory listing from {remoteDirectoryPath} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10004}");

                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }
                return null;
            }
        }

        public async Task<bool> DownloadFileAsync(string remoteDirectoryPath, FileTransferObject file)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();

                if (!String.IsNullOrWhiteSpace(remoteDirectoryPath))
                {
                    sftp.ChangeDirectory(remoteDirectoryPath);
                }

                try
                {
                    file.FileData = new MemoryStream();
                    sftp.DownloadFile(file.Name, file.FileData);
                    _logger.Information($"Downloaded file {file.Name} to memoryStream.");

                    sftp.Disconnect();
                    return await Task.FromResult(true);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error occured downloading file {file.Name} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10002}");

                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }

                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DownloadFilesAsync(string remoteDirectoryPath, List<FileTransferObject> files, Func<FileTransferObject, Task> processDownloadedFile)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();

                if (!String.IsNullOrWhiteSpace(remoteDirectoryPath))
                {
                    sftp.ChangeDirectory(remoteDirectoryPath);
                }

                foreach (var file in files)
                {
                    try
                    {
                        file.FileData = new MemoryStream();
                        sftp.DownloadFile(file.Name, file.FileData);
                        _logger.Information($"Downloaded file {file.Name} to memoryStream.");
                        await processDownloadedFile(file);
                    }
                    catch (Exception exception)
                    {
                        _logger.Error(exception, $"Error occured downloading file {file.Name} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10002}");
                        if (sftp.IsConnected)
                            sftp.Disconnect();
                        return false;
                    }
                }

                sftp.Disconnect();
                return true;
            }
        }

        public async Task<bool> UploadFileAsync(string remoteDirectoryPath, FileTransferObject file)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();

                if (!String.IsNullOrWhiteSpace(remoteDirectoryPath))
                {
                    sftp.ChangeDirectory(remoteDirectoryPath);
                }

                try
                {
                    file.FileData.Position = 0;
                    sftp.UploadFile(file.FileData, file.Name);
                    _logger.Information($"Uploaded file {file.Name} to the host.");

                    sftp.Disconnect();
                    return await Task.FromResult(true);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error occured uploading file {file.Name} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10003}");

                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }

                return await Task.FromResult(false);
            }
        }

        public async Task<bool> UploadDirectoryAsync(string localPath, string remotePath)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();

                try
                {
                    Console.WriteLine("Uploading directory {0} to {1}", localPath, remotePath);

                    IEnumerable<FileSystemInfo> infos =
                        new DirectoryInfo(localPath).EnumerateFileSystemInfos();
                    foreach (FileSystemInfo info in infos)
                    {
                        if (info.Attributes.HasFlag(FileAttributes.Directory))
                        {
                            string subPath = remotePath + "/" + info.Name;
                            if (!sftp.Exists(subPath))
                            {
                                sftp.CreateDirectory(subPath);
                            }
                            await UploadDirectoryAsync(info.FullName, remotePath + "/" + info.Name);
                        }
                        else
                        {
                            using (Stream fileStream = new FileStream(info.FullName, FileMode.Open))
                            {
                                Console.WriteLine(
                                    "Uploading {0} ({1:N0} bytes)",
                                    info.FullName, ((FileInfo)info).Length);

                                sftp.UploadFile(fileStream, remotePath + "/" + info.Name);
                            }
                        }
                    }

                    return await Task.FromResult(true);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error occured uploading file FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10003}");

                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }

                return await Task.FromResult(false);
            }
        }

        public async Task<bool> CreateRemoteFolder(string remoteDirectoryPath)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                try
                {
                    sftp.CreateDirectory(remoteDirectoryPath);
                    _logger.Information("created " + remoteDirectoryPath);
                    return await Task.FromResult(true);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error creating directory in {remoteDirectoryPath} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10003}");
                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }
                return await Task.FromResult(false);
            }
        }

        public bool DeleteFile(string remoteDirectoryPath, string fileName)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();

                if (!String.IsNullOrWhiteSpace(remoteDirectoryPath))
                {
                    sftp.ChangeDirectory(remoteDirectoryPath);
                }

                try
                {
                    sftp.DeleteFile(fileName);
                    _logger.Information($"Deleted file {fileName} from host.");
                    return true;
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error occured deleting file {fileName} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10005}");
                }
                finally
                {
                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }
                return false;
            }
        }

        public async Task<bool> MoveEntireDirectoryAsync(string remoteDirectorySourcePath, string remoteDirectoryDestinationPath)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();
                try
                {
                    var directoryListing = FindAllFilesToMove(sftp, remoteDirectorySourcePath).Result;
                    _logger.Information($"Obtained directoryListing from remote diretory path {remoteDirectorySourcePath}.");

                    var fileListings = directoryListing.Where(x => !x.IsDirectory);

                    foreach (var folder in directoryListing.Where(x => x.IsDirectory))//copy folder structure
                    {
                        sftp.CreateDirectory(folder.FullName.Replace(remoteDirectorySourcePath, remoteDirectoryDestinationPath));
                        _logger.Information("move " + folder.FullName.Replace(remoteDirectorySourcePath, remoteDirectoryDestinationPath));
                    }
                    foreach (var file in fileListings)//move actual files into new folders
                    {
                        file.MoveFile(file.FullName.Replace(remoteDirectorySourcePath, remoteDirectoryDestinationPath));
                        _logger.Information("move " + file.FullName.Replace(remoteDirectorySourcePath, remoteDirectoryDestinationPath));
                    }
                    //cleanup
                    _filesFound.Clear();
                    await DeleteEntireDirectoryAsync(remoteDirectorySourcePath);

                    sftp.Disconnect();
                    return await Task.FromResult(true);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error Moving directory listing from {remoteDirectorySourcePath} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10006}");
                    _filesFound.Clear();
                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }
                return await Task.FromResult(false);
            }
            //this class is necassary due to how SSH.NET will dispose of a connection after leaving the using block so it needs to share the same connection
            async Task<IEnumerable<FileTransferObject>> FindAllFilesToMove(SftpClient sftp, string remoteDirectoryPath, bool excludeDirectories = false)
            {
                try
                {
                    var directoryListing = sftp.ListDirectory(remoteDirectoryPath).Select(file => new FileTransferObject(file));
                    _logger.Information($"Obtained directoryListing from remote diretory path {remoteDirectoryPath}.");

                    foreach (var file in directoryListing)
                    {
                        if (!file.IsDirectory)
                        {
                            _filesFound.Add(file);
                        }
                    }
                    foreach (var folder in directoryListing)
                    {
                        if (folder.Name != "." && folder.Name != ".." && folder.IsDirectory)
                        {
                            _filesFound.Add(folder);
                        }
                    }
                    foreach (var subfolder in directoryListing)
                    {
                        if (subfolder.Name != "." && subfolder.Name != ".." && subfolder.IsDirectory)
                        {
                            await FindAllFilesToMove(sftp, subfolder.FullName);
                        }
                    }

                    return excludeDirectories
                        ? _filesFound.Where(x => !x.IsDirectory)
                        : _filesFound;
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error getting directory listing from {remoteDirectoryPath} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10004}");

                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }
                return null;
            }
        }

        public async Task<bool> DeleteEntireDirectoryAsync(string targetPath)
        {
            using (var sftp = new SftpClient(_settings.HostName, _settings.PortNumber, _settings.UserName, _settings.Password))
            {
                sftp.ErrorOccurred += Sftp_ErrorOccurred;
                sftp.Connect();
                try
                {
                    var directoryListing = GetAllDirectoryListingsAsync(targetPath).Result;
                    _logger.Information($"Obtained directoryListing from remote diretory path {targetPath}.");

                    var fileListings = directoryListing.Where(x => !x.IsDirectory);
                    directoryListing = directoryListing.Reverse();//to make sure the subfolders are deleted from the deepest folder first

                    foreach (var file in fileListings)//delete files first since SSH.NET does not allow you to delete non empty folders
                    {
                        sftp.DeleteFile(file.FullName);
                        _logger.Information("Delete " + file.FullName);
                    }
                    foreach (var folder in directoryListing.Where(x => x.IsDirectory))//delete all empty folders
                    {
                        sftp.DeleteDirectory(folder.FullName);
                        _logger.Information("Delete " + folder.FullName);
                    }
                    _filesFound.Clear();

                    sftp.Disconnect();

                    return await Task.FromResult(true);
                }
                catch (Exception exception)
                {
                    _logger.Error(exception, $"Error deleting directory listing from {targetPath} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10005}");
                    if (sftp.IsConnected)
                        sftp.Disconnect();
                }
            }
            return await Task.FromResult(false);
        }

        private static void Sftp_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            _logger.Error(e.Exception, $"Sftp Error Occurred {e.ToString()} FileTransferErrorCodes:{FileTransferErrorCodes.FileTransfer_10001}");
        }
    }
}