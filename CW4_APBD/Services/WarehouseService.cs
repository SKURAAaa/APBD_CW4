// using APBD3_ASYNC.Models;
// using CW4_APBD.Repository;

using APBD3_ASYNC.Services;
using CW4_APBD;

namespace CW4_APBD
{
    public class WarehouseService : IWarehouseService
    {

        private readonly IWarehouseRepository _repository;

        public WarehouseService(IWarehouseRepository warehouseRepository) 
        { 

            _repository = warehouseRepository;

        }
            
        public async Task<string> AddNewProductQuery(Warehouse warehouse)  
        {
            if (await _repository.VerifyExistingProduct(warehouse) && await _repository.VerifyExistingWarehouse(warehouse))
            {
                if(await _repository.VerifyExistingOrder(warehouse) == false && await _repository.IsThatCompletedOrdersExist(warehouse))
                {
                    var result = await _repository.InsertNewOrder(warehouse);

                    return "Wartość klucza z Product_Warehouse: " + result.ToString();
                }
                else
                {
                    return "Taki order już istnieje";
                }
            }
            else
            {
                return "magazyn lub produkt nie istnieje";
            }
        }


        public async Task<string> AddNewProductByProcedure(Warehouse warehouse)  
        {
            var result = await _repository.ExecStoredProcedure(warehouse);

            return result;

        }


    }
}
