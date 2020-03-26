import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgForm } from '@angular/forms';
import { NgProgress } from '@ngx-progressbar/core';
import { Store, ActionsSubject } from '@ngrx/store';

import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';

import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { SystemRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { AppPage } from "src/app/app.base";
import { DataService } from 'src/app/shared/services';
import { PlSheetPopupComponent } from './pl-sheet-popup/pl-sheet.popup';
import { CsTransactionDetail, Container, Customer } from 'src/app/shared/models';
import { DocumentationRepo } from 'src/app/shared/repositories/documentation.repo';
import { SystemConstants } from 'src/constants/system.const';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { ShareBussinessSellingChargeComponent, ShareBussinessContainerListPopupComponent } from '../../share-business';

import { catchError, finalize, takeUntil } from 'rxjs/operators';

import * as fromShareBussiness from './../../share-business/store';
import _groupBy from 'lodash/groupBy';
import { OPSTransactionGetDetailSuccessAction } from '../store';
import { formatDate } from '@angular/common';
import { CommonEnum } from '@enums';

@Component({
    selector: 'app-ops-module-billing-job-edit',
    templateUrl: './job-edit.component.html',
})
export class OpsModuleBillingJobEditComponent extends AppPage implements OnInit {
    @ViewChild(PlSheetPopupComponent, { static: false }) plSheetPopup: PlSheetPopupComponent;
    @ViewChild('confirmCancelUpdate', { static: false }) confirmCancelJobPopup: ConfirmPopupComponent;
    @ViewChild('notAllowDelete', { static: false }) canNotDeleteJobPopup: InfoPopupComponent;
    @ViewChild('confirmDelete', { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild('confirmLockShipment', { static: false }) confirmLockShipmentPopup: ConfirmPopupComponent;
    @ViewChild(ShareBussinessSellingChargeComponent, { static: false }) sellingChargeComponent: ShareBussinessSellingChargeComponent;
    @ViewChild(ShareBussinessContainerListPopupComponent, { static: false }) containerPopup: ShareBussinessContainerListPopupComponent;

    @ViewChild('addOpsForm', { static: false }) formOps: NgForm;
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

    lstUnits: any[] = [];
    lstCurrencies: any[] = [];

    listPackageTypes: any[];
    packageTypes: any[] = [];

    tab: string = '';
    tabCharge: string = '';
    jobId: string = '';
    hblid: string = '';

    deleteMessage: string = '';

    packagesUnitActive = [];

    submitted: boolean = false;

    constructor(
        private _spinner: NgxSpinnerService,
        private route: ActivatedRoute,
        private router: Router,
        private _data: DataService,
        private systemRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo,
        private _ngProgressService: NgProgress,
        private _documentRepo: DocumentationRepo,
        private _router: Router,
        private _toastService: ToastrService,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _actionStoreSubject: ActionsSubject
    ) {
        super();

        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.getShipmentCommonData();
        this.getUnits();
        this.getListCurrencies();
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

        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: fromShareBussiness.ContainerAction) => {
                    if (action.type === fromShareBussiness.ContainerActionTypes.SAVE_CONTAINER) {
                        this.lstMasterContainers = action.payload;
                        console.log(this.lstMasterContainers);
                        this.updateData(this.lstMasterContainers);
                    }
                }
            );
    }
    updateData(lstMasterContainers: any[]) {
        let sumCbm = 0;
        let sumPackages = 0;
        let sumContainers = 0;
        let sumNetWeight = 0;
        let sumGrossWeight = 0;
        this.opsTransaction.containerDescription = '';
        lstMasterContainers.forEach(x => {
            sumCbm = sumCbm + x.cbm;
            sumPackages = sumPackages + x.packageQuantity;
            sumContainers = sumContainers + x.quantity;
            sumNetWeight = sumNetWeight + x.nw;
            sumGrossWeight = sumGrossWeight + x.gw;
        });
        const contData = [];
        for (const item of Object.keys(_groupBy(lstMasterContainers, 'containerTypeName'))) {
            contData.push({
                cont: item,
                quantity: _groupBy(lstMasterContainers, 'containerTypeName')[item].map(i => i.quantity).reduce((a: any, b: any) => a += b)
            });
        }
        for (const item of contData) {
            this.opsTransaction.containerDescription = this.opsTransaction.containerDescription + item.quantity + "x" + item.cont + "; ";
            this.opsTransaction.sumCbm = sumCbm;
            this.opsTransaction.sumPackages = sumPackages;
            this.opsTransaction.sumContainers = sumContainers;
            this.opsTransaction.sumNetWeight = sumNetWeight;
            this.opsTransaction.sumGrossWeight = sumGrossWeight;
        }
        if (this.opsTransaction.containerDescription.length > 1) {
            this.opsTransaction.containerDescription = this.opsTransaction.containerDescription.substring(0, this.opsTransaction.containerDescription.length - 3);
        }
    }

    getListContainersOfJob() {
        this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblId: this.jobId }));
        this._store.dispatch(new fromShareBussiness.GetContainersHBLAction({ hblid: this.opsTransaction.hblid }));

        this._store.select<any>(fromShareBussiness.getContainerSaveState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (containers: any) => {
                    this.lstMasterContainers = containers || [];
                }
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


    saveShipment() {
        this.lstMasterContainers.forEach((c: Container) => {
            c.mblid = this.jobId;
            c.hblid = this.hblid;
        });
        this.submitted = true;
        this.opsTransaction.serviceDate = !!this.serviceDate ? (this.serviceDate.startDate != null ? formatDate(this.serviceDate.startDate, 'yyyy-MM-dd', 'en') : null) : null;
        this.opsTransaction.finishDate = !!this.finishDate ? (this.finishDate.startDate != null ? formatDate(this.finishDate.startDate, 'yyyy-MM-dd', 'en') : null) : null;
        this.opsTransaction.csMawbcontainers = this.lstMasterContainers;
        const isValidDate = this.finishDate.startDate != null && this.serviceDate.startDate != null && (this.finishDate.startDate < this.serviceDate.startDate);
        if (this.formOps.invalid || this.opsTransaction.shipmentMode == null
            || (this.opsTransaction.pod === this.opsTransaction.pol && this.opsTransaction.pod != null && this.opsTransaction.pol != null)
            || this.opsTransaction.serviceMode == null
            || this.opsTransaction.productService == null
            || this.opsTransaction.customerId == null
            || this.opsTransaction.billingOpsId == null
            || this.opsTransaction.serviceDate == null
            || isValidDate
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

            this.updateShipment();
        }
    }

    updateShipment() {
        this._spinner.show();
        this._documentRepo.updateShipment(this.opsTransaction)
            .pipe(catchError(this.catchError), finalize(() => this._spinner.hide()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.getShipmentDetails(this.opsTransaction.id);
                    } else {
                        this._toastService.warning(res.message);
                    }
                }
            );
    }

    lockShipment() {
        this.confirmLockShipmentPopup.show();
    }

    onLockShipment() {
        this.opsTransaction.isLocked = true;
        this.confirmLockShipmentPopup.hide();

        this.updateShipment();
    }

    showListContainer() {
        this.containerPopup.mblid = this.jobId;
        this.containerPopup.show();
    }

    getListPackageTypes() {
        this._catalogueRepo.getUnit({ unitType: 'package' })
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (res: any) => {
                    this.listPackageTypes = res;
                    this.packageTypes = this.utility.prepareNg2SelectData(this.listPackageTypes, 'id', 'unitNameEn');
                },
            );
    }

    saveContainers(event) {
        this.opsTransaction.csMawbcontainers = event;
        this.getListContainersOfJob();
        this.getShipmentDetails(this.opsTransaction.id);
    }

    getWarehouses() {
        this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Warehouse, active: true }).subscribe((res: any) => {
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
                    this.commodityGroup = this.utility.prepareNg2SelectData(this.commodityGroup,
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
                    this.opsTransaction = new OpsTransaction(response);
                    this.hblid = this.opsTransaction.hblid;

                    if (this.opsTransaction != null) {
                        this.getListContainersOfJob();
                        if (this.opsTransaction != null) {
                            this.getSurCharges(CommonEnum.SurchargeTypeEnum.BUYING_RATE);
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

                            const hbl = new CsTransactionDetail(this.opsTransaction);
                            hbl.id = this.opsTransaction.hblid;

                            this._store.dispatch(new fromShareBussiness.GetDetailHBLSuccessAction(hbl));

                            // Tricking Update Transation Apply for isLocked..
                            this._store.dispatch(new fromShareBussiness.TransactionGetDetailSuccessAction(this.opsTransaction));

                            this._store.dispatch(new OPSTransactionGetDetailSuccessAction(this.opsTransaction));
                            this._store.dispatch(new fromShareBussiness.GetProfitHBLAction(this.opsTransaction.hblid));

                            this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: this.jobId }));
                            this._store.dispatch(new fromShareBussiness.GetContainersHBLAction({ hblid: this.opsTransaction.hblid }));

                        } else {
                            this.serviceDate = null;
                            this.finishDate = null;
                        }
                    }
                },
            );
    }

    getShipmentCommonData() {
        this._documentRepo.getOPSShipmentCommonData()
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (responses: any) => {
                    this.productServices = this.utility.prepareNg2SelectData(responses.productServices, 'value', 'displayName');
                    this.serviceModes = this.utility.prepareNg2SelectData(responses.serviceModes, 'value', 'displayName');
                    this.shipmentModes = this.utility.prepareNg2SelectData(responses.shipmentModes, 'value', 'displayName');
                },
            );
    }

    getPorts() {
        this._catalogueRepo.getListPort({ placeType: PlaceTypeEnum.Port, active: true })
            .subscribe((res: any) => {
                this.ports = res;
                this._data.setDataService(SystemConstants.CSTORAGE.PORT, this.ports);
            });
    }

    getCustomers() {
        this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CUSTOMER)
            .subscribe((res: any) => {
                this.customers = res;
                if (this.opsTransaction.salemanId === null) {

                    // Get default using opstransation's customer.
                    const customer: Customer = this.customers.find(x => x.id === this.opsTransaction.customerId);
                    if (!!customer) {
                        this.opsTransaction.salemanId = customer.salePersonId;
                    }
                }
                this._data.setDataService(SystemConstants.CSTORAGE.CUSTOMER, this.customers);
            });
    }

    getSuppliers() {
        this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER)
            .subscribe((res: any) => {
                this.suppliers = res;
                this._data.setDataService(SystemConstants.CSTORAGE.SUPPLIER, this.suppliers);
            });
    }

    getAgents() {
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
                    if (responses != null) {
                        this.salemans = responses.filter(x => x.active === true);
                        this.billingOps = responses.filter(x => x.active === true);
                    } else {
                        this.salemans = [];
                        this.billingOps = [];
                    }

                    this._data.setDataService(SystemConstants.CSTORAGE.SYSTEM_USER, responses);
                },
            );
    }

    getUnits() {
        this._catalogueRepo.getUnit({ active: true }).pipe(
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
                    this.lstCurrencies = this.utility.prepareNg2SelectData(responses, "id", "id");
                },
            );
    }

    getSurCharges(type: 'BUY' | 'SELL' | 'OBH') {
        if (type === CommonEnum.SurchargeTypeEnum.BUYING_RATE) {
            this._store.dispatch(new fromShareBussiness.GetBuyingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.BUYING_RATE, hblId: this.opsTransaction.hblid }));
        }
        if (type === CommonEnum.SurchargeTypeEnum.SELLING_RATE) {
            this._store.dispatch(new fromShareBussiness.GetSellingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.SELLING_RATE, hblId: this.opsTransaction.hblid }));
        }
        if (type === CommonEnum.SurchargeTypeEnum.OBH) {
            this._store.dispatch(new fromShareBussiness.GetOBHSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.OBH, hblId: this.opsTransaction.hblid }));
        }
    }

    selectTab($event: any, tabName: string) {
        this.tab = tabName;
        if (tabName === 'job-edit') {
            this.getShipmentDetails(this.jobId);
            this.getSurCharges(CommonEnum.SurchargeTypeEnum.BUYING_RATE);
        }
    }

    onOpePLPrint() {
        this.plSheetPopup.show();
    }

    selectTabCharge(tabName: string) {
        this.tabCharge = tabName;
        switch (tabName) {
            case 'buying':
                this.getSurCharges(CommonEnum.SurchargeTypeEnum.BUYING_RATE);
                break;
            case 'selling':
                this.getSurCharges(CommonEnum.SurchargeTypeEnum.SELLING_RATE);
                break;
            case 'obh':
                this.getSurCharges(CommonEnum.SurchargeTypeEnum.OBH);
                break;
            default:
                break;
        }
    }
}
