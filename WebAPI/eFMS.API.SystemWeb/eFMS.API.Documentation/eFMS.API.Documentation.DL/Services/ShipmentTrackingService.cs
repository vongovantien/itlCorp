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

                switch (criteria.ShipmentType)
                {
                    case "AIR":
                        var transactionDetailQuery = transactionDetailRepository.Where(x => string.IsNullOrEmpty(criteria.Hawb) || x.Hwbno == criteria.Hawb.Trim()).AsNoTracking();
                        var transactionQuery = transactionRepository.Where(x => string.IsNullOrEmpty(criteria.Mawb) || x.Mawb == criteria.Mawb.Trim()
                                    && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                                    && (criteria.ShipmentType == "AIR" && (x.TransactionType == "AE" || x.TransactionType == "AI"))).AsNoTracking();

                        var queryData = from trans in transactionQuery
                                        join transDetail in transactionDetailQuery
                                        on trans.Mawb equals transDetail.Mawb into joinedData
                                        from transDetail in joinedData.DefaultIfEmpty()
                                        select trans;
                        var shipmentExisted = await queryData.FirstAsync();

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

                        if (!request.IsSuccessStatusCode)
                        {
                            return trackShipment;
                        }
                        var dataReponse = await request.Content.ReadAsAsync<TrackingMoreReponseModel>();
                        statusShipment = dataReponse.Data.StatusNumber == 2 ? DocumentConstants.IN_TRANSIT : (dataReponse.Data.StatusNumber == 4 ? DocumentConstants.DONE : null);

                        if (shipmentExisted.TrackingStatus != DocumentConstants.DONE)
                        {
                            if (!DataContext.Any(x => x.JobId == shipmentExisted.Id))
                            {
                                lstTrackInfo = GetTrackInfoList(shipmentExisted.Id, dataReponse.Data.TrackInfo, partnerApi.Name);
                            }
                            else
                            {
                                var maxDateExisted = DataContext.Where(x => x.JobId == shipmentExisted.Id).Max(x => x.PlanDate);
                                var dataTrackingSort = dataReponse.Data.TrackInfo.Where(x => x.PlanDate > maxDateExisted);
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
            if (string.IsNullOrEmpty(criteria.Mawb) && string.IsNullOrEmpty(criteria.Hawb))
            {
                return false;
            }

            var transactionDetailQuery = transactionDetailRepository.Where(x => string.IsNullOrEmpty(criteria.Hawb) || x.Hwbno == criteria.Hawb.Trim()).AsNoTracking();

            if (!string.IsNullOrEmpty(criteria.Hawb) && !transactionDetailQuery.Any())
            {
                return false;
            }
            var transactionQuery = transactionRepository.Where(x => string.IsNullOrEmpty(criteria.Mawb) || x.Mawb == criteria.Mawb.Trim()
                        && x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED
                        && ((criteria.ShipmentType == "AIR" && (x.TransactionType == "AE" || x.TransactionType == "AI"))
                        || (criteria.ShipmentType == "SEA" && !(x.TransactionType == "AE" || x.TransactionType == "AI")))).AsNoTracking();

            var query = from trans in transactionQuery
                        join transDetail in transactionDetailQuery
                        on trans.Mawb equals transDetail.Mawb into joinedData
                        from transDetail in joinedData.DefaultIfEmpty()
                        select trans;

            return query.Any();
        }
    }
}
