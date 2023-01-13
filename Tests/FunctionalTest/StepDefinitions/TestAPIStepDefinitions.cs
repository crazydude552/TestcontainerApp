using System.Net.Http.Headers;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FunctionalTest.StepDefinitions
{
    [Binding, Scope(Feature = "FunctionalTest")]
    public class TestAPIStepDefinitions
    {
        private readonly String filename = "../../../test.json";
        public HttpClient restClient;
        public HttpResponseMessage restResponse;


        [Given(@"client requests the file")]
        public void GivenClientRequestsTheFile()
        {
            WebApplicationFactory<Program> _webApplicationFactory = new WebApplicationFactory<Program>();
            restClient = _webApplicationFactory.CreateClient();
        }


        [When(@"send a upload request to the api")]
        public async Task WhenSendAUploadRequestToTheApi()
        {
            string filePath = filename;
            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            // here it is important that second parameter matches with name given in API.
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            restResponse = await restClient.PostAsync($"/Webapi/UploadFileToBlob", form);

        }

        [Then(@"send a download request to the api")]
        public async Task WhenSendAGetRequestToTheApi()
        {

            restResponse = await restClient.GetAsync("/Webapi/GetFileFromBlob?file=test.json");

        }

        [When(@"send multiple files to upload using the api")]
        public async Task WhenSendMultipleFilesToUploadUsingTheApi()
        {
            string filePath = filename;
            using var form = new MultipartFormDataContent();
            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            // here it is important that second parameter matches with name given in API.
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            restResponse = await restClient.PostAsync($"/Webapi/UploadMultiFilesToStorage", form);
        }


        [Then(@"operation is ok")]
        public void ThenOperationIsOk()
        {
            Assert.Equal(HttpStatusCode.OK, restResponse.StatusCode);
        }

        [Then(@"delete the uploaded file")]
        public async Task ThenDeleteTheUploadedFile()
        {
            restResponse = await restClient.DeleteAsync("/Webapi/DeleteFileFromBlob?file=test.json");
        }


    }

}

