using MouseShenanigans.Core;

namespace MouseShenanigans.Core.Tests;

public sealed class RemappingProfileJsonParserTests
{
    [Fact]
    public void ParseReturnsValidatedProfileCollection()
    {
        const string json = """
            {
              "profiles": [
                {
                  "name": "horizontal-inversion",
                  "left": { "x": 1, "y": 0 },
                  "right": { "x": -1, "y": 0 },
                  "up": { "x": 0, "y": -1 },
                  "down": { "x": 0, "y": 1 }
                }
              ]
            }
            """;

        RemappingProfileSet collection = RemappingProfileJsonParser.Parse(json);

        RemappingProfile profile = collection.GetRequired("horizontal-inversion");
        Assert.Equal(new MovementVector(1, 0), profile.Left);
        Assert.Equal(new MovementVector(-1, 0), profile.Right);
        Assert.Equal(new MovementVector(0, -1), profile.Up);
        Assert.Equal(new MovementVector(0, 1), profile.Down);
    }

    [Fact]
    public void ParseRejectsMalformedJson()
    {
        Assert.Throws<InvalidDataException>(() => RemappingProfileJsonParser.Parse("{"));
    }

    [Fact]
    public void ParseRejectsDocumentWithoutProfiles()
    {
        Assert.Throws<InvalidDataException>(() => RemappingProfileJsonParser.Parse("""{ "profiles": [] }"""));
    }

    [Fact]
    public void ParseRejectsDuplicateProfileNames()
    {
        const string json = """
            {
              "profiles": [
                {
                  "name": "Invert",
                  "left": { "x": 1, "y": 0 },
                  "right": { "x": -1, "y": 0 },
                  "up": { "x": 0, "y": -1 },
                  "down": { "x": 0, "y": 1 }
                },
                {
                  "name": "invert",
                  "left": { "x": 1, "y": 0 },
                  "right": { "x": -1, "y": 0 },
                  "up": { "x": 0, "y": -1 },
                  "down": { "x": 0, "y": 1 }
                }
              ]
            }
            """;

        Assert.Throws<InvalidDataException>(() => RemappingProfileJsonParser.Parse(json));
    }

    [Fact]
    public void ParseRejectsProfileWithMissingMapping()
    {
        const string json = """
            {
              "profiles": [
                {
                  "name": "incomplete",
                  "left": { "x": 1, "y": 0 },
                  "right": { "x": -1, "y": 0 },
                  "up": { "x": 0, "y": -1 }
                }
              ]
            }
            """;

        Assert.Throws<InvalidDataException>(() => RemappingProfileJsonParser.Parse(json));
    }

    [Fact]
    public void ParseRejectsInvalidVectorValue()
    {
        const string json = """
            {
              "profiles": [
                {
                  "name": "invalid-vector",
                  "left": { "x": 1e999, "y": 0 },
                  "right": { "x": -1, "y": 0 },
                  "up": { "x": 0, "y": -1 },
                  "down": { "x": 0, "y": 1 }
                }
              ]
            }
            """;

        Assert.Throws<InvalidDataException>(() => RemappingProfileJsonParser.Parse(json));
    }
}
