using NetArchTest.Rules;

namespace Collective.Api.ArchitectureTests;

/// <summary>
/// Fronteras de PLAN_BACKEND.md 3.2. Hoy solo hay un modulo, pero la regla ya
/// rige: el dia que aparezca el segundo, este test la hara cumplir sola.
/// </summary>
public sealed class ModuleBoundariesTests
{
    private static readonly System.Reflection.Assembly Api = typeof(Program).Assembly;

    [Fact]
    public void Common_does_not_depend_on_any_module()
    {
        var result = Types.InAssembly(Api)
            .That().ResideInNamespaceStartingWith("Collective.Api.Common")
            .ShouldNot().HaveDependencyOn("Collective.Api.Modules")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Common_does_not_depend_on_infrastructure()
    {
        var result = Types.InAssembly(Api)
            .That().ResideInNamespaceStartingWith("Collective.Api.Common")
            .ShouldNot().HaveDependencyOn("Collective.Api.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result) =>
        result.FailingTypeNames is null
            ? "OK"
            : "Tipos que cruzan la frontera: " + string.Join(", ", result.FailingTypeNames);
}
