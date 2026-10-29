using Microsoft.AspNetCore.Mvc;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Controllers;

// The products API.
//
// Requirements:
// - GET /api/products answers 200 with every product. GET /api/products?categoryId=1 answers with only
//   that category's products.
// - GET /api/products/{id} answers 200 with the product, or 404 Not Found if there's no such product.
// - POST /api/products adds the product in the body and answers 201 Created, with the new product
//   (and its new id) in the body and its address in the Location header. A product that breaks the
//   Product class's validation rules answers 400 Bad Request without being saved.
// - PUT /api/products/{id} replaces the product's values with the body's, and answers 204 No Content,
//   or 404 Not Found if there's no such product.
// - DELETE /api/products/{id} deletes the product and answers 204 No Content, or 404 Not Found if
//   there's no such product.
// - Every action is async, and uses the IProductDao it's given.
//
// Securing it (Practice: Secure the App):
// - Anyone may GET the products.
// - Adding (POST) and changing (PUT) a product need a signed-in user: no token or a bad one answers 401.
// - Deleting a product needs the role Admin: a signed-in user who isn't an admin gets 403.
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductDao productDao;

    public ProductsController(IProductDao productDao)
    {
        this.productDao = productDao;
    }

    [HttpGet]
    public async Task<List<Product>> GetAll(int? categoryId)
    {
        return await productDao.GetAllAsync(categoryId);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        Product? product = await productDao.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return product;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Add(Product product)
    {
        Product added = await productDao.AddAsync(product);
        return CreatedAtAction(nameof(GetById), new { id = added.ProductId }, added);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        if (!await productDao.UpdateAsync(id, product))
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await productDao.DeleteAsync(id))
        {
            return NotFound();
        }
        return NoContent();
    }
}
