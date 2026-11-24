using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialSuite.Application.CreditCard.Commands.Create;
using FinancialSuite.Application.CreditCard.Commands.Toggle;
using FinancialSuite.Application.CreditCard.Commands.Update;
using FinancialSuite.Application.CreditCard.Queries.GetAll;
using FinancialSuite.Application.CreditCard.Queries.GetAllPaginated;
using FinancialSuite.Application.CreditCard.Queries.GetById;
using FinancialSuite.Application.CreditCard.Queries.GetOverview;

namespace FinancialSuite.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/credit-cards")]
    public class CreditCardController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCreditCardQuery query)
        {
            return Ok(await mediator.Send(query));
        }

        [HttpGet("paginated")]
        public async Task<IActionResult> GetAllPaginated([FromQuery] GetAllPaginatedCreditCardQuery query)
        {
            return Ok(await mediator.Send(query));
        }

        // Totals, per-card statement periods and chart series, all from the same calculation.
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview([FromQuery] GetCreditCardOverviewQuery query)
        {
            return Ok(await mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            return Ok(await mediator.Send(new GetByIdCreditCardQuery(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCreditCardCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update([FromRoute]Guid id, UpdateCreditCardCommand command)
        {
            command.CreditCardId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpPatch("toggle/{id}")]
        public async Task<IActionResult> Toggle([FromRoute]Guid id, ToggleCreditCardCommand command)
        {
            command.CreditCardId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]Guid id)
        {
            await mediator.Send(new ToggleCreditCardCommand { CreditCardId = id, IsActive = false });
            return NoContent();
        }
    }
}
