using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileSystemManage.Services;

namespace FileSystemManage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// 上传文件（自动判断是否为大文件）
        /// </summary>
        /// <param name="file">上传的文件</param>
        /// <returns>文件路径</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            string filePath = await _fileUploadService.UploadFileAsync(file);
            return Ok(new { filePath });
        }

        /// <summary>
        /// 初始化大文件上传
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="fileSize">文件大小</param>
        /// <param name="fileMd5">文件MD5</param>
        /// <returns>上传ID</returns>
        [HttpPost("largefile/init")]
        public async Task<IActionResult> InitLargeFileUpload([FromForm] string fileName, [FromForm] long fileSize, [FromForm] string fileMd5)
        {
            string uploadId = await _fileUploadService.InitLargeFileUploadAsync(fileName, fileSize, fileMd5);
            return Ok(new { uploadId });
        }

        /// <summary>
        /// 上传文件分片
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <param name="chunkIndex">分片索引</param>
        /// <param name="chunkData">分片数据</param>
        /// <param name="chunkMd5">分片MD5</param>
        /// <returns>是否上传成功</returns>
        [HttpPost("largefile/chunk")]
        public async Task<IActionResult> UploadChunk([FromForm] string uploadId, [FromForm] int chunkIndex, [FromForm] IFormFile chunkData, [FromForm] string chunkMd5)
        {
            bool success = await _fileUploadService.UploadChunkAsync(uploadId, chunkIndex, chunkData, chunkMd5);
            return Ok(new { success });
        }

        /// <summary>
        /// 完成大文件上传
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <returns>文件路径</returns>
        [HttpPost("largefile/complete")]
        public async Task<IActionResult> CompleteLargeFileUpload([FromForm] string uploadId)
        {
            string filePath = await _fileUploadService.CompleteLargeFileUploadAsync(uploadId);
            return Ok(new { filePath });
        }
    }
}