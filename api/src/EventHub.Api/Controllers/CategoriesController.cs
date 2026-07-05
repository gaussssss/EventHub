using EventHub.Domain.ReadModels;
using EventHub.Application.Categories;
using EventHub.Application.Common.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender) => _sender = sender;

    /// <summary>Chips de catégories (GET /api/categories).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(
        CancellationToken cancellationToken)
        => Ok(await _sender.Send(new GetCategoriesQuery(), cancellationToken));
}
