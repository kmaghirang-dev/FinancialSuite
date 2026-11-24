using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using FinancialSuite.API.Controllers;
using FinancialSuite.Application.CreditCard.Dtos;
using FinancialSuite.Application.CreditCardPayment.Commands.Create;
using FinancialSuite.Application.CreditCardStatement.Commands.Create;
using FinancialSuite.Application.CreditCardStatement.Dtos;
using FinancialSuite.Application.Users;
using FinancialSuite.Domain.Constants;
using FinancialSuite.Domain.Repositories;
using Xunit;

namespace FinancialSuite.API.Tests.Controllers;

[TestSubject(typeof(CreditCardController))]
public class CreditCardControllerTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Mock<ICreditCardRepository> _cardRepositoryMock = new();
    private readonly Mock<ICreditCardStatementRepository> _statementRepositoryMock = new();
    private readonly Mock<ICreditCardPaymentRepository> _paymentRepositoryMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();
    private readonly Domain.Entities.CreditCard _card;

    public CreditCardControllerTest(WebApplicationFactory<Program> factory)
    {
        _userContextMock.Setup(uc => uc.GetCurrentUser()).Returns(new CurrentUser("1", "test@example.com", [UserRoles.User], null));

        _card = new Domain.Entities.CreditCard { CreditCardId = Guid.NewGuid(), Name = "BPI", InitialBalance = 100_000m, InitialBalanceDate = new DateOnly(2026, 9, 30), IsActive = true };
        _card.Statements.Add(new Domain.Entities.CreditCardStatement { CreditCardStatementId = Guid.NewGuid(), CreditCardId = _card.CreditCardId, StatementDate = new DateOnly(2026, 10, 31), PeriodStartDate = new DateOnly(2026, 10, 1), PeriodEndDate = new DateOnly(2026, 10, 31), ClosingBalance = 105_000m });
        _card.Payments.Add(new Domain.Entities.CreditCardPayment { CreditCardPaymentId = Guid.NewGuid(), CreditCardId = _card.CreditCardId, PaymentDate = new DateOnly(2026, 10, 15), Amount = 10_000m });
        _cardRepositoryMock.Setup(repo => repo.GetWithHistoryAsync(_card.CreditCardId)).ReturnsAsync(_card);
        _cardRepositoryMock.Setup(repo => repo.GetByIdAsync(_card.CreditCardId)).ReturnsAsync(_card);
        _cardRepositoryMock.Setup(repo => repo.GetAllWithHistoryAsync(true)).ReturnsAsync(new List<Domain.Entities.CreditCard> { _card });

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.Replace(ServiceDescriptor.Singleton<IPolicyEvaluator, FakePolicyEvaluator>());
                services.Replace(ServiceDescriptor.Scoped(typeof(ICreditCardRepository), _ => _cardRepositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(ICreditCardStatementRepository), _ => _statementRepositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(ICreditCardPaymentRepository), _ => _paymentRepositoryMock.Object));
                services.Replace(ServiceDescriptor.Scoped(typeof(IUserContext), _ => _userContextMock.Object));
            });
        });
    }

    [Fact]
    public async Task GetOverview_Returns200OkWithCalculatedPeriods()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/credit-cards/overview", TestContext.Current.CancellationToken);
        var overview = await result.Content.ReadFromJsonAsync<CreditCardOverviewDto>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        overview!.Summary.TotalOutstanding.Should().Be(105_000m);
        overview.Summary.EstimatedNetActivity.Should().Be(15_000m);
        overview.Summary.NetDebtMovement.Should().Be(5_000m);
        overview.Cards.Should().ContainSingle().Which.Periods.Should().ContainSingle(p => p.TotalPayments == 10_000m && p.IsLatest);
    }

    [Fact]
    public async Task GetOverview_WithToBeforeFrom_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync("api/credit-cards/overview?from=2026-11-01&to=2026-10-01", TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetStatementPreview_Returns200OkWithTheCalculation()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var result = await client.GetAsync($"api/credit-card-statements/preview?creditCardId={_card.CreditCardId}&periodStartDate=2026-11-01&periodEndDate=2026-11-30&closingBalance=95000", TestContext.Current.CancellationToken);
        var preview = await result.Content.ReadFromJsonAsync<CreditCardStatementPreviewDto>(TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        preview!.PreviousBalance.Should().Be(105_000m);
        preview.EstimatedNetActivity.Should().Be(-10_000m);
    }

    [Fact]
    public async Task CreateStatement_WithPeriodEndBeforeStart_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var command = new CreateCreditCardStatementCommand { CreditCardId = _card.CreditCardId, StatementDate = new DateOnly(2026, 11, 30), PeriodStartDate = new DateOnly(2026, 11, 30), PeriodEndDate = new DateOnly(2026, 11, 1), ClosingBalance = 1m };

        // Act
        var result = await client.PostAsJsonAsync("api/credit-card-statements", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePayment_BeforeTheInitialBalanceDate_Returns201Created()
    {
        // Arrange
        _paymentRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<Domain.Entities.CreditCardPayment>())).ReturnsAsync(Guid.NewGuid());
        var client = _factory.CreateClient();
        var command = new CreateCreditCardPaymentCommand { CreditCardId = _card.CreditCardId, PaymentDate = new DateOnly(2026, 9, 15), Amount = 1_000m };

        // Act
        var result = await client.PostAsJsonAsync("api/credit-card-payments", command, TestContext.Current.CancellationToken);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
