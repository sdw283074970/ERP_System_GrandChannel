using AutoMapper;
using ClothResorting.Dtos;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
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
            //DefaultConnection
            Mapper.CreateMap<PreReceiveOrder, PreReceiveOrdersDto>();
            Mapper.CreateMap<PurchaseOrderSummary, PurchaseOrderSummaryDto>();
            Mapper.CreateMap<Measurement, MeasurementDto>();
            Mapper.CreateMap<CartonDetail, CartonDetailDto>();
            Mapper.CreateMap<SizeRatio, SizeRatioDto>();
            Mapper.CreateMap<CartonBreakDown, CartonBreakDownDto>();
            Mapper.CreateMap<ReplenishmentLocationDetail, ReplenishmentLocationDetailDto>();
            Mapper.CreateMap<PermanentLocIORecord, PermanentLocIORecordDto>();
            Mapper.CreateMap<PermanentLocation, PermanentLocationDto>();
            Mapper.CreateMap<PurchaseOrderInventory, PurchaseOrderInventoryDto>();
            Mapper.CreateMap<SpeciesInventory, SpeciesInventoryDto>();
            Mapper.CreateMap<AdjustmentRecord, AdjustmentRecordDto>();
            Mapper.CreateMap<POSummary, POSummaryDto>();
            Mapper.CreateMap<RegularCartonDetail, RegularCartonDetailDto>();
            Mapper.CreateMap<FCRegularLocationDetail, FCRegularLocationDetailDto>();
            Mapper.CreateMap<ShipOrder, ShipOrderDto>();
            Mapper.CreateMap<ShipOrder, ShipOrderDto>();
            Mapper.CreateMap<PickDetail, PickDetailDto>();
            Mapper.CreateMap<PullSheetDiagnostic, PullSheetDiagnosticDto>();
            Mapper.CreateMap<Container, ContainerDto>();
            Mapper.CreateMap<GeneralLocationSummary, GeneralLocationSummaryDto>();
            Mapper.CreateMap<UpperVendor, UpperVendorDto>();

            //FBAConnection
            Mapper.CreateMap<ChargeTemplate, ChargeTemplateDto>();
            Mapper.CreateMap<ChargeMethod, ChargeMethodDto>();
        }
    }
}