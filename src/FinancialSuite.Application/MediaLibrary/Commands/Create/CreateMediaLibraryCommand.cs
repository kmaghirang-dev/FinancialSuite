using MediatR;

namespace FinancialSuite.Application.MediaLibrary.Commands.Create
{
    public class CreateMediaLibraryCommand : IRequest<Guid>
    {
        public Guid CategoryId { get; set; }
        public Guid StatusId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Season { get; set; }
        public int EpisodeNo { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}
