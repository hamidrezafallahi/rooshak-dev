using Application.Dtos;
using Application.Queries;
using Common;
using Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using System.Drawing;
public class GetLandingSlideQueryHandler(ISlideRepository _repo,IEntityConfigRepository _configRepo) 
    : IRequestHandler<GetAllLandingSlidesQuery, ServiceResult<ListDto<LandingSliderDto>>>,
    IRequestHandler<GetLandingSlideByIdQuery, ServiceResult<LandingSliderDto>>,
    IRequestHandler<GetLandingSlideQuery, ServiceResult<IEnumerable<LandingSliderDto>>>
{
    public async Task<ServiceResult<ListDto<LandingSliderDto>>> Handle(GetAllLandingSlidesQuery request, CancellationToken cancellationToken)
        {
        int pageNumber = request.page ?? 1;
        int pageSize = request.pageSize ?? 10;
        IQueryable<Slide> query ;
       
            if (!request.OnlyActives.HasValue || request.OnlyActives == false)
            {
                query = _repo.Query();
            }
            else
            {
                query = _repo.Query(b => b.IsActive);
            }
        int totalCount = await query.CountAsync(cancellationToken);
        var pagedEntity = await query
    .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
            var Dtos = pagedEntity.Select(entity => new LandingSliderDto
            {
                BannerUrl = entity.BannerUrl,
                Id = entity.Id,
                FirstUrl = entity.FirstUrl,
                SecondUrl = entity.SecondUrl,
                VideoUrl = entity.VideoUrl,
                MobileBannerUrl = entity.MobileBannerUrl,
                MobileVideoUrl = entity.MobileVideoUrl,
                BannerTitle = entity.BannerTitle,
                BannerDescription = entity.BannerDescription,
                IsActive= entity.IsActive,
                IsHero = entity.IsHero

            }).ToList();
        dynamic? config = null;

        if (request.ByConfig == true)
        {
            config = await _configRepo.GetByEntityNameAsync("landing");
        }

        var resultDto = new ListDto<LandingSliderDto>
        {
            Records = Dtos,
            ColumnsJson = config?.ColumnsJson,
            ActionsJson = config?.ActionsJson,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        return ServiceResult<ListDto<LandingSliderDto>>.Ok(resultDto);
        }
    public async Task<ServiceResult<LandingSliderDto>> Handle(GetLandingSlideByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id);
        if (entity == null)
            return ServiceResult<LandingSliderDto?>.Failed("Slide not found");
        var Dto = new LandingSliderDto
        {
            Id = entity.Id,
            BannerUrl = entity.BannerUrl,
            FirstUrl = entity.FirstUrl,
            SecondUrl = entity.SecondUrl,
                VideoUrl = entity.VideoUrl,
                MobileBannerUrl = entity.MobileBannerUrl,
                MobileVideoUrl = entity.MobileVideoUrl,
            BannerTitle = entity.BannerTitle,
            BannerDescription = entity.BannerDescription,
            IsActive = entity.IsActive,
            IsHero = entity.IsHero
        };
        return ServiceResult<LandingSliderDto?>.Ok(Dto);
        
    }
    public async Task<ServiceResult<IEnumerable<LandingSliderDto>>> Handle(GetLandingSlideQuery request, CancellationToken cancellationToken)
    {

        IQueryable<Slide> query;

        if (!request.OnlyActives.HasValue || request.OnlyActives == false)
        {

            query = _repo.Query();
        }
        else
        {
            query = _repo.Query(b => b.IsActive);
        }




         
        var slidesDto = query.Select(entity => new LandingSliderDto
        {
            BannerUrl = entity.BannerUrl,
            FirstUrl = entity.FirstUrl,
            SecondUrl = entity.SecondUrl,
                VideoUrl = entity.VideoUrl,
                MobileBannerUrl = entity.MobileBannerUrl,
                MobileVideoUrl = entity.MobileVideoUrl,
            BannerTitle = entity.BannerTitle,
            BannerDescription = entity.BannerDescription,
            IsActive = entity.IsActive,
            IsHero = entity.IsHero
        }).ToList();



        return ServiceResult<IEnumerable<LandingSliderDto>>.Ok(slidesDto);
    }


}
