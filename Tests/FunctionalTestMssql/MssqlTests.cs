using System.Net;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.Data.SqlClient;

namespace FunctionalTestMssql
{
    public sealed class MssqlTests : IAsyncLifetime
    {
        private readonly MsSqlTestcontainer _mssqlContainer;
        private readonly String _connectionString;

        public MssqlTests()
        {
            var mssqlConfiguration = new MsSqlTestcontainerConfiguration();

            _mssqlContainer = new TestcontainersBuilder<MsSqlTestcontainer>()
                .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
                .WithDatabase(new MsSqlTestcontainerConfiguration
            {
                Database ="Restful",
                Password = "2Secure*Password2"
                })
              .Build();         

        }

        public Task InitializeAsync()
        {

            return _mssqlContainer.StartAsync();
            
        }

        public Task DisposeAsync()
        {
            return _mssqlContainer.DisposeAsync().AsTask();
        }

        public sealed class Api : IClassFixture<MssqlTests>, IDisposable
        {
            private readonly WebApplicationFactory<Program> _webApplicationFactory;
            private readonly IServiceScope _serviceScope;
            private readonly HttpClient _httpClient;
            private readonly String filename = "../../../test.json";
            private readonly IServiceCollection _serviceProvider;

            public Api(MssqlTests mssqlTests)
            {
                var connectionString = "Server=localhost,1433;Database=Restful;User Id=SA;Password=2Secure*Password2;TrustServerCertificate=True";
                Environment.SetEnvironmentVariable("ASPNETCORE_URLS", "https://+");
                Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path", "certificate.crt");
                Environment.SetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password", "password");
                Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", mssqlTests._mssqlContainer.ConnectionString);
                _webApplicationFactory = new WebApplicationFactory<Program>();
                _serviceScope = _webApplicationFactory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
                _httpClient = _webApplicationFactory.CreateClient();

                ExecuteCommand(mssqlTests);
            }

            public void Dispose()
            {
                _httpClient.Dispose();
                _serviceScope.Dispose();
                _webApplicationFactory.Dispose();
            }

            public async Task ExecuteCommand(MssqlTests mssqlTests)
            {
                await using (var connection = new SqlConnection(mssqlTests._mssqlContainer.ConnectionString))
                {
                    connection.Open();

                    await using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = "CREATE TABLE Authors (Id uniqueidentifier,Name nvarchar(max),DateOfBirth datetime2);";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "Insert into Authors values ('90d10994-3bdd-4ca2-a178-6a35fd653c59','J.K. Rowling','19650531');";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "Insert into Authors values ('60d10994-3bdd-4ca2-a178-6a35fd653c59','Jwling','19650531');";
                        cmd.ExecuteNonQuery();
                    }

                }
            }

            [Fact]
            public async Task GetAuthor()
            {
                // Using application factory
                string filePath = filename;
                                
                var response = await _httpClient.GetAsync($"/api/Author");    
                Assert.NotNull( response );
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
            }

            [Fact]
            public async Task GetAuthorById()
            {
                // Using application factory
                string filePath = filename;

                var response = await _httpClient.GetAsync($"/api/Author/id?id=60d10994-3bdd-4ca2-a178-6a35fd653c59");
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            }

            [Fact]
            public async Task DeleteAuthorById()
            {
                // Using application factory
                string filePath = filename;

                var response = await _httpClient.DeleteAsync($"/api/Author/id?id=90d10994-3bdd-4ca2-a178-6a35fd653c59");
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            }

            [Fact]
            public async Task AddNewAuthor()
            {
                StreamReader r = new StreamReader(filename);
                String json = r.ReadToEnd();
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/api/Author", content);
                Assert.NotNull(response);
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            }


        }
    }
}