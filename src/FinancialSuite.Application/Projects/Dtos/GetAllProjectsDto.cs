namespace FinancialSuite.Application.Projects.Dtos
{
    public class GetAllProjectsDto
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string DateUpdated { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
