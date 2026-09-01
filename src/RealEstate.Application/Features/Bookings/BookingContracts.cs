using System.ComponentModel.DataAnnotations;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;

namespace RealEstate.Application.Features.Bookings;

public sealed record CreateBookingRequest([property:Range(1,int.MaxValue)] int PropertyId);
public sealed record ChangeBookingStatusRequest(BookingStatus Status);
public sealed record BookingDto(int Id,int PropertyId,string PropertyTitle,string UserId,string UserName,DateTime BookingDate,BookingStatus Status,DateTime CreatedAt);
