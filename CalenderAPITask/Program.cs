using CalenderAPITask.Service;
using DotNetEnv;
using FirebaseAdmin;
using Google.Api;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//DotNetEnv.Env.Load();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddScoped<IUserService, UserService>();


var googleAppCred = builder.Configuration.GetValue<String>("GOOGLE_CREDENTIALS_PATH");

Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", googleAppCred);

var credential = GoogleCredential.FromFile(googleAppCred);
FirebaseApp.Create(new AppOptions
{
    Credential = credential,
});
builder.Services.AddSingleton(provider =>
{
    var firestoreDb = FirestoreDb.Create("calenderapitask");
    return firestoreDb;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string secretKey = builder.Configuration.GetValue<string>("JWT_SECRET_KEY");
        byte[] secretKeyAsBytes = Encoding.UTF8.GetBytes(secretKey);
        var secureSecretKey = new SymmetricSecurityKey(secretKeyAsBytes);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = secureSecretKey,
            ValidateIssuer= false,
            ValidateAudience = false,
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
