using System.Reflection;

namespace RealEstate.UnitTests.Architecture;

public sealed class ArchitectureDependencyTests
{
    [Fact]
    public void Domain_does_not_reference_outer_layers_or_frameworks()
    {
        var references = ReferencedAssemblyNames(typeof(Domain.Entities.Property).Assembly);

        Assert.DoesNotContain("RealEstate.Application", references);
        Assert.DoesNotContain("RealEstate.Infrastructure", references);
        Assert.DoesNotContain("RealEstate.Api", references);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", references);
    }

    [Fact]
    public void Application_does_not_reference_infrastructure_or_api()
    {
        var references = ReferencedAssemblyNames(typeof(Application.Interfaces.IPropertyQueryService).Assembly);

        Assert.DoesNotContain("RealEstate.Infrastructure", references);
        Assert.DoesNotContain("RealEstate.Api", references);
    }

    [Fact]
    public void Infrastructure_does_not_reference_api()
    {
        var references = ReferencedAssemblyNames(typeof(Infrastructure.Persistence.AppDbContext).Assembly);

        Assert.DoesNotContain("RealEstate.Api", references);
    }

    private static HashSet<string> ReferencedAssemblyNames(Assembly assembly) =>
        assembly.GetReferencedAssemblies()
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase)!;
}
