using Dropbox.Api;
using Dropbox.Api.Files;
using VideoBackend.Interface;
using VideoBackend.Model;
using VideoModels;

namespace VideoBackend
{
    public class FileOperation : IFileOperation
    {
        private const int CHUNK_SIZE = 1024 * 1024 * 4; // 4MB Chunks

        public FileOperation()
        {

        }

        public async Task<IList<CloudFileMetadata>> GetAllFiles(string accessToken)
        {
            var files = new List<CloudFileMetadata>();
            try
            {
                using (var dbx = new DropboxClient(accessToken))
                {
                    var list = await dbx.Files.ListFolderAsync(string.Empty, recursive: true);

                    foreach (var item in list.Entries)
                    {
                        if (item.IsFile)
                        {
                            // Process file information (e.g., item.Name, item.AsFile.Size)
                            //Console.WriteLine($"File: {item.Name}, Size: {item.AsFile.Size}");

                            files.Add(new CloudFileMetadata()
                            {
                                ID = item.AsFile.Id,
                                Name = item.Name,
                                Size = item.AsFile.Size,
                                PathDisplay = item.PathDisplay,
                                ServerModified = item.AsFile.ServerModified

                            });
                        }
                        else if (item.IsFolder)
                        {
                            // Process folder information (e.g., item.Name)
                            Console.WriteLine($"Folder: {item.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            return files;
        }

        public async Task Download(string accessToken, string filePath, string localSavePath)
        {
            try
            {
                using (var dbx = new DropboxClient(accessToken))
                using (var response = await dbx.Files.DownloadAsync(filePath))
                {
                    using (var fileStream = File.Create(localSavePath))
                    {
                        (await response.GetContentAsStreamAsync()).CopyTo(fileStream);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task Delete(string accessToken, string filePath)
        {
            try
            {
                using (var dbx = new DropboxClient(accessToken))
                {
                    var deletedFileMetadata = await dbx.Files.DeleteV2Async(filePath);
                    // Console.WriteLine($"File '{deletedFileMetadata.Metadata.PathDisplay}' deleted successfully.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UploadLargeFile(string accessToken, string localFilePath, string dropboxPath)
        {
            using (var dbx = new DropboxClient(accessToken))
            using (var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
            {
                if (fileStream.Length <= CHUNK_SIZE)
                {
                    // For small files, upload directly
                    await dbx.Files.UploadAsync(
                        dropboxPath,
                        WriteMode.Overwrite.Instance,
                        body: fileStream);
                }
                else
                {
                    // For large files, upload in chunks
                    byte[] buffer = new byte[CHUNK_SIZE];
                    string? sessionId = null;
                    int bytesRead;
                    ulong offset = 0;

                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        using (var memStream = new MemoryStream(buffer, 0, bytesRead))
                        {
                            if (offset == 0)
                            {
                                // Start upload session
                                var sessionStartResult = await dbx.Files.UploadSessionStartAsync(body: memStream);
                                sessionId = sessionStartResult.SessionId;
                            }
                            else if (offset + (ulong)bytesRead < (ulong)fileStream.Length)
                            {
                                // Append to upload session
                                await dbx.Files.UploadSessionAppendV2Async(
                                    new UploadSessionCursor(sessionId, offset),
                                    body: memStream);
                            }
                            else
                            {
                                // Finish upload session
                                await dbx.Files.UploadSessionFinishAsync(
                                    new UploadSessionCursor(sessionId, offset),
                                    new CommitInfo(dropboxPath, WriteMode.Overwrite.Instance),
                                    body: memStream);
                            }
                        }
                        offset += (ulong)bytesRead;
                    }
                }
            }
        }

        public async Task<IList<FileRevision>> GetFileRevisions(string accessToken, string filePath)
        {
            var fileRevisions = new List<FileRevision>();
            try
            {
                using (var dbx = new DropboxClient(accessToken))
                {
                    var revisions = await dbx.Files.ListRevisionsAsync(filePath);

                    foreach (var revision in revisions.Entries)
                    {
                        var fileRevision = new FileRevision()
                        {
                            Id = revision.Id,
                            Name = revision.Name,
                            Rev = revision.Rev,
                            Size = revision.Size,
                            ServerModified = revision.ServerModified,
                        };
                        fileRevisions.Add(fileRevision);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return fileRevisions;
        }
    }
}
