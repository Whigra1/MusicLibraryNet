using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MusicLibraryNet.Database;
using MusicLibraryNet.Database.Auth;
using MusicLibraryNet.Database.Chat;
using MusicLibraryNet.Database.Music;
using MusicLibraryNet.Dto;
using MusicLibraryNet.Dto.Chat;
using MusicLibraryNet.Dto.Music;
using MusicLibraryNet.Providers;
using MusicLibraryNet.Services;
using MusicLibraryNet.Services.Abstractions;
using MusicLibraryNet.Utils;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Add services to the container.
builder.Services
    .AddIdentity<MusicUser, MusicRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSingleton(_ => new OpenAIClient(builder.Configuration["OPENAI_API_KEY"]));

builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<EntityCrudExecutor<SongDto, Song>>();
builder.Services.AddScoped<EntityCrudExecutor<PlaylistDto, Playlist>>();
builder.Services.AddScoped<EntityCrudExecutor<FileDto, MusicLibraryNet.Database.File>>();
builder.Services.AddScoped<EntityCrudExecutor<ChatDto, Chat>>();
builder.Services.AddScoped<EntityCrudExecutor<MessageDto, Message>>();

builder.Services.AddScoped<ICrudService<PlaylistDto, Playlist>, PlaylistService>();
builder.Services.AddScoped<ICrudService<SongDto, Song>, SongService>();
builder.Services.AddScoped<ICrudService<FileDto, MusicLibraryNet.Database.File>, FileService>();
builder.Services.AddScoped<ICrudService<ChatDto, Chat>, ChatService>();
builder.Services.AddScoped<ICrudService<MessageDto, Message>, MessageService>();
builder.Services.AddScoped<ChatGptService>();

builder.Services.AddScoped<IPlaylistService<PlaylistDto, Playlist>, PlaylistService>();
builder.Services.AddScoped<ISongService<SongDto, Song>, SongService>();
builder.Services.AddScoped<IFileService<FileDto, MusicLibraryNet.Database.File>, FileService>();
builder.Services.AddScoped<IChatService<ChatDto, Chat>, ChatService>();
builder.Services.AddScoped<IMessageService<MessageDto, Message>, MessageService>();


builder.Services.AddScoped<MusicStoreProvider>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173", "https://localhost:5173") // Replace with your localhost URL and port
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // If your API supports cookies/auth
        });
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax; // TODO for aws needs to be SameSiteMode.Strict
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.MaxAge = TimeSpan.FromHours(5);
    // options.Cookie.Expiration = TimeSpan.FromHours(5);
    options.ExpireTimeSpan = TimeSpan.FromHours(5);

    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});

builder.Services.AddSwaggerGen(c =>
{
    
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});


var app = builder.Build();
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // This makes the Swagger UI accessible at '/'.
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var chatGptUserExists = await AssertChatGptUserExists();

if (!chatGptUserExists)
{
    throw new Exception("ChatGPT user does not exist");
}

app.UseCors("AllowLocalhost");

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var env = app.Services.GetRequiredService<IWebHostEnvironment>();

var rootDir = builder.Configuration["StoreDirectory"] ?? ".";

DirectoryUtils.EnsureDirectoryExists(rootDir.StartsWith('/') ? rootDir : Path.Combine(env.ContentRootPath, rootDir));


app.Run();

async Task<bool> AssertChatGptUserExists()
{
    var userManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<UserManager<MusicUser>>();
    var u = await userManager.FindByNameAsync("ChatGPT");
    if (u is not null) return true;
    
    var user = new MusicUser
    {
        
        FirstName = "ChatGPT",
        LastName = "ChatGpt",
        UserName = "ChatGPT",
        Email = "<EMAIL>",
        EmailConfirmed = true,
        PhoneNumberConfirmed = true,
        PhoneNumber = "1234567890"
    };
    var result = await userManager.CreateAsync(user);
    return result.Succeeded;

}

