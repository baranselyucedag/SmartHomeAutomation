using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHomeAutomation.API.Services;

namespace SmartHomeAutomation.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityTestService _securityTestService;

        public SecurityController(ISecurityTestService securityTestService)
        {
            _securityTestService = securityTestService;
        }

        [HttpGet("report")]
        public async Task<ActionResult<string>> GetSecurityReport()
        {
            var report = await _securityTestService.GetSecurityReportAsync();
            return Ok(report);
        }

        [HttpPost("test-sql-injection")]
        public async Task<ActionResult<bool>> TestSqlInjection([FromBody] string input)
        {
            var isProtected = await _securityTestService.TestSqlInjectionProtectionAsync(input);
            return Ok(new { IsProtected = isProtected, Message = isProtected ? "SQL Injection koruması aktif" : "SQL Injection koruması başarısız" });
        }

        [HttpGet("csrf-token")]
        [AllowAnonymous]
        public ActionResult GetCsrfToken()
        {
            var token = Request.Cookies["CSRF-Token"];
            return Ok(new { Token = token });
        }
    }
} 