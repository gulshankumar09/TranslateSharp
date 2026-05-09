using Microsoft.AspNetCore.Mvc;
using TranslateSharp.Filters;

namespace TranslateSharp.SampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    [TranslateResponse]
    public ActionResult<ProductDto> GetProduct(int id)
    {
        return Ok(new ProductDto
        {
            Id = id,
            Name = "Sample Product",
            Description = "This is a sample product description that will be translated.",
            InternalNotes = "This should NOT be translated"
        });
    }

    [HttpGet]
    [TranslateResponse]
    public ActionResult<List<ProductDto>> GetProducts()
    {
        return Ok(new List<ProductDto>
        {
            new() { Id = 1, Name = "Product One", Description = "First product description" },
            new() { Id = 2, Name = "Product Two", Description = "Second product description" }
        });
    }
}