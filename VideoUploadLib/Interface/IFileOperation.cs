using VideoBackend.Model;
using VideoModels;

namespace VideoBackend.Interface
{
    public interface IFileOperation
    {
        Task<IList<CloudFileMetadata>> GetAllFiles(string accessToken);

        Task<IList<FileRevision>> GetFileRevisions(string accessToken, string filePath);

        Task UploadLargeFile(string accessToken, string localFilePath, string dropboxPath);

        Task Delete(string accessToken, string filePath);

        Task Download(string accessToken, string filePath, string localSavePath);
    }
}
