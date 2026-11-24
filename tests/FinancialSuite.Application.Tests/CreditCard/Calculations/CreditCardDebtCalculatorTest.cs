using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using FinancialSuite.Application.CreditCard.Calculations;
using Xunit;

namespace FinancialSuite.Application.Tests.CreditCard.Calculations;

[TestSubject(typeof(CreditCardDebtCalculator))]
public class CreditCardDebtCalculatorTest
{
    private static readonly DateOnly SeptemberCutoff = new(2026, 9, 30);
    private static readonly DateOnly OctoberCutoff = new(2026, 10, 31);
    private static readonly DateOnly NovemberCutoff = new(2026, 11, 30);
    private static readonly DateOnly DecemberCutoff = new(2026, 12, 31);

    private static LedgerStatement Statement(DateOnly cutoff, decimal closing, Guid? id = null)
        => new(id ?? Guid.NewGuid(), cutoff.AddDays(5), new DateOnly(cutoff.Year, cutoff.Month, 1), cutoff, closing);

    private static LedgerPayment Payment(DateOnly date, decimal amount, Guid? id = null)
        => new(id ?? Guid.NewGuid(), date, amount);

    [Fact]
    public void Calculate_WithOnlyTheInitialBalance_HasNoCompletedPeriodsAndAnOpenPeriodFromTheBaseline()
    {
        // Act
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff, [], []);

        // Assert
        ledger.Periods.Should().BeEmpty();
        ledger.OpenPeriod.SinceDate.Should().Be(SeptemberCutoff);
        ledger.OpenPeriod.LatestStatementId.Should().BeNull();
        ledger.OpenPeriod.OpeningBalance.Should().Be(100_000m);
        ledger.OpenPeriod.PaymentsSoFar.Should().Be(0m);
        ledger.CurrentOutstanding.Should().Be(100_000m);
        ledger.DebtReduction.Should().Be(0m);
        ledger.DebtReductionPercent.Should().Be(0m);
    }

    [Fact]
    public void Calculate_FirstStatement_OpensFromTheInitialBalance()
    {
        // Act
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff, [Statement(OctoberCutoff, 104_000m)], []);

        // Assert
        var period = ledger.Periods.Should().ContainSingle().Subject;
        period.OpensFromInitialBalance.Should().BeTrue();
        period.PreviousCutoffDate.Should().Be(SeptemberCutoff);
        period.OpeningBalance.Should().Be(100_000m);
    }

    [Fact]
    public void Calculate_StatementWithNoPayments_ActivityIsTheBalanceIncrease()
    {
        // Act
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff, [Statement(OctoberCutoff, 104_000m)], []);

        // Assert
        var period = ledger.Periods.Single();
        period.TotalPayments.Should().Be(0m);
        period.PaymentCount.Should().Be(0);
        period.EstimatedNetActivity.Should().Be(4_000m);
        period.NetDebtMovement.Should().Be(4_000m);
        period.DebtStatus.Should().Be(DebtStatus.Increased);
    }

    [Fact]
    public void Calculate_StatementWithOnePayment_DebtIncreasingDespitePayment_MatchesTheOctoberExample()
    {
        // Arrange: September closing 100,000; paid 10,000; October closing 105,000.
        var statements = new[] { Statement(OctoberCutoff, 105_000m) };
        var payments = new[] { Payment(new DateOnly(2026, 10, 15), 10_000m) };

        // Act
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff, statements, payments);

        // Assert
        var period = ledger.Periods.Single();
        period.OpeningBalance.Should().Be(100_000m);
        period.TotalPayments.Should().Be(10_000m);
        period.PaymentCount.Should().Be(1);
        period.EstimatedNetActivity.Should().Be(15_000m);
        period.ClosingBalance.Should().Be(105_000m);
        period.NetDebtMovement.Should().Be(5_000m);
        period.DebtStatus.Should().Be(DebtStatus.Increased);
    }

    [Fact]
    public void Calculate_StatementWithMultiplePayments_DebtDecreasingDespiteNewActivity_MatchesTheNovemberExample()
    {
        // Arrange: previous 105,000; paid 12,000 + 8,000; next 95,000.
        var statements = new[] { Statement(OctoberCutoff, 105_000m), Statement(NovemberCutoff, 95_000m) };
        var payments = new[]
        {
            Payment(new DateOnly(2026, 11, 3), 12_000m),
            Payment(new DateOnly(2026, 11, 20), 8_000m)
        };

        // Act
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff, statements, payments);

        // Assert
        var november = ledger.Periods[1];
        november.OpeningBalance.Should().Be(105_000m);
        november.TotalPayments.Should().Be(20_000m);
        november.PaymentCount.Should().Be(2);
        november.EstimatedNetActivity.Should().Be(10_000m);
        november.ClosingBalance.Should().Be(95_000m);
        november.NetDebtMovement.Should().Be(-10_000m);
        november.DebtStatus.Should().Be(DebtStatus.Decreased);
    }

    [Fact]
    public void Calculate_RefundsExceedingCharges_GivesNegativeEstimatedNetActivity()
    {
        // Arrange: 100,000 → 80,000 with only 10,000 paid, so 10,000 more came off through credits/refunds.
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff,
            [Statement(OctoberCutoff, 80_000m)],
            [Payment(new DateOnly(2026, 10, 10), 10_000m)]);

        // Assert
        var period = ledger.Periods.Single();
        period.EstimatedNetActivity.Should().Be(-10_000m);
        period.NetDebtMovement.Should().Be(-20_000m);
        period.DebtStatus.Should().Be(DebtStatus.Decreased);
    }

    [Fact]
    public void Calculate_UnchangedBalance_IsNoChange()
    {
        // Arrange: paid exactly what was charged.
        var ledger = CreditCardDebtCalculator.Calculate(50_000m, SeptemberCutoff,
            [Statement(OctoberCutoff, 50_000m)],
            [Payment(new DateOnly(2026, 10, 10), 7_500m)]);

        // Assert
        var period = ledger.Periods.Single();
        period.EstimatedNetActivity.Should().Be(7_500m);
        period.NetDebtMovement.Should().Be(0m);
        period.DebtStatus.Should().Be(DebtStatus.NoChange);
    }

    [Fact]
    public void Calculate_FullHistoryTable_MatchesTheSpecifiedPeriodsAndCountsEachPaymentOnce()
    {
        // Arrange: Sep 100k baseline; Oct 105k (paid 10k); Nov 95k (paid 20k); Dec 88k (paid 15k).
        var statements = new[] { Statement(DecemberCutoff, 88_000m), Statement(OctoberCutoff, 105_000m), Statement(NovemberCutoff, 95_000m) };
        var payments = new[]
        {
            Payment(new DateOnly(2026, 10, 15), 10_000m),
            Payment(new DateOnly(2026, 11, 15), 20_000m),
            Payment(new DateOnly(2026, 12, 15), 15_000m)
        };

        // Act
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff, statements, payments);

        // Assert: periods come out in cutoff order whatever order the statements were given in.
        ledger.Periods.Select(p => (p.OpeningBalance, p.TotalPayments, p.EstimatedNetActivity, p.ClosingBalance, p.NetDebtMovement))
            .Should().Equal(
                (100_000m, 10_000m, 15_000m, 105_000m, 5_000m),
                (105_000m, 20_000m, 10_000m, 95_000m, -10_000m),
                (95_000m, 15_000m, 8_000m, 88_000m, -7_000m));
        ledger.Periods.Select(p => p.IsLatest).Should().Equal(false, false, true);
        ledger.Periods.Sum(p => p.TotalPayments).Should().Be(payments.Sum(p => p.Amount));
        ledger.CurrentOutstanding.Should().Be(88_000m);
        ledger.DebtReduction.Should().Be(12_000m);
        ledger.DebtReductionPercent.Should().Be(12m);
    }

    [Fact]
    public void Calculate_PaymentOnACutoffDate_CountsTowardThatStatementAndTheDayAfterTowardTheNext()
    {
        // Arrange
        var onCutoff = Payment(OctoberCutoff, 1_000m);
        var dayAfter = Payment(OctoberCutoff.AddDays(1), 2_000m);

        // Act
        var ledger = CreditCardDebtCalculator.Calculate(10_000m, SeptemberCutoff,
            [Statement(OctoberCutoff, 9_000m), Statement(NovemberCutoff, 7_000m)],
            [onCutoff, dayAfter]);

        // Assert
        ledger.Periods[0].TotalPayments.Should().Be(1_000m);
        ledger.Periods[1].TotalPayments.Should().Be(2_000m);
    }

    [Fact]
    public void Calculate_PaymentOnOrBeforeTheInitialBalanceDate_IsNotCounted()
    {
        // Arrange: the initial balance already reflects it.
        var ledger = CreditCardDebtCalculator.Calculate(10_000m, SeptemberCutoff,
            [Statement(OctoberCutoff, 9_000m)],
            [Payment(SeptemberCutoff, 5_000m), Payment(SeptemberCutoff.AddDays(-3), 1_000m)]);

        // Assert
        ledger.Periods.Single().TotalPayments.Should().Be(0m);
        ledger.CountedPayments.Should().BeEmpty();
        ledger.CurrentOutstanding.Should().Be(9_000m);
    }

    [Fact]
    public void Calculate_IncompletePeriod_KeepsPaymentsAfterTheLatestStatementOutOfCompletedPeriods()
    {
        // Arrange
        var latest = Statement(OctoberCutoff, 105_000m);
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff,
            [latest],
            [Payment(new DateOnly(2026, 10, 15), 10_000m), Payment(new DateOnly(2026, 11, 5), 6_000m), Payment(new DateOnly(2026, 11, 12), 4_000m)]);

        // Assert
        ledger.Periods.Single().TotalPayments.Should().Be(10_000m);
        ledger.Periods.Single().IsLatest.Should().BeTrue();
        ledger.OpenPeriod.SinceDate.Should().Be(OctoberCutoff);
        ledger.OpenPeriod.LatestStatementId.Should().Be(latest.StatementId);
        ledger.OpenPeriod.OpeningBalance.Should().Be(105_000m);
        ledger.OpenPeriod.PaymentsSoFar.Should().Be(10_000m);
        ledger.OpenPeriod.PaymentCount.Should().Be(2);
        ledger.LatestStatementBalance.Should().Be(105_000m);
        ledger.CurrentOutstanding.Should().Be(95_000m);
    }

    [Fact]
    public void Calculate_AfterAHistoricalPaymentIsChanged_RecalculatesBothAffectedPeriods()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var statements = new[] { Statement(OctoberCutoff, 105_000m), Statement(NovemberCutoff, 95_000m) };
        var before = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff, statements,
            [Payment(new DateOnly(2026, 10, 15), 10_000m, paymentId), Payment(new DateOnly(2026, 11, 15), 20_000m)]);

        // Act: the October payment was really 12,000 and made on November 2.
        var after = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff, statements,
            [Payment(new DateOnly(2026, 11, 2), 12_000m, paymentId), Payment(new DateOnly(2026, 11, 15), 20_000m)]);

        // Assert
        before.Periods[0].EstimatedNetActivity.Should().Be(15_000m);
        before.Periods[1].EstimatedNetActivity.Should().Be(10_000m);
        after.Periods[0].TotalPayments.Should().Be(0m);
        after.Periods[0].EstimatedNetActivity.Should().Be(5_000m);
        after.Periods[1].TotalPayments.Should().Be(32_000m);
        after.Periods[1].EstimatedNetActivity.Should().Be(22_000m);
        // Balances come from the statements, so movements are unchanged.
        after.Periods.Select(p => p.NetDebtMovement).Should().Equal(before.Periods.Select(p => p.NetDebtMovement));
    }

    [Fact]
    public void Calculate_AfterAHistoricalStatementIsChanged_RecalculatesItAndTheFollowingPeriod()
    {
        // Arrange
        var payments = new[] { Payment(new DateOnly(2026, 10, 15), 10_000m), Payment(new DateOnly(2026, 11, 15), 20_000m) };

        // Act: October's closing balance corrected from 105,000 to 103,000.
        var ledger = CreditCardDebtCalculator.Calculate(100_000m, SeptemberCutoff,
            [Statement(OctoberCutoff, 103_000m), Statement(NovemberCutoff, 95_000m)], payments);

        // Assert
        ledger.Periods[0].EstimatedNetActivity.Should().Be(13_000m);
        ledger.Periods[0].NetDebtMovement.Should().Be(3_000m);
        ledger.Periods[1].OpeningBalance.Should().Be(103_000m);
        ledger.Periods[1].EstimatedNetActivity.Should().Be(12_000m);
        ledger.Periods[1].NetDebtMovement.Should().Be(-8_000m);
    }

    [Fact]
    public void SyncOpeningBalances_ChainsEachStatementToThePreviousClosingBalance()
    {
        // Arrange: stored openings are stale after October was corrected and the initial balance changed.
        var october = new Domain.Entities.CreditCardStatement { PeriodEndDate = OctoberCutoff, OpeningBalance = 100_000m, ClosingBalance = 103_000m };
        var november = new Domain.Entities.CreditCardStatement { PeriodEndDate = NovemberCutoff, OpeningBalance = 105_000m, ClosingBalance = 95_000m };

        // Act
        var changed = CreditCardDebtCalculator.SyncOpeningBalances(99_000m, [november, october]);

        // Assert
        changed.Should().BeTrue();
        october.OpeningBalance.Should().Be(99_000m);
        november.OpeningBalance.Should().Be(103_000m);
        CreditCardDebtCalculator.SyncOpeningBalances(99_000m, [november, october]).Should().BeFalse();
    }

    [Fact]
    public void Calculate_KeepsCentPrecision()
    {
        // Act
        var ledger = CreditCardDebtCalculator.Calculate(10_000.10m, SeptemberCutoff,
            [Statement(OctoberCutoff, 10_250.35m)],
            [Payment(new DateOnly(2026, 10, 3), 0.1m), Payment(new DateOnly(2026, 10, 4), 0.2m)]);

        // Assert: 10,250.35 - 10,000.10 + 0.30, with no binary floating-point drift.
        ledger.Periods.Single().EstimatedNetActivity.Should().Be(250.55m);
    }

    [Fact]
    public void FindStatementFor_ReturnsTheFirstStatementWhoseCutoffIsOnOrAfterThePayment()
    {
        // Arrange
        var october = Statement(OctoberCutoff, 1m);
        var november = Statement(NovemberCutoff, 1m);
        var statements = new List<LedgerStatement> { november, october };

        // Act + Assert
        CreditCardDebtCalculator.FindStatementFor(new DateOnly(2026, 10, 2), SeptemberCutoff, statements).Should().Be(october);
        CreditCardDebtCalculator.FindStatementFor(OctoberCutoff, SeptemberCutoff, statements).Should().Be(october);
        CreditCardDebtCalculator.FindStatementFor(new DateOnly(2026, 11, 1), SeptemberCutoff, statements).Should().Be(november);
        CreditCardDebtCalculator.FindStatementFor(new DateOnly(2026, 12, 1), SeptemberCutoff, statements).Should().BeNull();
        CreditCardDebtCalculator.FindStatementFor(SeptemberCutoff, SeptemberCutoff, statements).Should().BeNull();
    }

    [Theory]
    [InlineData(12_000, 100_000, 12)]
    [InlineData(-5_000, 100_000, -5)]
    [InlineData(1, 3, 33.33)]
    public void Percent_RoundsToTwoDecimals(decimal part, decimal whole, decimal expected)
        => CreditCardDebtCalculator.Percent(part, whole).Should().Be(expected);

    [Fact]
    public void Percent_WithNoInitialDebt_IsNull()
        => CreditCardDebtCalculator.Percent(0m, 0m).Should().BeNull();
}
