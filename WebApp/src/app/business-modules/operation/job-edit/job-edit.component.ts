import { Component, OnInit, ViewChild } from '@angular/core';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import * as shipmentHelper from 'src/helper/shipment.helper';
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
import { OperationRepo, UnitRepo, SystemRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { AppPage } from "src/app/app.base";
import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { CancelCreateJobPopupComponent } from './job-confirm-popup/cancel-create-job-popup/cancel-create-job-popup.component';
import { CanNotDeleteJobPopupComponent } from './job-confirm-popup/can-not-delete-job-popup/can-not-delete-job-popup.component';
import { ConfirmDeleteJobPopupComponent } from './job-confirm-popup/confirm-delete-job-popup/confirm-delete-job-popup.component';

import { DataService } from 'src/app/shared/services';

import { ConfirmCancelJobPopupComponent } from './job-confirm-popup/confirm-cancel-job-popup/confirm-cancel-job-popup.component';
import { PlSheetPopupComponent } from './pl-sheet-popup/pl-sheet.popup';
import { CsShipmentSurcharge } from 'src/app/shared/models';
import { NgProgressComponent, NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-ops-module-billing-job-edit',
    templateUrl: './job-edit.component.html',
})
export class OpsModuleBillingJobEditComponent extends AppPage implements OnInit {

    @ViewChild(ContainerListComponent, { static: false }) popupContainerList: ContainerListComponent;
    @ViewChild(CancelCreateJobPopupComponent, { static: false }) cancelCreateJobPopup: CancelCreateJobPopupComponent;
    @ViewChild(CanNotDeleteJobPopupComponent, { static: false }) canNotDeleteJobPopup: CanNotDeleteJobPopupComponent;
    @ViewChild(ConfirmDeleteJobPopupComponent, { static: false }) confirmDeleteJobPopup: ConfirmDeleteJobPopupComponent;
    @ViewChild(ConfirmCancelJobPopupComponent, { static: false }) confirmCancelJobPopup: ConfirmCancelJobPopupComponent;
    @ViewChild(PlSheetPopupComponent, { static: false }) plSheetPopup: PlSheetPopupComponent;

    opsTransaction: OpsTransaction = null;
    productServices: any[] = [];
    serviceDate: any;
    finishDate: any;
    exchangeRateDate: any;
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
    searchcontainer: string = '';
    lstMasterContainers: any[];
    commodityGroupActive: any[] = [];

    lstBuyingRateChargesComboBox: any[] = [];
    lstSellingRateChargesComboBox: any[] = [];
    lstOBHChargesComboBox: any[] = [];
    lstPartners: any[] = [];
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

    listContainerType: any[] = [];

    listPackageTypes: any[];
    packageTypes: any[] = [];

    tab: string = '';
    tabCharge: string = '';
    jobId: string = '';

    items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4',
        'option 5', 'option 6', 'option 7'];

    packagesUnitActive = [];

    disabled: boolean = false;

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU,
        private route: ActivatedRoute,
        private router: Router,
        private _unitRepo: UnitRepo,
        private _operationRepo: OperationRepo,
        private _data: DataService,
        private _catalogueRepo: CatalogueRepo
        private systemRepo: SystemRepo,
        private catalogueRepo: CatalogueRepo,
        private _ngProgressService: NgProgress
    ) {
        super();

        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {

        this.route.params.subscribe(async (params: any) => {
            this.tab = 'job-edit';
            this.tabCharge = 'buying';
            // this.getPackageTypes();
            //this.getUnits();
            this.getPartners();
            this.getListCurrencies();
            // this.getCurrencies();
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
            // this.getContainerData();
            // this.getListPackageTypes();
            await this.getShipmentCommonData();
            if (!!params && !!params.id) {
                this.jobId = params.id;
                this.getShipmentDetails(params.id);
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

                    this.commodityGroupActive = this.commodityGroup.filter( i => i.id === this.opsTransaction.commodityGroupId);


                        // this.getAllSurCharges();
                        // this.getShipmentContainer();
                        this.getCustomClearances();
                    } else {
                        this.serviceDate = null;
                        this.finishDate = null;
                    }
                }
            }
        });

    }

    getListContainersOfJob() {
        this._operationRepo.getListContainersOfJob({ mblid: this.opsTransaction.id })
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.isLoading = false;
                })
            )
            .subscribe(
                (res: any) => {
                    this.lstMasterContainers = res;
                },
                (errors: any) => {
                },
                () => { }
            );
    }

    async confirmDelete() {
        const respone = await this.baseServices.getAsync(this.api_menu.Documentation.Operation.checkAllowDelete + this.opsTransaction.id, false, true);
        if (respone === true) {
            this.confirmDeleteJobPopup.show();
        } else {
            this.canNotDeleteJobPopup.show();
        }
    }

    async deleteJob() {
        const respone = await this.baseServices.deleteAsync(this.api_menu.Documentation.Operation.delete + this.opsTransaction.id, true, true);
        if (respone.status) {
            this.confirmDeleteJobPopup.hide();
            this.router.navigate(["/home/operation/job-management"]);
        }
    }

    cancelCreatJob() {
        this.cancelCreateJobPopup.show();
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
            await this.getShipmentDetails(this.opsTransaction.id);
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
        this.popupContainerList.show({ backdrop: 'static' });
    }

    getListPackageTypes() {
        this._unitRepo.getListUnitByType({ unitType: 'package' })
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.isLoading = false;
                    if (this.listPackageTypes != null) {
                        this.packageTypes = dataHelper.prepareNg2SelectData(this.listPackageTypes, 'id', 'unitNameEn');
                    }
                })
            )
            .subscribe(
                (res: any) => {
                    this.listPackageTypes = res;
                },
                (errors: any) => {
                },
                () => { }
            );
    }

    saveContainers(event) {
        this.opsTransaction.csMawbcontainers = event;
        this.getListContainersOfJob();
        this.getShipmentDetails(this.opsTransaction.id);
    }
    // -------------    End Container   -------------------//

    getWarehouses() {
        this.baseServices.post(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Warehouse, inactive: false }).subscribe((res: any) => {
            this.warehouses = res;
        });
    }

    getCommodityGroup() {
        this._catalogueRepo.getCommodityGroup()
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

    async getShipmentDetails(id: any) {
        // this.opsTransaction = await this.baseServices.getAsync(this.api_menu.Documentation.Operation.getById + "?id=" + id, false, true);
        // console.log("des", this.opsTransaction);
        this._operationRepo.getDetailShipment(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (response: any) => {
                    this.opsTransaction = response;
                },
            );
        this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
    }

    async getShipmentCommonData() {
        // const data = await shipmentHelper.getOPSShipmentCommonData(this.baseServices, this.api_menu);
        this._operationRepo.getOPSShipmentCommonData()
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

    private getPorts() {
        this.baseServices.post(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Port, inactive: false }).subscribe((res: any) => {
            this.ports = res;
        });
    }

    private getCustomers() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER, all: null }).subscribe((res: any) => {
            this.customers = res;
        });
    }

    private getSuppliers() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CARRIER, inactive: false, all: null }).subscribe((res: any) => {
            this.suppliers = res;
        });
    }

    private getAgents() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.AGENT, inactive: false, all: null }).subscribe((res: any) => {
            this.agents = res;
        });
    }

    private getBillingOps() {
        // this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
        //     this.billingOps = res;
        //     this.salemans = res;
        // });
        this.systemRepo.getListSystemUser()
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (responses: any) => {
                    this.salemans = responses;
                },
            );
    }

    public getListBuyingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'CREDIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstBuyingRateChargesComboBox = res['data'];
            // this._data.setData('buyingCharges', this.lstBuyingRateChargesComboBox);
        });
    }

    public getListSellingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'DEBIT', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstSellingRateChargesComboBox = res['data'];
            // this._data.setData('sellingCharges', this.lstSellingRateChargesComboBox);
        });
    }

    public getListOBHCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: ChargeConstants.CL_CODE }).subscribe(res => {
            this.lstOBHChargesComboBox = res['data'];
            // this._data.setData('obhCharges', this.lstOBHChargesComboBox);
        });

    }

    public getPartners() {
        this._data.getDataByKey('lstPartners')
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data: any) => {
                    this.lstPartners = data;
                }
            );
    }

    public getUnits() {
        // this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, { inactive: false }).subscribe((data: any) => {
        //     this.lstUnits = data;
        //     // this._data.setData('lstUnits', this.lstUnits);
        // });
        this.catalogueRepo.getUnit().pipe(
            catchError(this.catchError),
            finalize(() => this._progressRef.complete())
        ).subscribe(
            (responses: any) => {
                this.lstUnits = responses;
            },
        );
    }

    public getListCurrencies() {
        // this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
        //     // this._data.setData('lstCurrencies', res);
        // });
        this.catalogueRepo.getCurrency()
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (responses: any) => {

                    this._data.setData('lstCurrencies', responses);
                    this.lstCurrencies = prepareNg2SelectData(responses, "id", "id");
                },
            );
    }

    public getCurrencies(isAddNew = true) {
        if (isAddNew === true) {
            this.baseServices.post(this.api_menu.Catalogue.Currency.getAllByQuery, { inactive: false }).subscribe((res: any) => {
                this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
            });
        } else {
            this.baseServices.get(this.api_menu.Catalogue.Currency.getAll).subscribe((res: any) => {
                this.lstCurrencies = prepareNg2SelectData(res, "id", "id");
            });
        }
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


    private totalProfit() {
        this.totalProfitUSD = this.totalSellingUSD - this.totalBuyingUSD - this.totalLogisticChargeUSD;
        this.totalProfitLocal = this.totalSellingLocal - this.totalBuyingLocal - this.totalLogisticChargeLocal;
    }

    /**
     * Calculate total cost for all buying charges 
     */
    private totalBuyingCharge() {
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

    /**
    * Calculate total cost for all selling charges 
    */
    private totalSellingCharge() {
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

    /**
    * Calculate total cost for all obh charges 
    */
    private totalOBHCharge() {
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
        this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId + "?hbId=" + this.opsTransaction.hblid + "&type=" + type).subscribe((res: any) => {
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


    /**
     * get custom clearances
     */
    getCustomClearances() {
        this.baseServices.setData("CurrentOpsTransaction", this.opsTransaction);
    }

    selectTab($event: any, tabName: string) {
        this.tab = tabName;
        if (tabName === 'job-edit') {
            this.getShipmentDetails(this.jobId);
            this.getAllSurCharges();
        }
        // this.router.navigate([`home/operation/job-edit/${this.jobId}`], {queryParams: {tab: this.tab}});
    }

    onOpePLPrint() {
        this.plSheetPopup.show({ backdrop: 'static' });
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
