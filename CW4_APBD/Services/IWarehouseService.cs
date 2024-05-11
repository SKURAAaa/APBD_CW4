using CW4_APBD;
using static CW4_APBD.IWarehouseRepository;

namespace APBD3_ASYNC.Services
{
    public interface IWarehouseService
    {
        Task<string> AddNewProductQuery(Warehouse warehouse);

        Task<string> AddNewProductByProcedure(Warehouse warehouse);
    }
}