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
        // 當驗證失敗時，回應標頭會包含 WWW-Authenticate 標頭，這裡會顯示失敗的詳細錯誤原因
        options.IncludeErrorDetails = true; // 預設值為 true，有時會特別關閉

        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 透過這項宣告，就可以從 "sub" 取值並設定給 User.Identity.Name
            NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            // 透過這項宣告，就可以從 "roles" 取值，並可讓 [Authorize] 判斷角色
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",

            // 一般我們都會驗證 Issuer
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("JwtSettings:Issuer"),

            // 通常不太需要驗證 Audience
            ValidateAudience = false,
            //ValidAudience = "JwtAuthDemo", // 不驗證就不需要填寫

            // 一般我們都會驗證 Token 的有效期間
            ValidateLifetime = true,

            // 如果 Token 中包含 key 才需要驗證，一般都只有簽章而已
            ValidateIssuerSigningKey = false,

            // "1234567890123456" 應該從 IConfiguration 取得
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JwtSettings:SignKey")))
        };
    });

builder.WebHost.UseKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;  // 100MB
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //設定 Json 回應格式為 PascalCase
    options.JsonSerializerOptions.PropertyNamingPolicy = null;

    //https://docs.microsoft.com/zh-tw/dotnet/api/system.text.json.serialization.jsonignorecondition?view=net-6.0
    //設定值為 NULL 則不顯示
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
});

// 注入 HttpClient
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
        Console.WriteLine($"已連接到資料庫：{databaseName}");

        foreach (var entityType in dbContext.Model.GetEntityTypes())
        {
            Console.WriteLine($"資料表名稱： {entityType.GetTableName()}");
        }

        var users = dbContext.Users.ToList();

        // 印出Users資料表中的所有內容
        Console.WriteLine("Users資料表中的所有內容：");
        foreach (var user in users)
        {
            Console.WriteLine($"Id: {user.Id}, UserID: {user.UserID}, UserPassword: {user.UserPassword}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"連接到資料庫時發生錯誤: {ex.Message}");
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

// 限制未授權的 API 呼叫
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

// Step 1: 要安裝 dotnet-hosting-6.0.22-win.exe 
// >> https://dotnet.microsoft.com/en-us/download/dotnet/6.0
// >> 選擇 ASP.NET Core Runtime 6.0.22 再選 Windows 下 的 Hosting Bundle 
// Step 2: HTTP Error 502.5 - ANCM Out-Of-Process Startup Failure 
// >> 要設定 IIS 
// > 進階設定 > (一般) > 啟動模式 > AlwaysRunning
// > 產生處理序模型事件紀錄項目 > 載入使用者設定檔 > True
// Step 3: appsettings.json
// >> 主機要將連資料庫的, 設定成用 帳號 與 密碼
// >> 此憑證鏈結是由不受信任的授權單位發出的 : local 要多加設定 TrustServerCertificate=true