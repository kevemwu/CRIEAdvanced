using CRIEAdvanced.Data;
using CRIEAdvanced.Extensions;
using CRIEAdvanced.Filters;
using CRIEAdvanced.Helpers;
using CRIEAdvanced.Middlewares;
using CRIEAdvanced.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CRIEAdvanceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CRIEAdvancedDb"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure();
        }));

builder.Services.AddSingleton<IJWTManagerRepository, JWTManagerRepository>();

builder.Services.AddScoped<UsersRepository>();
builder.Services.AddScoped<IUserRefreshTokensRepository, UserRefreshTokensRepository>();

builder.Services.AddScoped<RsaHelper>();
builder.Services.AddScoped<LogsExtension>();
builder.Services.AddScoped<ToolExtension>();
builder.Services.AddScoped<VerifyLogicExtension>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // �����ҥ��ѮɡA�^�����Y�|�]�t WWW-Authenticate ���Y�A�o�̷|��ܥ��Ѫ��Բӿ��~��]
        options.IncludeErrorDetails = true; // �w�]�Ȭ� true�A���ɷ|�S�O����

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // �z�L�o���ŧi�A�N�i�H�q "sub" ���Ȩó]�w�� User.Identity.Name
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            // �z�L�o���ŧi�A�N�i�H�q "roles" ���ȡA�åi�� [Authorize] �P�_����
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            // �@��ڭ̳��|���� Issuer
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

            // �q�`���ӻݭn���� Audience
            ValidateAudience = false,
            //ValidAudience = "JwtAuthDemo", // �����ҴN���ݭn��g

            // �@��ڭ̳��|���� Token �����Ĵ���
            ValidateLifetime = true,

            // �p�G Token ���]�t key �~�ݭn���ҡA�@�볣�u��ñ���Ӥw
            ValidateIssuerSigningKey = false,

            // "1234567890123456" ���ӱq IConfiguration ���o
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
        };
    });

builder.WebHost.UseKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;  // 100MB
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //�]�w Json �^���榡�� PascalCase
    options.JsonSerializerOptions.PropertyNamingPolicy = null;

    //https://docs.microsoft.com/zh-tw/dotnet/api/system.text.json.serialization.jsonignorecondition?view=net-6.0
    //�]�w�Ȭ� NULL �h�����
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});

// �`�J HttpClient
builder.Services.AddHttpClient();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var serviceScope = app.Services.CreateScope())
{
    var serviceProvider = serviceScope.ServiceProvider;

    try
    {
        var dbContext = serviceProvider.GetRequiredService<CRIEAdvanceDbContext>();

        var databaseName = dbContext.Database.GetDbConnection().Database;
        Console.WriteLine($"�w�s�����Ʈw�G{databaseName}");

        foreach (var entityType in dbContext.Model.GetEntityTypes())
        {
            Console.WriteLine($"��ƪ�W�١G {entityType.GetTableName()}");
        }

        var users = dbContext.Users.ToList();

        // �L�XUsers��ƪ����Ҧ����e
        Console.WriteLine("Users��ƪ����Ҧ����e�G");
        foreach (var user in users)
        {
            Console.WriteLine($"Id: {user.Id}, UserID: {user.UserID}, UserPassword: {user.UserPassword}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"�s�����Ʈw�ɵo�Ϳ��~: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/error-development");

    app.UseSwagger();
    app.UseSwaggerUI();
}
//else
//{
//    //app.UseExceptionHandler("/error");
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

// ������v�� API �I�s
app.UseAuthorized();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//.NET Core 3.0 and IIS: HTTP Error 500.30 - ANCM In-Process Start Failure: failed to load coreclr
//hostingModel="InProcess" >> hostingModel="OutOfProcess"
//https://stackoverflow.com/questions/59369322/net-core-3-0-and-iis-http-error-500-30-ancm-in-process-start-failure-failed

//ASP.NET Core Runtime 6.0.2 > Windows	Hosting Bundle
//https://dotnet.microsoft.com/en-us/download/dotnet/6.0

// Step 1: �n�w�� dotnet-hosting-6.0.22-win.exe 
// >> https://dotnet.microsoft.com/en-us/download/dotnet/6.0
// >> ��� ASP.NET Core Runtime 6.0.22 �A�� Windows �U �� Hosting Bundle 
// Step 2: HTTP Error 502.5 - ANCM Out-Of-Process Startup Failure 
// >> �n�]�w IIS 
// > �i���]�w > (�@��) > �ҰʼҦ� > AlwaysRunning
// > ���ͳB�z�Ǽҫ��ƥ�������� > ���J�ϥΪ̳]�w�� > True
// Step 3: appsettings.json
// >> �D���n�N�s��Ʈw��, �]�w���� �b�� �P �K�X
// >> �������쵲�O�Ѥ����H�������v���o�X�� : local �n�h�[�]�w TrustServerCertificate=true