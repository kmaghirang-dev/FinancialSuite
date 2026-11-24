using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using FinancialSuite.Application.Common;
using FinancialSuite.Application.CreditCardPayment.Commands.Create;
using FinancialSuite.Application.CreditCardPayment.Commands.Update;
using FinancialSuite.Application.CreditCardPayment.Dtos;
using FinancialSuite.Application.CreditCardPayment.Queries.GetAllPaginated;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.Application.Tests.CreditCardPayment;

[TestSubject(typeof(CreateCreditCardPaymentCommandHandler))]
public class CreditCardPaymentHandlersTest
{
    private readonly Mock<ICreditCardRepository> _cardRepositoryMock = new();
    private readonly Mock<ICreditCardPaymentRepository> _repositoryMock = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<CreditCardPaymentProfile>()).CreateMapper();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Domain.Entities.CreditCard _card = new() { CreditCardId = Guid.NewGuid(), Name = "BPI", InitialBalanceDate = new DateOnly(2026, 9, 30) };

    public CreditCardPaymentHandlersTest()
    {
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(new CurrentUser("1", "user@example.com", [], null));
        _cardRepositoryMock.Setup(repo => repo.GetByIdAsync(_card.CreditCardId)).ReturnsAsync(_card);
    }

    private CreateCreditCardPaymentCommandHandler CreateHandler()
        => new(new Mock<ILogger<CreateCreditCardPaymentCommandHandler>>().Object, _mapper, _userContextMock.Object, _repositoryMock.Object, _cardRepositoryMock.Object);

    [Fact]
    public async Task Create_ForValidPayment_SavesItWithTrimmedText()
    {
        // Arrange
        Domain.Entities.CreditCardPayment created = null;
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.CreditCardPayment>()))
            .Callback<Domain.Entities.CreditCardPayment>(payment => created = payment)
            .ReturnsAsync(Guid.NewGuid());

        // Act
        await CreateHandler().Handle(new CreateCreditCardPaymentCommand
        {
            CreditCardId = _card.CreditCardId,
            PaymentDate = new DateOnly(2026, 10, 15),
            Amount = 10_000.50m,
            PaymentReference = " REF-1 ",
            Notes = null
        }, CancellationToken.None);

        // Assert
        created!.Amount.Should().Be(10_000.50m);
        created.PaymentReference.Should().Be("REF-1");
        created.Notes.Should().BeEmpty();
        created.CreatedBy.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Create_OnOrBeforeTheInitialBalanceDate_IsStillSaved()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.CreditCardPayment>())).ReturnsAsync(Guid.NewGuid());

        // Act
        await CreateHandler().Handle(new CreateCreditCardPaymentCommand { CreditCardId = _card.CreditCardId, PaymentDate = _card.InitialBalanceDate.AddDays(-3), Amount = 1m }, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(repo => repo.CreateAsync(It.Is<Domain.Entities.CreditCardPayment>(p => p.PaymentDate == _card.InitialBalanceDate.AddDays(-3))), Times.Once);
    }

    [Fact]
    public async Task GetAllPaginated_PaymentOnOrBeforeTheInitialBalanceDate_IsFlaggedAndCountsTowardNoStatement()
    {
        // Arrange
        var statement = new Domain.Entities.CreditCardStatement { CreditCardStatementId = Guid.NewGuid(), CreditCardId = _card.CreditCardId, PeriodStartDate = new DateOnly(2026, 10, 1), PeriodEndDate = new DateOnly(2026, 10, 31), ClosingBalance = 1m };
        _card.Statements.Add(statement);
        var early = new Domain.Entities.CreditCardPayment { CreditCardPaymentId = Guid.NewGuid(), CreditCardId = _card.CreditCardId, CreditCard = _card, PaymentDate = _card.InitialBalanceDate, Amount = 1m };
        var counted = new Domain.Entities.CreditCardPayment { CreditCardPaymentId = Guid.NewGuid(), CreditCardId = _card.CreditCardId, CreditCard = _card, PaymentDate = new DateOnly(2026, 10, 15), Amount = 1m };
        _repositoryMock.Setup(repo => repo.GetAllMatchingAsync(null, null, null, null, 10, 1, null, It.IsAny<SortDirection?>()))
            .ReturnsAsync((new[] { early, counted }, 2));
        _cardRepositoryMock.Setup(repo => repo.GetWithHistoryByIdsAsync(It.IsAny<IEnumerable<Guid>>())).ReturnsAsync(new[] { _card });
        var handler = new GetAllPaginatedCreditCardPaymentQueryHandler(new Mock<ILogger<GetAllPaginatedCreditCardPaymentQueryHandler>>().Object, _mapper, _repositoryMock.Object, _cardRepositoryMock.Object);

        // Act
        var result = await handler.Handle(new GetAllPaginatedCreditCardPaymentQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        // Assert
        var items = result.Items.ToList();
        items[0].IsBeforeInitialBalance.Should().BeTrue();
        items[0].StatementId.Should().BeNull();
        items[1].IsBeforeInitialBalance.Should().BeFalse();
        items[1].StatementId.Should().Be(statement.CreditCardStatementId);
    }

    [Fact]
    public async Task Create_ForUnknownCard_ShouldThrowNotFoundException()
    {
        // Act
        Func<Task> act = () => CreateHandler().Handle(new CreateCreditCardPaymentCommand { CreditCardId = Guid.NewGuid(), PaymentDate = _card.InitialBalanceDate, Amount = 1m }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<Domain.Entities.CreditCardPayment>()), Times.Never);
    }

    [Fact]
    public async Task Update_HistoricalPayment_ChangesAmountDateAndCard()
    {
        // Arrange
        var otherCard = new Domain.Entities.CreditCard { CreditCardId = Guid.NewGuid(), Name = "BDO", InitialBalanceDate = new DateOnly(2026, 9, 1) };
        _cardRepositoryMock.Setup(repo => repo.GetByIdAsync(otherCard.CreditCardId)).ReturnsAsync(otherCard);
        var payment = new Domain.Entities.CreditCardPayment { CreditCardPaymentId = Guid.NewGuid(), CreditCardId = _card.CreditCardId, CreditCard = _card, PaymentDate = new DateOnly(2026, 10, 15), Amount = 10_000m };
        _repositoryMock.Setup(repo => repo.GetByIdAsync(payment.CreditCardPaymentId)).ReturnsAsync(payment);
        var handler = new UpdateCreditCardPaymentCommandHandler(new Mock<ILogger<UpdateCreditCardPaymentCommandHandler>>().Object, _mapper, _userContextMock.Object, _repositoryMock.Object, _cardRepositoryMock.Object);

        // Act
        await handler.Handle(new UpdateCreditCardPaymentCommand
        {
            CreditCardPaymentId = payment.CreditCardPaymentId,
            CreditCardId = otherCard.CreditCardId,
            PaymentDate = new DateOnly(2026, 11, 2),
            Amount = 12_000m
        }, CancellationToken.None);

        // Assert
        payment.CreditCardId.Should().Be(otherCard.CreditCardId);
        payment.CreditCard.Should().Be(otherCard);
        payment.PaymentDate.Should().Be(new DateOnly(2026, 11, 2));
        payment.Amount.Should().Be(12_000m);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public void Validator_RejectsZeroAmountAndFutureDate()
    {
        // Act
        var result = new CreateCreditCardPaymentCommandValidator().Validate(new CreateCreditCardPaymentCommand
        {
            CreditCardId = _card.CreditCardId,
            PaymentDate = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
            Amount = 0m
        });

        // Assert
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCreditCardPaymentCommand.Amount));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCreditCardPaymentCommand.PaymentDate));
    }
}
