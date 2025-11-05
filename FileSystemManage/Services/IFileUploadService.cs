using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FileSystemManage.Services
{
    public interface IFileUploadService
    {
        /// <summary>
        /// 直接上传小文件
        /// </summary>
        /// <param name="file">上传的文件</param>
        /// <returns>文件路径</returns>
        Task<string> UploadFileAsync(IFormFile file);

        /// <summary>
        /// 初始化大文件上传
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="fileSize">文件大小</param>
        /// <param name="fileMd5">文件MD5</param>
        /// <returns>上传ID</returns>
        Task<string> InitLargeFileUploadAsync(string fileName, long fileSize, string fileMd5);

        /// <summary>
        /// 上传文件分片
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <param name="chunkIndex">分片索引</param>
        /// <param name="chunkData">分片数据</param>
        /// <param name="chunkMd5">分片MD5</param>
        /// <returns>是否上传成功</returns>
        Task<bool> UploadChunkAsync(string uploadId, int chunkIndex, IFormFile chunkData, string chunkMd5);

        /// <summary>
        /// 完成大文件上传
        /// </summary>
        /// <param name="uploadId">上传ID</param>
        /// <returns>文件路径</returns>
        Task<string> CompleteLargeFileUploadAsync(string uploadId);
    }
}