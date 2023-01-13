using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using System.Net;
using System.Net.Http.Headers;

namespace Tests.IntegrationTest;

public sealed class AzureBlobStorageTests : IAsyncLifetime
{
    private readonly AzuriteTestcontainer _azuriteTestcontainer;

    public AzureBlobStorageTests()
    {
        var azuriteConfiguration = new AzuriteTestcontainerConfiguration();
        azuriteConfiguration.BlobServiceOnlyEnabled = true;

        _azuriteTestcontainer = new TestcontainersBuilder<AzuriteTestcontainer>()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .WithPortBinding(10000)
            .WithCommand("azurite-blob", "--blobHost", "0.0.0.0", "--blobPort", "10000")
            .WithAutoRemove(false)
            .Build();
    }

    public Task InitializeAsync()
    {
        return _azuriteTestcontainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _azuriteTestcontainer.DisposeAsync().AsTask();
    }

    public sealed class Api : IClassFixture<AzureBlobStorageTests>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _webApplicationFactory;
        private readonly IServiceScope _serviceScope;
        private readonly HttpClient _httpClient;
        private readonly String filename = "../../../test.json";

        public Api(AzureBlobStorageTests azureBlobStorageTests)
        {
            var connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "https://+");
            Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path", "certificate.crt");
            Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password", "password");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
            _webApplicationFactory = new WebApplicationFactory<Program>();
            _serviceScope = _webApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            _httpClient = _webApplicationFactory.CreateClient();
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _serviceScope.Dispose();
            _webApplicationFactory.Dispose();
        }

        [Fact]
        public async Task UploadFIle()
        {
            // Using application factory
            string filePath = filename;
            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            // here it is important that second parameter matches with name given in API.
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync($"/Webapi/UploadFileToBlob", form);
            Console.WriteLine(response.Content);
            response.EnsureSuccessStatusCode();

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetFile()
        {
            // Using application factory
            string filePath = filename;
            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            // here it is important that second parameter matches with name given in API.
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync($"/Webapi/UploadFileToBlob", form);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var response_get = await _httpClient.GetAsync("/Webapi/GetFileFromBlob?file=test.json");
                Assert.Equal(HttpStatusCode.OK, response_get.StatusCode);
            }

        }

        [Fact]
        public async Task DeleteFileFromBlob()
        {
            // Using application factory
            string filePath = filename;
            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            // here it is important that second parameter matches with name given in API.
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var response = await _httpClient.PostAsync($"/Webapi/UploadFileToBlob", form);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var response_get = await _httpClient.DeleteAsync("/Webapi/DeleteFileFromBlob?file=test.json");
                Assert.Equal(HttpStatusCode.OK, response_get.StatusCode);
            }

        }

        [Fact]
        public async Task FilesUploadTest()
        {
            var factory = new WebApplicationFactory<Program>();
            string filePath = filename;
            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            // here it is important that second parameter matches with name given in API.
            form.Add(fileContent, "formFile", Path.GetFileName(filePath));

            var client = factory.CreateClient();

            var response = await _httpClient.PostAsync($"/Webapi/UploadMultiFilesToStorage", form);
            response.EnsureSuccessStatusCode();

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }



    }
}