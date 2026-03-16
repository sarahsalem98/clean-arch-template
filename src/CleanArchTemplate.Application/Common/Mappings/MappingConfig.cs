using CleanArchTemplate.Application.Auth.DTOs;
using CleanArchTemplate.Application.Profile.DTOs;
using CleanArchTemplate.Application.Users.DTOs;
using CleanArchTemplate.Domain.Entities;
using Mapster;

namespace CleanArchTemplate.Application.Common.Mappings;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // User → UserDto (auth response)
        config.NewConfig<User, UserDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.ProfileImage, src => src.ProfileImage)
            .Map(dest => dest.ThumbnailImage, src => src.ThumbnailImage)
            .Map(dest => dest.IsVerified, src => src.IsVerified)
            .Map(dest => dest.Status, src => src.Status.ToString().ToLower())
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // User → ProfileDto
        config.NewConfig<User, ProfileDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.ProfileImage, src => src.ProfileImage)
            .Map(dest => dest.ThumbnailImage, src => src.ThumbnailImage)
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
            .Map(dest => dest.Gender, src => src.Gender.HasValue ? src.Gender.Value.ToString().ToLower() : null)
            .Map(dest => dest.Address, src => src.Address)
            .Map(dest => dest.IsVerified, src => src.IsVerified)
            .Map(dest => dest.Status, src => src.Status.ToString().ToLower())
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);

        // User → UserAdminDto
        config.NewConfig<User, UserAdminDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.ProfileImage, src => src.ProfileImage)
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
            .Map(dest => dest.Gender, src => src.Gender.HasValue ? src.Gender.Value.ToString().ToLower() : null)
            .Map(dest => dest.IsVerified, src => src.IsVerified)
            .Map(dest => dest.Status, src => src.Status.ToString().ToLower())
            .Map(dest => dest.Roles, src => src.UserRoles.Select(ur => ur.Role.Name).ToList())
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
            .Map(dest => dest.DeletedAt, src => src.DeletedAt);

        // AuditLog → AuditLogDto
        config.NewConfig<AuditLog, AuditLogDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Action, src => src.Action)
            .Map(dest => dest.EntityName, src => src.EntityName)
            .Map(dest => dest.EntityId, src => src.EntityId)
            .Map(dest => dest.IpAddress, src => src.IpAddress)
            .Map(dest => dest.IsSuccess, src => src.IsSuccess)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}
