using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using FinancialSuite.Application.CreditCardStatement.Commands.Create;
using FinancialSuite.Application.CreditCardStatement.Commands.Delete;
using FinancialSuite.Application.CreditCardStatement.Commands.Update;
using FinancialSuite.Application.CreditCardStatement.Dtos;
using FinancialSuite.Application.CreditCardStatement.Queries.GetPreview;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.Application.Tests.CreditCardStatement;

[TestSubject(typeof(CreateCreditCardStatementCommandHandler))]
public class CreditCardStatementHandlersTest
{
    private readonly Mock<ICreditCardRepository> _cardRepositoryMock = new();
    private readonly Mock<ICreditCardStatementRepository> _statementRepositoryMock = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<CreditCardStatementProfile>()).CreateMapper();
    private readonly Mock<IUserContext> _userContextMock = new();

    public CreditCardStatementHandlersTest()
    {
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(new CurrentUser("1", "user@example.com", [], null));
    }

    // Initial balance 100,000 on Sep 30; October closed at 105,000 and November at 95,000; 20,000 paid in November.
    private Domain.Entities.CreditCard SetupCard()
    {
        var card = new Domain.Entities.CreditCard { CreditCardId = Guid.NewGuid(), Name = "BPI", InitialBalance = 100_000m, InitialBalanceDate = new DateOnly(2026, 9, 30) };
        card.Statements.Add(Statement(card, new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), 100_000m, 105_000m));
        card.Statements.Add(Statement(card, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 105_000m, 95_000m));
        card.Payments.Add(new Domain.Entities.CreditCardPayment { CreditCardPaymentId = Guid.NewGuid(), CreditCardId = card.CreditCardId, PaymentDate = new DateOnly(2026, 11, 15), Amount = 20_000m });
        _cardRepositoryMock.Setup(repo => repo.GetWithHistoryAsync(card.CreditCardId)).ReturnsAsync(card);
        return card;
    }

    private static Domain.Entities.CreditCardStatement Statement(Domain.Entities.CreditCard card, DateOnly start, DateOnly end, decimal opening, decimal closing)
        => new()
        {
            CreditCardStatementId = Guid.NewGuid(),
            CreditCardId = card.CreditCardId,
            StatementDate = end,
            PeriodStartDate = start,
            PeriodEndDate = end,
            OpeningBalance = opening,
            ClosingBalance = closing
        };

    private CreateCreditCardStatementCommandHandler CreateHandler()
        => new(new Mock<ILogger<CreateCreditCardStatementCommandHandler>>().Object, _mapper, _userContextMock.Object, _cardRepositoryMock.Object);

    [Fact]
    public async Task Create_NextStatement_TakesThePreviousClosingBalanceAsItsOpeningBalance()
    {
        // Arrange
        var card = SetupCard();
        var command = new CreateCreditCardStatementCommand
        {
            CreditCardId = card.CreditCardId,
            StatementDate = new DateOnly(2026, 12, 31),
            PeriodStartDate = new DateOnly(2026, 12, 1),
            PeriodEndDate = new DateOnly(2026, 12, 31),
            ClosingBalance = 88_000m,
            Notes = " December "
        };

        // Act
        await CreateHandler().Handle(command, CancellationToken.None);

        // Assert
        var created = card.Statements.Single(x => x.PeriodEndDate == command.PeriodEndDate);
        created.OpeningBalance.Should().Be(95_000m);
        created.ClosingBalance.Should().Be(88_000m);
        created.Notes.Should().Be("December");
        created.CreatedBy.Should().Be("user@example.com");
        _cardRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Create_ForAMissedEarlierPeriod_ReopensTheLaterStatementFromIt()
    {
        // Arrange: a card whose only statement is November; the October statement is added afterwards.
        var card = SetupCard();
        var october = card.Statements.First();
        card.Statements.Remove(october);
        card.Statements.First().OpeningBalance = 100_000m;

        // Act
        await CreateHandler().Handle(new CreateCreditCardStatementCommand
        {
            CreditCardId = card.CreditCardId,
            StatementDate = october.PeriodEndDate,
            PeriodStartDate = october.PeriodStartDate,
            PeriodEndDate = october.PeriodEndDate,
            ClosingBalance = 105_000m
        }, CancellationToken.None);

        // Assert
        card.Statements.OrderBy(x => x.PeriodEndDate).Select(x => x.OpeningBalance).Should().Equal(100_000m, 105_000m);
    }

    [Theory]
    [InlineData(2026, 11, 15, 2026, 12, 15)] // starts inside November
    [InlineData(2026, 11, 1, 2026, 11, 30)]  // the same period again
    public async Task Create_ForAnOverlappingPeriod_ShouldThrowConflictException(int startYear, int startMonth, int startDay, int endYear, int endMonth, int endDay)
    {
        // Arrange
        var card = SetupCard();
        var end = new DateOnly(endYear, endMonth, endDay);

        // Act
        Func<Task> act = () => CreateHandler().Handle(new CreateCreditCardStatementCommand
        {
            CreditCardId = card.CreditCardId,
            StatementDate = end,
            PeriodStartDate = new DateOnly(startYear, startMonth, startDay),
            PeriodEndDate = end,
            ClosingBalance = 1m
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("This period overlaps the statement for Nov 1, 2026*");
        _cardRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }

    [Fact]
    public async Task Create_EndingOnOrBeforeTheInitialBalanceDate_ShouldThrowConflictException()
    {
        // Arrange
        var card = SetupCard();

        // Act
        Func<Task> act = () => CreateHandler().Handle(new CreateCreditCardStatementCommand
        {
            CreditCardId = card.CreditCardId,
            StatementDate = card.InitialBalanceDate,
            PeriodStartDate = new DateOnly(2026, 9, 1),
            PeriodEndDate = card.InitialBalanceDate,
            ClosingBalance = 1m
        }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("The statement period must end after the card's initial balance date*");
    }

    [Fact]
    public async Task Create_ForUnknownCard_ShouldThrowNotFoundException()
    {
        // Act
        Func<Task> act = () => CreateHandler().Handle(new CreateCreditCardStatementCommand { CreditCardId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_HistoricalClosingBalance_RecalculatesTheNextStatementsOpeningBalance()
    {
        // Arrange
        var card = SetupCard();
        var october = card.Statements.First();
        var november = card.Statements.Last();
        _statementRepositoryMock.Setup(repo => repo.GetByIdAsync(october.CreditCardStatementId)).ReturnsAsync(october);
        var handler = new UpdateCreditCardStatementCommandHandler(new Mock<ILogger<UpdateCreditCardStatementCommandHandler>>().Object, _mapper, _userContextMock.Object, _statementRepositoryMock.Object, _cardRepositoryMock.Object);

        // Act
        await handler.Handle(new UpdateCreditCardStatementCommand
        {
            CreditCardStatementId = october.CreditCardStatementId,
            CreditCardId = card.CreditCardId,
            StatementDate = october.StatementDate,
            PeriodStartDate = october.PeriodStartDate,
            PeriodEndDate = october.PeriodEndDate,
            ClosingBalance = 103_000m
        }, CancellationToken.None);

        // Assert
        october.ClosingBalance.Should().Be(103_000m);
        november.OpeningBalance.Should().Be(103_000m);
        _statementRepositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Update_ToAnotherCard_ShouldThrowConflictException()
    {
        // Arrange
        var card = SetupCard();
        var october = card.Statements.First();
        _statementRepositoryMock.Setup(repo => repo.GetByIdAsync(october.CreditCardStatementId)).ReturnsAsync(october);
        var handler = new UpdateCreditCardStatementCommandHandler(new Mock<ILogger<UpdateCreditCardStatementCommandHandler>>().Object, _mapper, _userContextMock.Object, _statementRepositoryMock.Object, _cardRepositoryMock.Object);

        // Act
        Func<Task> act = () => handler.Handle(new UpdateCreditCardStatementCommand { CreditCardStatementId = october.CreditCardStatementId, CreditCardId = Guid.NewGuid() }, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Delete_HistoricalStatement_ReopensTheNextStatementFromTheOneBefore()
    {
        // Arrange
        var card = SetupCard();
        var october = card.Statements.First();
        var november = card.Statements.Last();
        _statementRepositoryMock.Setup(repo => repo.GetByIdAsync(october.CreditCardStatementId)).ReturnsAsync(october);
        var handler = new DeleteCreditCardStatementCommandHandler(new Mock<ILogger<DeleteCreditCardStatementCommandHandler>>().Object, _statementRepositoryMock.Object, _cardRepositoryMock.Object);

        // Act
        await handler.Handle(new DeleteCreditCardStatementCommand(october.CreditCardStatementId), CancellationToken.None);

        // Assert
        november.OpeningBalance.Should().Be(100_000m);
        _statementRepositoryMock.Verify(repo => repo.DeleteAsync(october), Times.Once);
    }

    [Fact]
    public void Preview_NewStatement_ShowsPreviousBalancePaymentsAndTheCalculatedActivity()
    {
        // Arrange
        var card = SetupCard();
        card.Payments.Add(new Domain.Entities.CreditCardPayment { PaymentDate = new DateOnly(2026, 12, 10), Amount = 15_000m });

        // Act
        var preview = GetCreditCardStatementPreviewQueryHandler.Preview(card, new GetCreditCardStatementPreviewQuery
        {
            CreditCardId = card.CreditCardId,
            PeriodStartDate = new DateOnly(2026, 12, 1),
            PeriodEndDate = new DateOnly(2026, 12, 31),
            ClosingBalance = 88_000m
        });

        // Assert
        preview.PreviousBalance.Should().Be(95_000m);
        preview.PreviousCutoffDate.Should().Be(new DateOnly(2026, 11, 30));
        preview.OpensFromInitialBalance.Should().BeFalse();
        preview.PaymentsSincePrevious.Should().Be(15_000m);
        preview.PaymentCount.Should().Be(1);
        preview.NewBalance.Should().Be(88_000m);
        preview.EstimatedNetActivity.Should().Be(8_000m);
        preview.NetDebtMovement.Should().Be(-7_000m);
        preview.DebtStatus.Should().Be("Decreased");
        preview.LaterStatementCount.Should().Be(0);
        preview.Problem.Should().BeNull();
    }

    [Fact]
    public void Preview_WithoutAClosingBalance_GivesOnlyTheOpeningSide()
    {
        // Arrange
        var card = SetupCard();

        // Act
        var preview = GetCreditCardStatementPreviewQueryHandler.Preview(card, new GetCreditCardStatementPreviewQuery { CreditCardId = card.CreditCardId, PeriodEndDate = new DateOnly(2026, 12, 31) });

        // Assert
        preview.PreviousBalance.Should().Be(95_000m);
        preview.EstimatedNetActivity.Should().BeNull();
        preview.NetDebtMovement.Should().BeNull();
    }

    [Fact]
    public void Preview_EditOfAHistoricalStatement_ExcludesItsStoredVersionAndCountsLaterStatements()
    {
        // Arrange
        var card = SetupCard();
        var october = card.Statements.First();

        // Act
        var preview = GetCreditCardStatementPreviewQueryHandler.Preview(card, new GetCreditCardStatementPreviewQuery
        {
            CreditCardId = card.CreditCardId,
            CreditCardStatementId = october.CreditCardStatementId,
            PeriodStartDate = october.PeriodStartDate,
            PeriodEndDate = october.PeriodEndDate,
            ClosingBalance = 103_000m
        });

        // Assert
        preview.OpensFromInitialBalance.Should().BeTrue();
        preview.PreviousBalance.Should().Be(100_000m);
        preview.EstimatedNetActivity.Should().Be(3_000m);
        preview.LaterStatementCount.Should().Be(1);
        preview.Problem.Should().BeNull();
    }

    [Fact]
    public void Preview_OverlappingPeriod_ReportsTheProblem()
    {
        // Arrange
        var card = SetupCard();

        // Act
        var preview = GetCreditCardStatementPreviewQueryHandler.Preview(card, new GetCreditCardStatementPreviewQuery
        {
            CreditCardId = card.CreditCardId,
            PeriodStartDate = new DateOnly(2026, 11, 20),
            PeriodEndDate = new DateOnly(2026, 12, 20),
            ClosingBalance = 90_000m
        });

        // Assert
        preview.Problem.Should().StartWith("This period overlaps");
    }
}
