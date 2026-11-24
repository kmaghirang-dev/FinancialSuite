namespace FinancialSuite.Application.MediaLibrary.Dtos
{
    public class GetAllMediaLibraryDto
    {
        public Guid MediaId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid StatusId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Season { get; set; }
        public int EpisodeNo { get; set; }
        public string Details { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public string DateUpdated { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
