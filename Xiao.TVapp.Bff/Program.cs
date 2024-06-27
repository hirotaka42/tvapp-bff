using Microsoft.EntityFrameworkCore;
using Xiao.TVapp.Bff.Contexts;

var builder = WebApplication.CreateBuilder(args);

// CORS ポリシーの設定
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//// DbContext の登録
builder.Services.AddDbContext<StreamingUrlsContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("MariaDbContext");
    options.UseMySql(connectionString, 
        new MySqlServerVersion(new Version(10, 5, 25)),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        })
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors();
});

// CORS ポリシーの適用
var app = builder.Build();
app.UseCors();

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

