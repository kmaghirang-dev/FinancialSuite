using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinancialSuite.Application.CreditCardPayment.Commands.Create;
using FinancialSuite.Application.CreditCardPayment.Commands.Delete;
using FinancialSuite.Application.CreditCardPayment.Commands.Update;
using FinancialSuite.Application.CreditCardPayment.Queries.GetAllPaginated;
using FinancialSuite.Application.CreditCardPayment.Queries.GetById;

namespace FinancialSuite.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/credit-card-payments")]
    public class CreditCardPaymentController(IMediator mediator) : ControllerBase
    {
        [HttpGet("paginated")]
        public async Task<IActionResult> GetAllPaginated([FromQuery] GetAllPaginatedCreditCardPaymentQuery query)
        {
            return Ok(await mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute]Guid id)
        {
            return Ok(await mediator.Send(new GetByIdCreditCardPaymentQuery(id)));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCreditCardPaymentCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, null);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update([FromRoute]Guid id, UpdateCreditCardPaymentCommand command)
        {
            command.CreditCardPaymentId = id;
            await mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute]Guid id)
        {
            await mediator.Send(new DeleteCreditCardPaymentCommand(id));
            return NoContent();
        }
    }
}
