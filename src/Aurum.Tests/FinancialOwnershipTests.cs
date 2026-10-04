using Aurum.Application.Financial;
using Aurum.Domain.Entities.Accounts;
using Aurum.Domain.Entities.Workspace;
using Aurum.Domain.Enums;
using Aurum.Infrastructure.Identity;
using Aurum.Infrastructure.Persistence;
using Aurum.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Tests;

public sealed class FinancialOwnershipTests
{
    [Fact]
    public void FinancialAccount_is_owned_by_a_user()
    {
        var ownerId = Guid.NewGuid();
        var account = new FinancialAccount(
            ownerId,
            "Conta pessoal",
            FinancialAccountType.Checking,
            0,
            null,
            ownerId);

        Assert.Equal(ownerId, account.OwnerUserId);
    }

    [Fact]
    public void FinancialTransaction_requires_exactly_one_account_or_card()
    {
        var ownerId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => CreateTransaction(ownerId, null, null));
        Assert.Throws<ArgumentException>(() => CreateTransaction(ownerId, resourceId, resourceId));
        Assert.Equal(ownerId, CreateTransaction(ownerId, resourceId, null).OwnerUserId);
    }

    [Fact]
    public void Walletspace_transfers_ownership_without_creating_two_owners()
    {
        var ownerId = Guid.NewGuid();
        var newOwnerId = Guid.NewGuid();
        var space = new Walletspace("Casa", ownerId);
        space.AddMember(new WalletspaceMember(space.Id, newOwnerId, WalletspaceRole.Member));

        space.TransferOwnership(newOwnerId, ownerId);

        Assert.Equal(WalletspaceRole.Admin, space.Members.Single(member => member.UserId == ownerId).Role);
        Assert.Equal(WalletspaceRole.Owner, space.Members.Single(member => member.UserId == newOwnerId).Role);
        Assert.Single(space.Members, member => member.Role == WalletspaceRole.Owner);
    }

    [Fact]
    public void Walletspace_cannot_transfer_ownership_to_its_current_owner()
    {
        var ownerId = Guid.NewGuid();
        var space = new Walletspace("Casa", ownerId);

        Assert.Throws<ArgumentException>(() => space.TransferOwnership(ownerId, ownerId));
        Assert.Equal(WalletspaceRole.Owner, Assert.Single(space.Members).Role);
    }

    [Fact]
    public async Task Transactions_are_private_until_owner_shares_them_and_membership_is_required()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var outsiderId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<AurumDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AurumDbContext(options);
        db.Users.AddRange(
            new AurumUser { Id = ownerId, UserName = $"owner-{ownerId}", NormalizedUserName = $"OWNER-{ownerId}" },
            new AurumUser { Id = memberId, UserName = $"member-{memberId}", NormalizedUserName = $"MEMBER-{memberId}" },
            new AurumUser { Id = outsiderId, UserName = $"outsider-{outsiderId}", NormalizedUserName = $"OUTSIDER-{outsiderId}" });
        var space = new Walletspace("Casa", ownerId);
        space.AddMember(new WalletspaceMember(space.Id, memberId, WalletspaceRole.Member));
        db.Walletspaces.Add(space);
        await db.SaveChangesAsync();

        var useCases = new FinancialUseCases(new FinancialRepository(db));
        var account = await useCases.CreateAccountAsync(
            ownerId,
            new FinancialAccountInput("Conta privada", FinancialAccountType.Checking, 100, null),
            CancellationToken.None);
        var created = await useCases.CreateTransactionsAsync(
            ownerId,
            new FinancialTransactionInput(
                account.Account.Id,
                null,
                "Mercado",
                "Casa",
                25,
                FinancialTransactionType.Expense,
                FinancialTransactionStatus.Paid,
                new DateOnly(2026, 10, 1),
                null,
                FinancialTransactionRecurrence.None,
                null,
                false),
            CancellationToken.None);

        Assert.Single(await useCases.ListTransactionsAsync(ownerId, null, null, CancellationToken.None));
        Assert.Empty(await useCases.ListWalletspaceTransactionsAsync(
            memberId, space.Id, null, null, CancellationToken.None));
        await Assert.ThrowsAsync<FinancialUseCaseException>(() =>
            useCases.ListWalletspaceTransactionsAsync(outsiderId, space.Id, null, null, CancellationToken.None));
        await Assert.ThrowsAsync<FinancialUseCaseException>(() =>
            useCases.ShareTransactionAsync(outsiderId, created[0].Id, space.Id, CancellationToken.None));

        await useCases.ShareTransactionAsync(ownerId, created[0].Id, space.Id, CancellationToken.None);

        var shared = await useCases.ListWalletspaceTransactionsAsync(
            memberId, space.Id, null, null, CancellationToken.None);
        Assert.Single(shared);
        Assert.Equal("Mercado", shared[0].Transaction.Description);
        Assert.Equal(ownerId, shared[0].Transaction.OwnerUserId);
        Assert.Empty(await useCases.ListTransactionsAsync(memberId, null, null, CancellationToken.None));
    }

    private static FinancialTransaction CreateTransaction(
        Guid ownerId,
        Guid? accountId,
        Guid? cardId) =>
        new(
            ownerId,
            accountId,
            cardId,
            "Teste",
            null,
            10,
            FinancialTransactionType.Expense,
            FinancialTransactionStatus.Pending,
            new DateOnly(2026, 10, 1),
            null,
            ownerId);
}
