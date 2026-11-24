using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialSuite.Application.Projects.Commands.Create;
using FinancialSuite.Application.Projects.Commands.Delete;
using FinancialSuite.Application.Projects.Commands.Toggle;
using FinancialSuite.Application.Projects.Commands.Update;
using FinancialSuite.Application.Projects.Queries.GetAll;
using FinancialSuite.Application.Projects.Queries.GetAllPaginated;
using FinancialSuite.Application.Projects.Queries.GetById;

namespace FinancialSuite.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/projects")]
    public class ProjectsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await mediator.Send(new GetAllProjectsQuery()));
        }

        [HttpGet("paginated")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPaginated([FromQuery] GetAllPaginatedProjectsQuery query)
        {
            return Ok(await mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            return Ok(await mediator.Send(new GetByIdProjectQuery(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProjectCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update([FromRoute]Guid id, UpdateProjectCommand command)
        {
            command.ProjectId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle([FromRoute]Guid id, ToggleProjectCommand command)
        {
            command.ProjectId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]Guid id)
        {
            await mediator.Send(new DeleteProjectCommand(id));
            return NoContent();
        }
    }
}
