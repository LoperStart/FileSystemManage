
namespace FileSystemManage
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 配置文件上传设置
builder.Services.Configure<FileSystemManage.Models.FileUploadSettings>(builder.Configuration.GetSection("FileUploadSettings"));

// 注册文件上传服务
builder.Services.AddScoped<FileSystemManage.Services.IFileUploadService, FileSystemManage.Services.FileUploadService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置请求大小限制
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
    options.ValueLengthLimit = int.MaxValue;
});

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
