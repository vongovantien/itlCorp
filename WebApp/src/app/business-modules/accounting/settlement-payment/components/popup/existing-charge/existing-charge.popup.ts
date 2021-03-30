import { Component, Output, EventEmitter, ViewChild, ChangeDetectorRef } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { catchError, concatMap, finalize, map, tap } from 'rxjs/operators';
import { SortService } from '@services';
import { AccountingRepo, DocumentationRepo, CatalogueRepo, SystemRepo } from '@repositories';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { Surcharge, User } from '@models';
import { ToastrService } from 'ngx-toastr';
import cloneDeep from 'lodash/cloneDeep';
import { ShareModulesInputShipmentPopupComponent } from 'src/app/business-modules/share-modules/components';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { from, Observable, of } from 'rxjs';
import { RouterStateSnapshot } from '@angular/router';
import { filter, split } from 'lodash';
import { formatDate } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { SystemConstants } from '@constants';
import { NullLogger } from '@microsoft/signalr';

@Component({
    selector: 'existing-charge-popup',
    templateUrl: './existing-charge.popup.html',
    styleUrls: ['./existing-charge.popup.scss']
})

export class SettlementExistingChargePopupComponent extends PopupBase {
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();

    headers: CommonInterface.IHeaderTable[];
    formSearch: FormGroup;
    referenceNo: AbstractControl;
    referenceInput: AbstractControl;
    serviceDate: AbstractControl;
    personInCharge: AbstractControl;
    exchangeRateInput: number = 0;
    advanceNo: any[] = [];
    totalAmountVnd: string = '';

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
    referenceNos: CommonInterface.ICommonTitleValue[];

    services: any[] = [];
    initService: any[] = [];
    selectedServices: any[] = [];
    advanceNoSelect: any = null;

    charges: Surcharge[] = [];
    selectedCharge: Surcharge[] = [];
    shipments: ShipmentChargeSettlement[] = [];
    total: any = {};
    orgChargeShipment: IGetExistsCharge;

    shipmentInput: OperationInteface.IInputShipment;
    isLoadingShipmentGrid: boolean = false;
    isLoadingPartnerGrid: boolean = false;
    isSOACDNote: boolean = false;

    numberOfShipment: string = '0/0';
    state: string = 'new';
    isCollapsed: boolean = false;
    searchCondition: any[] = ['SOA', 'Credit Note', 'JOB No', 'HBL/HAWB', 'MBL/MAWB', 'Custom No'];
    personInCharges: any[] = [];

    constructor(
        private _catalogue: CatalogueRepo,
        private _accoutingRepo: AccountingRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _documentRepo: DocumentationRepo,
        private _sysRepo: SystemRepo,
        private _cd: ChangeDetectorRef,
        private _fb: FormBuilder,
    ) {
        super();
    }

    ngOnInit() {

        this.headers = [
            { title: 'No', field: 'no', sortable: true , width: 50 },
            { title: 'Charge Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'Org NetAmount', field: 'unitName', sortable: true },
            { title: 'VAT', field: 'unitPrice', sortable: true },
            { title: 'Org Amount', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'Exc Rate', field: 'total', sortable: true },
            { title: 'VND NetAmount', field: 'settlementCode', sortable: true },
            { title: 'VAT VNDAmount', field: 'total', sortable: true },
            { title: 'VND Amount', field: 'total', sortable: true },
            { title: 'Invoice No', field: 'total', sortable: true },
            { title: 'Invoice Date', field: 'total', sortable: true },
            { title: 'Serial No', field: 'total', sortable: true },
            { title: 'Note', field: 'total', sortable: true },
        ];
        this.initFormSearch();
        this.referenceNos = this.getRefCombobox();
        this.getPartner();
        this.getProductService();
        this._sysRepo.getListUserWithPermission('settlement-payment', 'Detail')
            .pipe(
                tap((d) => {
                    this.personInCharge.setValue(!!d ? d.id : null);
                })
            )
            .subscribe((res: any) =>{
                this.personInCharges = res;
            }
            );
        this.referenceNo.setValue(this.referenceNos[0]);
    }

    getRefCombobox(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'SOA', value: 'Soa' },
            { title: 'Credit Note', value: 'CreditNote' },
            { title: 'JOB No', value: 'JOBNo' },
            { title: 'HBL/HAWB', value: 'HBL' },
            { title: 'MBL/MAWB', value: 'MBL' },
            { title: 'Custom No', value: 'CustomNo' },
        ];
    }

    getPartner() {
        this.isLoadingShipmentGrid = true;
        this._catalogue.getListPartner(null, null, { active: true })
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoadingShipmentGrid = false;
            }))
            .subscribe(
                (dataPartner: any) => {
                    this.getPartnerData(dataPartner);
                },
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

    initFormSearch() {
        this.formSearch = this._fb.group({
            referenceInput: ['', Validators.required],
            referenceNo: [Validators.required],
            serviceDate: [],
            personInCharge: [],
        });

        this.referenceInput = this.formSearch.controls['referenceInput'];
        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.serviceDate = this.formSearch.controls['serviceDate'];
        this.personInCharge = this.formSearch.controls['personInCharge'];
        this.selectedPartnerData = null;
    }

    getProductService() {
        this._sysRepo.getListServiceByPermision()
            .pipe(catchError(this.catchError))
            .subscribe(
                (data: CommonInterface.IValueDisplay[]) => {
                    this.initService = data;
                    this.services = (data || []).map((item: CommonInterface.IValueDisplay) => ({ id: item.value, text: item.displayName }));
                    this.selectedServices = this.services;

                    this.getShipment(null, this.selectedServices.map((service: { id: string, text: string }) => service.id));
                }
            );
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.selectedPartner = { field: 'id', value: data.id };
                this.selectedPartnerData = data;
                break;
            case 'service':
                this.selectedServices = [];
                this.selectedServices.push(...data);
                this.charges = [];

                if (!!this.selectedPartner.value && !!this.selectedPartnerData) {
                    // this.resetShipment();
                    this.getShipment(this.selectedPartnerData.id, this.selectedServices.map((service: { id: string, text: string }) => service.id));
                }

                break;
            case 'reference':
                if (this.referenceNo) {

                }
                break;
            default:
                break;
        }
    }

    onRemoveDataFormInfo(type: string) {
        switch (type) {
            case 'partner':
                this.selectedPartner = {};
                this.selectedPartnerData = null;
                // this.resetShipment();
                break;
            case 'shipment':
                this.selectedShipment = {};
                this.selectedShipmentData = null;
                break;
            default:
                break;
        }
    }

    refTypeSelectedChange(){
        switch (this.referenceNo.value) {
            case this.referenceNos[2].value:
            case this.referenceNos[3].value:
            case this.referenceNos[4].value:
            case this.referenceNos[5].value:

                break;
        }
    }

    getShipment(partnerId: string, service: string[]) {
        this.isLoadingShipmentGrid = true;
        this._documentRepo.getShipmentByPartnerOrService(partnerId, service)
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoadingShipmentGrid = false;
            }))
            .subscribe(
                (res: any) => {
                    this.configShipment.dataSource.length = 0;
                    this.configShipment.dataSource = [...this.configShipment.dataSource, ...cloneDeep(res)];
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

    

    mapDataSearch() {
        let _data = [];
        if (this.referenceInput) {
            if (this.referenceInput.value.length > 0) {
                const _keyword = this.referenceInput.value.split(/\n/).filter(item => item.trim() !== '').map(item => item.trim());
                _data = _keyword;
            }
        }
        return _data;
    }

    onAdvanceNoChange(event: any) {

    }

    reset() {
        this.referenceNo.setValue(null);
        this.referenceInput.setValue(null)
        // this.resetShipment();
        this.resetPartner();
        this.isCheckAll = false;

        this.shipments = [];
        this.selectedServices = (this.initService || []).map((item: CommonInterface.IValueDisplay) => ({ id: item.value, text: item.displayName }));

        this.resetFormShipmentInput();
    }

    onChangeCheckBoxCharge() {
        // this.isCheckAll = this.charges.every((item: Surcharge) => item.isSelected);
    }

    checkUncheckAllCharge(hblId: string) {
        this.shipments.filter((shipment: any) => shipment.hblId === hblId)
            .map((shipment: any) => {
                for (const charge of shipment.chargeSettlements) {
                    charge.isSelected = shipment.isSelected;
                }
            }
            );
    }

    // resetShipment() {
    //     this.selectedShipment = {};
    //     this.selectedShipmentData = null;
    //     this.configShipment = {
    //         ...this.configShipment,
    //         dataSource: [],
    //     };
    //     this._cd.detectChanges();
    // }

    resetPartner() {
        this.selectedPartnerData = null;
        this.selectedPartner = {};
        // this.getPartner();
    }

    sortSurcharge(dataSort: any) {
        this.shipments = this._sortService.sort(this.shipments, dataSort.sortField, dataSort.order);
    }

    closePopup() {
        this.resetFormShipmentInput();
        this.resetData();
        // this.resetShipment();
        this.resetPartner();
        this.serviceDate.setValue(null);
        this.hide();
    }

    getDetailShipmentOfSettle(surcharges: Surcharge[]) {
        if (!!surcharges) {
            const surcharge = [...surcharges].shift();
            let shipment = new ShipmentChargeSettlement();
            shipment.jobId = surcharge.jobId;
            shipment.mbl = surcharge.mblno;
            shipment.hbl = surcharge.hblno;
            shipment.hblId = surcharge.hblid;
            shipment.advanceNo = surcharge.advanceNo;
            shipment.chargeSettlements = surcharges.map((surcharge: Surcharge) => new Surcharge(surcharge));
            shipment.totalNetAmount = surcharges.filter((charge: Surcharge) => charge.currencyId === 'USD').reduce((net: number, charge: Surcharge) => net += charge.netAmount, 0);
            shipment.totalNetAmountVND = surcharges.filter((charge: Surcharge) => charge.currencyId === 'VND').reduce((net: number, charge: Surcharge) => net += charge.netAmount, 0);
            shipment.totalVATUSD = surcharges.reduce((net: number, charge: Surcharge) => net += charge.vatAmountVnd, 0);
            shipment.totalAmount = surcharges.filter((charge: Surcharge) => charge.currencyId === 'USD').reduce((net: number, charge: Surcharge) => net += charge.total, 0);
            shipment.totalAmountVND = surcharges.filter((charge: Surcharge) => charge.currencyId === 'VND').reduce((net: number, charge: Surcharge) => net += charge.total, 0);
            shipment.totalNetVND = surcharges.reduce((net: number, charge: Surcharge) => net += charge.amountVnd, 0);
            shipment.totalVATVND = surcharges.reduce((net: number, charge: Surcharge) => net += charge.vatAmountVnd, 0);
            shipment.totalVND = shipment.totalNetVND + shipment.totalVATVND;
            this.shipments = [...this.shipments, shipment];
            //     this.total = {};
            this.total.totalUSDStr = (this.shipments[0].totalNetAmount + this.shipments[0].totalVATUSD).toString() + ' = ' + this.shipments[0].totalNetAmount.toString() + ' + ' + this.shipments[0].totalVATUSD.toString();
            this.totalAmountVnd = (this.shipments[0].totalNetAmountVND + this.shipments[0].totalVATVND).toLocaleString() + ' = ' + this.shipments[0].totalNetAmountVND.toLocaleString() + '+' + this.shipments[0].totalVATVND.toLocaleString();
            this.total.totalShipment = 1;
            this.total.totalCharges = surcharges.length;
            this.checkedAllCharges();
        }
    }

    onShipmentList(data: any) {
        // this.shipmentInput = data;
        // if (data) {
        //     this.numberOfShipment = this.shipmentInput.keyword.split(/\n/).filter(item => item.trim() !== '').map(item => item.trim()).length;
        //     this.resetShipment();
        // } else {
        //     this.numberOfShipment = 0;
        // }
    }

    resetData(){
        this.shipments = [];
        this.total = {};
        this.totalAmountVnd = '';
    }
    resetFormShipmentInput() {
        this.referenceNo.setValue(null);
        this.referenceInput.setValue(null);
        this.selectedServices = (this.initService || []).map((item: CommonInterface.IValueDisplay) => ({ id: item.value, text: item.displayName }));
        // this.inputShipmentPopupComponent.shipmentSearch = '';
        // this.shipmentInput = null;
        // this.inputShipmentPopupComponent.selectedShipmentType = "JOBID";
    }

    updateExchangeRateForCharges() {
        this.shipments.forEach((shipment: any) =>
            shipment.chargeSettlements.filter((charge: Surcharge) => charge.isSelected)
                .map((surcharge: Surcharge) => this.selectedCharge.push(new Surcharge(surcharge)))
        );
        if (this.selectedCharge.length === 0) {
            this._toastService.warning(`None of charges are selected, Please recheck again! `);
            return;
        } else {
            const exchangeRate = !this.exchangeRateInput ? 1 : this.exchangeRateInput;
            this.shipments.forEach((shipment: ShipmentChargeSettlement) =>
                {
                    shipment.chargeSettlements.filter((charge: Surcharge) => charge.isSelected)
                        .map((charge: Surcharge) => {
                            if (charge.currencyId === 'USD') {
                                charge.amountVnd = Number(charge.netAmount * exchangeRate);
                                charge.vatAmountVnd = Number(charge.vatrate * exchangeRate);
                                charge.totalAmountVnd = Number(charge.amountVnd + charge.vatAmountVnd);
                                charge.finalExchangeRate = Number(exchangeRate);
                        }});
                        shipment.totalNetVND = shipment.chargeSettlements.reduce((net : number , charge: Surcharge) => net += charge.amountVnd, 0);
                        shipment.totalVATVND = shipment.chargeSettlements.reduce((vat : number , charge: Surcharge) => vat += charge.vatAmountVnd, 0);
                        shipment.totalVND = shipment.totalNetVND + shipment.totalVATVND;
                }
            );
            // this.selectedCharge.forEach((charge: Surcharge) => {
            //     if (charge.currencyId === 'USD') {
            //         charge.amountVnd = charge.netAmount * exchangeRate;
            //         charge.vatAmountVnd = charge.vatrate * exchangeRate;
            //         charge.totalAmountVnd = charge.amountVnd + charge.vatAmountVnd;
            //         charge.finalExchangeRate = exchangeRate;
            //     }
            // });
            let netAmountVND = 0, vatAmountVND = 0, totalAmountVnd = 0;
            this.shipments.forEach((shipment: ShipmentChargeSettlement) => {
                netAmountVND += shipment.chargeSettlements.reduce((net : number , charge: Surcharge) => net += charge.amountVnd, 0);
                vatAmountVND += shipment.chargeSettlements.reduce((vat : number , charge: Surcharge) => vat += charge.vatAmountVnd, 0);
            });
            totalAmountVnd = vatAmountVND + netAmountVND;
            this.totalAmountVnd = (totalAmountVnd.toLocaleString() + ' = ' + netAmountVND.toLocaleString() + ' + ' + vatAmountVND.toLocaleString());
        }
    }

    resetUpdateExchangeRateForCharges() {
        this.resetData();
        this.shipments = cloneDeep(this.orgChargeShipment.shipmentSettlement);
        this.total = {...this.orgChargeShipment.total};
        let netAmountVND = 0, vatAmountVND = 0, totalAmountVnd = 0;
        this.shipments.forEach((shipment: ShipmentChargeSettlement) => {
            netAmountVND += shipment.chargeSettlements.reduce((net : number , charge: Surcharge) => net += charge.amountVnd, 0);
            vatAmountVND += shipment.chargeSettlements.reduce((vat : number , charge: Surcharge) => vat += charge.vatAmountVnd, 0);
        });
        totalAmountVnd = vatAmountVND + netAmountVND;
        this.totalAmountVnd = (totalAmountVnd.toLocaleString() + ' : ' + netAmountVND.toLocaleString() + '+' + vatAmountVND.toLocaleString());
        console.log('old1', this.orgChargeShipment)
        this.checkedAllCharges();
    }

    onBlurAnyCharge(e: any, hblId: string) {
        this.updateForChargerByFieldName(e.target.name, e.target.value, hblId);
    }

    updateForChargerByFieldName(field: string, value: string, hblId: string) {
        this.shipments.filter((shipment: any) => shipment.hblId === hblId).map((shipment: any) => {
            shipment.chargeSettlements.forEach(ele => {
                if (ele[field] === null || ele[field] === "") {
                    ele[field] = value;
                }
            });
        });
    }

    setAdvanceToSurcharge(shipment: any, advanceNo: string){
        shipment.chargeSettlements.forEach(element => {
            element.advanceNo = advanceNo;
        });
    }

    // Get List Charges
    searchCharge() {
        
        this.isSubmitted = true;
        let soaNo = [], cdNote = [], jobNo = [], hblNo = [], mblNo = [], customNo = [];
        if (!!this.referenceNo) {
            switch (this.referenceNo.value) {
                case this.referenceNos[0].value:
                    soaNo = this.mapDataSearch();
                    break;
                case this.referenceNos[1].value:
                    cdNote = this.mapDataSearch();
                    break;
                case this.referenceNos[2].value:
                    jobNo = this.mapDataSearch();
                    break;
                case this.referenceNos[3].value:
                    hblNo = this.mapDataSearch();
                    break;
                case this.referenceNos[4].value:
                    mblNo = this.mapDataSearch();
                    break;
                case this.referenceNos[5].value:
                    customNo = this.mapDataSearch();
                    break;
            }
        }
        let body: ISearchExistsChargeSettlePayment = {
            partnerId: !this.selectedPartnerData ? null : this.selectedPartnerData.id,
            soaNo: soaNo || [],
            creditNo: cdNote || [],
            jobIds: jobNo || [],
            hbls: hblNo || [],
            mbls: mblNo || [],
            customNos: customNo || [],
            servicesType: !this.selectedServices ? null : this.selectedServices.map((item: any) => item.id).toString(),
            serviceDateFrom: !!this.serviceDate.value ? (!this.serviceDate.value.startDate ? null : formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en')) : null,
            serviceDateTo: !!this.serviceDate.value ? (!this.serviceDate.value.endDate ? null : formatDate(this.serviceDate.value.endDate, 'yyyy-MM-dd', 'en')) : null,
            personInCharge: !this.personInCharge.value ? this.personInCharges.map((item: any) => item.id).join(';') : this.personInCharge.value.join(';'),
        };
console.log('body', body)
console.log('body2', this.selectedPartnerData)
        this.isSoaCDNoteSelected();
        if (!this.selectedPartnerData && (this.referenceNo.value === this.referenceNos[0].value || this.referenceNo.value === this.referenceNos[1].value)) {
            if(!this.referenceInput.value){
                return;
            }
            this.searchWithSOAOrCDNote(body);
            // if(this.configPartner.dataSource.length === 0){
            //     this._toastService.warning('There is no partner data to search, please recheck data!');
            // }  
        }else{
            if(!this.selectedPartnerData){
                return
            }
            this._accoutingRepo.getExistingCharge(body)
            .pipe(catchError(this.catchError), finalize(() => this.isLoading = false))
            .subscribe(
                (res: any = {}) => {
                    if (!!res && res.status === false) {
                        this._toastService.error(res.message);
                        return;
                    }
                    if(!!res){
                        // this.charges = res.shipmentSettlement.chargeSettlements;
                        this.shipments = res.shipmentSettlement;
                        this.total = res.total;
                        this.totalAmountVnd = this.total.totalVNDStr;
                        console.log('ref2', this.referenceNo.value)
                        this.checkedAllCharges();
                        this.orgChargeShipment = cloneDeep(res);
                        this.isSubmitted = false;
                    }
                }
            );
        }
        // this.isLoading = false;
        // this.isCheckAll = false;
    }

    searchWithSOAOrCDNote(body: any){
        this._accoutingRepo.getPartnerForSettlement(body)
        .pipe(
            catchError(this.catchError), finalize(() => this.isLoading = false),
            concatMap((res) => {
                if(!res){
                    return of(false);
                }
                if (res.length > 1) {
                    this._toastService.warning('Please Select Partner!');
                    return of(false);
                }
                // this.configPartner.dataSource = res;
                this.selectedPartnerData = res[0];
                this.selectedPartner = { field: 'id', value: res[0].id };
                body.partnerId = this.selectedPartnerData.id;
                if(!!res.length){
                    return this._accoutingRepo.checkSoaCDNoteIsSynced(body).pipe(
                        catchError((err, caught) => this.catchError),
                        concatMap((rs: any) => {
                            if (!!rs) {
                                this._toastService.error(rs);
                                return;
                            }else{
                                return this._accoutingRepo.getExistingCharge(body);
                            }
                        })
                    );
                }
                // return this._accoutingRepo.getExistingCharge(body);
            }))
        .subscribe(
            (res: IGetExistsCharge) => {
                if(!!res){
                this.orgChargeShipment = cloneDeep(res);
                this.shipments = res.shipmentSettlement;
                this.total = res.total;
                this.totalAmountVnd = this.total.totalVNDStr;
                this.checkedAllCharges();
                this.isSubmitted = false;
                }
            }
            // (res: HttpErrorResponse) => {
            //     console.log('body5', res)
            //     return;
            // }
        );
    }

    isSoaCDNoteSelected() {
        if (this.referenceNo.value === this.referenceNos[0].value || this.referenceNo.value === this.referenceNos[1].value) {
            this.isSOACDNote = true;
        } else {
            this.isSOACDNote = false;
        }
    }

    checkedAllCharges() {
        this.shipments.forEach((shipment: any) => {
            shipment.isSelected = true;
            shipment.chargeSettlements.map((i: Surcharge) => {
            i.isSelected = true;
                if (!!i.invoiceDate) {
                    i.invoiceDate = formatDate(new Date(i.invoiceDate), 'dd/MM/yyyy', 'en');
                } else {
                    i.invoiceDate = null;
                }
                }
            ); 
        });
        // this.shipments.forEach((shipment: any) => {
        //     shipment.chargeSettlements.map((i: any) => {
        //         if (!!i.invoiceDate) {
        //             i.invoiceDate = formatDate(new Date(i.invoiceDate), 'dd/MM/yyyy', 'en');
        //         }
        //     });
        // });
    }
    
    // Add Charge Button event
    submit() {
        this.selectedCharge = [];
        this.shipments.forEach((shipment: any) =>
            shipment.chargeSettlements.filter((charge: Surcharge) => charge.isSelected)
                .map((surcharge: Surcharge) => this.selectedCharge.push(new Surcharge(surcharge)))
        );
        // for (const charge of this.selectedCharge) {
        //     const date = charge.invoiceDate;
        //     if (typeof date !== 'string') {
        //         if (Object.prototype.toString.call(date) !== '[object Date]') {
        //             if (!charge.invoiceDate) {
        //                 charge.invoiceDate = null;
        //             } else {
        //                 charge.invoiceDate = new Date(date.startDate);
        //             }
        //         }
        //     }
        // }
        console.log('charges', this.selectedCharge)
        if (!this.selectedCharge.length) {
            this._toastService.warning(`Don't have any charges in this period, Please check it again! `);
            return;
        } else {
            this.onRequest.emit(this.selectedCharge);
            this.selectedCharge = [];
            // this.total = {};
            // this.hide();
            this.closePopup();
        }
    }
    
}
export interface ISearchExistsChargeSettlePayment {
    soaNo: string[];
    creditNo: string[];
    jobIds: string[];
    hbls: string[];
    mbls: string[];
    customNos: string[];
    partnerId: string;
    servicesType: string;
    serviceDateFrom: string;
    serviceDateTo: string;
    personInCharge: string;
}

interface IGetExistsCharge{
    shipmentSettlement: ShipmentChargeSettlement[];
    total: any;
}

class ShipmentChargeSettlement {
    isSelected: boolean = true;
    settlementNo: string = '';
    jobId: string = '';
    mbl: string = '';
    hbl: string = '';
    hblId: string = '';
    type: string = '';
    shipmentId: string = '';
    customNo: string = '';
    advanceNo: string = '';
    advanceNoList: [] = [];
    totalNetAmount: number=0;
    totalNetAmountVND: number=0;
    totalAmount: number=0;
    totalAmountVND: number=0;
    totalNetVND: number=0;
    totalVATVND: number=0;
    totalNetUSD: number=0;
    totalVATUSD: number=0;
    totalVND: number=0;
    advanceAmount: number=0;
    balance: number=0;
    chargeSettlements: Surcharge[] = [];
}