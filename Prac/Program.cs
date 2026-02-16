using Microsoft.EntityFrameworkCore;
using Prac;
using Prac.Data;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy => policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200", "https://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// WebSocket
builder.Services.AddSingleton<WebSocketConnectionManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

app.Map("/ws/upload", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var socket = await context.WebSockets.AcceptWebSocketAsync();
    var manager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();
    manager.AddSocket(socket);

    while (socket.State == WebSocketState.Open)
        await Task.Delay(1000);
});

var wsManager = app.Services.GetRequiredService<WebSocketConnectionManager>();
var tcpServer = new TcpFileUploadServer(wsManager);
Task.Run(() => tcpServer.StartAsync());


app.UseAuthorization();

app.MapControllers();

app.Run();
