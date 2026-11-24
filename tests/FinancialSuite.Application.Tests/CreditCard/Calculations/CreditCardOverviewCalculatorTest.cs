using System;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using FinancialSuite.Application.CreditCard.Calculations;
using FinancialSuite.Domain.Entities;
using Xunit;

namespace FinancialSuite.Application.Tests.CreditCard.Calculations;

[TestSubject(typeof(CreditCardOverviewCalculator))]
public class CreditCardOverviewCalculatorTest
{
    private static readonly DateOnly Today = new(2026, 12, 10);

    private static Domain.Entities.CreditCard Card(string name, decimal initialBalance, DateOnly initialDate)
        => new() { CreditCardId = Guid.NewGuid(), Name = name, InitialBalance = initialBalance, InitialBalanceDate = initialDate, IsActive = true };

    private static void AddStatement(Domain.Entities.CreditCard card, DateOnly cutoff, decimal closing)
        => card.Statements.Add(new Domain.Entities.CreditCardStatement
        {
            CreditCardStatementId = Guid.NewGuid(),
            CreditCardId = card.CreditCardId,
            StatementDate = cutoff,
            PeriodStartDate = new DateOnly(cutoff.Year, cutoff.Month, 1),
            PeriodEndDate = cutoff,
            ClosingBalance = closing
        });

    private static void AddPayment(Domain.Entities.CreditCard card, DateOnly date, decimal amount)
        => card.Payments.Add(new Domain.Entities.CreditCardPayment { CreditCardPaymentId = Guid.NewGuid(), CreditCardId = card.CreditCardId, PaymentDate = date, Amount = amount });

    // BPI: Sep 100k → Oct 105k (paid 10k) → Nov 95k (paid 20k), 5k paid on Dec 5 (open period).
    // BDO: Oct 50k → Nov 52k (paid 3k), nothing since.
    private static (Domain.Entities.CreditCard Bpi, Domain.Entities.CreditCard Bdo) TwoCards()
    {
        var bpi = Card("BPI", 100_000m, new DateOnly(2026, 9, 30));
        AddStatement(bpi, new DateOnly(2026, 10, 31), 105_000m);
        AddStatement(bpi, new DateOnly(2026, 11, 30), 95_000m);
        AddPayment(bpi, new DateOnly(2026, 10, 15), 10_000m);
        AddPayment(bpi, new DateOnly(2026, 11, 15), 20_000m);
        AddPayment(bpi, new DateOnly(2026, 12, 5), 5_000m);

        var bdo = Card("BDO", 50_000m, new DateOnly(2026, 10, 25));
        AddStatement(bdo, new DateOnly(2026, 11, 25), 52_000m);
        AddPayment(bdo, new DateOnly(2026, 11, 10), 3_000m);

        return (bpi, bdo);
    }

    [Fact]
    public void Build_MultipleCards_KeepsEachCardIndependentAndCombinesTheTotals()
    {
        // Arrange
        var (bpi, bdo) = TwoCards();

        // Act
        var overview = CreditCardOverviewCalculator.Build([bpi, bdo], null, null, Today);

        // Assert
        var bpiLedger = overview.Cards.Single(x => x.Name == "BPI");
        var bdoLedger = overview.Cards.Single(x => x.Name == "BDO");
        bpiLedger.Periods.Select(p => p.EstimatedNetActivity).Should().Equal(15_000m, 10_000m);
        bpiLedger.CurrentOutstanding.Should().Be(90_000m);
        bdoLedger.Periods.Select(p => p.EstimatedNetActivity).Should().Equal(5_000m);
        bdoLedger.CurrentOutstanding.Should().Be(52_000m);

        var summary = overview.Summary;
        summary.CardCount.Should().Be(2);
        summary.TotalOutstanding.Should().Be(142_000m);
        summary.TotalPayments.Should().Be(38_000m);
        summary.PaymentCount.Should().Be(4);
        summary.EstimatedNetActivity.Should().Be(30_000m);
        summary.CompletedPeriodCount.Should().Be(3);
        // +5,000 - 10,000 (BPI) + 2,000 (BDO)
        summary.NetDebtMovement.Should().Be(-3_000m);
        summary.DebtStatus.Should().Be(DebtStatus.Decreased);
        summary.InitialTotalBalance.Should().Be(150_000m);
        summary.DebtReduction.Should().Be(8_000m);
        summary.DebtReductionPercent.Should().Be(5.33m);
    }

    [Fact]
    public void Build_WithADateRange_CountsPeriodsByCutoffAndPaymentsByDate()
    {
        // Arrange
        var (bpi, bdo) = TwoCards();

        // Act: November only.
        var overview = CreditCardOverviewCalculator.Build([bpi, bdo], new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), Today);

        // Assert
        overview.Summary.TotalPayments.Should().Be(23_000m);
        overview.Summary.EstimatedNetActivity.Should().Be(15_000m);
        overview.Summary.CompletedPeriodCount.Should().Be(2);
        overview.Summary.NetDebtMovement.Should().Be(-8_000m);
        // Outstanding and overall reduction are not limited by the range.
        overview.Summary.TotalOutstanding.Should().Be(142_000m);
        overview.Cards.Single(x => x.Name == "BPI").Periods.Should().ContainSingle(p => p.ClosingBalance == 95_000m);
        // Both open periods start in late November, so they still show within a range ending Nov 30.
        overview.Cards.Single(x => x.Name == "BDO").OpenPeriod.Should().NotBeNull();
        overview.Cards.Single(x => x.Name == "BPI").OpenPeriod.Should().BeNull();
        overview.PeriodTotals.Should().ContainSingle().Which.Month.Should().Be("2026-11");
    }

    [Fact]
    public void Build_PeriodTotals_GroupCompletedPeriodsByCutoffMonth()
    {
        // Arrange
        var (bpi, bdo) = TwoCards();

        // Act
        var overview = CreditCardOverviewCalculator.Build([bpi, bdo], null, null, Today);

        // Assert
        overview.PeriodTotals.Select(x => (x.Month, x.PeriodCount, x.TotalPayments, x.EstimatedNetActivity, x.NetDebtMovement))
            .Should().Equal(
                ("2026-10", 1, 10_000m, 15_000m, 5_000m),
                ("2026-11", 2, 23_000m, 15_000m, -8_000m));
    }

    [Fact]
    public void Build_BalanceTrend_UsesMonthEndStatementBalancesThenTodaysOutstanding()
    {
        // Arrange
        var (bpi, bdo) = TwoCards();

        // Act
        var trend = CreditCardOverviewCalculator.Build([bpi, bdo], null, null, Today).BalanceTrend;

        // Assert: Sep: BPI 100k; Oct: BPI 105k + BDO 50k; Nov: 95k + 52k; now: 90k + 52k.
        trend.Select(x => (x.Month, x.Balance, x.IsCurrent)).Should().Equal(
            ("2026-09", 100_000m, false),
            ("2026-10", 155_000m, false),
            ("2026-11", 147_000m, false),
            ("2026-12", 142_000m, true));
    }

    [Fact]
    public void Build_WithNoCards_ReturnsZeroTotals()
    {
        // Act
        var overview = CreditCardOverviewCalculator.Build([], null, null, Today);

        // Assert
        overview.Summary.TotalOutstanding.Should().Be(0m);
        overview.Summary.DebtReductionPercent.Should().BeNull();
        overview.Summary.DebtStatus.Should().Be(DebtStatus.NoChange);
        overview.BalanceTrend.Should().BeEmpty();
    }
}
