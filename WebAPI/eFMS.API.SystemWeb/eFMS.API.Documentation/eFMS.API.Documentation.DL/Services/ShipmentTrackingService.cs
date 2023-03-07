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

                CsTransactionDetail hbl = await transactionDetailRepository.Where(x =>
                        (!string.IsNullOrEmpty(criteria.Mawb) && x.Mawb.Contains(criteria.Mawb)) ||
                        (!string.IsNullOrEmpty(criteria.Hawb) && x.Hwbno.Contains(criteria.Hawb))).FirstOrDefaultAsync();

                switch (criteria.ShipmentType)
                {
                    case "AIR":
                        CsTransaction shipmentExisted = await transactionRepository.Where(x => x.Id == hbl.JobId && (x.TransactionType == "AI" || x.TransactionType == "AE")).FirstOrDefaultAsync();
                        var partnerApi = new SysPartnerApi();
                        partnerApi = partnerApiRepository.First(x => x.Name.Contains(_trackingApi.Value.ApiName));
                        var baseUrl = _trackingApi.Value.Url;
                        var payload = new TrackingMoreRequestModel { AwbNumber = shipmentExisted.Mawb };
                        List<KeyValuePair<string, string>> headers = new List<KeyValuePair<string, string>>();
                        headers.Add(new KeyValuePair<string, string>("Tracking-Api-Key", partnerApi.ApiKey));
                        HttpResponseMessage request = await HttpClientService.PostAPI(baseUrl, payload, null, headers);
                        if (!request.IsSuccessStatusCode)
                        {
                            return trackShipment;
                        }
                        var dataReponse = await request.Content.ReadAsAsync<TrackingMoreReponseModel>();
                        statusShipment = dataReponse.Data.StatusNumber == 2 ? DocumentConstants.IN_TRANSIT : (dataReponse.Data.StatusNumber == 4 ? DocumentConstants.DONE : null);

                        //Lô hàng chưa tồn tại thông tin tracking
                        if (!DataContext.Any(x => x.Hblid == shipmentExisted.Id))
                        {
                            foreach (var item in dataReponse.Data.TrackInfo)
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
                                data.Hblid = shipmentExisted.Id;
                                data.Source = partnerApi.Name;
                                lstTrackInfo.Add(data);
                            }
                        }

                        //Cập nhật thêm thời gian tracking mới 
                        if (shipmentExisted.TrackingStatus != DocumentConstants.DONE)
                        {
                            var maxDateExisted = DataContext.Where(x => x.Hblid == shipmentExisted.Id).Max(x => x.PlanDate);
                            var dataTrackingSort = dataReponse.Data.TrackInfo.Where(x => x.PlanDate > maxDateExisted);
                            if (dataTrackingSort?.Count() > 0)
                            {
                                foreach (var item in dataTrackingSort)
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
                                    data.Hblid = shipmentExisted.Id;
                                    data.Source = partnerApi.Name;
                                    lstTrackInfo.Add(data);
                                }
                            }
                        }
                        shipmentExisted.TrackingStatus = statusShipment;
                        hs = await transactionRepository.UpdateAsync(shipmentExisted, x => x.Id == shipmentExisted.Id);
                        hs = await DataContext.AddAsync(lstTrackInfo);

                        var returnData = DataContext.Get(x => x.Hblid == shipmentExisted.Id).OrderByDescending(x => x.ActualDate);

                        //Reponse data
                        trackShipment.trackInfos = _mapper.Map<List<SysTrackInfoModel>>(returnData);
                        trackShipment.Status = statusShipment;
                        trackShipment.Departure = placeRepository.First(x => x.Id == shipmentExisted.Pol)?.NameVn;
                        trackShipment.Destination = placeRepository.First(x => x.Id == shipmentExisted.Pod)?.NameVn;
                        trackShipment.FlightNo = shipmentExisted.FlightVesselName;
                        trackShipment.FlightDate = shipmentExisted.FlightDate;
                        trackShipment.ColoaderName = partnerRepository.First(x => x.Id == shipmentExisted.ColoaderId)?.PartnerNameVn;
                        foreach (var item in trackShipment.trackInfos)
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
            switch (criteria.ShipmentType)
            {
                case "SEA":
                    isExisted = transactionDetailRepository.Any(x =>
                    (!string.IsNullOrEmpty(criteria.Mawb) && x.Mawb == criteria.Mawb) ||
                    (!string.IsNullOrEmpty(criteria.Hawb) && x.Hwbno == criteria.Hawb));
                    break;
                case "AIR":
                    isExisted = transactionDetailRepository.Any(x =>
                    (!string.IsNullOrEmpty(criteria.Mawb) && x.Mawb == criteria.Mawb) ||
                    (!string.IsNullOrEmpty(criteria.Hawb) && x.Hwbno == criteria.Hawb));
                    break;
                default:
                    break;
            }

            return isExisted;
        }

    }
}
