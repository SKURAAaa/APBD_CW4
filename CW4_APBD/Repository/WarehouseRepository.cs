using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Emit;


namespace CW4_APBD
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly IConfiguration _configuration;

        public WarehouseRepository(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> IsThatCompletedOrdersExist(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);

            await connection.OpenAsync();

            await using var command = new SqlCommand();
            command.Connection = connection;

            command.CommandText = " IF EXISTS " +
                                  "(select * from [ApbdAsync].[dbo].[Product_Warehouse] " +
                                  "where IdOrder = (Select IdOrder from [ApbdAsync].[dbo].[Order] where IdProduct = @IdProduct and Amount = @Amount)) " +
                                  "BEGIN " +
                                  "SELECT 1 " +
                                  "END " +
                                  "ELSE " +
                                  "BEGIN " +
                                  "SELECT 2 " +
                                  "END";

            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);

            var queryResult = await command.ExecuteScalarAsync();

            var result = (int)queryResult == 2;

            return result;
        }

        public async Task<bool> VerifyExistingOrder(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);

            await connection.OpenAsync();

            await using var command = new SqlCommand();

            command.Connection = connection;

            command.CommandText = "IF EXISTS " +
                                  "(SELECT * FROM [ApbdAsync].[dbo].[Order] " +
                                  "WHERE IdProduct = @IdProduct " +
                                  "AND Amount = @Amount " +
                                  "AND CreatedAt < GETDATE()) " +
                                  "BEGIN " +
                                  "SELECT 1 " +
                                  "END " +
                                  "ELSE " +
                                  "BEGIN " +
                                  "SELECT 2 " +
                                  "END";

            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);

            var queryResult = await command.ExecuteScalarAsync();

            var result = (int)queryResult == 1;

            return result;
        }

        public async Task<bool> VerifyExistingProduct(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);

            await connection.OpenAsync();

            await using var command = new SqlCommand();

            command.Connection = connection;

            command.CommandText = " IF EXISTS " +
                                  "(select * from [ApbdAsync].[dbo].[Product] where IdProduct = @IdProduct) " +
                                  "BEGIN   " +
                                  "SELECT 1 " +
                                  "END " +
                                  "ELSE " +
                                  "BEGIN    " +
                                  "SELECT 2 " +
                                  "END";

            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);

            var queryResult = await command.ExecuteScalarAsync();

            var result = (int)queryResult == 1;

            return result;
        }

        public async Task<bool> VerifyExistingWarehouse(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);

            await connection.OpenAsync();

            await using var command = new SqlCommand();

            command.Connection = connection;

            command.CommandText = " IF EXISTS " +
                                  "(select * from [ApbdAsync].[dbo].[Warehouse] where IdWarehouse = @IdWarehouse) " +
                                  "BEGIN " +
                                  " SELECT 1 " +
                                  "END " +
                                  "ELSE " +
                                  "BEGIN " +
                                  "SELECT 2 " +
                                  "END";

            command.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);

            var queryResult = await command.ExecuteScalarAsync();

            var result = (int)queryResult == 1;

            return result;
        }

        private async Task UpdateFullfilledAt(DateTime createdAt, decimal orderId)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);

            await connection.OpenAsync();

            await using var command = new SqlCommand();
            command.Connection = connection;

            command.CommandText =
                "UPDATE [ApbdAsync].[dbo].[Order] set FulfilledAt = @createdAt where IdOrder = @orderId";

            command.Parameters.AddWithValue("@createdAt", createdAt);
            command.Parameters.AddWithValue("@orderId", orderId);

            await command.ExecuteNonQueryAsync();

            Console.WriteLine("UpdateFullfilledAt executed");
        }

        public async Task<decimal> InsertNewOrder(Warehouse warehouse)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);

            await connection.OpenAsync();

            await using var command = new SqlCommand();

            command.Connection = connection;

            command.CommandText =
                "INSERT INTO[ApbdAsync].[dbo].[Order]([IdProduct], [Amount], [CreatedAt], [FulfilledAt]) " +
                "VALUES(@IdProduct, @Amount, @CreatedAt, null); " +
                "SELECT SCOPE_IDENTITY()";

            command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
            command.Parameters.AddWithValue("@Amount", warehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);


            var orderIdentity = await command.ExecuteScalarAsync();

            await UpdateFullfilledAt(warehouse.CreatedAt, (decimal)orderIdentity);

            command.CommandText = "select [Price] from [ApbdAsync].[dbo].[Product] where [IdProduct] = @IdProduct";

            var productPrice = await command.ExecuteScalarAsync();

            command.CommandText =
                "INSERT INTO [ApbdAsync].[dbo].[Product_Warehouse] ([IdWarehouse],[IdProduct],[IdOrder],[Amount],[Price],[CreatedAt]) " +
                "VALUES (@IdWarehouse, @IdProduct,@IdOrder,@Amount,@Price,@CreatedAt); " +
                "SELECT SCOPE_IDENTITY()";

            command.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
            command.Parameters.AddWithValue("@IdOrder", orderIdentity);
            command.Parameters.AddWithValue("@Price", warehouse.Amount * (decimal)productPrice);

            var idProductWarehouse = await command.ExecuteScalarAsync();

            return (decimal)idProductWarehouse;
        }

        public async Task<string> ExecStoredProcedure(Warehouse warehouse)
        {
            try
            {
                await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);

                await using var command = new SqlCommand("AddProductToWarehouse", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@IdProduct", warehouse.IdProduct);
                command.Parameters.AddWithValue("@IdWarehouse", warehouse.IdWarehouse);
                command.Parameters.AddWithValue("@Amount", warehouse.Amount);
                command.Parameters.AddWithValue("@CreatedAt", warehouse.CreatedAt);

                await connection.OpenAsync();

                var result = await command.ExecuteScalarAsync();

                return result.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}