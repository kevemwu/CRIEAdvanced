using CRIEAdvanced.Data;
using CRIEAdvanced.Extensions;
using CRIEAdvanced.Helpers;
using CRIEAdvanced.Models;
using CRIEAdvanced.Repository;
using CRIEAdvanced.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CRIEAdvanced.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class CRIEController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IJWTManagerRepository _jWTManager;
        private readonly RsaHelper _rsaHelper;
        private readonly LogsExtension _logs;
        private readonly ToolExtension _tool;
        private readonly VerifyLogicExtension _verifyLogic;

        private readonly UsersRepository _usersRepository;
        private readonly IUserRefreshTokensRepository _userRefreshTokensRepository;

        public CRIEController(IConfiguration configuration, IJWTManagerRepository jWTManager,
            RsaHelper rsaHelper, LogsExtension logs, ToolExtension tool,
            VerifyLogicExtension verifyLogic,
            UsersRepository usersRepository, IUserRefreshTokensRepository userRefreshTokensRepository)
        {
            this._configuration = configuration;
            this._jWTManager = jWTManager;
            this._rsaHelper = rsaHelper;
            this._logs = logs;
            this._tool = tool;
            this._verifyLogic = verifyLogic;

            this._usersRepository = usersRepository;
            this._userRefreshTokensRepository = userRefreshTokensRepository;
        }

        /// <summary>
        /// 先進行 帳號、密碼驗證 成功後, 取得 Bearer Token
        /// </summary>
        [AllowAnonymous]
        [HttpPost("Validate", Name = nameof(Validate))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Validate(ValidateUser model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (model == null)
                    return BadRequest($"Missing required content");

                await this._logs.RecordValidate(HttpContext, model);

                var result = await _usersRepository.ValidateUser(model);
                if (result != null)
                {
                    String userIP = this._tool.GetUserIP(HttpContext);

                    var tmpUserIP = _verifyLogic.SetTokenStr(userIP);
                    var tmpUserID = _verifyLogic.SetTokenStr(result.Id.ToString());
                    if (tmpUserIP != null && tmpUserID != null)
                    {
                        var tokens = _jWTManager.GenerateToken(tmpUserIP, tmpUserID);
                        if (tokens != null)
                        {
                            var entity = new UserRefreshTokens
                            {
                                UserIP = userIP,
                                UserId = result.Id,
                                RefreshToken = tokens.RefreshToken
                            };

                            await _userRefreshTokensRepository.Create(entity);

                            var publicKey = _configuration.GetValue<string>("RsaSettings:PublicKey")!;
                            String refreshTokenEncrypt = _rsaHelper.RsaEncrypt(publicKey, tokens.RefreshToken);
                            if (refreshTokenEncrypt != "WrongDataCrypt")
                            {
                                tokens.RefreshToken = refreshTokenEncrypt;
                                return Ok(tokens);
                            }
                        }
                    }
                }

                return Unauthorized("Invalid ID or password. Please try again");
            }
            catch (ValidationException ex)
            {
                return BadRequest($"Validation error: " + ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken", Name = nameof(RefreshToken))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> RefreshToken(Tokens model)
        {
            //取出舊的Token
            var principal = _jWTManager.GetPrincipalFromExpiredToken(model.AccessToken);

            var nameClaim = principal.Claims.FirstOrDefault(c => c.Type == "name")?.Value;

            (var userIP, var userID) = this._verifyLogic.GetTokenInfo(principal.Identity!.IsAuthenticated, 
                principal.FindFirstValue(ClaimTypes.NameIdentifier), nameClaim!);
            if (userIP.Length == 0 || userID.Length == 0)
                return BadRequest("Invalid attempt!");

            if (!int.TryParse(userID, out int parsedInt))
                return BadRequest("Invalid attempt!");

            //將舊的RefreshToken解密
            var privateKey = _configuration.GetValue<string>("RsaSettings:PrivateKey");
            String refreshTokenDecrypt = _rsaHelper.RsaDecrypt(privateKey, model.RefreshToken);
            if (refreshTokenDecrypt == "WrongDataCrypt")
                return BadRequest("Invalid attempt!");

            //將舊的Token, RefreshToken與資料庫比對
            var oldEntity = new UserRefreshTokens
            {
                UserIP = userIP,
                UserId = int.Parse(userID),
                RefreshToken = refreshTokenDecrypt
            };

            var savedRefreshToken = await _userRefreshTokensRepository.Read(oldEntity);
            if (savedRefreshToken == null)
                return BadRequest("Invalid attempt!");

            //將新的Token加密
            var tmpUserIP = _verifyLogic.SetTokenStr(this._tool.GetUserIP(HttpContext));
            var tmpUserID = _verifyLogic.SetTokenStr(userID);
            if (tmpUserIP == null || tmpUserID == null)
                return BadRequest("Invalid attempt!");

            //產出新的Token, RefreshToken
            var tokens = _jWTManager.GenerateRefreshToken(tmpUserIP, tmpUserID);
            if (tokens == null)
                return BadRequest("Invalid attempt!");

            //更新資料庫
            var newEntity = new UserRefreshTokens
            {
                UserIP = this._tool.GetUserIP(HttpContext),
                UserId = int.Parse(userID),
                RefreshToken = tokens.RefreshToken
            };

            await _userRefreshTokensRepository.Update(oldEntity);
            await _userRefreshTokensRepository.Create(newEntity);

            //將新的RefreshToken加密
            var publicKey = _configuration.GetValue<string>("RsaSettings:PublicKey");
            String refreshTokenEncrypt = _rsaHelper.RsaEncrypt(publicKey, tokens.RefreshToken);
            if (refreshTokenEncrypt == "WrongDataCrypt")
                return BadRequest("Invalid attempt!");

            tokens.RefreshToken = refreshTokenEncrypt;
            return Ok(tokens);
        }
    }
}
