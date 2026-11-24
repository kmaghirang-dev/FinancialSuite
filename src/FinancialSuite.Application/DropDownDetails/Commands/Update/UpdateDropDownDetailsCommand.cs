using MediatR;

namespace FinancialSuite.Application.DropDownDetails.Commands.Update
{
    public class UpdateDropDownDetailsCommand : IRequest
    {
        public Guid DropDownDetailId { get; set; }
        public Guid DropDownCategoryId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DisplayValue { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
