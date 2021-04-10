import { Component, Output, EventEmitter, ViewChild, ChangeDetectorRef } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { catchError, concatMap, finalize, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { SortService } from '@services';
import { AccountingRepo, DocumentationRepo, CatalogueRepo, SystemRepo } from '@repositories';
import { Surcharge } from '@models';
import { ToastrService } from 'ngx-toastr';
import cloneDeep from 'lodash/cloneDeep';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { of } from 'rxjs';
import { formatCurrency, formatDate } from '@angular/common';
import { getMenuUserPermissionState, IAppState } from '@store';
import { Store } from '@ngrx/store';

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
        private _store: Store<IAppState>
    ) {
        super();
    }

    ngOnInit() {

        this.headers = [
            { title: 'No', field: 'no', sortable: true, width: 50 },
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
        this.referenceNos = this.getRefCombobox();
        this.initFormSearch();
        this.getPartner();
        this.getProductService();

        this._store.select(getMenuUserPermissionState)
            .pipe(
                switchMap((res: any) => this._sysRepo.getListUserWithPermission(res.menuId, 'Detail')),
                takeUntil(this.ngUnsubscribe)
            ).subscribe((res: any) => {
                this.personInCharges = res;
            });
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
            referenceInput: [null, Validators.required],
            referenceNo: [this.referenceNos[0].value, Validators.required],
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
                    // sort A -> Z theo text services 
                    this.sortIncreaseServices('text', true);
                    this.selectedServices = this.services;
                }
            );
    }

    sortIncreaseServices(sortField: string, order: boolean) {
        this.services = this._sortService.sort(this.services, sortField, order);
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
                break;
            default:
                break;
        }
    }

    mapDataSearch() {
        let _data = [];
        if (!!this.referenceInput.value) {
            if (this.referenceInput.value.length > 0) {
                const _keyword = this.referenceInput.value.split(/\n/).filter(item => item.trim() !== '').map(item => item.trim());
                _data = _keyword;
            }
        }
        return _data;
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

    isSoaCDNoteSelected() {
        if (this.referenceNo.value === this.referenceNos[0].value || this.referenceNo.value === this.referenceNos[1].value) {
            this.isSOACDNote = true;
        } else {
            this.isSOACDNote = false;
        }
    }

    sortSurcharge(dataSort: any) {
        this.shipments = this._sortService.sort(this.shipments, dataSort.sortField, dataSort.order);
    }

    closePopup() {
        this.resetFormShipmentInput();
        this.resetData();
        this.resetPartner();
        this.state = 'new';
        this.isSubmitted = false;
        this.serviceDate.setValue(null);
        this.personInCharge.setValue(null);
        this.hide();
    }

    getDetailShipmentOfSettle(surcharges: Surcharge[]) {
        if (!!surcharges) {
            const surcharge = [...surcharges].shift();
            let shipment = new ShipmentChargeSettlement();
            shipment.jobId = surcharge.jobId;
            shipment.mbl = surcharge.mbl;
            shipment.hbl = surcharge.hbl;
            shipment.hblId = surcharge.hblid;
            shipment.advanceNo = surcharge.advanceNo;
            shipment.customNo = surcharge.clearanceNo;
            shipment.chargeSettlements = surcharges.map((surcharge: Surcharge) => new Surcharge(surcharge));
            shipment.totalNetAmount = surcharges.filter((charge: Surcharge) => charge.currencyId === 'USD').reduce((net: number, charge: Surcharge) => net += charge.netAmount, 0);
            shipment.totalNetAmountVND = surcharges.filter((charge: Surcharge) => charge.currencyId === 'VND').reduce((net: number, charge: Surcharge) => net += charge.netAmount, 0);
            shipment.totalAmount = surcharges.filter((charge: Surcharge) => charge.currencyId === 'USD').reduce((net: number, charge: Surcharge) => net += charge.total, 0);
            shipment.totalAmountVND = surcharges.filter((charge: Surcharge) => charge.currencyId === 'VND').reduce((total: number, charge: Surcharge) => total += (charge.amountVnd + charge.vatAmountVnd), 0);
            shipment.totalNetVND = surcharges.reduce((net: number, charge: Surcharge) => net += charge.amountVnd, 0);
            shipment.totalVATVND = surcharges.reduce((net: number, charge: Surcharge) => net += charge.vatAmountVnd, 0);
            shipment.totalNetUSD = surcharges.reduce((net: number, charge: Surcharge) => net += charge.amountUSD, 0);
            shipment.totalVATUSD = surcharges.reduce((net: number, charge: Surcharge) => net += charge.vatAmountUSD, 0);
            shipment.totalVND = shipment.totalNetVND + shipment.totalVATVND;
            this.shipments = [...this.shipments, shipment];
            this.total.totalUSDStr = formatCurrency(this.shipments[0].totalNetUSD + this.shipments[0].totalVATUSD, 'en', '') + ' = ' + formatCurrency(this.shipments[0].totalNetUSD, 'en', '') + ' + ' + formatCurrency(this.shipments[0].totalVATUSD, 'en', '');
            this.totalAmountVnd = (this.shipments[0].totalVND).toLocaleString() + ' = ' + this.shipments[0].totalNetVND.toLocaleString() + ' + ' + this.shipments[0].totalVATVND.toLocaleString();
            this.total.totalShipment = 1;
            this.total.totalCharges = surcharges.length;

            this.orgChargeShipment = { shipmentSettlement: cloneDeep(this.shipments), total: cloneDeep(this.total) };
            this.getAdvnaceList(surcharge.hblid);
            this.checkedAllCharges();
        }
    }

    getAdvnaceList(hblId: string){
        this._accoutingRepo.getListAdvanceNoForShipment(hblId)
        .pipe(catchError(this.catchError), finalize(() => {
        }))
        .subscribe(
            (res: any[]) => {
                console.log('res', res)
                this.shipments.map(x=> x.advanceNoList = res);
            },
        );
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
            this.shipments.forEach((shipment: ShipmentChargeSettlement) => {
                shipment.chargeSettlements.filter((charge: Surcharge) => charge.isSelected)
                    .map((charge: Surcharge) => {
                        if (charge.currencyId === 'USD') {
                            charge.amountVnd = Number(charge.netAmount * exchangeRate);
                            charge.vatAmountVnd = charge.vatrate < 0 ? Number(charge.vatrate * exchangeRate) : (charge.amountVnd * (charge.vatrate / 100));
                            charge.finalExchangeRate = Number(exchangeRate);
                        }
                    });
                shipment.totalNetVND = shipment.chargeSettlements.reduce((net: number, charge: Surcharge) => net += charge.amountVnd, 0);
                shipment.totalVATVND = shipment.chargeSettlements.reduce((vat: number, charge: Surcharge) => vat += charge.vatAmountVnd, 0);
                shipment.totalVND = shipment.totalNetVND + shipment.totalVATVND;
            }
            );
            this.getTotalAmountVND();
        }
    }

    resetUpdateExchangeRateForCharges() {
        this.resetData();
        this.shipments = cloneDeep(this.orgChargeShipment.shipmentSettlement);
        this.total = { ...this.orgChargeShipment.total };
        this.getTotalAmountVND();
        this.checkedAllCharges();
    }

    getTotalAmountVND() {
        let netAmountVND = 0, vatAmountVND = 0, totalAmountVnd = 0;
        this.shipments.forEach((shipment: ShipmentChargeSettlement) => {
            netAmountVND += shipment.chargeSettlements.reduce((net: number, charge: Surcharge) => net += charge.amountVnd, 0);
            vatAmountVND += shipment.chargeSettlements.reduce((vat: number, charge: Surcharge) => vat += charge.vatAmountVnd, 0);
        });
        totalAmountVnd = vatAmountVND + netAmountVND;
        this.totalAmountVnd = (totalAmountVnd.toLocaleString() + ' : ' + netAmountVND.toLocaleString() + '+' + vatAmountVND.toLocaleString());
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

    setAdvanceToSurcharge(shipment: any, advanceNo: string) {
        shipment.chargeSettlements.forEach(element => {
            element.advanceNo = advanceNo;
        });
    }

    checkInputReference(){
        if(!!this.referenceInput.value){
            this.referenceInput.setValue(this.referenceInput.value.trim());
            if(this.referenceInput.value.length === 0){
                this.referenceInput.setValue(null);
            }
        }
    }
    // Get List Charges
    searchCharge() {
        this.isSubmitted = true;
        this.checkInputReference();
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
        this.isSoaCDNoteSelected();
        if (!this.referenceInput.value) {
            return;
        }
        if (this.isSOACDNote) {
            this.searchWithSOAOrCDNote(body);
        } else {
            if (!this.selectedPartnerData) {
                return
            }
            this._accoutingRepo.getExistingCharge(body)
                .pipe(catchError(this.catchError), finalize(() => this.isLoading = false))
                .subscribe(
                    (res: any = {}) => {
                        if (!!res && res.status === false) {
                            this._toastService.error(res.message);
                            this.isSubmitted = false;
                            return of(false);
                        }
                        if (!!res) {
                            this.shipments = res.shipmentSettlement;
                            this.total = res.total;
                            this.totalAmountVnd = this.total.totalVNDStr;
                            this.shipments.forEach((shipment: ShipmentChargeSettlement) => {
                                this.setAdvanceToSurcharge(shipment, shipment.advanceNo);
                            });
                            this.checkedAllCharges();
                            this.orgChargeShipment = cloneDeep(res);
                            this.isSubmitted = false;
                        }
                    }
                );
        }
    }

    searchWithSOAOrCDNote(body: any) {
        if (!this.selectedPartnerData) {
            this._accoutingRepo.getPartnerForSettlement(body)
                .pipe(
                    catchError(this.catchError), finalize(() => this.isLoading = false),
                    concatMap((res) => {
                        if (!res) {
                            this.isSubmitted = false;
                            return of(false);
                        }
                        if (res.length > 1) {
                            this._toastService.warning('Please Select Partner!');
                            this.isSubmitted = false;
                            return of(false);
                        }
                        this.selectedPartnerData = res[0];
                        this.selectedPartner = { field: 'id', value: res[0].id };
                        body.partnerId = this.selectedPartnerData.id;
                        if (!!res.length) {
                            return this._accoutingRepo.checkSoaCDNoteIsSynced(body).pipe(
                                catchError((err, caught) => this.catchError),
                                concatMap((rs: any) => {
                                    if (!rs.status) {
                                        this._toastService.error(rs.message);
                                        this.isSubmitted = false;
                                        return of(false);
                                    } else {
                                        return this._accoutingRepo.getExistingCharge(body);
                                    }
                                })
                            );
                        }
                    }))
                .subscribe(
                    (res: IGetExistsCharge) => {
                        if (!!res) {
                            this.orgChargeShipment = cloneDeep(res);
                            this.shipments = res.shipmentSettlement;
                            this.total = res.total;
                            this.totalAmountVnd = this.total.totalVNDStr;
                            this.shipments.forEach((shipment: ShipmentChargeSettlement) => {
                                this.setAdvanceToSurcharge(shipment, shipment.advanceNo);
                            });
                            this.checkedAllCharges();
                            this.isSubmitted = false;
                        }
                    }
                );
        } else{
            this._accoutingRepo.checkSoaCDNoteIsSynced(body).pipe(
                catchError((err, caught) => this.catchError),
                concatMap((rs: any) => {
                    if (!rs.status) {
                        console.log('mess', rs)
                        this._toastService.error(rs.message);
                        this.isSubmitted = false;
                        return of(false);
                    } else {
                        return this._accoutingRepo.getExistingCharge(body);
                    }
                })
            )
            .subscribe(
                (res: IGetExistsCharge) => {
                    if (!!res) {
                        this.orgChargeShipment = cloneDeep(res);
                        this.shipments = res.shipmentSettlement;
                        this.total = res.total;
                        this.totalAmountVnd = this.total.totalVNDStr;
                        this.shipments.forEach((shipment: ShipmentChargeSettlement) => {
                            this.setAdvanceToSurcharge(shipment, shipment.advanceNo);
                        });
                        this.checkedAllCharges();
                        this.isSubmitted = false;
                    }
                }
            );
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
    }

    // Add Charge Button event
    submit() {
        this.selectedCharge = [];
        this.shipments.forEach((shipment: any) =>
            shipment.chargeSettlements.filter((charge: Surcharge) => charge.isSelected)
                .map((surcharge: Surcharge) => this.selectedCharge.push(new Surcharge(surcharge)))
        );
        console.log('charges', this.selectedCharge)
        if (!this.selectedCharge.length) {
            this._toastService.warning(`Don't have any charges in this period, Please check it again! `);
            return;
        } else {
            this.selectedCharge.forEach(c => {
                if (!!c.invoiceDate) {
                    const [day, month, year]: string[] = c.invoiceDate.split("/");
                    c.invoiceDate = formatDate(new Date(+year, +month - 1, +day), 'yyyy-MM-dd', 'en');
                } else {
                    c.invoiceDate = null;
                }
                c.amountVnd = !c.amountVnd ? 0 : c.amountVnd;
                c.vatAmountVnd = !c.vatAmountVnd ? 0 : c.vatAmountVnd;
            });
            this.onRequest.emit(this.selectedCharge);
            this.selectedCharge = [];
            this.closePopup();
        }
    }

    reset() {
        this.referenceNo.setValue(null);
        this.referenceInput.setValue(null)
        this.resetPartner();
        this.isCheckAll = false;

        this.resetData();
        this.selectedServices = (this.initService || []).map((item: CommonInterface.IValueDisplay) => ({ id: item.value, text: item.displayName }));
        this.serviceDate.setValue(null);
        this.personInCharge.setValue(null);
        this.resetFormShipmentInput();
    }

    resetPartner() {
        this.selectedPartnerData = null;
        this.selectedPartner = {};
    }

    resetData() {
        this.shipments = [];
        this.total = {};
        this.totalAmountVnd = '';
    }

    resetFormShipmentInput() {
        this.referenceNo.setValue(this.referenceNos[0].value);
        this.referenceInput.setValue(null);
        this.selectedServices = (this.initService || []).map((item: CommonInterface.IValueDisplay) => ({ id: item.value, text: item.displayName }));
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

interface IGetExistsCharge {
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
    advanceNoList: string[] = [];
    totalNetAmount: number = 0;
    totalNetAmountVND: number = 0;
    totalAmount: number = 0;
    totalAmountVND: number = 0;
    totalNetVND: number = 0;
    totalVATVND: number = 0;
    totalNetUSD: number = 0;
    totalVATUSD: number = 0;
    totalVND: number = 0;
    advanceAmount: number = 0;
    balance: number = 0;
    chargeSettlements: Surcharge[] = [];
}