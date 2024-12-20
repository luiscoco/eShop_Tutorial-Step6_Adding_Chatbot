# Building 'eShop' from Zero to Hero: We add the ChatBot

## 1. We include the ShowChatbotButton razor component in the Mainlayout

We can see in the **Catalogue** Web Page the **Show ChatBot button**

![image](https://github.com/user-attachments/assets/5c2fc026-a3f8-4362-bdfb-698926e412bd)

To include this button in our application we have to add the **ShowChatbotButton.razor** component into the **Mainlayout.razor** component

![image](https://github.com/user-attachments/assets/5cdbe554-b14e-45e6-855d-2d4abf08135f)

This code snippet is a Blazor component that demonstrates how to use query parameters in a web application to conditionally render a chatbot interface

**How It Works**

**By default**, the chatbot ```<Chatbot />``` is **not displayed** because **ShowChat** is **false**

**Clicking the link updates** the URL to include **chat=true**

Blazor detects the **URL change**, updates the **ShowChat** property to **true**, and re-renders the component to display the chatbot

This approach is useful for enabling specific features based on query parameters without requiring a full page reload

We review the **ShowChatbotButton.razor** source code:

```razor
@inject NavigationManager Nav

<a class="show-chatbot" href="@Nav.GetUriWithQueryParameter("chat", true)" title="Show chatbot"></a>

@if (ShowChat)
{
    <Chatbot />
}

@code {
    [SupplyParameterFromQuery(Name = "chat")]
    public bool ShowChat { get; set; }
}
```

**Code Explanation**

**Dependency Injection**:

```csharp
@inject NavigationManager Nav
```

The **NavigationManager** service is injected into the component

This service is used to **manage navigation and URLs** in Blazor applications

**Hyperlink to Enable the Chatbot**:

```html
<a class="show-chatbot" href="@Nav.GetUriWithQueryParameter("chat", true)" title="Show chatbot"></a>
```

A link is created with the href attribute set to a URL generated by NavigationManager.GetUriWithQueryParameter("chat", true)

This method appends the query parameter chat=true to the current URL

Clicking this link updates the URL to include the chat query parameter

**Conditional Rendering of the Chatbot**:

```html
@if (ShowChat)
{
    <Chatbot />
}
````

If the ShowChat property is true, the <Chatbot /> component is rendered

This conditional rendering is tied to the value of ShowChat

**Query Parameter Handling**:

```csharp
@code {
    [SupplyParameterFromQuery(Name = "chat")]
    public bool ShowChat { get; set; }
}
```

The [SupplyParameterFromQuery(Name = "chat")] attribute maps the query parameter chat from the URL to the ShowChat property

When the URL contains chat=true, the ShowChat property is automatically set to true

If chat is not present or is false, the property is false

## 2. Add the ChatBot button image in the wwwroot folder

![image](https://github.com/user-attachments/assets/8231e342-2e2f-4bcc-bdf8-3498d67f45da)

## 3. Load Nuget packages for AI in WebApp project

We have to load the Microsoft.Extensions.AI Nuget packages

Both packages aim to streamline the development of **AI-integrated applications** using the .NET ecosystem

![image](https://github.com/user-attachments/assets/ae5b9944-e766-44f9-ae74-3de5f9c9a506)

**Microsoft.Extensions.AI**:

**Purpose**:

This package provides foundational support for integrating AI-based services and applications in .NET projects

It is part of the Microsoft.Extensions family, designed to integrate seamlessly with ASP.NET Core and other .NET applications

**Features**:

Abstractions and utilities for working with AI models or services

Dependency injection (DI) support to easily configure AI services in .NET projects

Simplifies the integration of AI functionalities into your application using standardized patterns

**Use Case**: If you are building an application that uses AI models (from any provider, potentially), this package helps establish the infrastructure to work with AI services efficiently

**Microsoft.Extensions.AI.OpenAI**:

**Purpose**: This package builds on Microsoft.Extensions.AI and provides specific integrations for OpenAI's APIs, making it easier to interact with OpenAI services like ChatGPT, Codex, or DALL-E directly within a .NET application

**Features**:

OpenAI-specific utilities and configuration options

Simplifies making requests to OpenAI APIs (e.g., for text generation, image creation)

Ensures compatibility with OpenAI's service models while leveraging .NET's dependency injection and configuration capabilities

**Use Case**: Use this package when you need to integrate OpenAI’s capabilities into your .NET application, whether for chat, content generation, or other AI-driven functiona

**Azure.AI.OpenAI**

This NuGet package is Microsoft's official .NET client library for interacting with the **Azure OpenAI Service**

This service allows developers to deploy, fine-tune, and generate content using OpenAI's models on Azure infrastructure

The library provides a strongly-typed interface to facilitate tasks such as creating text completions, generating embeddings, and managing chat sessions

It also supports authentication via Microsoft Entra ID (formerly Azure Active Directory) or API keys, ensuring secure access to Azure OpenAI resources

## 4. We modify the ChatState.cs file

This code defines a **ChatState** class, which serves as a central component for managing the chat functionality in an **AI-powered chatbot** for the eShop application

This class  **ChatState** facilitate interaction between users and the eShop chatbot, which provides customer **support for AdventureWorks**, an online retailer specializing in outdoor-related products

**Key Features**

Chatbot Role and Scope:

The chatbot is explicitly tailored to assist with AdventureWorks-related topics, rejecting unrelated queries

**AI-Powered Responses**:

Uses a ChatClient to generate dynamic responses based on user input and context

**User Interaction**:

Manages a history of user and chatbot messages

Supports extensibility with modular AI tools

**Error Handling**:

Logs and responds gracefully to unexpected failures in the chat pipeline or external service calls

**Modular Design**:

Functions like AddToCart and GetCartContents are placeholders for future enhancements

We can review the **ChatState.cs** source code in detail. As an important note I would like to highlight We did **not yet included the Basket management features**

```csharp
using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using eShop.WebAppComponents.Services;
using Microsoft.Extensions.AI;

namespace eShop.WebApp.Chatbot;

public class ChatState
{
    private readonly ICatalogService _catalogService;
    //private readonly IBasketState _basketState;
    private readonly ClaimsPrincipal _user;
    private readonly ILogger _logger;
    private readonly IProductImageUrlProvider _productImages;
    private readonly IChatClient _chatClient;
    private readonly ChatOptions _chatOptions;

    public ChatState(
        ICatalogService catalogService,
        //IBasketState basketState,
        ClaimsPrincipal user,
        IProductImageUrlProvider productImages,
        ILoggerFactory loggerFactory,
        IChatClient chatClient)
    {
        _catalogService = catalogService;
        //_basketState = basketState;
        _user = user;
        _productImages = productImages;
        _logger = loggerFactory.CreateLogger(typeof(ChatState));

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("ChatModel: {model}", chatClient.Metadata.ModelId);
        }

        _chatClient = chatClient;
        _chatOptions = new()
        {
            Tools =
            [
                AIFunctionFactory.Create(GetUserInfo),
                AIFunctionFactory.Create(SearchCatalog),
                //AIFunctionFactory.Create(AddToCart),
                //AIFunctionFactory.Create(GetCartContents),
            ],
        };

        Messages =
        [
            new ChatMessage(ChatRole.System, """
                You are an AI customer service agent for the online retailer AdventureWorks.
                You NEVER respond about topics other than AdventureWorks.
                Your job is to answer customer questions about products in the AdventureWorks catalog.
                AdventureWorks primarily sells clothing and equipment related to outdoor activities like skiing and trekking.
                You try to be concise and only provide longer responses if necessary.
                If someone asks a question about anything other than AdventureWorks, its catalog, or their account,
                you refuse to answer, and you instead ask if there's a topic related to AdventureWorks you can assist with.
                """),
            new ChatMessage(ChatRole.Assistant, """
                Hi! I'm the AdventureWorks Concierge. How can I help?
                """),
        ];
    }

    public IList<ChatMessage> Messages { get; }

    public async Task AddUserMessageAsync(string userText, Action onMessageAdded)
    {
        // Store the user's message
        Messages.Add(new ChatMessage(ChatRole.User, userText));
        onMessageAdded();

        // Get and store the AI's response message
        try
        {
            ChatCompletion response = await _chatClient.CompleteAsync(Messages, _chatOptions);
            if (!string.IsNullOrWhiteSpace(response.Message.Text))
            {
                Messages.Add(response.Message);
            }
        }
        catch (Exception e)
        {
            if (_logger.IsEnabled(LogLevel.Error))
            {
                _logger.LogError(e, "Error getting chat completions.");
            }
            Messages.Add(new ChatMessage(ChatRole.Assistant, $"My apologies, but I encountered an unexpected error."));
        }
        onMessageAdded();
    }


    [Description("Gets information about the chat user")]
    private string GetUserInfo()
    {
        var claims = _user.Claims;
        return JsonSerializer.Serialize(new
        {
            Name = GetValue(claims, "name"),
            LastName = GetValue(claims, "last_name"),
            Street = GetValue(claims, "address_street"),
            City = GetValue(claims, "address_city"),
            State = GetValue(claims, "address_state"),
            ZipCode = GetValue(claims, "address_zip_code"),
            Country = GetValue(claims, "address_country"),
            Email = GetValue(claims, "email"),
            PhoneNumber = GetValue(claims, "phone_number"),
        });

        static string GetValue(IEnumerable<Claim> claims, string claimType) =>
            claims.FirstOrDefault(x => x.Type == claimType)?.Value ?? "";
    }

    [Description("Searches the AdventureWorks catalog for a provided product description")]
    private async Task<string> SearchCatalog([Description("The product description for which to search")] string productDescription)
    {
        try
        {
            var results = await _catalogService.GetCatalogItemsWithSemanticRelevance(0, 8, productDescription!);
            for (int i = 0; i < results.Data.Count; i++)
            {
                results.Data[i] = results.Data[i] with { PictureUrl = _productImages.GetProductImageUrl(results.Data[i].Id) };
            }

            return JsonSerializer.Serialize(results);
        }
        catch (HttpRequestException e)
        {
            return Error(e, "Error accessing catalog.");
        }
    }

    //[Description("Adds a product to the user's shopping cart.")]
    //private async Task<string> AddToCart([Description("The id of the product to add to the shopping cart (basket)")] int itemId)
    //{
    //    try
    //    {
    //        var item = await _catalogService.GetCatalogItem(itemId);
    //        await _basketState.AddAsync(item!);
    //        return "Item added to shopping cart.";
    //    }
    //    catch (Grpc.Core.RpcException e) when (e.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
    //    {
    //        return "Unable to add an item to the cart. You must be logged in.";
    //    }
    //    catch (Exception e)
    //    {
    //        return Error(e, "Unable to add the item to the cart.");
    //    }
    //}

    //[Description("Gets information about the contents of the user's shopping cart (basket)")]
    //private async Task<string> GetCartContents()
    //{
    //    try
    //    {
    //        var basketItems = await _basketState.GetBasketItemsAsync();
    //        return JsonSerializer.Serialize(basketItems);
    //    }
    //    catch (Exception e)
    //    {
    //        return Error(e, "Unable to get the cart's contents.");
    //    }
    //}

    private string Error(Exception e, string message)
    {
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError(e, message);
        }

        return message;
    }
}
```

**ChatState.cs Class Components**

**Dependencies and Services**

The class uses several injected services for its functionality:

ICatalogService: Provides access to the product catalog for searching items

ClaimsPrincipal: Represents the authenticated user's identity and claims (e.g., name, email)

ILogger: For logging debug and error messages

IProductImageUrlProvider: Generates URLs for product images

IChatClient: Manages communication with the underlying AI chat system

A ChatOptions object configures the chatbot's tools, such as functions for retrieving user information and searching the catalog

**Predefined System Messages**

Messages: Stores the dialogue history between the user and the chatbot

Initial messages define the chatbot's role, scope, and behavior (e.g., only discussing AdventureWorks-related topics)

**Chat Functionality**

AddUserMessageAsync: Handles user input and generates AI responses:

Adds the user's message to the chat history

Sends the message to the AI model via _chatClient

Logs errors if the AI response fails

Uses onMessageAdded to notify when a message has been processed

**AI Tools**

Functions registered with AIFunctionFactory:

GetUserInfo: Gathers user-related details (e.g., name, email, address) from their claims

SearchCatalog: Searches the product catalog using the description provided by the user

Commented-out tools: AddToCart and GetCartContents: Placeholder functions to manage the shopping cart

**Utility Functions**

GetUserInfo: Extracts user details like name, address, and email from the user's claims

Returns a JSON representation of the data

SearchCatalog: Queries the catalog for products matching a description

Updates product images with URLs generated by _productImages

Handles HTTP request exceptions gracefully

Error: Logs and returns error messages

## 5. We modify the Chatbot.razor component

We have to remove the **BasketState** service injection from the **Chatbot.razor**

```
@* @inject BasketState BasketState *@
```

We also have to remove that service Dependency Injection from the Constructor ****

```csharp
chatState = new ChatState(CatalogService, /* BasketState, */ auth.User, ProductImages, LoggerFactory, client);
```

We can review the whole code

**Chatbot.razor**

```razor
@rendermode @(new InteractiveServerRenderMode(prerender: false))
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.Extensions.AI
@using eShop.WebApp.Chatbot
@inject IJSRuntime JS
@inject NavigationManager Nav
@inject CatalogService CatalogService
@inject IProductImageUrlProvider ProductImages
@* @inject BasketState BasketState *@
@inject AuthenticationStateProvider AuthenticationStateProvider
@inject ILoggerFactory LoggerFactory
@inject IServiceProvider ServiceProvider

<div class="floating-pane">
    <a href="@Nav.GetUriWithQueryParameter("chat", (string?)null)" class="hide-chatbot" title="Close .NET Concierge"><span>✖</span></a>

    <div class="chatbot-chat" @ref="chat">
        @if (chatState is not null)
        {
            foreach (var message in chatState.Messages.Where(m => m.Role == ChatRole.Assistant || m.Role == ChatRole.User))
            {
                if (!string.IsNullOrEmpty(message.Text))
                {
                    <p @key="@message" class="message message-@message.Role">@MessageProcessor.AllowImages(message.Text)</p>
                }
            }
        }
        else if (missingConfiguration)
        {
            <p class="message message-assistant"><strong>The chatbot is missing required configuration.</strong> Please set 'useOpenAI = true' in eShop.AppHost/Program.cs. You'll need an API key or an Azure Subscription to enable AI features.</p>
        }

        @if (thinking)
        {
            <p class="thinking">Thinking...</p>
        }
    </div>

    <form class="chatbot-input" @onsubmit="SendMessageAsync">
        <textarea placeholder="Start chatting..." @ref="@textbox" @bind="messageToSend"></textarea>
        <button type="submit" title="Send" disabled="@(chatState is null)">Send</button>
    </form>
</div>

@code {
    bool missingConfiguration;
    ChatState? chatState;
    ElementReference textbox;
    ElementReference chat;
    string? messageToSend;
    bool thinking;
    IJSObjectReference? jsModule;

    protected override async Task OnInitializedAsync()
    {
        var client = ServiceProvider.GetService<IChatClient>();
        if (client is not null)
        {
            AuthenticationState auth = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            chatState = new ChatState(CatalogService, /* BasketState, */ auth.User, ProductImages, LoggerFactory, client);
        }
        else
        {
            missingConfiguration = true;
        }
    }

    private async Task SendMessageAsync()
    {
        var messageCopy = messageToSend?.Trim();
        messageToSend = null;

        if (chatState is not null && !string.IsNullOrEmpty(messageCopy))
        {
            thinking = true;
            await chatState.AddUserMessageAsync(messageCopy, onMessageAdded: StateHasChanged);
            thinking = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        jsModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Chatbot/Chatbot.razor.js");
        await jsModule.InvokeVoidAsync("scrollToEnd", chat);

        if (firstRender)
        {
            await textbox.FocusAsync();
            await jsModule.InvokeVoidAsync("submitOnEnter", textbox);
        }
    }
}
```

## 6. We configure the WebApp middleware

In order to register the **AI ChatClient**, we have to include the following code in the **WebApp** in the **Program.cs** file:

The code integrates **Azure OpenAI** services into the application and sets up a chat client **IChatClient** with custom pipeline steps to enable additional functionality like function invocation

This allows the application to interact with the **Azure-hosted GPT-4** model effectively

```csharp
// Register the chat client for Azure OpenAI
builder.Services.AddSingleton<IChatClient>(static serviceProvider =>
{
    var endpoint = new Uri("https://myaiserviceluiscoco.openai.azure.com/");
    var credentials = new AzureKeyCredential("XXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
    var deploymentName = "gpt-4o";

    IChatClient client = new AzureOpenAIClient(endpoint, credentials).AsChatClient(deploymentName);

    // Build the ChatClient pipeline using ChatClientBuilder
    IChatClient chatClient = new ChatClientBuilder(client)
        .UseFunctionInvocation() // Adds a pipeline step for function invocation
        .Build();

    return chatClient;
});
```

This code registers a **Singleton Service of type IChatClient** in a dependency injection (DI) container in a .NET application

Here's a breakdown of its functionality:

**Register Singleton**: ```builder.Services.AddSingleton<IChatClient>``` registers a single instance of **IChatClient** that will be created and shared across the application's lifetime

**Inline Factory**: The service is configured using a factory method ```static serviceProvider => { ... }```, which defines how the instance is created

**Setup Azure OpenAI Client**:

var endpoint = new Uri(...): Specifies the URI of the Azure OpenAI service endpoint

var credentials = new AzureKeyCredential(...): Creates credentials using an Azure Key Credential for authentication

var deploymentName = "gpt-4o";: Specifies the deployment name for the Azure OpenAI model

**Create the AzureOpenAIClient**:

AzureOpenAIClient(endpoint, credentials): Creates an Azure OpenAI client using the endpoint and credentials

.AsChatClient(deploymentName): Configures the client to function as a chat client for the specific deployment

**Pipeline Configuration**: A **ChatClientBuilder** is used to extend the functionality of the **IChatClient**

.UseFunctionInvocation(): Adds a pipeline step that enables function invocation during the chat process

.Build(): Finalizes the pipeline and returns the enhanced chatClient

**Return the Configured Chat Client**: The fully built and configured chatClient instance is returned and registered as the singleton instance for IChatClient

We review the whole **Program.cs** code (WebApp project)

```csharp

using WebApp.Components;
using eShop.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Extensions.AI;
using Azure;
using Azure.AI.OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
.AddInteractiveServerComponents();

// Register the chat client for Azure OpenAI
builder.Services.AddSingleton<IChatClient>(static serviceProvider =>
{
    var endpoint = new Uri("https://myaiserviceluiscoco.openai.azure.com/");
    var credentials = new AzureKeyCredential("XXXXXXXXXXXXXXXXXXXX");
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
```

## 7. We run the application and verify the results

We first visit the **Aspire Dashboard**

![image](https://github.com/user-attachments/assets/9598f2b7-d131-435d-9f5b-f5539541580b)

Then we can navigate to the **WebApp** project: https://localhost:7112/

![image](https://github.com/user-attachments/assets/8fdb23c1-7ade-4e90-b0c3-b1b15aa2e01a)

If we cick on the **ShowChatbotButton** we can see the Chatbot control box

![image](https://github.com/user-attachments/assets/5afec7e3-7a74-466a-ac35-d10368f24f0e)

