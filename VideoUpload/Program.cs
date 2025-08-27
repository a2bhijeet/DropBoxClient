// See https://aka.ms/new-console-template for more information
using VideoBackend;

var videoUpload = new VideoUpload();

try
{
    Console.WriteLine("Get current user");
    var task = Task.Run((Func<Task>)videoUpload.GetCurrentAccountAsync);
    task.Wait();

    Console.WriteLine("List all files...");
    var listFilesTask = Task.Run(() => videoUpload.ListFiles());
    listFilesTask.Wait();

    /*
    Console.WriteLine("Uploading...");
    var filePath = @"C:\Users\abhij\Downloads\Sample1.mp4";
    var fileName = Path.GetFileName(filePath);
    var uploadTas = Task.Run(() => videoUpload.UploadLargeFile(filePath, "/" + fileName));
    uploadTas.Wait();
    */
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

