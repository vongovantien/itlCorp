import { Component, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SystemConstants } from 'src/constants/system.const';
import { takeUntil, catchError, finalize } from 'rxjs/operators';
import { DataService, SortService } from 'src/app/shared/services';
import { OperationRepo, AccoutingRepo, DocumentationRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { Surcharge } from 'src/app/shared/models';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'existing-charge-popup',
    templateUrl: './existing-charge.popup.html'
})

export class SettlementExistingChargePopupComponent extends PopupBase {
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();

    headers: CommonInterface.IHeaderTable[];

    configPartner: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    selectedPartner: Partial<CommonInterface.IComboGridData> = {};
    selectedPartnerData: any;

    configShipment: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    selectedShipment: Partial<CommonInterface.IComboGridData> = {};
    selectedShipmentData: OperationInteface.IShipment;

    services: any[] = [];
    initService: any[] = [];
    selectedServices: any[] = [];

    charges: Surcharge[] = [];
    selectedCharge: Surcharge[] = [];

    resetButtonSetting: ButtonModalSetting = {
        buttonAttribute: Object.assign(this.cancelButtonSetting.buttonAttribute, { titleButton: 'reset', icon: 'la la-refresh' }),
        typeButton: ButtonType.reset,
    };


    constructor(
        private _dataService: DataService,
        private _catalogue: CatalogueRepo,
        private _catalogueRepo: CatalogueRepo,
        private _accoutingRepo: AccoutingRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _documentRepo: DocumentationRepo
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'Qty', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unitName', sortable: true },
            { title: 'Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Amount', field: 'total', sortable: true },
            { title: 'Settlement No', field: 'settlementCode', sortable: true },
        ];

        this.getPartner();
        this.getProductService();
    }

    getPartner() {
        this._dataService.getDataByKey(SystemConstants.CSTORAGE.PARTNER)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError)
            )
            .subscribe(
                (data: any) => {
                    if (!data) {
                        this._catalogue.getListPartner(null, null, { inactive: false })
                            .pipe(catchError(this.catchError))
                            .subscribe(
                                (dataPartner: any) => {
                                    this.getPartnerData(dataPartner);
                                },
                            );
                    } else {
                        this.getPartnerData(data);
                    }
                }
            );
    }

    getPartnerData(data: any) {
        this.configPartner.dataSource = data;
        this.configPartner.displayFields = [
            { field: 'shortName', label: 'Name' },
            { field: 'partnerNameEn', label: 'Customer Name' },
        ];
        this.configPartner.selectedDisplayFields = ['shortName'];
    }

    getProductService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (data: CommonInterface.IValueDisplay[]) => {
                    this.initService = data;
                    this.services = (data || []).map((item: CommonInterface.IValueDisplay) => ({ id: item.value, text: item.displayName }));
                    this.selectedServices = this.services;
                }
            );
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.selectedPartner = { field: data.partnerNameEn, value: data.partnerNameEn };
                this.selectedPartnerData = data;

                this.resetShipment();
                this.getShipment(this.selectedPartnerData.id, this.selectedServices.map((service: { id: string, text: string }) => service.id));
                break;
            case 'service':
                this.selectedServices = [];
                this.selectedServices.push(...data);
                this.charges = [];

                if (!!this.selectedPartner.value && !!this.selectedPartnerData) {
                    this.resetShipment();
                    this.getShipment(this.selectedPartnerData.id, this.selectedServices.map((service: { id: string, text: string }) => service.id));
                }

                break;
            case 'shipment':
                this.selectedShipment = { field: data.jobId, value: data.hbl };
                this.selectedShipmentData = data;
                break;
            default:
                break;
        }
    }


    getShipment(partnerId: string, service: string[]) {
        this._documentRepo.getShipmentByPartnerOrService(partnerId, service)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.configShipment.dataSource = res;
                    this.configShipment.displayFields = [
                        { field: 'jobId', label: 'Job No' },
                        { field: 'mbl', label: 'MBL' },
                        { field: 'hbl', label: 'HBL' },
                    ];
                    this.configShipment.selectedDisplayFields = ['jobId', `mbl`, 'hbl'];

                    // * IF PARTNER HAS ONLY ONE SHIPMENT => SELECT THAT SHIPMENT AS WELL.
                    if (this.configShipment.dataSource.length === 1) {
                        this.selectedShipment = { field: 'jobId', value: this.configShipment.dataSource[0].jobId };
                        this.selectedShipmentData = this.configShipment.dataSource[0];
                    }

                    this.isCheckAll = false;
                }
            );
    }

    searchCharge() {
        if (!this.selectedShipmentData) {
            return;
        } else {
            this.isLoading = true;
            this.isCheckAll = false;

            this._accoutingRepo.getExistingCharge(this.selectedShipmentData.jobId, this.selectedShipmentData.hbl, this.selectedShipmentData.mbl)
                .pipe(catchError(this.catchError), finalize(() => this.isLoading = false))
                .subscribe(
                    (res: any) => {
                        this.charges = res;
                    }
                );
        }
    }

    reset() {
        this.resetShipment();
        this.resetPartner();
        this.isCheckAll = false;

        this.charges = [];
        this.selectedServices = (this.initService || []).map((item: CommonInterface.IValueDisplay) => ({ id: item.value, text: item.displayName }));

    }

    onChangeCheckBoxCharge() {
        this.isCheckAll = this.charges.every((item: Surcharge) => item.isSelected);
    }

    checkUncheckAllCharge() {
        for (const charge of this.charges) {
            charge.isSelected = this.isCheckAll;
        }
    }

    resetShipment() {
        this.selectedShipment = {};
        this.selectedShipmentData = null;
        this.configShipment.dataSource = [];
    }

    resetPartner() {
        this.selectedPartnerData = null;
        this.selectedPartner = {};
    }

    sortSurcharge(dataSort: any) {
        this.charges = this._sortService.sort(this.charges, dataSort.sortField, dataSort.order);
    }

    closePopup() {
        this.charges = [];
        this.selectedServices = (this.initService || []).map((item: CommonInterface.IValueDisplay) => ({ id: item.value, text: item.displayName }));
        this.isCheckAll = false;

        this.resetShipment();
        this.resetPartner();
        this.hide();
    }

    submit() {
        this.selectedCharge = this.charges
            .filter((charge: Surcharge) => charge.isSelected && !charge.settlementCode)
            .map((surcharge: Surcharge) => new Surcharge(surcharge));

        if (!this.selectedCharge.length) {
            this._toastService.warning(`Don't have any charges in this period, Please check it again! `, '', { positionClass: 'toast-bottom-right' });
            return;
        } else {
            this.onRequest.emit(this.selectedCharge);
            this.hide();
        }
    }
}

