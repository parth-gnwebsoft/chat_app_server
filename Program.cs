using Api;
using Services.Logic;
using WebSocketTest.Hubs;
using WebSocketTest.Services;

var builder = WebApplication.CreateBuilder(args); 

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddScoped<IMessageRepository, MessageApiService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<ReactionService>();
builder.Services.AddSingleton<ConnectionTracker>(); 


var app = builder.Build();
app.UseAuthorization();
app.MapHub<EchoHub>("/chathub");
app.MapControllers(); 

app.Run();
