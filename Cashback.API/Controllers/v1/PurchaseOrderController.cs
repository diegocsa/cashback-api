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
    public class PurchaseOrderController : ControllerBase
    {
        private readonly ILogger<PurchaseOrderController> _logger;
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrderController(IPurchaseOrderService purchaseOrderService, ILoggerFactory logFactory)
        {
            _logger = logFactory.CreateLogger<PurchaseOrderController>();
            _purchaseOrderService = purchaseOrderService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(NewPurchaseOrderModel model)
        {
            try
            {
                _logger.LogInformation("New Purchase");
                var item = await _purchaseOrderService.Add(model);
                return StatusCode(StatusCodes.Status201Created, TinyMapper.Map<PurchaseOrderModel>(item));
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] string cpf, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                _logger.LogInformation("Get Purchases");
                var items = _purchaseOrderService.Get(cpf, start, end);
                return StatusCode(StatusCodes.Status201Created, items);
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
