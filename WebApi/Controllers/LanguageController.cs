using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/languages")]
    [ApiController]
    public class LanguageController : ControllerBase
    {
        private readonly ILanguageService _lang;

        public LanguageController(ILanguageService lang)
        {
            _lang = lang;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var res = _lang.GetArray([
                "Welcome",
                "LoginToContinue",
                "Username",
                "Password",
                "Login",
                "Register1",
                "Register2",
                "ForgotPassword1",
                "ForgotPassword2",
            ]);

            return ApiResponse.Success(res);
        }
    }
}
