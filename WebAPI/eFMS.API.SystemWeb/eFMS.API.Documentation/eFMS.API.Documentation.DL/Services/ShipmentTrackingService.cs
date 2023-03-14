using AutoMapper;
using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
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

        public ShipmentTrackingService(IContextBase<SysTrackInfo> repository, IMapper mapper,
            IContextBase<CsTransactionDetail> transactionDetailRepo,
            IContextBase<CsTransaction> transactionRepo,
            IContextBase<SysPartnerApi> partnerApiRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<CatPlace> placeRepo,
            IOptions<TrackingApi> trackingApi) : base(repository, mapper)
        {
            transactionDetailRepository = transactionDetailRepo;
            transactionRepository = transactionRepo;
            partnerApiRepository = partnerApiRepo;
            placeRepository = placeRepo;
            partnerRepository = partnerRepo;
            _trackingApi = trackingApi;
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
                data.JobId = hblId;
                data.Source = source;
                trackInfoList.Add(data);
            }

            return trackInfoList;
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

                        SysPartnerApi partnerApi = partnerApiRepository.First(x => x.Name.Contains(_trackingApi.Value.ApiName));
                        var baseUrl = _trackingApi.Value.Url;
                        var payload = new TrackingMoreRequestModel { AwbNumber = shipmentExisted.Mawb };
                        var headers = new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("Tracking-Api-Key", partnerApi.ApiKey)
                        };

                        var request = await HttpClientService.PostAPI(baseUrl, payload, null, headers);

                        trackShipment.Departure = placeRepository.First(x => x.Id == shipmentExisted.Pol)?.NameVn;
                        trackShipment.Destination = placeRepository.First(x => x.Id == shipmentExisted.Pod)?.NameVn;
                        trackShipment.FlightNo = shipmentExisted.FlightVesselName;
                        trackShipment.FlightDate = shipmentExisted.FlightDate;
                        trackShipment.ColoaderName = partnerRepository.First(x => x.Id == shipmentExisted.ColoaderId)?.PartnerNameVn;

                        var dataResponse = await request.Content.ReadAsAsync<TrackingMoreResponseModel>();
                        if (!request.IsSuccessStatusCode || dataResponse?.Data == null)
                        {
                            return trackShipment;
                        }
                        statusShipment = dataResponse.Data.StatusNumber == 2 ? DocumentConstants.IN_TRANSIT : (dataResponse.Data.StatusNumber == 4 ? DocumentConstants.DONE : null);

                        if (shipmentExisted.TrackingStatus != DocumentConstants.DONE)
                        {
                            if (!DataContext.Any(x => x.JobId == shipmentExisted.Id))
                            {
                                lstTrackInfo = GetTrackInfoList(shipmentExisted.Id, dataResponse.Data.TrackInfo, partnerApi.Name);
                            }
                            else
                            {
                                var maxDateExisted = DataContext.Where(x => x.JobId == shipmentExisted.Id).Max(x => x.PlanDate);
                                var dataTrackingSort = dataResponse.Data.TrackInfo.Where(x => x.PlanDate > maxDateExisted);
                                if (dataTrackingSort?.Any() == true)
                                {
                                    lstTrackInfo = GetTrackInfoList(shipmentExisted.Id, dataTrackingSort, partnerApi.Name);
                                }
                            }
                            shipmentExisted.TrackingStatus = statusShipment;
                            hs = await transactionRepository.UpdateAsync(shipmentExisted, x => x.Id == shipmentExisted.Id);
                            hs = await DataContext.AddAsync(lstTrackInfo);
                        }

                        var returnData = DataContext.Get(x => x.JobId == shipmentExisted.Id).OrderBy(x => x.ActualDate);
                        trackShipment.TrackInfos = _mapper.Map<List<SysTrackInfoModel>>(returnData);
                        trackShipment.Status = statusShipment;
                        trackShipment.FlightNo = dataResponse.Data.FlightInfo.Any() ? string.Join(", ", dataResponse.Data.FlightInfo.Select(f => f.FlightNumber).Distinct()) : shipmentExisted.FlightVesselName;
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
