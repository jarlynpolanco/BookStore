using BlazorInputFile;
using BookStore_UI.Contracts;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BookStore_UI.Services
{
    public class FileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _env;

        public FileUpload(IWebHostEnvironment env) 
        {
            _env = env;
        }

        public async Task<bool> UploadFileAsync(string base64Image, string picName)
        {
            bool isSuccess;
            try
            {
                var path = $"{_env.WebRootPath}\\uploads\\{picName}";
                var imgBytes = Convert.FromBase64String(base64Image);

                using(var imageFile = new FileStream(path, FileMode.Create))
                {
                    await imageFile.WriteAsync(imgBytes, 0, imgBytes.Length);
                    await imageFile.FlushAsync();
                }
                 
                isSuccess = true;
            }
            catch
            {
                isSuccess = false;
            }

            return isSuccess;
        }

        public void RemoveFile(string picName)
        {
            var path = $"{_env.WebRootPath}\\uploads\\{picName}";
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
