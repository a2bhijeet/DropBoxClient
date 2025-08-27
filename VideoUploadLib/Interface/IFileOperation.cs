using VideoBackend.Model;
using VideoModels;

namespace VideoBackend.Interface
{
    public interface IFileOperation
    {
        Task<IList<CloudFile>> GetAllFiles(string accessToken);

        Task Delete(string accessToken, string filePath);

        Task UploadLargeFile(string accessToken, string localFilePath, string dropboxPath);

        Task Download(string accessToken, string filePath, string localSavePath);
    }
}
