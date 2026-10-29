using System.ComponentModel.DataAnnotations;

namespace NorthwindApi.Models;

// One product. It's finished.
public class Product
{
    public int ProductId { get; set; }

    [Required]
    [StringLength(40)]
    public string ProductName { get; set; } = "";

    public int? CategoryId { get; set; }

    [Range(0, 10000)]
    public decimal? UnitPrice { get; set; }

    [Range(0, 32767)]
    public short? UnitsInStock { get; set; }

    public bool Discontinued { get; set; }
}
