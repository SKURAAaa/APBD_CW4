using System;
using System.ComponentModel.DataAnnotations;

namespace CW4_APBD
{
    public class Warehouse
    {
        [Required] public int IdProduct { get; set; }

        [Required] public int IdWarehouse { get; set; }

        [Required] public int Amount { get; set; }

        public DateTime CreatedAt { get; } = DateTime.Now;
    }
}