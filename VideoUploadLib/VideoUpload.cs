using Dropbox.Api;
using Dropbox.Api.Files;
using VideoBackend.Interface;
using VideoBackend.Model;
using VideoModels;

namespace VideoBackend
{
    public class VideoUpload : IFileOperation, IAuthenticationOperation, IUserOperation
    {
        private const int CHUNK_SIZE = 1024 * 1024 * 4; // 4MB Chunks

        public VideoUpload()
        {
        }

        public async Task<User> GetCurrentAccountAsync(string accessToken)
        {
            try
            {
                using (var dbx = new DropboxClient(accessToken))
                {
                    var full = await dbx.Users.GetCurrentAccountAsync();
                    return new User()
                    {
                        DisplayName = full.Name.DisplayName,
                        Email = full.Email
                    };
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

        public async Task<IList<CloudFile>> GetAllFiles(string accessToken)
        {
            var files = new List<CloudFile>();
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

                            files.Add(new CloudFile()
                            {
                                ID = item.AsFile.Id,
                                Name = item.Name,
                                Size = item.AsFile.Size,
                                PathDisplay = item.PathDisplay,
                                ServerModified = item.AsFile.ServerModified

                            });

                            //var metadata = await dbx.Files.GetMetadataAsync(filePath);

                            //if (metadata is FileMetadata fileMetadata)
                            //{
                            //    Console.WriteLine($"File Name: {fileMetadata.Name}");
                            //    Console.WriteLine($"File ID: {fileMetadata.Id}");
                            //    Console.WriteLine($"Revision: {fileMetadata.Rev}");
                            //    Console.WriteLine($"Size: {fileMetadata.Size} bytes");
                            //    Console.WriteLine($"Last Modified: {fileMetadata.ServerModified}");
                            //}
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

        public async Task Authorize()
        {
            // Your app key and secret from the Dropbox App Console
            string appKey = "VideoUpload262";
            string appSecret = "eehgx19oildrdpq";

            // Define the scopes your app needs
            var scopes = new List<string>
                {
                    "files.metadata.read",  // Example: Read file metadata
                    "files.content.read",   // Example: Read file content
                    "account_info.read"     // Example: Access account info
                };

            // OAuth2 authorization URL
            var redirectUri = "https://localhost/authorize"; // Replace with your redirect URI
            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, appKey, redirectUri, state: null, tokenAccessType: TokenAccessType.Offline, scopeList: scopes.ToArray());

            Console.WriteLine("1. Go to: " + authorizeUri);
            Console.WriteLine("2. Authorize the app and paste the authorization code here:");
            string authCode = Console.ReadLine();

            // Exchange the authorization code for an access token
            var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(authCode, appKey, appSecret, redirectUri);

            Console.WriteLine("Access Token: " + response.AccessToken);
        }

        public async Task UploadLargeFile(string accessToken, string localFilePath, string dropboxPath)
        {
            using (var dbx = new DropboxClient(accessToken))
            {
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
    }
}
