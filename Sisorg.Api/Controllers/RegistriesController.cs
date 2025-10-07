using Microsoft.AspNetCore.Mvc;
using Sisorg.Api.Services;
using Sisorg.Api.ViewModels;

namespace Sisorg.Api.Controllers;

/// <summary>
/// Handles registries upload, retrieval and deletion.
/// </summary>
[Route("api/[controller]")]
public class RegistriesController : BaseController
{
    private readonly IRegistryService _service;
    public RegistriesController(IRegistryService service) => _service = service;

    /// <summary>
    /// Upload a TXT file (multipart key: "file") with rows "Name#Value#Color".
    /// Returns the saved registry with ID, Count, Timestamp and Rows.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(10_000_000)]
    [ProducesResponseType(typeof(DataViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DataViewModel>> Upload([FromForm] IFormFile file, CancellationToken ct)
    {
        return Ok(await _service.UploadAsync(file, ct));
    }

    /// <summary>
    /// Get a registry by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DataViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataViewModel>> GetById([FromRoute] int id, CancellationToken ct)
    {
        var vm = await _service.GetAsync(id, ct);
        return vm is null ? ErrorNotFound($"Registry {id} not found.") : Ok(vm);
    }

    /// <summary>
    /// Delete a registry by ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorViewModel), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(id, ct);
        return deleted ? NoContent() : ErrorNotFound($"Registry {id} not found.");
    }
}
