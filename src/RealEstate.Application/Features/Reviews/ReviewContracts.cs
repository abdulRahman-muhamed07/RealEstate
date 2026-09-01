using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.Features.Reviews;

public sealed record CreateReviewRequest([property:Range(1,int.MaxValue)] int PropertyId,[property:Range(1,5)] int Rating,[property:Required,StringLength(1000)] string Comment);
public sealed record UpdateReviewRequest([property:Range(1,5)] int Rating,[property:Required,StringLength(1000)] string Comment);
public sealed record ReviewDto(int Id,string UserId,string UserName,int Rating,string Comment,DateTime CreatedAt);
