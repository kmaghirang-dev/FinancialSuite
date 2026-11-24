using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialSuite.Application.CreditCardStatement.Commands.Create;
using FinancialSuite.Application.CreditCardStatement.Commands.Delete;
using FinancialSuite.Application.CreditCardStatement.Commands.Update;
using FinancialSuite.Application.CreditCardStatement.Queries.GetById;
using FinancialSuite.Application.CreditCardStatement.Queries.GetPreview;

namespace FinancialSuite.API.Controllers
{
    // Statement lists with their calculated periods come from GET api/credit-cards/overview.
    [ApiController]
    [Authorize]
    [Route("api/credit-card-statements")]
    public class CreditCardStatementController(IMediator mediator) : ControllerBase
    {
        [HttpGet("preview")]
        public async Task<IActionResult> GetPreview([FromQuery] GetCreditCardStatementPreviewQuery query)
        {
            return Ok(await mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            return Ok(await mediator.Send(new GetByIdCreditCardStatementQuery(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCreditCardStatementCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update([FromRoute]Guid id, UpdateCreditCardStatementCommand command)
        {
            command.CreditCardStatementId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]Guid id)
        {
            await mediator.Send(new DeleteCreditCardStatementCommand(id));
            return NoContent();
        }
    }
}
