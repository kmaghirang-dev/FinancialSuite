using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FinancialSuite.Application.DropDownCategory.Commands.Toggle;
using FinancialSuite.Application.DropDownDetails.Commands.Create;
using FinancialSuite.Application.DropDownDetails.Commands.DeleteAllByCategory;
using FinancialSuite.Application.DropDownDetails.Commands.DeleteById;
using FinancialSuite.Application.DropDownDetails.Commands.Toggle;
using FinancialSuite.Application.DropDownDetails.Commands.Update;
using FinancialSuite.Application.DropDownDetails.Queries.GetAllByCategoryId;
using FinancialSuite.Application.DropDownDetails.Queries.GetAllByCategoryValue;
using FinancialSuite.Application.DropDownDetails.Queries.GetAllPaginated;
using FinancialSuite.Application.DropDownDetails.Queries.GetById;
using FinancialSuite.Domain.Constants;

namespace FinancialSuite.API.Controllers
{
    // Managing details is SuperAdmin-only like DropDownCategoryController; GetAllByCategory feeds every page's selects, so any signed-in user may call it.
    [Route("api/dropdown/category/{id}/details")]
    [ApiController]
    [Authorize]
    public class DropDownDetailsController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Create(Guid id, CreateDropDownDetailsCommand command)
        {
            command.DropDownCategoryId = id;
            var detailId = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id , detailId}, null);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllByCategoryId(Guid id)
        {
            var data = await mediator.Send(new GetDropDownDetailsByCategoryIdQuery(id));
            return Ok(data);
        }

        [HttpGet("~/api/dropdown/details/{category}")]
        public async Task<IActionResult> GetAllByCategory(string category)
        {
            var data = await mediator.Send(new GetDropDownDetailsByCategoryValueQuery(category));
            return Ok(data);
        }

        [HttpGet("paginated")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetAllPaginated(Guid id, [FromQuery] GetAllPaginatedDropDownDetailsQuery query)
        {
            query.DropDownCategoryId = id;
            var data = await mediator.Send(query);
            return Ok(data);
        }

        [HttpGet("{detailId}")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> GetById(Guid id, Guid detailId)
        {
            var data = await mediator.Send(new GetByIdDropDownDetailsQuery(id, detailId));
            return Ok(data);
        }

        [HttpDelete]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeleteAllDropDownDetailsByCategoryCommand(id));
            return NoContent();
        }

        [HttpDelete("{detailId}")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteById(Guid id, Guid detailId)
        {
            await mediator.Send(new DeleteByIdDropDownDetailsCommand(id, detailId));
            return NoContent();
        }

        [HttpPatch("{detailId}")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Update(Guid id, Guid detailId, UpdateDropDownDetailsCommand command)
        {
            command.DropDownCategoryId = id;
            command.DropDownDetailId = detailId;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpPatch("toggle/{detailId}")]
        [Authorize(Roles = UserRoles.SuperAdmin)]
        public async Task<IActionResult> Toggle(Guid id, Guid detailId, ToggleDropDownDetailsCommand command)
        {
            command.DropDownCategoryId = id;
            command.DropDownDetailId = detailId;
            await mediator.Send(command);
            return NoContent();
        }
    }
}