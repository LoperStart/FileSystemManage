using System;using System.Collections.Generic;using System.IO;using System.Linq;using System.Security.Cryptography;using System.Threading.Tasks;using Microsoft.AspNetCore.Http;using Microsoft.Extensions.Options;using FileSystemManage.Models;

namespace FileSystemManage.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly FileUploadSettings _settings;
        private readonly string _uploadPath;
        private readonly string _tempPath;

        public FileUploadService(IOptions<FileUploadSettings> settings)
        {
            _settings = settings.Value;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            _tempPath = Path.Combine(Directory.GetCurrentDirectory(), "Temp");

            // 确保目录存在
            Directory.CreateDirectory(_uploadPath);
            Directory.CreateDirectory(_tempPath);
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public Task<string> InitLargeFileUploadAsync(string fileName, long fileSize, string fileMd5)
        {
            // 创建一个唯一的上传ID
            string uploadId = Guid.NewGuid().ToString();

            // 创建临时目录存储分片
            string tempDir = Path.Combine(_tempPath, uploadId);
            Directory.CreateDirectory(tempDir);

            // 可以在这里保存上传信息到数据库或缓存

            return Task.FromResult(uploadId);
        }

        public async Task<bool> UploadChunkAsync(string uploadId, int chunkIndex, IFormFile chunkData, string chunkMd5)
        {
            if (string.IsNullOrEmpty(uploadId))
                throw new ArgumentException("UploadId is empty");

            if (chunkData == null || chunkData.Length == 0)
                throw new ArgumentException("Chunk data is empty");

            // 验证分片MD5
            string calculatedMd5 = await CalculateMd5Async(chunkData);
            if (calculatedMd5 != chunkMd5)
                return false;

            // 保存分片到临时目录
            string tempDir = Path.Combine(_tempPath, uploadId);
            string chunkFileName = $"chunk_{chunkIndex}";
            string chunkPath = Path.Combine(tempDir, chunkFileName);

            using (var stream = new FileStream(chunkPath, FileMode.Create))
            {
                await chunkData.CopyToAsync(stream);
            }

            return true;
        }

        public async Task<string> CompleteLargeFileUploadAsync(string uploadId)
        {
            if (string.IsNullOrEmpty(uploadId))
                throw new ArgumentException("UploadId is empty");

            string tempDir = Path.Combine(_tempPath, uploadId);
            if (!Directory.Exists(tempDir))
                throw new DirectoryNotFoundException("Temp directory not found");

            // 获取所有分片文件
            var chunkFiles = Directory.GetFiles(tempDir)
                .OrderBy(f => int.Parse(Path.GetFileName(f).Split('_')[1]))
                .ToList();

            if (!chunkFiles.Any())
                throw new InvalidOperationException("No chunks found");

            // 创建最终文件
            string fileName = Guid.NewGuid().ToString() + ".tmp"; // 这里可以根据实际情况获取原始文件扩展名
            string filePath = Path.Combine(_uploadPath, fileName);

            using (var outputStream = new FileStream(filePath, FileMode.Create))
            {
                foreach (var chunkFile in chunkFiles)
                {
                    using (var inputStream = new FileStream(chunkFile, FileMode.Open))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }
                }
            }

            // 删除临时目录和分片文件
            Directory.Delete(tempDir, true);

            return filePath;
        }

        private async Task<string> CalculateMd5Async(IFormFile file)
        {
            using (var md5 = MD5.Create())
            {
                var stream = file.OpenReadStream();
                var hash = await md5.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}