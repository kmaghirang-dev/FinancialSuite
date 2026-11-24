using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;

namespace FinancialSuite.Application.DropDownDetails.Commands.Update
{
    public class UpdateDropDownDetailsCommandHandler(
        ILogger<UpdateDropDownDetailsCommandHandler> logger,
        IMapper mapper,
        IDropDownCategoryRepository categoryRepository,
        IDropDownDetailsRepository detailsRepository) : IRequestHandler<UpdateDropDownDetailsCommand>
    {
        public async Task Handle(UpdateDropDownDetailsCommand request, CancellationToken cancellationToken)
        {
            var detailId = request.DropDownDetailId;
            var categoryId = request.DropDownCategoryId;

            var data = await categoryRepository.GetByIdAsync(categoryId);

            if (data == null) throw new NotFoundException(nameof(DropDownCategory), categoryId.ToString());

            var details = data.DropDownDetails?.FirstOrDefault(x => x.DropDownDetailId == detailId);

            if (details == null) throw new NotFoundException(nameof(DropDownDetails), detailId.ToString());

            mapper.Map(request, details);

            await detailsRepository.SaveChanges();
        }
    }
}
