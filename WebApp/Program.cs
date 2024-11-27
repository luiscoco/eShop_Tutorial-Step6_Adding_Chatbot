
using WebApp.Components;
using eShop.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.JsonWebTokens;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
.AddInteractiveServerComponents();

// Register the chat client for Azure OpenAI
builder.Services.AddSingleton<IChatClient>(static serviceProvider =>
{
    var endpoint = new Uri("https://myaiserviceluiscoco.openai.azure.com/");
    var credentials = new AzureKeyCredential("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
    var deploymentName = "gpt-4o";

    IChatClient client = new AzureOpenAIClient(endpoint, credentials).AsChatClient(deploymentName);

    // Build the ChatClient pipeline using ChatClientBuilder
    IChatClient chatClient = new ChatClientBuilder(client)
        .UseFunctionInvocation() // Adds a pipeline step for function invocation
        .Build();

    return chatClient;
});

builder.AddApplicationServices();

//builder.AddAuthenticationServices();

//builder.Services.AddHttpForwarderWithServiceDiscovery();

//builder.Services.AddSingleton<IProductImageUrlProvider, ProductImageUrlProvider>();
//builder.Services.AddHttpClient<CatalogService>(o => o.BaseAddress = new("http://localhost:5301"))
//    .AddApiVersion(1.0);

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapForwarder("/product-images/{id}", "http://localhost:5301", "/api/catalog/items/{id}/pic");

app.Run();

