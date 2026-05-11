using BancoKRT.Domain.Common;
using BancoKRT.Domain.ValueObjects;
using FluentAssertions;

namespace BancoKRT.Tests.Domain.ValueObjects;

public class CpfTests
{
    [Fact]
    public void Create_ShouldNormalizeFormattedCpf_WhenValueIsValid()
    {
        var result = Cpf.Create("529.982.247-25");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Value.Should().Be("52998224725");
        result.Value.ToString().Should().Be("52998224725");
    }

    [Fact]
    public void Create_ShouldFail_WhenValueIsNull()
    {
        var result = Cpf.Create(null!);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenValueIsEmpty(string value)
    {
        var result = Cpf.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("123456789012")]
    public void Create_ShouldFail_WhenLengthIsInvalid(string value)
    {
        var result = Cpf.Create(value);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenAllDigitsAreEqual()
    {
        var result = Cpf.Create("11111111111");

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenVerifierDigitsAreInvalid()
    {
        var result = Cpf.Create("52998224724");

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }
}
