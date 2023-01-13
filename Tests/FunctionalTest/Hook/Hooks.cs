using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

using TechTalk.SpecFlow;

namespace FunctionalTest.Hook;
[Binding]
public sealed class Hooks
{

    public AzuriteTestcontainer _azuriteTestcontainer;
    [BeforeScenario("@api")]
    public Task BeforeScenarioWithTag()
    {
        var azuriteConfiguration = new AzuriteTestcontainerConfiguration();
        azuriteConfiguration.BlobServiceOnlyEnabled = true;

        _azuriteTestcontainer = new TestcontainersBuilder<AzuriteTestcontainer>()
            .WithImage("mcr.microsoft.com/azure-storage/azurite")
            .WithPortBinding(10000)
            .WithCommand("azurite-blob", "--blobHost", "0.0.0.0", "--blobPort", "10000")
            .WithAutoRemove(false)
            .Build();

        return _azuriteTestcontainer.StartAsync();
    }

    

    [AfterScenario]
    public Task AfterScenario()
    {
        
            return _azuriteTestcontainer.DisposeAsync().AsTask();
        
    }
}