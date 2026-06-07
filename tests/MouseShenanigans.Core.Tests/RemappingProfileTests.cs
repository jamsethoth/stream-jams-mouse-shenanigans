using MouseShenanigans.Core;

namespace MouseShenanigans.Core.Tests;

public sealed class RemappingProfileTests
{
    [Fact]
    public void ProfileAcceptsAllDirectionalMappings()
    {
        RemappingProfile profile = CreateIdentityProfile("identity");

        Assert.Equal("identity", profile.Name);
        Assert.Equal(new MovementVector(-1, 0), profile.Left);
        Assert.Equal(new MovementVector(1, 0), profile.Right);
        Assert.Equal(new MovementVector(0, -1), profile.Up);
        Assert.Equal(new MovementVector(0, 1), profile.Down);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ProfileRejectsEmptyName(string name)
    {
        Assert.Throws<ArgumentException>(() => CreateIdentityProfile(name));
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, 0)]
    [InlineData(0, double.NegativeInfinity)]
    public void VectorRejectsNonFiniteCoordinates(double x, double y)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MovementVector(x, y));
    }

    [Fact]
    public void CollectionRejectsDuplicateNamesIgnoringCase()
    {
        RemappingProfile first = CreateIdentityProfile("Invert");
        RemappingProfile second = CreateIdentityProfile("invert");

        Assert.Throws<ArgumentException>(() => RemappingProfileSet.Create([first, second]));
    }

    [Fact]
    public void CollectionLooksUpConfiguredProfileName()
    {
        RemappingProfile profile = CreateIdentityProfile("horizontal-inversion");
        RemappingProfileSet collection = RemappingProfileSet.Create([profile]);

        RemappingProfile resolved = collection.GetRequired("HORIZONTAL-INVERSION");

        Assert.Same(profile, resolved);
    }

    [Fact]
    public void CollectionDoesNotFallbackWhenProfileNameIsAbsent()
    {
        RemappingProfile profile = CreateIdentityProfile("horizontal-inversion");
        RemappingProfileSet collection = RemappingProfileSet.Create([profile]);

        Assert.Throws<KeyNotFoundException>(() => collection.GetRequired("missing"));
    }

    private static RemappingProfile CreateIdentityProfile(string name)
    {
        return new RemappingProfile(
            name,
            left: new MovementVector(-1, 0),
            right: new MovementVector(1, 0),
            up: new MovementVector(0, -1),
            down: new MovementVector(0, 1));
    }
}
