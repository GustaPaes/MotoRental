using Microsoft.EntityFrameworkCore;
using Minio;
using MongoDB.Driver;
using MotoRental.Application.Interfaces;
using MotoRental.Application.Services;
using MotoRental.Domain.Entities;
using MotoRental.Infrastructure.Consumers;
using MotoRental.Infrastructure.Data;
using MotoRental.Infrastructure.Repositories;
using MotoRental.Infrastructure.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Configure MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB");
    return new MongoClient(connectionString);
});

builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(builder.Configuration["MongoDB:DatabaseName"] ?? "motorental");
});

// Configure RabbitMQ
builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["RabbitMQ:HostName"],
        UserName = builder.Configuration["RabbitMQ:UserName"],
        Password = builder.Configuration["RabbitMQ:Password"],
        DispatchConsumersAsync = true
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

// MongoDB Repository
builder.Services.AddScoped<IMongoRepository<Notification>>(provider =>
{
    var database = provider.GetRequiredService<IMongoDatabase>();
    var logger = provider.GetRequiredService<ILogger<MongoRepository<Notification>>>();
    return new MongoRepository<Notification>(database, "notifications", logger);
});

// Register the Consumer as a Hosted Service
builder.Services.AddHostedService<MotorcycleCreatedConsumer>();

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