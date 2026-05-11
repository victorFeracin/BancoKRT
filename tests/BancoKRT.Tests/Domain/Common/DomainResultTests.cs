using BancoKRT.Domain.Common;
using FluentAssertions;

namespace BancoKRT.Tests.Domain.Common;

public class DomainResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = DomainResult.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailureResultWithError()
    {
        var result = DomainResult.Failure(DomainErrorType.Validation, "erro");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
        result.Error.Message.Should().Be("erro");
    }

    [Fact]
    public void GenericSuccess_ShouldCreateSuccessfulResultWithValue()
    {
        var result = DomainResult<int>.Success(10);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(10);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void GenericFailure_ShouldCreateFailureResultWithoutValue()
    {
        var result = DomainResult<int>.Failure(DomainErrorType.BusinessRule, "falha");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().Be(0);
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(DomainErrorType.BusinessRule);
        result.Error.Message.Should().Be("falha");
    }
}
