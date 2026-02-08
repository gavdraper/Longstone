var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Longstone_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpsHealthCheck("/health");

builder.Build().Run();
