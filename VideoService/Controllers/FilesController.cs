using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Mvc;
using VideoBackend;
using VideoBackend.Model;

namespace VideoService.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private VideoUpload _videoUpload;

        public FilesController()
        {
            _videoUpload = new VideoUpload();
        }

        [HttpGet("delete")]
        public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
        {
            try
            {
                string accessToken = GetAccessTokenFromHeader();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return new JsonResult(new { Message = "Authentication error" })
                    {
                        StatusCode = 401
                    };
                }

                await _videoUpload.Delete(accessToken, filePath);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetFiles()
        {
            try
            {
                string accessToken = GetAccessTokenFromHeader();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return new JsonResult(new { Message = "Authentication error" })
                    {
                        StatusCode = 401
                    };
                }

                var files = await _videoUpload.GetAllFiles(accessToken);
                return new JsonResult(files);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { Message = "Internal Server Error.", ErrorDetails = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }

        [HttpGet("get-upload-url")]
        public async Task<IActionResult> GetPresignedUrl([FromQuery] string fileName)
        {
            try
            {
                string accessToken = GetAccessTokenFromHeader();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return new JsonResult(new { Message = "Authentication error" })
                    {
                        StatusCode = 401
                    };
                }

                var fullPath = $"/{fileName}";
                var commitInfo = new CommitInfo(
                    path: fullPath,
                    mode: WriteMode.Overwrite.Instance // Overwrite if the file already exists.
                );
                using (var dropBoxClient = new DropboxClient(accessToken))
                {
                    var uploadUrl = await dropBoxClient.Files.GetTemporaryUploadLinkAsync(new GetTemporaryUploadLinkArg(commitInfo));
                    return Ok(new { Url = uploadUrl.Link });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private string GetAccessTokenFromHeader()
        {
            string accessToken = string.Empty;

            if (HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

                // Check if the header starts with "Bearer "
                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract the token by removing the "Bearer " prefix
                    accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();
                }
            }
            return accessToken;
        }
    }
}
