using MediatR;

namespace FinancialSuite.Application.CreditCard.Commands.Toggle
{
    // IsActive = false is the soft delete (the DELETE endpoint sends it): history is kept but the card leaves lists and the combined overview.
    public class ToggleCreditCardCommand : IRequest
    {
        public Guid CreditCardId { get; set; }
        public bool IsActive { get; set; }
    }
}
