using CRIEAdvanced.ViewModels;

namespace CRIEAdvanced.Extensions
{
    public class LogsExtension
    {
        private readonly string _folder;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public LogsExtension(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _folder = $"{_hostingEnvironment.WebRootPath}";
        }

        public async Task RecordValidate(HttpContext context, ValidateUser model)
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            if (remoteIp != null)
            {
                if (remoteIp.IsIPv4MappedToIPv6)
                {
                    remoteIp = remoteIp.MapToIPv4();
                }

                await LogValidate(remoteIp.ToString(), model);
            }
        }

        public async Task LogValidate(String _ip, ValidateUser _model)
        {
            string fileName = String.Format("{0}_{1}.txt", "Validate", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            string contents = String.Format("{0}\t{1}\t{2}\r\n", _ip, _model.Account, _model.Password.Length);
            var path = $@"{_folder}/AppLogs/{fileName}";

            await File.WriteAllTextAsync(path, contents);
        }

        public async Task RecordCompute(HttpContext context, String _userID, String _selectedIndexes, String _text)
        {
            var remoteIp = context.Connection.RemoteIpAddress;

            if (remoteIp != null)
            {
                if (remoteIp.IsIPv4MappedToIPv6)
                {
                    remoteIp = remoteIp.MapToIPv4();
                }

                await LogCompute(remoteIp.ToString(), _userID, _selectedIndexes, _text);
            }
        }

        public async Task LogCompute(String _ip, String _userID, String _selectedIndexes, String _text)
        {
            string fileName = String.Format("{0}_{1}.txt", "Compute", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            string contents = String.Format("{0}\t{1}\t{2}\t{3}\r\n", _ip, _userID, _selectedIndexes, _text.Length);
            var path = $@"{_folder}/AppLogs/{fileName}";

            await File.WriteAllTextAsync(path, contents);
        }
    }
}
