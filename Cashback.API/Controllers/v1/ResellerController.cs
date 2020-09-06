using Cashback.Domain.Exceptions;
using Cashback.Domain.Interfaces.Service;
using Cashback.Domain.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nelibur.ObjectMapper;
using System;
using System.Threading.Tasks;

namespace Cashback.API.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ResellerController : ControllerBase
    {
        private readonly ILogger<ResellerController> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly IResellerService _resellerService;

        public ResellerController(IAuthenticationService authenticationService, IResellerService resellerService, ILoggerFactory logFactory)
        {
            _logger = logFactory.CreateLogger<ResellerController>();
            _authenticationService = authenticationService;
            _resellerService = resellerService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                _logger.LogInformation("Login");
                var token = await _authenticationService.Login(model);
                return Ok(new { token });
            }
            catch (CashbackServiceException ex)
            {
                _logger.LogWarning($"BadRequest - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error - {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("accumulated")]
        public async Task<IActionResult> GetAccumulated([FromQuery] string cpf)
        {
            try
            {
                _logger.LogInformation("GetAccumulated");
                var credit = await _resellerService.GetAccumulated(cpf);
                return Ok(new { credit });
            }
            catch (CashbackServiceException ex)
            {
                _logger.LogWarning($"BadRequest - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error - {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] NewResellerModel model)
        {
            try
            {
                _logger.LogInformation("NewReseller");
                var item = await _resellerService.Add(model);
                return StatusCode(StatusCodes.Status201Created, TinyMapper.Map<ResellerModel>(item));
            }
            catch (CashbackServiceException ex)
            {
                _logger.LogWarning($"BadRequest - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error - {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

        }
    }
}
