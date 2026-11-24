namespace FinancialSuite.Application.DropDownCategory.Dtos
{
    public class GetAllDropDownCategoryDto
    {
        public Guid DropDownCategoryId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string DateUpdated { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
