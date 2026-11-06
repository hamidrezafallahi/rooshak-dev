using Application.Common;
using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Enums;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace Application.Handler.QueryHandler
{
    public class UserQueryHandler(IUserRepository _repo, IEntityConfigRepository _configRepo, IUserAddressRepository _userAddressRepository,IRateRepository _rateRepo, IHttpContextAccessor _accessor) : 
        IRequestHandler<GetUsersQuery, ServiceResult<ListDto<UserDto>>>,
        IRequestHandler<GetUserByIdQuery, ServiceResult<UserDto?>>,
        IRequestHandler<GetAddressesQuery, ServiceResult<ListDto<UserAddressDto>>>,
        IRequestHandler<GetUserAddressByIdQuery, ServiceResult<UserAddressDto>>,
        IRequestHandler<GetUserAddressByUserQuery, ServiceResult<List<UserAddressDto>>>,
        IRequestHandler<GetUserAddressByUserIdQuery, ServiceResult<List<UserAddressDto>>>,
        IRequestHandler<GetDefaultUserAddressByUserIdQuery, ServiceResult<UserAddressDto?>>,
        IRequestHandler<GetUsers4selectOptionQuery, ServiceResult<ListDto<SelectOptionDto>>>,
        IRequestHandler<GetAllUsersSlugsQuery, ServiceResult<IEnumerable<SlugDto>>>
    {

        public async Task<ServiceResult<ListDto<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            string filter = request.Q;
            IQueryable<User> query;
            if (request.Q is not null && request.Q.Length > 0)
            {
                if (!request.OnlyActives.HasValue || request.OnlyActives == false)
                {
                    query = _repo.Query(b => b.FullName.Contains(request.Q));
                }
                else
                {
                    query = _repo.Query(b => b.IsActive && b.FullName.Contains(request.Q));
                }
            }
            else
            {
                if (!request.OnlyActives.HasValue || request.OnlyActives == false)
                {

                    query = _repo.Query();
                }
                else
                {
                    query = _repo.Query(b => b.IsActive);
                }
            }
            int totalCount = await query.CountAsync(cancellationToken);
            var pagedUsers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);


            var usersDto = pagedUsers.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Slug = u.Slug,
                PhoneNumber = u.PhoneNumber,
                UserDescription = u.UserDescription,
                IsActive=u.IsActive,
                UserImage = u.Image.TrimStart('/'),
            }).ToList();
            dynamic? config = null;

            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("users");
            }
 
            var resultDto = new ListDto<UserDto>
            {
                Records = usersDto,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            return ServiceResult<ListDto<UserDto>>.Ok(resultDto);

 
        }
        public async Task<ServiceResult<UserDto?>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var key = (request.IdOrSlug ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(key))
                return ServiceResult<UserDto?>.Failed("کاربر موردنظر پیدا نشد.");

            IQueryable<User> query = _repo.Query(u => !u.IsDeleted);
            if (int.TryParse(key, out var id))
                query = query.Where(u => u.Id == id);
            else
                query = query.Where(u => u.Slug == key);

            var userDto = await query
            .Select(us => new UserDto
            {
                Id = us.Id,
                Email = us.Email,
                FullName = us.FullName,
                Slug = us.Slug,
                PhoneNumber = us.PhoneNumber,
                UserImage = us.Image.TrimStart('/'),
                UserDescription = us.UserDescription,
            })
            .FirstOrDefaultAsync(cancellationToken);
            if (userDto is not null)
            {
                var rate = await _rateRepo.GetAverageRateAsync(EnumTargetType.Supplier, userDto.Id);
                if (rate is not null)
                {
                    userDto.RateCount = rate.Count;
                    userDto.AverageRate = rate.Average;
                }
            }
            return ServiceResult<UserDto?>.Ok(userDto);
        }

        public async Task<ServiceResult<IEnumerable<SlugDto>>> Handle(GetAllUsersSlugsQuery request, CancellationToken cancellationToken)
        {
            var slugs = await _repo.Query(u => u.IsActive && !u.IsDeleted && !string.IsNullOrWhiteSpace(u.Slug))
                .Select(u => new SlugDto
                {
                    Id = u.Id,
                    Slug = u.Slug,
                    UpdatedAt = u.UpdatedAt ?? u.CreatedAt,
                    ImageUrls = string.IsNullOrWhiteSpace(u.Image)
                        ? new List<string>()
                        : new List<string> { u.Image.TrimStart('/') },
                })
                .ToListAsync(cancellationToken);
            return ServiceResult<IEnumerable<SlugDto>>.Ok(slugs);
        }
        public async Task<ServiceResult<ListDto<UserAddressDto>>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            string filter = request.Q;
            IQueryable<UserAddress> query;
            if (request.Q is not null && request.Q.Length > 0)
            {
                if (!request.OnlyActives.HasValue || request.OnlyActives == false)
                {
                    query = _userAddressRepository.Query(b => b.FullAddress.Contains(request.Q)).Include(x=>x.User);
                }
                else
                {
                    query = _userAddressRepository.Query(b => b.IsActive && b.FullAddress.Contains(request.Q)).Include(x => x.User);
                }
            }
            else
            {
                if (!request.OnlyActives.HasValue || request.OnlyActives == false)
                {

                    query = _userAddressRepository.Query().Include(x => x.User);
                }
                else
                {
                    query = _userAddressRepository.Query(b => b.IsActive).Include(x => x.User);
                }
            }
            int totalCount = await query.CountAsync(cancellationToken);
            var pagedUsers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);


            var addressesDto = pagedUsers.Select(a => new UserAddressDto
            {
                Id = a.Id,
                City = a.City,
                PhoneNumber = a.PhoneNumber,
                Name = a.Name,
                FullAddress = a.FullAddress,
                IsDefault = a.IsDefault,
                PostalCode = a.PostalCode,
                State = a.State,
                IsActive=a.IsActive,
                UserId = a.Id,
                UserName=a.User.FullName

            }).ToList();
            dynamic? config = null;

            if (request.ByConfig == true)
            {
                config = await _configRepo.GetByEntityNameAsync("address");
            }

            var resultDto = new ListDto<UserAddressDto>
            {
                Records = addressesDto,
                ColumnsJson = config?.ColumnsJson,
                ActionsJson = config?.ActionsJson,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            return ServiceResult<ListDto<UserAddressDto>>.Ok(resultDto);
           
        }
        public async Task<ServiceResult<UserAddressDto>> Handle(GetUserAddressByIdQuery request, CancellationToken cancellationToken)
        {
            var address = _userAddressRepository
                .Query()
                .Where(a => a.Id == request.Id)
                 .Select(us => new UserAddressDto
                 {
                     Id = us.Id,
                     City = us.City,
                     PhoneNumber = us.PhoneNumber,
                     Name = us.Name,
                     FullAddress = us.FullAddress,
                     IsDefault = us.IsDefault,
                     PostalCode = us.PostalCode,
                     State = us.State,
                     UserId = us.Id,
                 })
                .FirstOrDefault();


            return ServiceResult<UserAddressDto>.Ok(address);
        }
        public async Task<ServiceResult<List<UserAddressDto>>> Handle(GetUserAddressByUserIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _userAddressRepository
                .Query(a => a.UserId == request.UserId)
                .ToListAsync(cancellationToken);
            var dtoList = data
           .Select(us => new UserAddressDto
           {
               Id = us.Id,
               City = us.City,
               PhoneNumber = us.PhoneNumber,
               Name = us.Name,
               FullAddress = us.FullAddress,
               IsDefault = us.IsDefault,
               PostalCode = us.PostalCode,
               State = us.State,
               UserId = us.Id,
           }).ToList();

            return ServiceResult<List<UserAddressDto>>.Ok(dtoList);
        }
        public async Task<ServiceResult<List<UserAddressDto>>> Handle(GetUserAddressByUserQuery request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<List<UserAddressDto>>.Unauthorized();
            var data = await _userAddressRepository
                .Query(a => a.UserId == userId.Value)
                .ToListAsync(cancellationToken);
            var dtoList = data
           .Select(us => new UserAddressDto
           {
               Id = us.Id,
               City = us.City,
               PhoneNumber = us.PhoneNumber,
               Name = us.Name,
               FullAddress = us.FullAddress,
               IsDefault = us.IsDefault,
               PostalCode = us.PostalCode,
               State = us.State,
               UserId = us.Id,
           }).ToList();

            return ServiceResult<List<UserAddressDto>>.Ok(dtoList);
        }

        public async Task<ServiceResult<UserAddressDto?>> Handle(GetDefaultUserAddressByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _accessor.HttpContext.GetUserId();
            if (userId == null)
                return ServiceResult<UserAddressDto>.Unauthorized();
            var data = await _userAddressRepository
                .Query()
                 .Include(a => a.User)
                .Where(a => a.UserId == userId && a.IsDefault && !a.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
                return ServiceResult<UserAddressDto?>.Failed("آدرس پیش‌فرض یافت نشد");
            var dto = new UserAddressDto
            {
                Id = data.Id,
                City = data.City,
                PhoneNumber = data.PhoneNumber,
                Name = data.Name,
                FullAddress = data.FullAddress,
                IsDefault = data.IsDefault,
                PostalCode = data.PostalCode,
                State = data.State,
                UserId = data.User.Id,
            };

            return ServiceResult<UserAddressDto?>.Ok(dto);
        }
        public async Task<ServiceResult<ListDto<SelectOptionDto>>> Handle(GetUsers4selectOptionQuery request, CancellationToken ct)
        {
            int pageNumber = request.page ?? 1;
            int pageSize = request.pageSize ?? 10;
            int currentId = request.UserId ?? 0;
            IQueryable<User> query;
            query = _repo.Query(c => c.IsActive && c.Id != currentId);
            int totalCount = await query.CountAsync(c => c.IsActive);
            var pagedUsers = await query
        .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync(ct);
            var flatDtos = pagedUsers.Select(c => new SelectOptionDto
            {
                Id = c.Id,
                PersianLabel = c.FullName,
                EnglishLabel = c.FullName
            }).ToList();
            var resultDto = new ListDto<SelectOptionDto>
            {
                Records = flatDtos,
                ColumnsJson = null,
                ActionsJson = null,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
            };

            return ServiceResult<ListDto<SelectOptionDto>>.Ok(resultDto);

        }

    }
 }
