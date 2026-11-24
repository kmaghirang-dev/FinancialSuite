using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialSuite.Application.MediaLibrary.Commands.Create;
using FinancialSuite.Application.MediaLibrary.Commands.Delete;
using FinancialSuite.Application.MediaLibrary.Commands.Toggle;
using FinancialSuite.Application.MediaLibrary.Commands.Update;
using FinancialSuite.Application.MediaLibrary.Queries.GetAll;
using FinancialSuite.Application.MediaLibrary.Queries.GetAllPaginated;
using FinancialSuite.Application.MediaLibrary.Queries.GetById;

namespace FinancialSuite.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/media-library")]
    public class MediaLibraryController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await mediator.Send(new GetAllMediaLibraryQuery()));
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetAllPaginated([FromQuery] GetAllPaginatedMediaLibraryQuery query)
        {
            return Ok(await mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            return Ok(await mediator.Send(new GetByIdMediaLibraryQuery(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMediaLibraryCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update([FromRoute]Guid id, UpdateMediaLibraryCommand command)
        {
            command.MediaId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle([FromRoute]Guid id, ToggleMediaLibraryCommand command)
        {
            command.MediaId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]Guid id)
        {
            await mediator.Send(new DeleteMediaLibraryCommand(id));
            return NoContent();
        }
    }
}
