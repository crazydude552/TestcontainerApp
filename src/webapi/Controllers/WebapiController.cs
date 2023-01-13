using Microsoft.AspNetCore.Mvc;
using webapi.Service;

namespace webapi.Controllers;

[ApiController]
[Route("[controller]")]
public class WebapiController : ControllerBase
{
    private readonly ILogger<WebapiController> _logger;
    private readonly IStorageService _storageService;

    public WebapiController(IStorageService storageService,IConfiguration configuration, ILogger<WebapiController> logger)
    {
        _logger = logger;
        _storageService = storageService;

        var builder = new ConfigurationBuilder()
            .AddUserSecrets<WebapiController>(true);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetFileFromBlob(string file)
    {
        var stream = await _storageService.GetFileBlobAsync(file);

        return File(stream, "application/octet-stream", "test.txt"); ;
    }


    [HttpPost("[action]")]
    public async Task<IActionResult> UploadMultiFilesToStorage(IList<IFormFile> files)
    {
        await _storageService.UploadMultiFile(files);

        return Ok("Files uploaded successfully");
    }


    [HttpPost("[action]")]
    public async Task<IActionResult> UploadFileToBlob(IFormFile file)
    {
        await _storageService.UploadFile(file);

        return Ok("File uploaded");
    }


    [HttpDelete("[action]")]
    public async Task<IActionResult> DeleteFileFromBlob(String file)
    {
        var response = await _storageService.DeleteFromBlob(file);
        return Ok(response);
    }

}
