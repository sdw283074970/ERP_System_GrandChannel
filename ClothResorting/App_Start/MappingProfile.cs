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
            Mapper.CreateMap<SilkIconPreReceiveOrder, SilkIconPreReceiveOrdersDto>();
            Mapper.CreateMap<SilkIconPackingList, SilkIconPackingListDto>();
            Mapper.CreateMap<Measurement, MeasurementDto>();
            Mapper.CreateMap<SilkIconCartonDetail, SilkIconCartonDetailDto>();
            Mapper.CreateMap<SizeRatio, SizeRatioDto>();
            Mapper.CreateMap<CartonBreakDown, CartonBreakDownDto>();
            Mapper.CreateMap<RetrievingRecord, RetrievingRecordDto>();
        }
    }
}