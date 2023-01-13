	POC-dn6 
//****************/

Requirements:
Dotnet 6
Docker daemon

Api perform four basic rest operations
1. Upload a file to blob
2. download a file from blob
3. delete a file from blob
4. upload multiple files to blob

For testing there are two projects 
1. Specflow sample as FunctionalTest
2. tests a general test with xunit

Testing of the application uses Testcontainer, which means azurite testcontainer will be created
before the test starts and perform a rest operation and close the Container
------------------------------------------------------------

poc-dn6 is a solution package which contains a sample API and testing the api requests using the TestContainers.

Folder Structure:

poc-dn6
->docs
->pipeline
->src
	->webapi (Rest api contains uploadfile, upload multiple files, download a file and delete a file)
		->Container
		->Controllers
		->Properties
		->Service
		-appsetting.Development.json
		-appsetting.json
		-Program.cs
		-webapi.csproj
		-webapi.sln
->FunctionalTest (Specflow sample test project for Api testing of 4 api calls, upload, upload files,download,delete from blobs.)
->Tests
	->IntegrationTest
		-AzureBlobStrorageTests.cs (Integration test for Add a file to Blob Storage, Add multiple files to BlobStorage, Delete a file from BlobStorage)
	-test.json (sample test file to upload)
	-Tests.csproj
	-Usings.cs

	

