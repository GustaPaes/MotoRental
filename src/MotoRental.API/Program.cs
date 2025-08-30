using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Minio;
using MotoRental.Infrastructure.Data;
using MotoRental.Application.Interfaces;
using MotoRental.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Configure RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["RabbitMQ:HostName"],
        UserName = builder.Configuration["RabbitMQ:UserName"],
        Password = builder.Configuration["RabbitMQ:Password"]
    };
    return factory.CreateConnection();
});

builder.Services.AddScoped<IMessageService, MessageService>();

// Configure MinIO
builder.Services.AddSingleton<MinioClient>(sp =>
{
    var endpoint = builder.Configuration["MinIO:Endpoint"];
    var accessKey = builder.Configuration["MinIO:AccessKey"];
    var secretKey = builder.Configuration["MinIO:SecretKey"];

    return new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .WithSSL(false)
        .Build();
});

builder.Services.AddScoped<IStorageService, StorageService>();

// Application Services
builder.Services.AddScoped<IRentalService, RentalService>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();