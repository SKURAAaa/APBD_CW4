// using CW4_APBD.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using APBD3_ASYNC.Services;
using static CW4_APBD.IWarehouseRepository;

namespace CW4_APBD
{
    [Route("api/warehouse")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost("AddNewProduct")]
        public async Task<IActionResult> AddNewProduct(Warehouse warehouse)
        {
            // Walidacja czy ilość przekazana w żądaniu jest większa od zera
            if (warehouse.Amount <= 0)
            {
                return BadRequest("Amount musi być większe od zera");
            }

            var result = await _warehouseService.AddNewProductQuery(warehouse);
            return Ok(result);
        }

        [HttpPost("AddNewProductByProcedure")]
        public async Task<IActionResult> AddNewProductByProcedure(Warehouse warehouse)
        {
            var result = await _warehouseService.AddNewProductByProcedure(warehouse);
            return Ok(result);
        }
    }
}