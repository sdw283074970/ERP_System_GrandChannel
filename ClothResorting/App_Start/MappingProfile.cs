using AutoMapper;
using ClothResorting.Controllers.Api.Warehouse;
using ClothResorting.Dtos;
using ClothResorting.Dtos.Fba;
using ClothResorting.Models;
using ClothResorting.Models.FBAModels;
using Microsoft.AspNet.Identity.EntityFramework;
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
            //Mapper.CreateMap<PermanentLocIORecord, PermanentLocIORecordDto>();
            //Mapper.CreateMap<PermanentLocation, PermanentLocationDto>();
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
            Mapper.CreateMap<UpperVendor, UpperVendorDto>()
                .ForMember(dest => dest.LinkedAccount, opt => opt.MapFrom(src => src.ApplicationUser.UserName));
            Mapper.CreateMap<ChargingItem, ChargingItemDto>();
            Mapper.CreateMap<Invoice, InvoiceDto>();
            Mapper.CreateMap<InvoiceDetail, InvoiceDetailDto>();
            Mapper.CreateMap<PermanentSKU, PermanentSKUDto>();
            Mapper.CreateMap<OutboundHistory, OutboundHistoryDto>();
            Mapper.CreateMap<NameCrossReference, NameCrossReferenceDto>();

            //FBA(Under DefaultConnection)
            Mapper.CreateMap<FBAMasterOrder, FBAMasterOrderDto>()
                .ForMember(dest => dest.ActualCtns, opt => opt.MapFrom(src => src.FBAOrderDetails == null ? 0 : src.FBAOrderDetails.Sum(x => x.ActualQuantity)))
                .ForMember(dest => dest.ActualPlts, opt => opt.MapFrom(src => src.FBAPallets == null ? 0 : src.FBAPallets.Sum(x => x.ActualPallets))) 
                .ForMember(dest => dest.TotalCtns, opt => opt.MapFrom(src => src.FBAOrderDetails == null ? 0 : src.FBAOrderDetails.Sum(x => x.Quantity)))
                .ForMember(dest => dest.SKUNumber, opt => opt.MapFrom(src => src.FBAOrderDetails == null ? 0 : src.FBAOrderDetails.GroupBy(x => x.ShipmentId).Count()))
                .ForMember(dest => dest.ActualCBM, opt => opt.MapFrom(src => src.FBAOrderDetails == null ? 0 : src.FBAOrderDetails.Sum(x => x.ActualCBM)))
                .ForMember(dest => dest.TotalCBM, opt => opt.MapFrom(src => src.FBAOrderDetails == null ? 0 : src.FBAOrderDetails.Sum(x => x.CBM)))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.InvoiceDetails == null ? 0 : src.InvoiceDetails.Sum(x => x.Cost)))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.InvoiceDetails == null ? 0 : src.InvoiceDetails.Sum(x => x.Amount)))
                .ForMember(dest => dest.Net, opt => opt.MapFrom(src => src.InvoiceDetails == null ? 0 : (src.InvoiceDetails.Sum(x => x.Amount) - src.InvoiceDetails.Sum(x => x.Cost))));
            Mapper.CreateMap<FBAOrderDetail, FBAOrderDetailDto>()
                .ForMember(dest => dest.LabelFileNumbers, opt => opt.MapFrom(src => src.LabelFiles.Split('{').Length - 1));
            Mapper.CreateMap<FBACartonLocation, FBACartonLocationDto>()
                .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.FBAOrderDetail.Barcode))
                .ForMember(dest => dest.LabelFileNumbers, opt => opt.MapFrom(src => (src.FBAOrderDetail == null ? -1 : src.FBAOrderDetail.LabelFiles.Split('{').Length - 1)));
            Mapper.CreateMap<FBAPallet, FBAPalletDto>();
            Mapper.CreateMap<FBAPalletLocation, FBAPalletLocationDto>();
            Mapper.CreateMap<FBAShipOrder, FBAShipOrderDto>()
                .ForMember(dest => dest.TotalCtns, opt => opt.MapFrom(src => src.FBAPickDetails == null ? 0 : src.FBAPickDetails.Sum(x => x.ActualQuantity)))
                .ForMember(dest => dest.TotalPlts, opt => opt.MapFrom(src => src.FBAPickDetails == null ? 0 : src.FBAPickDetails.Sum(x => x.ActualPlts)));
            Mapper.CreateMap<FBAPickDetail, FBAPickDetailsDto>();
                //.ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => "MIX"));
            Mapper.CreateMap<FBAAddressBook, FBAAddressBookDto>();
            Mapper.CreateMap<ChargingItemDetail, ChargingItemDetailDto>();
            Mapper.CreateMap<FBAShipOrder, WarehouseOutboundLog>();
            Mapper.CreateMap<AuthAppInfo, AuthAppInfoDto>();
            Mapper.CreateMap<WarehouseLocation, WarehouseLocationDto>();
            Mapper.CreateMap<FBAShipOrderPickLog, FBAShipOrderPickLogDto>();
            Mapper.CreateMap<FBAShipOrderPutBackLog, FBAShipOrderPutBackLogDto>();

            //FBAConnection
            Mapper.CreateMap<ChargeTemplate, ChargeTemplateDto>();
            Mapper.CreateMap<ChargeMethod, ChargeMethodDto>();

            //General
            Mapper.CreateMap<EFile, EFileDto>();
            Mapper.CreateMap<ApplicationUser, ApplicationUserDto>();
            Mapper.CreateMap<IdentityRole, IdentityRoleDto>();
            Mapper.CreateMap<InstructionTemplate, InstructionTemplateDto>();
        }
    }
}