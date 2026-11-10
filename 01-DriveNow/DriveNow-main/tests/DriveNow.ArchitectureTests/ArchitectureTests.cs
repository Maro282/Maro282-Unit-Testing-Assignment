
using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace DriveNow.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly =
        Assembly.Load("DriveNow.Domain");

    private static readonly Assembly ApplicationAssembly =
        Assembly.Load("DriveNow.Application");

    private static readonly Assembly InfrastructureAssembly =
        Assembly.Load("DriveNow.Infrastructure");

    private static readonly Assembly ConsoleAppAssembly =
        Assembly.Load("DriveNow.ConsoleApp");

    private static readonly Assembly[] ProductionAssemblies =
    [
        DomainAssembly,
        ApplicationAssembly,
        InfrastructureAssembly,
        ConsoleAppAssembly
    ];

    // R1: Domain must not depend on another DriveNow project.
    [Fact]
    public void Domain_HasNoDriveNowProjectDependencies_PassesArchitectureRule()
    {
        ShouldNotDependOn(DomainAssembly, "DriveNow.Application");
        ShouldNotDependOn(DomainAssembly, "DriveNow.Infrastructure");
        ShouldNotDependOn(DomainAssembly, "DriveNow.ConsoleApp");
    }

    // R2: Application may depend only on Domain.
    [Fact]
    public void Application_DependsOnlyOnDomain_PassesArchitectureRule()
    {
        ShouldNotDependOn(ApplicationAssembly, "DriveNow.Infrastructure");
        ShouldNotDependOn(ApplicationAssembly, "DriveNow.ConsoleApp");
    }

    // R3: Infrastructure must not depend on ConsoleApp.
    [Fact]
    public void Infrastructure_DoesNotDependOnConsoleApp_PassesArchitectureRule()
    {
        ShouldNotDependOn(InfrastructureAssembly, "DriveNow.ConsoleApp");
    }

    // R4: Production naming conventions.
    [Fact]
    public void ProductionTypes_FollowNamingConventions_PassesArchitectureRule()
    {
        var types = ProductionAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                       (type.IsClass || type.IsInterface) &&
                       !type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false))
             .ToList();

        // Interfaces must start with I.
        var invalidInterfaces = types
            .Where(type => type.IsInterface &&
                           !type.Name.StartsWith("I"))
            .ToList();

        // Repository classes must end with Repository
        // and live in a Repositories namespace.
        var invalidRepositories = types
            .Where(type => type.IsClass &&
                (type.Name.Contains("Repository") ||
                 type.Namespace?.Split('.').Contains("Repositories") == true))
            .Where(type =>
                !type.Name.EndsWith("Repository") ||
                !type.Namespace!.Split('.').Contains("Repositories"))
            .ToList();

        // Service classes must end with Service
        // and live in a Services namespace.
        var invalidServices = types
            .Where(type => type.IsClass &&
                (type.Name.Contains("Service") ||
                 type.Namespace?.Split('.').Contains("Services") == true))
            .Where(type =>
                !type.Name.EndsWith("Service") ||
                !type.Namespace!.Split('.').Contains("Services"))
            .ToList();

        // Exception classes must end with Exception
        // and live in DriveNow.Domain.Exceptions.
        var invalidExceptions = types
            .Where(type => type.IsClass &&
                           type != typeof(Exception) &&
                           typeof(Exception).IsAssignableFrom(type))
            .Where(type =>
                !type.Name.EndsWith("Exception") ||
                type.Namespace != "DriveNow.Domain.Exceptions")
            .ToList();

        var errors = new List<string>();

        errors.AddRange(invalidInterfaces.Select(type =>
            $"Interface: {type.FullName} must start with I"));

        errors.AddRange(invalidRepositories.Select(type =>
            $"Repository: {type.FullName} must end with Repository and live in a Repositories namespace"));

        errors.AddRange(invalidServices.Select(type =>
            $"Service: {type.FullName} must end with Service and live in a Services namespace"));

        errors.AddRange(invalidExceptions.Select(type =>
            $"Exception: {type.FullName} must end with Exception and live in DriveNow.Domain.Exceptions"));

        Assert.True(
            errors.Count == 0,
            "Production naming violations:" + Environment.NewLine +
            string.Join(Environment.NewLine, errors));
    }

    // R5: Unit test naming conventions.
    [Fact]
    public void UnitTests_FollowTestNamingConventions_PassesArchitectureRule()
    {
        var unitTestsAssembly = Assembly.Load("DriveNow.UnitTests");

        var allTypes = unitTestsAssembly.GetTypes();

        var testMethods = allTypes
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static))
            .Where(IsFactOrTheory)
            .ToList();

        var testClasses = testMethods
            .Select(method => method.DeclaringType!)
            .Distinct()
            .ToList();

        // Every test class must end with Tests.
        var invalidTestClasses = testClasses
            .Where(type => !type.Name.EndsWith("Tests"))
            .ToList();

        // Method_Scenario_ExpectedResult
        var methodPattern =
            @"^[A-Za-z][A-Za-z0-9]*_[A-Za-z][A-Za-z0-9]*_[A-Za-z][A-Za-z0-9]*$";

        var invalidTestMethods = testMethods
            .Where(method =>
                !System.Text.RegularExpressions.Regex.IsMatch(
                    method.Name,
                    methodPattern))
            .ToList();

        var errors = new List<string>();

        errors.AddRange(invalidTestClasses.Select(type =>
            $"Test class: {type.FullName} must end with Tests"));

        errors.AddRange(invalidTestMethods.Select(method =>
            $"Test method: {method.DeclaringType?.FullName}.{method.Name} does not match the naming pattern"));

        Assert.True(
            errors.Count == 0,
            "Test naming violations:" + Environment.NewLine +
            string.Join(Environment.NewLine, errors));
    }



    private static void ShouldNotDependOn(
        Assembly assembly,
        string forbiddenNamespace)
    {
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(forbiddenNamespace)
            .GetResult();

        var failingTypes = result.FailingTypeNames ?? Array.Empty<string>();

        Assert.True(
            result.IsSuccessful,
            $"{assembly.GetName().Name} must not depend on {forbiddenNamespace}. " +
            $"Violations: {string.Join(", ", failingTypes)}");
    }


    private static bool IsFactOrTheory(MethodInfo method)
    {
        return method
            .GetCustomAttributes(inherit: true)
            .Any(attribute =>
                attribute.GetType().FullName == "Xunit.FactAttribute" ||
                attribute.GetType().FullName == "Xunit.TheoryAttribute");
    }
}
