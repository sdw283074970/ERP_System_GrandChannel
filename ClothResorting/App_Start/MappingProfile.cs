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
            Mapper.CreateMap<PreReceiveOrder, PreReceiveOrdersDto>();
            Mapper.CreateMap<PurchaseOrderSummary, PurchaseOrderSummaryDto>();
            Mapper.CreateMap<Measurement, MeasurementDto>();
            Mapper.CreateMap<CartonDetail, CartonDetailDto>();
            Mapper.CreateMap<SizeRatio, SizeRatioDto>();
            Mapper.CreateMap<CartonBreakDown, CartonBreakDownDto>();
            Mapper.CreateMap<LocationDetail, LocationDetailDto>();
            Mapper.CreateMap<PermanentLocIORecord, PermanentLocIORecordDto>();
            Mapper.CreateMap<PermanentLocation, PermanentLocationDto>();
            Mapper.CreateMap<PurchaseOrderInventory, PurchaseOrderInventoryDto>();
            Mapper.CreateMap<SpeciesInventory, SpeciesInventoryDto>();
            Mapper.CreateMap<AdjustmentRecord, AdjustmentRecordDto>();
            Mapper.CreateMap<POSummary, POSummaryDto>();
            Mapper.CreateMap<RegularCartonDetail, RegularCartonDetailDto>();
            Mapper.CreateMap<FCRegularLocationDto, FCRegularLocationDto>();
        }
    }
}