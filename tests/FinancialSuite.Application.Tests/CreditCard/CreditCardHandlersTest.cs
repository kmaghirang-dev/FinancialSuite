using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using FinancialSuite.Application.CreditCard.Commands.Create;
using FinancialSuite.Application.CreditCard.Commands.Toggle;
using FinancialSuite.Application.CreditCard.Commands.Update;
using FinancialSuite.Application.CreditCard.Dtos;
using FinancialSuite.Application.CreditCardStatement.Commands.Create;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Entities;
using FinancialSuite.Domain.Exceptions;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.Application.Tests.CreditCard;

[TestSubject(typeof(UpdateCreditCardCommandHandler))]
public class CreditCardHandlersTest
{
    private readonly Mock<ICreditCardRepository> _repositoryMock = new();
    private readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<CreditCardProfile>()).CreateMapper();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Domain.Entities.CreditCard _card = new() { CreditCardId = Guid.NewGuid(), Name = "BPI", InitialBalance = 100_000m, InitialBalanceDate = new DateOnly(2026, 9, 30), IsActive = true };

    public CreditCardHandlersTest()
    {
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(new CurrentUser("1", "user@example.com", [], null));
        _repositoryMock.Setup(repo => repo.GetWithHistoryAsync(_card.CreditCardId)).ReturnsAsync(_card);
        _card.Statements.Add(new Domain.Entities.CreditCardStatement { PeriodStartDate = new DateOnly(2026, 10, 1), PeriodEndDate = new DateOnly(2026, 10, 31), OpeningBalance = 100_000m, ClosingBalance = 105_000m });
    }

    private UpdateCreditCardCommandHandler UpdateHandler()
        => new(new Mock<ILogger<UpdateCreditCardCommandHandler>>().Object, _mapper, _userContextMock.Object, _repositoryMock.Object);

    private UpdateCreditCardCommand UpdateCommand(decimal initialBalance, DateOnly initialDate)
        => new() { CreditCardId = _card.CreditCardId, Name = " BPI Gold ", Nickname = null, InitialBalance = initialBalance, InitialBalanceDate = initialDate };

    [Fact]
    public async Task Create_ForValidCommand_CreatesAnActiveCard()
    {
        // Arrange
        Domain.Entities.CreditCard created = null;
        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.CreditCard>()))
            .Callback<Domain.Entities.CreditCard>(card => created = card)
            .ReturnsAsync(Guid.NewGuid());
        var handler = new CreateCreditCardCommandHandler(new Mock<ILogger<CreateCreditCardCommandHandler>>().Object, _mapper, _userContextMock.Object, _repositoryMock.Object);

        // Act
        await handler.Handle(new CreateCreditCardCommand { Name = " BDO ", Nickname = " Main ", InitialBalance = 50_000m, InitialBalanceDate = new DateOnly(2026, 10, 25), CreditLimit = 80_000m }, CancellationToken.None);

        // Assert
        created!.Name.Should().Be("BDO");
        created.Nickname.Should().Be("Main");
        created.Notes.Should().BeEmpty();
        created.IsActive.Should().BeTrue();
        created.InitialBalance.Should().Be(50_000m);
    }

    [Fact]
    public async Task Update_InitialBalance_RecalculatesTheFirstStatementsOpeningBalance()
    {
        // Act
        await UpdateHandler().Handle(UpdateCommand(98_000m, _card.InitialBalanceDate), CancellationToken.None);

        // Assert
        _card.Name.Should().Be("BPI Gold");
        _card.InitialBalance.Should().Be(98_000m);
        _card.Statements[0].OpeningBalance.Should().Be(98_000m);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Update_InitialBalanceDateOnOrAfterTheFirstStatement_ShouldThrowConflictException()
    {
        // Act
        Func<Task> act = () => UpdateHandler().Handle(UpdateCommand(100_000m, new DateOnly(2026, 10, 31)), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("The initial balance date must be before the first statement's period end*");
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Never);
    }

    [Fact]
    public async Task Update_InitialBalanceDateOnOrAfterAPayment_IsAllowed()
    {
        // Arrange
        _card.Statements.Clear();
        _card.Payments.Add(new Domain.Entities.CreditCardPayment { PaymentDate = new DateOnly(2026, 10, 5), Amount = 1m });

        // Act
        await UpdateHandler().Handle(UpdateCommand(100_000m, new DateOnly(2026, 10, 5)), CancellationToken.None);

        // Assert
        _card.InitialBalanceDate.Should().Be(new DateOnly(2026, 10, 5));
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
    }

    [Fact]
    public async Task Toggle_ToInactive_SoftDeletesTheCardAndKeepsItsHistory()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetByIdAsync(_card.CreditCardId)).ReturnsAsync(_card);
        var handler = new ToggleCreditCardCommandHandler(new Mock<ILogger<ToggleCreditCardCommandHandler>>().Object, _mapper, _userContextMock.Object, _repositoryMock.Object);

        // Act
        await handler.Handle(new ToggleCreditCardCommand { CreditCardId = _card.CreditCardId, IsActive = false }, CancellationToken.None);

        // Assert
        _card.IsActive.Should().BeFalse();
        _card.Statements.Should().HaveCount(1);
        _repositoryMock.Verify(repo => repo.SaveChanges(), Times.Once);
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.CreditCard>()), Times.Never);
    }

    [Fact]
    public void Validator_RejectsBlankNameFutureDateAndOutOfRangeDays()
    {
        // Act
        var result = new CreateCreditCardCommandValidator().Validate(new CreateCreditCardCommand
        {
            Name = " ",
            InitialBalanceDate = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
            StatementCutoffDay = 0,
            PaymentDueDay = 32,
            CreditLimit = 0m
        });

        // Assert
        result.Errors.Select(e => e.PropertyName).Should().Contain([
            nameof(CreateCreditCardCommand.Name),
            nameof(CreateCreditCardCommand.InitialBalanceDate),
            nameof(CreateCreditCardCommand.StatementCutoffDay),
            nameof(CreateCreditCardCommand.PaymentDueDay),
            nameof(CreateCreditCardCommand.CreditLimit)]);
    }

    [Fact]
    public void StatementValidator_RejectsReversedPeriodAndStatementDateBeforeCutoff()
    {
        // Act
        var result = new CreateCreditCardStatementCommandValidator().Validate(new CreateCreditCardStatementCommand
        {
            CreditCardId = Guid.NewGuid(),
            PeriodStartDate = new DateOnly(2026, 10, 31),
            PeriodEndDate = new DateOnly(2026, 10, 1),
            StatementDate = new DateOnly(2026, 9, 30),
            ClosingBalance = 1m
        });

        // Assert
        result.Errors.Should().Contain(e => e.ErrorMessage == "The period end date must be on or after the period start date.");
        result.Errors.Should().Contain(e => e.ErrorMessage == "The statement date must be on or after the period end date.");
    }
}
