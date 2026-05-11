using BancoKRT.Application.Common;
using FluentAssertions;

namespace BancoKRT.Tests.Application.Common;

public class ApplicationResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = ApplicationResult.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ShouldCreateFailureResultWithError()
    {
        var result = ApplicationResult.Failure(ApplicationErrorType.Conflict, "erro");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ApplicationErrorType.Conflict);
        result.Error.Message.Should().Be("erro");
    }

    [Fact]
    public void GenericSuccess_ShouldCreateSuccessfulResultWithValue()
    {
        var result = ApplicationResult<int>.Success(10);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(10);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void GenericFailure_ShouldCreateFailureResultWithoutValue()
    {
        var result = ApplicationResult<int>.Failure(ApplicationErrorType.BusinessRule, "falha");

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().Be(0);
        result.Error.Should().NotBeNull();
        result.Error!.Type.Should().Be(ApplicationErrorType.BusinessRule);
        result.Error.Message.Should().Be("falha");
    }
}
