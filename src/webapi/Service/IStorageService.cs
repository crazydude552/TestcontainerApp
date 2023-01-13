

namespace webapi.Service

{
    public interface IStorageService
    {
        Task UploadFile(IFormFile formfile);

        Task UploadMultiFile(IList<IFormFile> files);

        Task<byte[]> GetFileBlobAsync(string fileName);

        Task<String> DeleteFromBlob(string fileName);
    }
}
