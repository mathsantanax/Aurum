using Aurum.Domain.Entities.Workspace;
using Aurum.Domain.Enums;
using Aurum.Infrastructure.Identity;
using Aurum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Aurum.Tests;

public sealed class WalletspaceMemberPersistenceTests
{
    [Fact]
    public async Task AddMember_to_tracked_walletspace_persists_the_new_member()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<AurumDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new AurumDbContext(options);
        db.Users.AddRange(
            new AurumUser
            {
                Id = ownerId,
                UserName = $"owner-{ownerId}",
                NormalizedUserName = $"OWNER-{ownerId}"
            },
            new AurumUser
            {
                Id = memberId,
                UserName = $"member-{memberId}",
                NormalizedUserName = $"MEMBER-{memberId}"
            });

        var space = new Walletspace("Casa", ownerId);
        db.Walletspaces.Add(space);
        await db.SaveChangesAsync();

        var trackedSpace = await db.Walletspaces
            .Include(item => item.Members)
            .SingleAsync(item => item.Id == space.Id);
        var member = new WalletspaceMember(trackedSpace.Id, memberId, WalletspaceRole.Member);
        trackedSpace.AddMember(member);
        db.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, db.Entry(member).State);

        db.WalletspaceMembers.Add(member);
        Assert.Equal(EntityState.Added, db.Entry(member).State);

        await db.SaveChangesAsync();

        var savedMember = await db.WalletspaceMembers
            .AsNoTracking()
            .SingleAsync(item => item.WalletspaceId == trackedSpace.Id && item.UserId == memberId);
        Assert.Equal(WalletspaceRole.Member, savedMember.Role);
    }
}
