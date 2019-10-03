import { Component, OnInit, ViewChild } from '@angular/core';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as dataHelper from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { ActivatedRoute, Router } from '@angular/router';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { NgForm } from '@angular/forms';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { ChargeConstants } from 'src/constants/charge.const';
import { ContainerListComponent } from './container-list/container-list.component';
import { SystemRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { AppPage } from "src/app/app.base";
import { catchError, finalize } from 'rxjs/operators';
import { DataService } from 'src/app/shared/services';
import { PlSheetPopupComponent } from './pl-sheet-popup/pl-sheet.popup';
import { CsShipmentSurcharge } from 'src/app/shared/models';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from 'src/app/shared/repositories/documentation.repo';
import { SystemConstants } from 'src/constants/system.const';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'app-ops-module-billing-job-edit',
    templateUrl: './job-edit.component.html',
})
export class OpsModuleBillingJobEditComponent extends AppPage implements OnInit {

    @ViewChild(ContainerListComponent, { static: false }) popupContainerList: ContainerListComponent;
    @ViewChild(PlSheetPopupComponent, { static: false }) plSheetPopup: PlSheetPopupComponent;

    @ViewChild('confirmCancelUpdate', { static: false }) confirmCancelJobPopup: ConfirmPopupComponent;
    @ViewChild('notAllowDelete', { static: false }) canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild('confirmDelete', { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;

    opsTransaction: OpsTransaction = null;
    productServices: any[] = [];
    serviceDate: any;
    finishDate: any;
    serviceModes: any[] = [];
    shipmentModes: any[] = [];
    customers: any[] = [];
    ports: any[] = [];
    suppliers: any[] = [];
    agents: any[] = [];
    billingOps: any[] = [];
    warehouses: any[] = [];
    salemans: any[] = [];
    commodityGroup: any[] = [];

    productServiceActive: any[] = [];
    serviceModeActive: any[] = [];
    shipmentModeActive: any[] = [];
    lstMasterContainers: any[];
    commodityGroupActive: any[] = [];

    lstBuyingRateChargesComboBox: any[] = [];
    lstSellingRateChargesComboBox: any[] = [];
    lstOBHChargesComboBox: any[] = [];
    lstUnits: any[] = [];
    lstCurrencies: any[] = [];

    ListBuyingRateCharges: CsShipmentSurcharge[] = [];
    ConstListBuyingRateCharges: any = [];
    numberOfTimeSaveContainer: number = 0;

    ListSellingRateCharges: any[] = [];
    ConstListSellingRateCharges: any[] = [];

    ListOBHCharges: CsShipmentSurcharge[] = [];
    ConstListOBHCharges: any[] = [];

    totalSellingUSD: number = 0;
    totalSellingLocal: number = 0;

    totalProfitUSD: number = 0;
    totalProfitLocal: number = 0;

    totalLogisticChargeUSD: number = 0;
    totalLogisticChargeLocal: number = 0;

    totalBuyingUSD: number = 0;
    totalBuyingLocal: number = 0;

    totalOBHUSD: number = 0;
    totalOBHLocal: number = 0;


    listPackageTypes: any[];
    packageTypes: any[] = [];

    tab: string = '';
    tabCharge: string = '';
    jobId: string = '';
    deleteMessage: string = '';

    packagesUnitActive = [];


    constructor(private baseServices: BaseService,
        private api_menu: API_MENU,
        private route: ActivatedRoute,
        private router: Router,
        private _data: DataService,
        private systemRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo,
        private _ngProgressService: NgProgress,
        private _documentRepo: DocumentationRepo,
        private _router: Router,
    ) {
        super();

        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.getShipmentCommonData();
        this.getUnits();
        this.getListCurrencies();
        this.getListBuyingRateCharges();
        this.getListSellingRateCharges();
        this.getListOBHCharges();
        this.getCustomers();
        this.getPorts();
        this.getSuppliers();
        this.getAgents();
        this.getBillingOps();
        this.getWarehouses();
        this.getCommodityGroup();
        this.getListPackageTypes();
        this.route.params.subscribe((params: any) => {
            this.tab = 'job-edit';
            this.tabCharge = 'buying';

            if (!!params && !!params.id) {
                this.jobId = params.id;
                this.getShipmentDetails(params.id);
            }
        });

    }

    getListContainersOfJob() {
        this._documentRepo.getListContainersOfJob({ mblid: this.opsTransaction.id })
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.isLoading = false;
                })
            )
            .subscribe(
                (res: any) => {
                    this.lstMasterContainers = res || [];
                },
                (errors: any) => {
                },
                () => { }
            );
    }

    checkDelete() {
        this._documentRepo.checkShipmentAllowToDelete(this.opsTransaction.id)
            .subscribe(
                (respone: boolean) => {
                    if (respone === true) {
                        this.deleteMessage = `Do you want to delete job No <span class="font-weight-bold">${this.opsTransaction.jobNo}</span>?`;
                        this.confirmDeleteJobPopup.show();
                    } else {
                        this.canNotDeleteJobPopup.show();
                    }
                }
            );
    }

    onDeleteJob() {
        this._documentRepo.deleteShipment(this.opsTransaction.id)
            .subscribe(
                (response: CommonInterface.IResult) => {
                    if (response.status) {
                        this.confirmDeleteJobPopup.hide();
                        this.router.navigate(["/home/operation/job-management"]);
                    }
                }
            );
    }

    onCancelUpdateJob() {
        this._router.navigate(["home/operation/job-management"]);
    }

    confirmCancelJob() {
        this.confirmCancelJobPopup.show();
    }

    async saveShipment(form: NgForm) {
        this.opsTransaction.serviceDate = this.serviceDate.startDate != null ? dataHelper.dateTimeToUTC(this.serviceDate.startDate) : null;
        this.opsTransaction.finishDate = this.finishDate.startDate != null ? dataHelper.dateTimeToUTC(this.finishDate.startDate) : null;
        const s = this.finishDate.startDate != null && this.serviceDate.startDate != null && (this.finishDate.startDate < this.serviceDate.startDate);
        if (form.invalid || this.opsTransaction.shipmentMode == null
            || (this.opsTransaction.pod === this.opsTransaction.pol && this.opsTransaction.pod != null && this.opsTransaction.pol != null)
            || this.opsTransaction.serviceMode == null
            || this.opsTransaction.productService == null
            || this.opsTransaction.customerId == null
            || this.opsTransaction.billingOpsId == null
            || this.opsTransaction.serviceDate == null
            || s
            || this.opsTransaction.sumGrossWeight === 0
            || this.opsTransaction.sumNetWeight === 0
            || this.opsTransaction.sumCbm === 0
            || this.opsTransaction.sumPackages === 0
            || this.opsTransaction.sumContainers === 0
        ) {
            return;
        } else {
            this.opsTransaction.sumGrossWeight = this.opsTransaction.sumGrossWeight != null ? Number(this.opsTransaction.sumGrossWeight.toFixed(2)) : null;
            this.opsTransaction.sumNetWeight = this.opsTransaction.sumNetWeight != null ? Number(this.opsTransaction.sumNetWeight.toFixed(2)) : null;
            this.opsTransaction.sumCbm = this.opsTransaction.sumCbm != null ? Number(this.opsTransaction.sumCbm.toFixed(2)) : null;
            await this.baseServices.putAsync(this.api_menu.Documentation.Operation.update, this.opsTransaction, true, true);
            this.getShipmentDetails(this.opsTransaction.id);
        }
    }
    // -------------     Container   ------------------- //
    /**
     * Show popup & init new first row( if container list is null)
     */
    showListContainer() {
        if (this.lstMasterContainers.length === 0) {
            this.lstMasterContainers.push(this.popupContainerList.initNewContainer());
        }
        this.popupContainerList.show();
    }

    getListPackageTypes() {
        this._catalogueRepo.getUnit({ unitType: 'package' })
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (res: any) => {
                    this.packageTypes = dataHelper.prepareNg2SelectData(this.listPackageTypes, 'id', 'unitNameEn');
                },
            );
    }

    saveContainers(event) {
        this.opsTransaction.csMawbcontainers = event;
        this.getListContainersOfJob();
        this.getShipmentDetails(this.opsTransaction.id);
    }

    getWarehouses() {
        this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Warehouse, inactive: false }).subscribe((res: any) => {
            this.warehouses = res;
            this._data.setDataService(SystemConstants.CSTORAGE.WAREHOUSE, this.warehouses);
        });
    }

    getCommodityGroup() {
        this._catalogueRepo.getCommodityGroup({})
            .pipe()
            .subscribe(
                (res: any) => {
                    this.commodityGroup = res;
                    this.commodityGroup = dataHelper.prepareNg2SelectData(this.commodityGroup,
                        "id",
                        "groupNameEn"
                    );
                }
            );
    }

    getShipmentDetails(id: any) {
        this._documentRepo.getDetailShipment(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (response: any) => {
                    this.opsTransaction = response;
                    if (this.opsTransaction != null) {
                        this.getListContainersOfJob();
                        if (this.opsTransaction != null) {
                            this.getAllSurCharges();
                            this.serviceDate = (this.opsTransaction.serviceDate !== null) ? { startDate: new Date(this.opsTransaction.serviceDate), endDate: new Date(this.opsTransaction.serviceDate) } : null;
                            this.finishDate = this.opsTransaction.finishDate != null ? { startDate: new Date(this.opsTransaction.finishDate), endDate: new Date(this.opsTransaction.finishDate) } : null;
                            let index = this.productServices.findIndex(x => x.id === this.opsTransaction.productService);
                            if (index > -1) { this.productServiceActive = [this.productServices[index]]; }
                            index = this.serviceModes.findIndex(x => x.id === this.opsTransaction.serviceMode);
                            if (index > -1) { this.serviceModeActive = [this.serviceModes[index]]; }
                            index = this.shipmentModes.findIndex(x => x.id === this.opsTransaction.shipmentMode);
                            if (index > -1) { this.shipmentModeActive = [this.shipmentModes[index]]; }
                            index = this.packageTypes.findIndex(x => x.id === this.opsTransaction.packageTypeId);
                            if (index > -1) { this.packagesUnitActive = [this.packageTypes[index]]; }

                            this.commodityGroupActive = this.commodityGroup.filter(i => i.id === this.opsTransaction.commodityGroupId);

                            this.getCustomClearances();
                        } else {
                            this.serviceDate = null;
                            this.finishDate = null;
                        }
                    }
                },
            );
        this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
    }

    getShipmentCommonData() {
        this._documentRepo.getOPSShipmentCommonData()
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (responses: any) => {
                    this.productServices = dataHelper.prepareNg2SelectData(responses.productServices, 'value', 'displayName');
                    this.serviceModes = dataHelper.prepareNg2SelectData(responses.serviceModes, 'value', 'displayName');
                    this.shipmentModes = dataHelper.prepareNg2SelectData(responses.shipmentModes, 'value', 'displayName');
                },
            );
    }

    getPorts() {
        this._catalogueRepo.getListPort({ placeType: PlaceTypeEnum.Port, inactive: false })
            .subscribe((res: any) => {
                this.ports = res;
                this._data.setDataService(SystemConstants.CSTORAGE.PORT, this.ports);
            });
    }

    getCustomers() {
        this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CUSTOMER)
            .subscribe((res: any) => {
                this.customers = res;
                this._data.setDataService(SystemConstants.CSTORAGE.PARTNER, this.customers);
            });
    }

    getSuppliers() {
        // this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CARRIER, inactive: false, all: null })
        //     .subscribe((res: any) => {
        //         this.suppliers = res;
        //         this._data.setDataService(SystemConstants.CSTORAGE.SUPPLIER, this.suppliers);
        //     });
        this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER)
            .subscribe((res: any) => {
                this.suppliers = res;
                this._data.setDataService(SystemConstants.CSTORAGE.SUPPLIER, this.suppliers);
            });
    }

    getAgents() {
        // this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.AGENT, inactive: false, all: null })
        //     .subscribe((res: any) => {
        //         this.agents = res;
        //         this._data.setDataService(SystemConstants.CSTORAGE.AGENT, this.agents);
        //     });
        this._catalogueRepo.getPartnersByType(PartnerGroupEnum.AGENT)
            .subscribe((res: any) => {
                this.agents = res;
                this._data.setDataService(SystemConstants.CSTORAGE.AGENT, this.agents);
            });
    }

    getBillingOps() {
        this.systemRepo.getListSystemUser()
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (responses: any) => {
                    this.billingOps = responses;
                    this.salemans = responses;

                    this._data.setDataService(SystemConstants.CSTORAGE.SYSTEM_USER, responses);
                },
            );
    }

    public getListBuyingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'CREDIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstBuyingRateChargesComboBox = res['data'];
            this._data.setDataService('buyingCharges', this.lstBuyingRateChargesComboBox);
        });
    }

    public getListSellingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'DEBIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstSellingRateChargesComboBox = res['data'];
            this._data.setDataService('sellingCharges', this.lstSellingRateChargesComboBox);
        });
    }

    public getListOBHCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstOBHChargesComboBox = res['data'];
            this._data.setDataService('obhCharges', this.lstOBHChargesComboBox);
        });

    }

    getUnits() {
        this._catalogueRepo.getUnit().pipe(
            catchError(this.catchError),
            finalize(() => this._progressRef.complete())
        ).subscribe(
            (responses: any) => {
                this.lstUnits = responses;

                this._data.setDataService(SystemConstants.CSTORAGE.UNIT, responses || []);
            },
        );
    }

    getListCurrencies() {
        this._catalogueRepo.getCurrency()
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (responses: any) => {
                    this._data.setDataService(SystemConstants.CSTORAGE.CURRENCY, responses || []);
                    this.lstCurrencies = prepareNg2SelectData(responses, "id", "id");
                },
            );
    }


    onSaveBuyingRate(event) {
        if (event === true) {
            this.getSurCharges('BUY');
        }
    }
    onSaveSellingRate(event) {
        if (event === true) {
            this.getSurCharges('SELL');
        }
    }
    onSaveOHBRate(event) {
        if (event === true) {
            this.getSurCharges('OBH');
        }
    }

    totalProfit() {
        this.totalProfitUSD = this.totalSellingUSD - this.totalBuyingUSD - this.totalLogisticChargeUSD;
        this.totalProfitLocal = this.totalSellingLocal - this.totalBuyingLocal - this.totalLogisticChargeLocal;
    }

    totalBuyingCharge() {
        this.totalBuyingUSD = 0;
        this.totalBuyingLocal = 0;
        if (this.ListBuyingRateCharges.length > 0) {

            this.ListBuyingRateCharges.forEach((element: any) => {
                const currentLocalBuying = element.total * element.exchangeRate;
                this.totalBuyingLocal += currentLocalBuying;
                // this.totalBuyingUSD += currentLocalBuying / element.exchangeRateUSDToVND;
                this.totalBuyingUSD += element.total * element.rateToUSD;
                this.totalProfit();
            });
        }
    }

    totalSellingCharge() {
        this.totalSellingUSD = 0;
        this.totalSellingLocal = 0;
        if (this.ListSellingRateCharges.length > 0) {
            this.ListSellingRateCharges.forEach(element => {
                const currentLocalSelling = element.total * element.exchangeRate;
                this.totalSellingLocal += currentLocalSelling;
                // this.totalSellingUSD += currentLocalSelling / element.exchangeRateUSDToVND;
                this.totalSellingUSD += element.total * element.rateToUSD;
                this.totalProfit();
            });
        }
    }

    totalOBHCharge() {
        this.totalOBHUSD = 0;
        this.totalOBHLocal = 0;
        if (this.ListOBHCharges.length > 0) {

            this.ListOBHCharges.forEach((element: any) => {
                const currentOBHCharge = element.total * element.exchangeRate;
                this.totalOBHLocal += currentOBHCharge;
                // this.totalOBHUSD += currentOBHCharge / element.exchangeRateUSDToVND;
                this.totalOBHUSD += element.total * element.rateToUSD;
                this.totalProfit();
            });

        }
    }

    getSurCharges(type: 'BUY' | 'SELL' | 'OBH') {
        this._documentRepo.getSurchargeByHbl(type, this.opsTransaction.hblid)
            .subscribe((res: any) => {
                if (type === 'BUY') {
                    this.ListBuyingRateCharges = res;
                    this.ConstListBuyingRateCharges = res;
                    this.totalBuyingCharge();
                }
                if (type === 'SELL') {
                    this.ListSellingRateCharges = res;
                    this.ConstListSellingRateCharges = res;
                    this.totalSellingCharge();
                }
                if (type === 'OBH') {
                    this.ListOBHCharges = res;
                    this.ConstListOBHCharges = res;
                    this.totalOBHCharge();
                }
            });
    }

    getAllSurCharges() {
        this.getSurCharges('BUY');
        this.getSurCharges('SELL');
        this.getSurCharges('OBH');
    }

    getCustomClearances() {
        this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
    }

    selectTab($event: any, tabName: string) {
        this.tab = tabName;
        if (tabName === 'job-edit') {
            this.getShipmentDetails(this.jobId);
            this.getAllSurCharges();
        }
    }

    onOpePLPrint() {
        this.plSheetPopup.show();
    }

    selectTabCharge(tabName: string) {
        this.tabCharge = tabName;
        switch (tabName) {
            case 'buying':
                this.getSurCharges('BUY');
                break;
            case 'selling':
                this.getSurCharges('SELL');
                break;
            case 'obh':
                this.getSurCharges('OBH');
                break;
            default:
                this.getSurCharges('BUY');
                break;
        }
    }
}
