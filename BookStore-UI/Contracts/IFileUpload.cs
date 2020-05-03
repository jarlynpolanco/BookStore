using System.Threading.Tasks;

namespace BookStore_UI.Contracts
{
    public interface IFileUpload
    {
        public Task<bool> UploadFileAsync(string base64Image, string picName);

        public void RemoveFile(string picName);
    }
}
