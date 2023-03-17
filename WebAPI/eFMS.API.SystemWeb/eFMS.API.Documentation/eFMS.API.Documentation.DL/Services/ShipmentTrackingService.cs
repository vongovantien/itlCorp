using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ShipmentTrackingService : RepositoryBase<SysTrackInfo, SysTrackInfoModel>, IShipmentTrackingService
    {
        private readonly IContextBase<CsTransactionDetail> transactionDetailRepository;
        private readonly IContextBase<CsTransaction> transactionRepository;
        private readonly IContextBase<SysPartnerApi> partnerApiRepository;
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<CatPlace> placeRepository;
        private readonly IOptions<TrackingApi> _trackingApi;
        private readonly ICurrentUser _currentUser;

        public ShipmentTrackingService(IContextBase<SysTrackInfo> repository, IMapper mapper,
            IContextBase<CsTransactionDetail> transactionDetailRepo,
            IContextBase<CsTransaction> transactionRepo,
            IContextBase<SysPartnerApi> partnerApiRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<CatPlace> placeRepo,
            ICurrentUser currentUser,
            IOptions<TrackingApi> trackingApi) : base(repository, mapper)
        {
            transactionDetailRepository = transactionDetailRepo;
            transactionRepository = transactionRepo;
            partnerApiRepository = partnerApiRepo;
            placeRepository = placeRepo;
            partnerRepository = partnerRepo;
            _trackingApi = trackingApi;
            _currentUser = currentUser;
        }

        private List<SysTrackInfo> GetTrackInfoList(Guid? hblId, IEnumerable<TrackInfo> trackInfos, string source)
        {
            var trackInfoList = new List<SysTrackInfo>();
            foreach (var item in trackInfos)
            {
                var data = new SysTrackInfo();
                data.Id = Guid.NewGuid();
                data.PlanDate = item.PlanDate;
                data.Quantity = item.Piece;
                data.EventDescription = item.Event;
                data.Station = placeRepository.First(x => x.Code == item.Station)?.Id;
                data.Weight = item.Weight;
                data.ActualDate = item.ActualDate;
                data.FlightNo = item.FlightNumber;
                data.DatetimeCreated = data.DatetimeModified = DateTime.Now;
                data.UserCreated = data.UserModified = _currentUser.UserID;
                data.Type = "AIR";
                data.Status = MapCodeToName(item.Status?.Trim());
                data.JobId = hblId;
                data.Source = source;
                trackInfoList.Add(data);
            }

            return trackInfoList;
        }

        private string MapCodeToName(string statusCode)
        {
            var descriptions = new Dictionary<string, string>{
                {"ACC", "Accepted"},
                {"AST", "Assigned to another flight"},
                {"ARR", "Arrived"},
                {"ARE", "Arrival estimated"},
                {"DLV", "Delivered"},
                {"DDL", "Documents Delivered"},
                {"RCV", "Received"},
                {"DEP", "Departed"},
                {"MAN", "Manifested"},
                {"BKC", "Booking Confirmed"},
                {"BKD", "Booked"},
                {"BKG", "Booking Generated"},
                {"RCS", "Received from Shipper"},
                {"RCF", "Received from Flight"},
                {"NFD", "Consignee Notified"},
                {"FOH", "Freight on hand"},
                {"AWD", "Documentation Delivered"},
                {"CLD", "Cargo Loaded"},
                {"PRE", "Shipment Prepared"},
                {"ARV", "Arrived"},
                {"FWB", "Electronic AWB"},
                {"TRA", "In Transit"},
                {"AWR", "Documents Received"},
                {"TFD", "Transferred"},
                {"RCT", "Received from other airline"},
                {"SCW", "Shipment Checked Into Warehouse"},
                {"CLC", "Cleared by Customs"},
                {"TPL", "Temperature Log"},
                {"PIC", "Available for pickup"},
                {"SOH", "Shipment on hold"},
                {"PDD", "Pre-declaration is done"},
                {"MCC", "Matching cancelled"},
                {"DCD", "Documentation check is done"}
            };
            return descriptions.ContainsKey(statusCode) ? descriptions[statusCode] : statusCode;

        }
        public async Task<TrackingShipmentViewModel> TrackShipmentProgress(TrackingShipmentCriteria criteria)
        {
            try
            {
                var hs = new HandleState();
                string trackInfo = string.Empty;
                string statusShipment = string.Empty;
                DateTime lastEvent = DateTime.Now;
                var lstTrackInfo = new List<SysTrackInfo>();
                var trackShipment = new TrackingShipmentViewModel();
                CsTransaction shipmentExisted = new CsTransaction();
                switch (criteria.ShipmentType)
                {
                    case "AIR":
                        if (!string.IsNullOrEmpty(criteria.Hawb))
                        {
                            var hbl = await transactionDetailRepository.Where(x => string.IsNullOrEmpty(criteria.Hawb) || x.Hwbno == criteria.Hawb.Trim()).FirstAsync();
                            shipmentExisted = await transactionRepository.Where(x => x.Id == hbl.JobId && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                                && (criteria.ShipmentType == "AIR" && (x.TransactionType == "AE" || x.TransactionType == "AI"))).FirstAsync();
                        }
                        else
                        {
                            shipmentExisted = await transactionRepository.Where(x => string.IsNullOrEmpty(criteria.Mawb) || x.Mawb == criteria.Mawb.Trim()
                                                && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                                && (criteria.ShipmentType == "AIR" && (x.TransactionType == "AE" || x.TransactionType == "AI"))).FirstAsync();
                        }

                        //SysPartnerApi partnerApi = partnerApiRepository.First(x => x.Name.Contains(_trackingApi.Value.ApiName));
                        var baseUrl = _trackingApi.Value.Url;
                        var payload = new TrackingMoreRequestModel { AwbNumber = shipmentExisted.Mawb };
                        var headers = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("Tracking-Api-Key", "xs2pcepz-ek7p-01ol-fhds-6ivds4elzpc")
                        };

                        var request = await HttpClientService.PostAPI(baseUrl, payload, null, headers);
                        var dataResponse = await request.Content.ReadAsAsync<TrackingMoreResponseModel>();
                        if (request.IsSuccessStatusCode)
                        {
                            if (dataResponse?.Data != null)
                            {
                                statusShipment = dataResponse.Data.StatusNumber == 2 ? DocumentConstants.IN_TRANSIT : (dataResponse.Data.StatusNumber == 4 ? DocumentConstants.DONE : null);
                                if (shipmentExisted.TrackingStatus != DocumentConstants.DONE)
                                {

                                    if (!DataContext.Any(x => x.JobId == shipmentExisted.Id))
                                    {
                                        lstTrackInfo = GetTrackInfoList(shipmentExisted.Id, dataResponse.Data.TrackInfo, "");
                                    }
                                    else
                                    {
                                        var dataExisted = DataContext.Count(x => x.JobId == shipmentExisted.Id);
                                        var dataTrackingSort = dataResponse.Data.TrackInfo.OrderBy(x => x.ActualDate).Skip(dataExisted);
                                        if (dataTrackingSort?.Any() == true)
                                        {
                                            lstTrackInfo = GetTrackInfoList(shipmentExisted.Id, dataTrackingSort, "");
                                        }
                                    }
                                    if (lstTrackInfo.Count() > 0)
                                    {
                                        hs = await DataContext.AddAsync(lstTrackInfo);
                                    }
                                }

                                shipmentExisted.TrackingStatus = statusShipment != string.Empty ? statusShipment : shipmentExisted.TrackingStatus;
                                hs = await transactionRepository.UpdateAsync(shipmentExisted, x => x.Id == shipmentExisted.Id);
                            }
                        }

                        var returnData = DataContext.Get(x => x.JobId == shipmentExisted.Id).OrderBy(x => x.ActualDate);

                        trackShipment.TrackInfos = _mapper.Map<List<SysTrackInfoModel>>(returnData);
                        trackShipment.Departure = placeRepository.First(x => x.Id == shipmentExisted.Pol)?.NameVn;
                        trackShipment.Destination = placeRepository.First(x => x.Id == shipmentExisted.Pod)?.NameVn;
                        trackShipment.FlightDate = shipmentExisted.FlightDate;
                        trackShipment.ColoaderName = partnerRepository.First(x => x.Id == shipmentExisted.ColoaderId)?.PartnerNameVn;
                        trackShipment.Status = shipmentExisted.TrackingStatus;
                        trackShipment.FlightNo = returnData.Any() == true ? string.Join(", ", returnData.Where(x => !string.IsNullOrEmpty(x.FlightNo) && x.FlightNo != "-").Select(x => x.FlightNo).Distinct()) : shipmentExisted.FlightVesselName;
                        foreach (var item in trackShipment.TrackInfos)
                        {
                            item.StationName = placeRepository.First(x => x.Id == item.Station)?.NameVn;
                        };
                        break;
                    default:
                        break;
                }

                return trackShipment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool CheckExistShipment(TrackingShipmentCriteria criteria)
        {
            bool isExisted = false;
            if (string.IsNullOrEmpty(criteria.Mawb) && string.IsNullOrEmpty(criteria.Hawb))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(criteria.Hawb))
            {
                var hbl = transactionDetailRepository.Where(x => string.IsNullOrEmpty(criteria.Hawb) || x.Hwbno == criteria.Hawb.Trim())?.FirstOrDefault();
                if (hbl != null)
                {
                    isExisted = transactionRepository.Any(x => x.Id == hbl.JobId && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                   && ((criteria.ShipmentType == "AIR" && (x.TransactionType == "AE" || x.TransactionType == "AI"))
                    || (criteria.ShipmentType == "SEA" && !(x.TransactionType == "AE" || x.TransactionType == "AI"))));
                }
                return isExisted;
            }
            return transactionRepository.Any(x => string.IsNullOrEmpty(criteria.Mawb) || x.Mawb == criteria.Mawb.Trim()
                                && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                && ((criteria.ShipmentType == "AIR" && (x.TransactionType == "AE" || x.TransactionType == "AI"))
                                 || (criteria.ShipmentType == "SEA" && !(x.TransactionType == "AE" || x.TransactionType == "AI"))));
        }
    }
}
