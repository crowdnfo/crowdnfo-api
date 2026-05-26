using Domain.Assets;
using Domain.Releases;
using Shouldly;

namespace Domain.UnitTests.Releases;

/// <summary>
/// Content-mutating behavior must run only on a persisted release: the id-carrying domain events
/// (<see cref="BestNfoChangedDomainEvent"/>, <see cref="ProductLinkedDomainEvent"/>) snapshot the
/// store-generated int <c>Id</c> at raise time, so raising before the first save would emit id 0.
/// A violation is a wiring bug, hence a throw rather than a Result.
/// </summary>
public class ReleasePersistenceInvariantTests
{
    private static Release CreateTransientRelease() =>
        Release.Create("Some.Release.2026.1080p.GROUP", ReleaseCategory.Movies, createdByUserId: null);

    private static NfoAsset CreateNfoAsset() =>
        NfoAsset.Create(
            contentHash: "hash-1",
            originalFileName: "release.nfo",
            storedFileName: "stored.nfo",
            fileSizeBytes: 1024,
            text: "nfo body");

    [Fact]
    public void SubmitAsset_OnUnpersistedRelease_Throws()
    {
        Release release = CreateTransientRelease();
        release.Id.ShouldBe(0);

        Action submit = () => release.SubmitAsset(CreateNfoAsset(), Guid.NewGuid(), trustScoreAtSubmit: 15);

        submit.ShouldThrow<InvalidOperationException>();
    }

    [Fact]
    public void LinkProduct_OnUnpersistedRelease_Throws()
    {
        Release release = CreateTransientRelease();
        release.Id.ShouldBe(0);

        Action link = () => release.LinkProduct(productId: 42);

        link.ShouldThrow<InvalidOperationException>();
    }
}
