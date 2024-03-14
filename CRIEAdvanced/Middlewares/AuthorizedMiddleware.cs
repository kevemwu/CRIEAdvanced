namespace CRIEAdvanced.Middlewares
{
    /// <summary>權限驗證中介程序</summary>
    /// https://gist.github.com/poychang/60570f178dfb1e4566b45b5b83589b01#file-authorizedmiddleware-cs
    public class AuthorizedMiddleware
    {
        private readonly string _folder;
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly RequestDelegate _next;

        public AuthorizedMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnvironment)
        {
            _next = next;

            _hostingEnvironment = hostingEnvironment;
            _folder = $"{_hostingEnvironment.WebRootPath}";
        }

        public async Task Invoke(HttpContext context)
        {
            if (context != null)
            {
                var remoteIp = context.Connection.RemoteIpAddress;

                if (remoteIp != null)
                {
                    if (remoteIp.IsIPv4MappedToIPv6)
                    {
                        remoteIp = remoteIp.MapToIPv4();
                    }

                    if (!context.Request.Path.StartsWithSegments("/CRIE"))
                    {
                        await LogErrMsg(context.Request.Path, remoteIp.ToString());

                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        return;
                    }

                    await _next.Invoke(context);
                }
            }
        }
        public async Task LogErrMsg(String _path, String _ip)
        {
            string fileName = String.Format("{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            string contents = String.Format("{0}\t{1}\r\n", _path, _ip);
            var path = $@"{_folder}/AppLogs/{fileName}";

            await File.WriteAllTextAsync(path, contents);
        }
    }

    /// <summary>權限驗證中介程序的擴充方法</summary>
    public static class MyAuthorizeExtensions
    {
        /// <summary>驗證呼叫 API 的條件</summary>
        /// <param name="builder">中介程序建構器</param>
        public static IApplicationBuilder UseAuthorized(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizedMiddleware>();
        }
    }
}
