using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinancialSuite.Application.DropDownCategory.Commands.Create;
using FinancialSuite.Application.DropDownCategory.Commands.Delete;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;
using FinancialSuite.Application.DropDownCategory.Commands.Update;
using FinancialSuite.Application.DropDownCategory.Queries.GetAll;
using FinancialSuite.Application.DropDownCategory.Queries.GetAllPaginated;
using FinancialSuite.Application.DropDownCategory.Queries.GetById;
using FinancialSuite.Domain.Constants;

namespace FinancialSuite.API.Controllers
{
    [ApiController]
    [Authorize(Roles = UserRoles.SuperAdmin)]
    [Route("api/dropdown/category")]
    public class DropDownCategoryController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await mediator.Send(new GetAllDropDownCategoryQuery()));
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetAllPaginated([FromQuery] GetAllPaginatedDropDownCategoryQuery query)
        {
            return Ok(await mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            var res = await mediator.Send(new GetByIdDropDownCategoryQuery(id));
            if (res is null) return NotFound();
            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDropDownCategoryCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update([FromRoute]Guid id, UpdateDropDownCategoryCommand command)
        {
            command.DropDownCategoryId = id;
            await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = command.DropDownCategoryId }, null);
        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle([FromRoute]Guid id, ToggleDropDownCategoryCommand command)
        {
            command.DropDownCategoryId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeleteDropDownCategoryCommand(id));
            return NoContent();
        }
    }
}