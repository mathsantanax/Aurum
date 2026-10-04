using Aurum.Domain.Entities.Accounts;
using Aurum.Domain.Enums;

namespace Aurum.Domain.Services;

public sealed record ScheduledOccurrence(
    int Number,
    DateOnly TransactionDate,
    DateOnly? DueDate,
    decimal Amount);

public static class TransactionScheduler
{
    public static IReadOnlyList<ScheduledOccurrence> Plan(
        FinancialTransactionRecurrence recurrence,
        int occurrences,
        decimal amount,
        bool amountIsPerInstallment,
        DateOnly transactionDate,
        DateOnly? dueDate,
        int? cardClosingDay = null,
        int? cardDueDay = null)
    {
        if (recurrence == FinancialTransactionRecurrence.None)
            return [new ScheduledOccurrence(1, transactionDate, dueDate, amount)];
        if (occurrences < 2 || occurrences > FinancialTransaction.MaxOccurrences)
            throw new ArgumentOutOfRangeException(
                nameof(occurrences),
                $"Informe entre 2 e {FinancialTransaction.MaxOccurrences} ocorrências.");
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor deve ser maior que zero.");

        var amounts = recurrence == FinancialTransactionRecurrence.Installment && !amountIsPerInstallment
            ? SplitTotal(amount, occurrences)
            : Enumerable.Repeat(amount, occurrences).ToArray();

        DateOnly? firstDue = dueDate;
        var preferredDueDay = dueDate?.Day ?? cardDueDay;
        if (firstDue is null && cardClosingDay.HasValue && cardDueDay.HasValue)
            firstDue = FirstInvoiceDueDate(transactionDate, cardClosingDay.Value, cardDueDay.Value);

        var result = new List<ScheduledOccurrence>(occurrences);
        for (var index = 0; index < occurrences; index++)
        {
            // Sempre calculado a partir da data-base para não "perder" o dia (31 -> 28 -> 28).
            var due = firstDue is null
                ? (DateOnly?)null
                : AddMonthsKeepingDay(firstDue.Value, index, preferredDueDay ?? firstDue.Value.Day);
            result.Add(new ScheduledOccurrence(
                index + 1,
                AddMonthsKeepingDay(transactionDate, index, transactionDate.Day),
                due,
                amounts[index]));
        }

        return result;
    }

    // Divide o total em centavos; a diferença de arredondamento fica na primeira parcela.
    public static decimal[] SplitTotal(decimal total, int count)
    {
        var baseValue = Math.Round(total / count, 2, MidpointRounding.ToZero);
        var values = Enumerable.Repeat(baseValue, count).ToArray();
        values[0] = total - baseValue * (count - 1);
        return values;
    }

    public static DateOnly FirstInvoiceDueDate(DateOnly purchase, int closingDay, int dueDay)
    {
        var closingMonth = new DateOnly(purchase.Year, purchase.Month, 1);
        if (purchase.Day > Math.Min(closingDay, DateTime.DaysInMonth(purchase.Year, purchase.Month)))
            closingMonth = closingMonth.AddMonths(1);

        var dueMonth = dueDay > closingDay ? closingMonth : closingMonth.AddMonths(1);
        return new DateOnly(
            dueMonth.Year,
            dueMonth.Month,
            Math.Min(dueDay, DateTime.DaysInMonth(dueMonth.Year, dueMonth.Month)));
    }

    private static DateOnly AddMonthsKeepingDay(DateOnly baseDate, int months, int preferredDay)
    {
        var target = new DateOnly(baseDate.Year, baseDate.Month, 1).AddMonths(months);
        return new DateOnly(
            target.Year,
            target.Month,
            Math.Min(preferredDay, DateTime.DaysInMonth(target.Year, target.Month)));
    }
}
