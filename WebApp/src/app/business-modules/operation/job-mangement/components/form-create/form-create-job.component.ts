import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { AppForm } from '@app';
import { ComboGridVirtualScrollComponent, InfoPopupComponent } from '@common';
import { ChargeConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { CommodityGroup, Customer, LinkAirSeaModel, PortIndex, Unit, User } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { CatalogueRepo, DocumentationRepo, SystemRepo } from '@repositories';
import { ClearContainerAction, getContainerSaveState, IShareBussinessState, ShareBussinessContainerListPopupComponent } from '@share-bussiness';
import { GetCatalogueAgentAction, getCatalogueAgentState, GetCatalogueCarrierAction, getCatalogueCarrierState, GetCatalogueCommodityGroupAction, getCatalogueCommodityGroupState, GetCataloguePortAction, getCataloguePortState, getMenuUserSpecialPermissionState, getCurrentUserState } from '@store';
import { FormValidators } from '@validators';

import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { catchError, takeUntil } from 'rxjs/operators';
import { Container } from './../../../../../shared/models/document/container.model';
@Component({
    selector: 'job-mangement-form-create',
    templateUrl: './form-create-job.component.html'
})

export class JobManagementFormCreateComponent extends AppForm implements OnInit {
    @ViewChild('comboGridCustomerCpn') comboGridCustomerCpn: ComboGridVirtualScrollComponent;
    @ViewChild(ShareBussinessContainerListPopupComponent) containerPopup: ShareBussinessContainerListPopupComponent;
    @Input() transactionType: string = '';
    formCreate: FormGroup;

    hwbno: AbstractControl;
    mblno: AbstractControl;
    serviceDate: AbstractControl;
    productService: AbstractControl;
    serviceMode: AbstractControl;
    shipmentMode: AbstractControl;
    customerId: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    supplierId: AbstractControl;
    flightVessel: AbstractControl;
    agentId: AbstractControl;
    purchaseOrderNo: AbstractControl;
    billingOpsId: AbstractControl;
    commodityGroupId: AbstractControl;
    shipmentType: AbstractControl;
    salemansId: AbstractControl;
    eta: AbstractControl;
    deliveryDate: AbstractControl;
    suspendTime: AbstractControl;
    clearanceDate: AbstractControl;

    sumGrossWeight: AbstractControl;
    sumNetWeight: AbstractControl;
    sumContainers: AbstractControl;
    packageTypeId: AbstractControl;
    sumCbm: AbstractControl;
    sumPackages: AbstractControl;
    note: AbstractControl;
    containerDescription: AbstractControl;

    productServices: string[] = JobConstants.COMMON_DATA.PRODUCTSERVICE;
    serviceModes: string[] = JobConstants.COMMON_DATA.SERVICEMODES;
    shipmentModes: string[] = JobConstants.COMMON_DATA.SHIPMENTMODES;
    shipmentTypes: string[] = JobConstants.COMMON_DATA.SHIPMENTTYPES;
    commodityGroups: Observable<CommodityGroup[]>;

    customers: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;
    carries: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    users: Observable<User[]>;
    salesmans: User[]; // * Load động theo Partner được chọn.
    customerName: string;

    jobLinkAirSeaNo: string = '';
    jobLinkAirSeaInfo: LinkAirSeaModel;

    packageTypes: Observable<Unit[]>;
    containers: Container[];

    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Port Code' },
        { field: 'nameEn', label: 'Port Name' },
        { field: 'countryNameEN', label: 'Country' },
    ];

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'shortName', label: 'Name Abbr' },
        { field: 'partnerNameEn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' },
    ];

    displayFieldSalesman: CommonInterface.IComboGridDisplayField[] = [
        { field: 'username', label: 'User Name' },
        { field: 'employeeNameEn', label: 'Full Name' }
    ];

    userLogged: SystemInterface.IClaimUser;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        protected _documentRepo: DocumentationRepo,
        private _store: Store<IShareBussinessState>,
        private _fb: FormBuilder,
        private _toaster: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
    ) {
        super();
    }

    ngOnInit() {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this._store.dispatch(new GetCataloguePortAction({ placeType: CommonEnum.PlaceTypeEnum.Port }));
        this._store.dispatch(new GetCatalogueCarrierAction());
        this._store.dispatch(new GetCatalogueAgentAction());
        this._store.dispatch(new GetCatalogueCommodityGroupAction());

        this.ports = this._store.select(getCataloguePortState);
        this.carries = this._store.select(getCatalogueCarrierState);
        this.agents = this._store.select(getCatalogueAgentState);
        this.commodityGroups = this._store.select(getCatalogueCommodityGroupState);
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
        this.users = this._systemRepo.getListSystemUser();
        this.packageTypes = this._catalogueRepo.getUnit({ active: true, unitType: CommonEnum.UnitType.PACKAGE });
        this._store.dispatch(new ClearContainerAction());
        this._store.select(getContainerSaveState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res: any) => {
                if (!!res) {
                    this.containers = res;
                }
            })
        this._store.select(getCurrentUserState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res: any) => {
                if (!!res) {
                    this.userLogged = res;
                }
            })
        this.initForm();
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'supplier':
                this.supplierId.setValue(data.id);
                break;
            case 'pol':
                this.pol.setValue(data.id);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            case 'agent':
                this.agentId.setValue(data.id);
                break;
            case 'customer':
                this._toaster.clear();
                this.customerName = data.shortName;
                this.customerId.setValue(data.id);
                this._catalogueRepo.GetListSalemanByShipmentType(data.id, this.transactionType === 'TK' ? ChargeConstants.TK_CODE : ChargeConstants.CL_CODE, this.shipmentType.value)
                    .subscribe(
                        (res: any) => {
                            if (!!res) {
                                this.salesmans = res || [];
                                if (!!this.salesmans.length) {
                                    this.salemansId.setValue(res[0].id);
                                } else {
                                    this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                                        title: 'Notification',
                                        label: 'Ok',
                                        body: `<strong>${data.shortName}</strong> not have any agreement for service in this office <br/> Please check again!`
                                    });
                                    this.salemansId.setValue(null);
                                }
                            } else {
                                this.salesmans = [];
                                this.salemansId.setValue(null);
                            }
                        }
                    )
                break;
            case 'salesman':
                this.salemansId.setValue(data.id);
                break;
            case 'billingOps':
                this.billingOpsId.setValue(data.id);
                break;
            default:
                break;
        }
    }

    initForm() {
        this.formCreate = this._fb.group({
            hwbno: [null, Validators.compose([
                Validators.required,
                FormValidators.validateSpecialChar
            ])],
            mblno: [null, Validators.compose([
                Validators.required,
                FormValidators.validateSpecialChar
            ])],
            flightVessel: [],
            purchaseOrderNo: [],

            serviceDate: [null, Validators.required],

            productService: [null, Validators.required],
            serviceMode: [null, Validators.required],
            shipmentMode: [null, Validators.required],
            commodityGroupId: [],
            shipmentType: [this.shipmentTypes[0], Validators.required],

            customerId: [null, Validators.required],
            pol: [],
            pod: [],
            supplierId: [null, Validators.required],
            agentId: [],
            billingOpsId: [this.userLogged.id, Validators.required],
            salemansId: [null, Validators.required],
            isReplicate: [false],
            noProfit: [false],
            eta: [null],
            deliveryDate: [null],
            suspendTime: [null, Validators.compose([
                Validators.maxLength(150),
            ])],
            clearanceDate: [null],
            sumGrossWeight: [null],
            sumNetWeight: [null],
            sumContainers: [null],
            sumPackages: [null],
            packageTypeId: [null],
            sumCbm: [null],
            note: [null],
            containerDescription: [{ value: null, disabled: true }]

        }, { validator: FormValidators.comparePort });

        this.hwbno = this.formCreate.controls['hwbno'];
        this.mblno = this.formCreate.controls['mblno'];
        this.serviceDate = this.formCreate.controls['serviceDate'];
        this.productService = this.formCreate.controls['productService'];
        this.serviceMode = this.formCreate.controls['serviceMode'];
        this.shipmentMode = this.formCreate.controls['shipmentMode'];
        this.commodityGroupId = this.formCreate.controls['commodityGroupId'];
        this.customerId = this.formCreate.controls['customerId'];
        this.pol = this.formCreate.controls['pol'];
        this.pod = this.formCreate.controls['pod'];
        this.supplierId = this.formCreate.controls['supplierId'];
        this.agentId = this.formCreate.controls['agentId'];
        this.billingOpsId = this.formCreate.controls['billingOpsId'];
        this.shipmentType = this.formCreate.controls['shipmentType'];
        this.salemansId = this.formCreate.controls['salemansId'];
        this.eta = this.formCreate.controls['eta'];
        this.deliveryDate = this.formCreate.controls['deliveryDate'];
        this.suspendTime = this.formCreate.controls['suspendTime'];
        this.clearanceDate = this.formCreate.controls['clearanceDate'];
        this.sumGrossWeight = this.formCreate.controls['sumGrossWeight'];
        this.sumNetWeight = this.formCreate.controls['sumNetWeight'];
        this.sumContainers = this.formCreate.controls['sumContainers'];
        this.sumPackages = this.formCreate.controls['sumPackages'];
        this.packageTypeId = this.formCreate.controls['packageTypeId'];
        this.sumCbm = this.formCreate.controls['sumCbm']
        this.containerDescription = this.formCreate.controls['containerDescription'];
        if (this.transactionType === 'TK') {
            this.initTruckingData();
        }
    }

    initTruckingData() {
        this.productService.setValue('Trucking');
        this.shipmentModes = JobConstants.COMMON_DATA.SHIPMENTMODESTKI;
    }


    getASInfoToLink() {
        if (!this.hwbno.value || !this.mblno.value) {
            this._toaster.warning("MBL No and HBL No is empty. Please complete first!");
            return;
        }
        if (!this.productService.value || !this.serviceMode.value
            || (this.productService.value.indexOf('Sea') < 0 && this.productService.value !== 'Air')) {
            this._toaster.warning("Service's not valid to link. Please select another!");
        } else {
            this._documentRepo.getASTransactionInfo(null, this.mblno.value, this.hwbno.value, this.productService.value, this.serviceMode.value)
                .pipe(catchError(this.catchError))
                .subscribe((res: LinkAirSeaModel) => {
                    if (!!res?.jobNo) {
                        this.jobLinkAirSeaNo = res.jobNo;
                        this.jobLinkAirSeaInfo = res;
                        if (!this.customerId.value && !!res.customerId) {
                            this.customerId.setValue(res.customerId);
                        }
                        if (!this.salemansId.value && !!res.salemanId) {
                            this.salemansId.setValue(res.salemanId);
                        }
                        if (!this.serviceDate.value?.startDate && !!res.serviceDate) {
                            this.serviceDate.setValue({ startDate: new Date(res.serviceDate), endDate: new Date(res.serviceDate) });
                        }
                    }
                    else {
                        this.jobLinkAirSeaNo = null;
                        this._toaster.warning("There's no valid Air/Sea Shipment to display. Please check again!");
                    }
                });
        }
    }

    getSalesmanList(selectedShipmentType: any) {
        this.shipmentType.setValue(selectedShipmentType);
        if (!!this.customerId.value) {
            this._catalogueRepo.GetListSalemanByShipmentType(this.customerId.value, this.transactionType === 'TK' ? ChargeConstants.TK_CODE : ChargeConstants.CL_CODE, this.shipmentType.value)
                .subscribe(
                    (res: any) => {
                        if (!!res) {
                            this.salesmans = res || [];
                            if (!!this.salesmans.length) {
                                this.salemansId.setValue(res[0].id);
                            } else {
                                this.salemansId.setValue(null);
                                this.showPopupDynamicRender(InfoPopupComponent, this.viewContainerRef.viewContainerRef, {
                                    title: 'Notification',
                                    label: 'Ok',
                                    body: `<strong>${this.customerName}</strong> not have any agreement for service in this office <br/> please check again!`
                                });
                            }
                        } else {
                            this.salesmans = [];
                            this.salemansId.setValue(null);
                        }
                    }
                );
        }
    }

    showListContainer() {
        this.containerPopup.show();
    }
}
