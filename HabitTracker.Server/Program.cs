using HabitTracker.Server.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавляем контекст базы данных SQLite
builder.Services.AddDbContext<ServerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка CORS для разработки (разрешаем все источники)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // разрешить любой источник
              .AllowAnyMethod()   // разрешить любые HTTP методы (GET, POST, PUT, DELETE)
              .AllowAnyHeader();  // разрешить любые заголовки
    });
});

var app = builder.Build();

// Применяем миграции автоматически при запуске (для разработки)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServerDbContext>();
    dbContext.Database.Migrate(); // создаёт БД и применяет миграции, если их нет
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Важно: UseCors должен быть вызван до остальных middleware
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();