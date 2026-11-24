using FinancialSuite.Application.DropDownDetails.Dtos;

namespace FinancialSuite.Application.DropDownCategory.Dtos
{
    public class GetByIdDropDownCategoryDto
    {
        public Guid DropDownCategoryId { get; set; }
        public string Value { get; set; } = string.Empty;
        public List<GetAllDropDownDetailsDto> DropDownDetails { get; set; } = new List<GetAllDropDownDetailsDto>();
    }
}
