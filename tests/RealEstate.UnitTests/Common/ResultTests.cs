using RealEstate.Application.Common;

namespace RealEstate.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Ok_ShouldReturnSuccessfulResult()
    {
        var result = Result<int>.Ok(42);

        Assert.True(result.Success);
        Assert.Equal(42, result.Data);
        Assert.Equal(ErrorCode.None, result.ErrorCode);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Fail_ShouldReturnErrorWithoutHttpCoupling()
    {
        var result = Result<int>.Fail(ErrorCode.NotFound, "Property not found.");

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Equal(ErrorCode.NotFound, result.ErrorCode);
        Assert.Equal("Property not found.", result.Error);
    }
}
