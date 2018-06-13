using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothResorting.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            Mapper.CreateMap<PreReceiveOrder, SilkIconPreReceiveOrdersDto>();
            Mapper.CreateMap<PackingList, SilkIconPackingListDto>();
            Mapper.CreateMap<Measurement, MeasurementDto>();
            Mapper.CreateMap<CartonDetail, SilkIconCartonDetailDto>();
            Mapper.CreateMap<SizeRatio, SizeRatioDto>();
            Mapper.CreateMap<CartonBreakDown, CartonBreakDownDto>();
            Mapper.CreateMap<RetrievingRecord, RetrievingRecordDto>();
        }
    }
}