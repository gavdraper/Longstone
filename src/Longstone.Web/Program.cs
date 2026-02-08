using FluentValidation;
using Longstone.Application;
using Longstone.Infrastructure;
using Longstone.Web.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(ApplicationAssemblyReference.Assembly));
builder.Services.AddValidatorsFromAssembly(ApplicationAssemblyReference.Assembly);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutputCache();

var app = builder.Build();

await app.Services.InitialiseDatabaseAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
