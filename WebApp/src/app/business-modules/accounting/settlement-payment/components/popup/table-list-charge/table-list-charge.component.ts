import { Component, OnInit, Output, EventEmitter, QueryList, ViewChildren, Input, ViewChild } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, FormControl } from '@angular/forms';
import { formatDate } from '@angular/common';

import { CustomDeclaration, Surcharge, Partner, Unit } from '@models';
import { CatalogueRepo, DocumentationRepo, OperationRepo, AccountingRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { PopupBase } from '@app';
import { ComboGridVirtualScrollComponent } from '@common';
import { IAppState, GetCatalogueUnitAction, getCatalogueUnitState } from '@store';
import { SystemConstants } from '@constants';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';

import cloneDeep from 'lodash/cloneDeep';
import { Observable, forkJoin, of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';


@Component({
    selector: 'table-list-charge-popup',
    templateUrl: './table-list-charge.component.html',
    styleUrls: ['./table-list-charge.component.scss']
})

export class SettlementTableListChargePopupComponent extends PopupBase implements OnInit {
    @Output() onChange: EventEmitter<Surcharge[]> = new EventEmitter<Surcharge[]>();
    @Output() onUpdate: EventEmitter<Surcharge[]> = new EventEmitter<Surcharge[]>();
    @ViewChildren('comboGridCharge') comboGridCharges: QueryList<ComboGridVirtualScrollComponent>;
    @ViewChild('comboGridAdv') comboGridAdv: ComboGridVirtualScrollComponent;

    headers: CommonInterface.IHeaderTable[];
    headerPartner: CommonInterface.IHeaderTable[] = [];
    partnerType: CommonInterface.IValueDisplay[];

    listCharges: any[];
    listUnits: Observable<Unit[]>;
    shipments: OperationInteface.IShipment[];
    cds: CustomDeclaration[];
    advs: IAdvanceShipment[] = [];
    listPartner: Partner[] = [];

    selectedShipment: OperationInteface.IShipment;
    selectedCD: CustomDeclaration;
    selectedAdvance: IAdvanceShipment;

    configChargeDisplayFields: CommonInterface.IComboGridDisplayField[];
    configShipmentDisplayFields: CommonInterface.IComboGridDisplayField[];
    configAdvanceDisplayFields: CommonInterface.IComboGridDisplayField[];
    configCustomDisplayFields: CommonInterface.IComboGridDisplayField[];

    formGroup: FormGroup;
    shipment: AbstractControl;
    customNo: AbstractControl;
    advanceNo: AbstractControl;

    serviceTypeId: string; // * service id for get charge catalogue.
    currencyId: string = 'VND'; // * Currency from form create.
    settlementCode: string = null; // * Settlement Code current if update, === null if create.

    charges: Surcharge[] = [];

    isUpdate: boolean = false;

    initShipments: OperationInteface.IShipment[];
    initCDs: CustomDeclaration[] = [];

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _documentRepo: DocumentationRepo,
        private _operationRepo: OperationRepo,
        private _accountingRepo: AccountingRepo,
        private _fb: FormBuilder,
        private _toastService: ToastrService,
        private _store: Store<IAppState>
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Name', field: 'chargeNameEn', sortable: true, required: true, width: 250 },
            { title: 'Payer', field: 'payerName', sortable: true, required: true, width: 250 },
            { title: 'Qty', field: 'quantity', sortable: true, required: true },
            { title: 'Unit', field: 'unitId', sortable: true, required: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true, required: true },
            { title: 'Currency', field: 'currencyId', sortable: true, required: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Total Amount', field: 'amount', sortable: true },
            { title: 'OBH Partner', field: 'partnerName', sortable: true, required: false, width: 250 },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Serie No', field: 'serieNo', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'Cont No', field: 'contNo', sortable: true },
        ];

        this.configChargeDisplayFields = [
            { field: 'chargeNameEn', label: 'Name' },
            { field: 'unitPrice', label: 'Unit Price' },
            { field: 'unit', label: 'Unit' },
            { field: 'code', label: 'Code' },
        ];

        this.configShipmentDisplayFields = [
            { field: 'jobId', label: 'JobID' },
            { field: 'mbl', label: 'MBL' },
            { field: 'hbl', label: 'HBL' },
        ];

        this.configAdvanceDisplayFields = [
            { field: 'advanceNo', label: 'Advance No' },
            { field: 'amount', label: 'Amount' },
            { field: 'requestCurrency', label: 'Currency' },
            { field: 'requestDate', label: 'Request Date' },
        ];

        this.configCustomDisplayFields = [
            { field: 'clearanceNo', label: 'Custom No' },
            { field: 'jobNo', label: 'JobID' },
        ];

        this.partnerType = [
            { displayName: 'Customer', value: CommonEnum.PartnerGroupEnum.CUSTOMER, fieldName: 'CUSTOMER' },
            { displayName: 'Carrier', value: CommonEnum.PartnerGroupEnum.CARRIER, fieldName: 'CARRIER' },
            { displayName: 'Agent', value: CommonEnum.PartnerGroupEnum.AGENT, fieldName: 'AGENT' },
        ];

        this.headerPartner = [
            { title: 'Name', field: 'partnerNameEn' },
            { title: 'Partner Code', field: 'taxCode' },
        ];

        this._store.dispatch(new GetCatalogueUnitAction());

        this.getMasterCharges();
        this.getShipmentCommonData();
        this.getCustomDecleration();
        this.initForm();
        this.getPartner();
        this.getUnits();
    }

    initForm() {
        this.formGroup = this._fb.group({
            shipment: [],
            customNo: [],
            advanceNo: []
        });

        this.shipment = this.formGroup.controls['shipment'];
        this.customNo = this.formGroup.controls['customNo'];
        this.advanceNo = this.formGroup.controls['advanceNo'];
    }

    getShipmentCommonData() {
        this._documentRepo.getShipmentAssginPIC().pipe(catchError(this.catchError))
            .subscribe(
                (res: OperationInteface.IShipment[]) => {
                    this.shipments = this.initShipments = res;
                }
            );
    }

    getCustomDecleration() {
        this._operationRepo.getListCustomNoAsignPIC()
            .pipe(
                catchError(this.catchError),
                map((response: any[]) => response.map((item: CustomDeclaration) => new CustomDeclaration(item))),
            )
            .subscribe(
                (res: any) => {
                    this.cds = this.initCDs = res || [];
                },
            );
    }

    getAdvances(jobNo: string, isUpdateFControl: boolean = true, settleCode: string = null) {
        this._accountingRepo.getAdvanceOfShipment(jobNo, settleCode)
            .pipe(
                catchError(this.catchError),
                map((res: IAdvanceShipment[]) => {
                    res.forEach((item: IAdvanceShipment) => {
                        item.requestDate = formatDate(item.requestDate, 'dd/MM/yyyy', 'en');
                    });
                    return res;
                })
            ).subscribe(
                (res: any[] = []) => {
                    this.advs = cloneDeep(res);

                    // ? Have rewrite default value
                    if (!this.advanceNo.value && isUpdateFControl) {
                        const advance: IAdvanceShipment = this.advs.find(i => i.jobId === this.selectedShipment.jobId);
                        if (!!advance) {
                            this.advanceNo.setValue(advance.advanceNo);
                            this.selectedAdvance = advance;
                        } else {
                            this.selectedAdvance = null;
                        }
                    }
                    if (!this.advs) {
                        this.selectedAdvance = null;
                    }

                    if (!isUpdateFControl) {
                        this.comboGridAdv.displaySelectedStr = '';
                    }
                }
            );
    }

    getPartner() {
        this._catalogueRepo.getListPartner(null, null, { active: true })
            .pipe(catchError(this.catchError))
            .subscribe(
                (partners: Partner[]) => {
                    this.listPartner = partners;
                }
            );
    }

    getUnits() {
        this.listUnits = this._store.select(getCatalogueUnitState);
    }

    getMasterCharges(serviceTypeId: string = null, isChangeService: boolean = false) {
        forkJoin([
            this._catalogueRepo.getListCharge(null, null, { active: true, type: CommonEnum.CHARGE_TYPE.CREDIT, serviceTypeId: serviceTypeId }),
            this._catalogueRepo.getListCharge(null, null, { active: true, type: CommonEnum.CHARGE_TYPE.OBH, serviceTypeId: serviceTypeId }),
        ]).pipe(
            map(([chargeCredit, chargeOBH]) => {
                return [...chargeCredit, ...chargeOBH];
            })
        ).subscribe(
            (res: any[]) => {
                this.listCharges = [...res];
                if (isChangeService) {
                    this.charges.forEach((charge: Surcharge) => {
                        if (charge.chargeId) {
                            charge.chargeId = null;
                            charge.chargeName = null;
                        }
                    });

                    this.comboGridCharges.forEach(c => {
                        c.displaySelectedStr = '';
                    });
                }
            }
        );
    }

    onSelectDataFormInfo(data: OperationInteface.IShipment | IAdvanceShipment | any, type: string) {
        this.isSubmitted = false;

        switch (type) {
            case 'shipment':
                this.serviceTypeId = data.service;
                this.selectedShipment = data;

                this.resetAdvanceData();

                this.customNo.setValue(null);
                this.getAdvances(this.selectedShipment.jobId, true, this.settlementCode);

                this.shipment.setValue(this.selectedShipment.hblid);

                const _customDeclarations = this.filterCDByShipment(data);

                if (_customDeclarations.length > 0) {
                    this.customNo.setValue(_customDeclarations[0].clearanceNo);
                }
                if (!!this.charges.length) {
                    if (this.utility.getServiceType(this.charges[0].jobId) !== this.utility.getServiceType(data.jobId)) {
                        this.getMasterCharges(this.serviceTypeId, true);
                    }
                    for (const charge of this.charges) {
                        charge.jobId = this.selectedShipment.jobId;
                        charge.jobNo = this.selectedShipment.jobId;
                    }
                }
                break;
            case 'cd':
                this.selectedCD = data;
                this.customNo.setValue(data.clearanceNo);

                this.resetAdvanceData();

                const _shipments = this.filterShipmentByCD(data);
                if (_shipments.length > 0) {
                    this.shipment.setValue(_shipments[0].hblid);
                    this.selectedShipment = _shipments[0];

                    if (!!this.charges.length && this.utility.getServiceType(this.charges[0].jobId) !== this.utility.getServiceType((data as CustomDeclaration).jobNo)) {
                        this.getMasterCharges(this.serviceTypeId, true);

                        for (const charge of this.charges) {
                            charge.jobId = this.selectedShipment.jobId;
                            charge.jobNo = this.selectedShipment.jobId;
                            charge.clearanceNo = data.clearanceNo
                        }
                    }
                    this.getAdvances(data.jobNo, true, this.settlementCode);

                } else {
                    this.selectedAdvance = null;
                }
                break;
            case 'advanceNo':
                this.selectedAdvance = data;
                this.advanceNo.setValue(data.advanceNo);

                if (!!this.charges.length) {
                    this.charges.forEach(c => c.advanceNo = this.selectedAdvance.advanceNo);
                }

                break;
            default:
                break;
        }
    }

    onSelectUnit(unitId: number, charge: Surcharge) {
        this.listUnits.subscribe(
            (units: Unit[] = []) => {
                const selectedUnit: Unit = units.find(u => u.id === unitId);
                if (selectedUnit) {
                    charge.unitName = selectedUnit.unitNameEn;
                    charge.unitId = unitId;
                }
            });
    }

    checkExistCharge(chargeId: string, charges: Surcharge[] = []) {
        if (!charges.length) {
            return false;
        }
        return charges.some((charge: Surcharge) => (charge.chargeId === chargeId));
    }

    filterCDByShipment(shipment: OperationInteface.IShipment): CustomDeclaration[] {
        return this.initCDs.filter((item: CustomDeclaration) => {
            return (item.jobNo === shipment.jobId);
        });
    }

    filterShipmentByCD(cd: CustomDeclaration): OperationInteface.IShipment[] {
        return this.initShipments.filter((item: OperationInteface.IShipment) => {
            return (item.jobId === cd.jobNo);
        });
    }

    onSelectDataTableInfo(data: any, chargeItem: Surcharge, type: string) {
        this.isSubmitted = false;
        chargeItem.isDuplicate = false;

        switch (type) {
            case 'charge':
                chargeItem.chargeCode = data.code;
                chargeItem.chargeName = data.chargeNameEn;
                chargeItem.chargeId = data.id;
                chargeItem.type = this.updateChargeType(data.type);
                chargeItem.chargeGroup = data.chargeGroup;
                // * Unit, Unit Price had value
                if (!chargeItem.unitId || chargeItem.unitPrice == null) {
                    this.listUnits.pipe(
                        switchMap((units: Unit[]) => of(units.find(u => u.id === data.unitId))),
                    ).subscribe(
                        (unit: Unit) => {
                            chargeItem.unitId = unit.id;
                            chargeItem.unitPrice = data.unitPrice;
                            chargeItem.unitName = unit.unitNameEn;

                            this.calculateTotal(chargeItem.vatrate, chargeItem.quantity, chargeItem.unitPrice, chargeItem);
                        }
                    );
                }

                // * Detect charge type
                if ((data.type || '').toLowerCase() !== CommonEnum.CHARGE_TYPE.OBH.toLowerCase()) {
                    chargeItem.obhPartnerName = null;
                } else {
                    this.headers = [...this.headers];
                    this.headers[8].required = true;
                }
                break;
            case 'payer':
                chargeItem.payer = data.shortName;
                chargeItem.objectBePaid = 'OTHER';
                chargeItem.paymentObjectId = data.id;
                chargeItem.payerId = null;
                chargeItem.obhPartnerName = '';
                break;
            case 'obh':
                chargeItem.obhPartnerName = data.shortName;
                chargeItem.obhId = data.id;
                chargeItem.objectBePaid = null;
                chargeItem.payerId = chargeItem.paymentObjectId;
                break;
            default:
                break;
        }
    }

    onSelectPartnerType(partnerType: CommonInterface.IValueDisplay, chargeItem: Surcharge, type: string, ) {
        let partner: Partner;
        switch (type) {
            case 'partner-type':
                switch (partnerType.value) {
                    case CommonEnum.PartnerGroupEnum.CUSTOMER:
                        partner = this.getPartnerById(this.selectedShipment.customerId);
                        break;
                    case CommonEnum.PartnerGroupEnum.CARRIER:
                        partner = this.getPartnerById(this.selectedShipment.carrierId);
                        break;
                    case CommonEnum.PartnerGroupEnum.AGENT:
                        partner = this.getPartnerById(this.selectedShipment.agentId);
                        break;
                    default:
                        break;
                }
                if (!!partner) {
                    chargeItem.paymentObjectId = partner.id;
                    chargeItem.payer = partner.shortName;
                }
                break;
            case 'obh-type':
                switch (partnerType.value) {
                    case CommonEnum.PartnerGroupEnum.CUSTOMER:
                        partner = this.getPartnerById(this.selectedShipment.customerId);
                        break;
                    case CommonEnum.PartnerGroupEnum.CARRIER:
                        partner = this.getPartnerById(this.selectedShipment.carrierId);
                        break;
                    case CommonEnum.PartnerGroupEnum.AGENT:
                        partner = this.getPartnerById(this.selectedShipment.agentId);
                        break;
                    default:
                        break;
                }
                if (!!partner) {
                    chargeItem.obhId = partner.id;
                    chargeItem.obhPartnerName = partner.shortName;
                }
                break;
        }
    }

    addCharge() {
        this.isSubmitted = false;
        this.charges.push(new Surcharge({
            currencyId: this.currencyId,
            id: SystemConstants.EMPTY_GUID,
            hblid: this.selectedShipment.hblid,
            isFromShipment: false,
            jobId: this.selectedShipment.jobId,
            mbl: this.selectedShipment.mbl,
            hbl: this.selectedShipment.hbl,
            settlementCode: this.settlementCode,
            invoiceDate: null,
            clearanceNo: !!this.selectedCD ? this.selectedCD.clearanceNo : null,
            chargeId: null,
            unitId: null,
            advanceNo: !!this.selectedAdvance ? this.selectedAdvance.advanceNo : null,
            jobNo: this.selectedShipment.jobId,
            mblno: this.selectedShipment.mbl,
            hblno: this.selectedShipment.hbl,
            quantity: 1
        }));
    }

    duplicateCharge(index: number) {
        this.isSubmitted = false;

        const newCharge = cloneDeep(this.charges[index]);

        newCharge.currencyId = this.currencyId;
        newCharge.id = SystemConstants.EMPTY_GUID;
        newCharge.hblid = this.selectedShipment.hblid;
        newCharge.isFromShipment = false;
        newCharge.jobId = this.selectedShipment.jobId;
        newCharge.mbl = this.selectedShipment.mbl;
        newCharge.hbl = this.selectedShipment.hbl;
        newCharge.advanceNo = !!this.selectedAdvance ? this.selectedAdvance.advanceNo : null;
        newCharge.clearanceNo = !!this.selectedCD ? this.selectedCD.clearanceNo : null;
        newCharge.settlementCode = this.settlementCode;
        if (!newCharge.invoiceDate || !newCharge.invoiceDate.startDate) {
            newCharge.invoiceDate = null;
        }

        this.charges.push(new Surcharge(newCharge));
        // this.charges = [...this.charges, new Surcharge(newCharge)];
    }

    deleteCharge(index: number) {
        this.isSubmitted = false;
        this.charges.splice(index, 1);
    }

    saveChargeList() {
        this.isSubmitted = true;

        if (!this.charges.length) {
            this._toastService.warning("Please add charge");
            return;
        }

        if (!this.checkValidate()) {
            return;
        }

        if (!this.checkDuplicate()) {
            this._toastService.warning("The Charge code and InvoiceNo is duplicated");
            return;
        }

        const listChargesToSave = cloneDeep(this.charges);
        for (const charge of listChargesToSave) {
            // *start: cập nhật shipment charges
            charge.clearanceNo = !!this.selectedCD ? this.selectedCD.clearanceNo : null;
            charge.advanceNo = !!this.selectedAdvance ? this.selectedAdvance.advanceNo : null;
            charge.jobId = this.selectedShipment.jobId;
            charge.jobNo = this.selectedShipment.jobId;
            charge.mblno = this.selectedShipment.mbl;
            charge.mbl = this.selectedShipment.mbl;
            charge.hblno = this.selectedShipment.hbl;
            charge.hbl = this.selectedShipment.hbl;
            charge.hblid = this.selectedShipment.hblid;
            // *end: cập nhật shipment charges

            if (charge.type === CommonEnum.CHARGE_TYPE.OBH) {
                // swap để map field cho chage obh
                charge.payerId = charge.paymentObjectId;
                charge.paymentObjectId = charge.obhId;
            }
            const date = charge.invoiceDate;
            if (typeof date !== 'string') {
                if (!!date && !!date.startDate) {
                    charge.invoiceDate = new Date(date.startDate);
                } else if (Object.prototype.toString.call(date) === '[object Date]') {
                    charge.invoiceDate = new Date(date);
                }
                else {
                    charge.invoiceDate = null;
                }
            }
        }
        console.log(listChargesToSave);
        if (this.isUpdate) {
            this.onUpdate.emit(listChargesToSave);
        } else {
            this.onChange.emit(listChargesToSave);
        }
        this.hide();
    }

    calculateTotal(vat: number, quantity: number, unitPrice: number, chargeItem: Surcharge) {
        this.isSubmitted = false;
        chargeItem.total = Math.round(this.utility.calculateTotalAmountWithVat(vat, quantity, unitPrice));
    }

    getPartnerById(id: string) {
        const partner: Partner = this.listPartner.find((p: Partner) => p.id === id);
        return partner || null;
    }

    initTableListCharge() {
        this.charges = [];
    }

    checkValidate() {
        let valid: boolean = true;
        for (const charge of this.charges) {
            if (
                !charge.paymentObjectId
                || !charge.chargeId
                || charge.quantity === null
                || !charge.unitId
                || charge.unitPrice === null
                || charge.quantity < 0
                || charge.unitPrice < 0
                || charge.vatrate > 100
                || charge.type.toLowerCase() === CommonEnum.CHARGE_TYPE.OBH.toLowerCase() && !charge.obhId
            ) {
                valid = false;
                break;
            }
        }
        return valid;
    }

    checkDuplicate() {
        let valid: boolean = true;
        const chargeIdInvoiceGroup = this.charges.map(c => {
            if (!!c.invoiceNo) {
                return c.chargeId + c.invoiceNo;
            }
            return null;
        }).filter(x => Boolean(x));   // * charge + Invoice

        const isDup: boolean = new Set(chargeIdInvoiceGroup).size !== chargeIdInvoiceGroup.length;

        if (isDup) {
            const arrayDuplicates = [...new Set(this.utility.findDuplicates(chargeIdInvoiceGroup))];
            const chargeMatchId: any[] = arrayDuplicates.map((c: string) => c.match(SystemConstants.CPATTERN.GUID));

            const chargeIds: string[] = [].concat.apply([], chargeMatchId);

            this.charges.forEach((c: Surcharge) => {
                if (chargeIds.includes(c.chargeId)) {
                    c.isDuplicate = true;
                } else {
                    c.isDuplicate = false;
                }
            });
            valid = false;
        } else {
            valid = true;
        }

        return valid;
    }

    resetFormControl(control: FormControl | AbstractControl) {
        if (!!control && control instanceof FormControl) {
            control.setValue(null);
            control.markAsUntouched({ onlySelf: true });
            control.markAsPristine({ onlySelf: true });
        }
    }

    updateChargeType(type: string) {
        switch (type) {
            case CommonEnum.CHARGE_TYPE.CREDIT:
                return CommonEnum.SurchargeTypeEnum.BUYING_RATE;
            case CommonEnum.CHARGE_TYPE.DEBIT:
                return CommonEnum.SurchargeTypeEnum.SELLING_RATE;
            default:
                return CommonEnum.SurchargeTypeEnum.OBH;
        }
    }

    removeAdvanceNo(advNo: string) {
        this.resetFormControl(this.advanceNo);
        this.selectedAdvance = null;
    }

    resetAdvanceData() {
        this.advanceNo.reset();
        this.comboGridAdv.displaySelectedStr = '';
        this.advs.length = 0;
    }

}

interface IAdvanceShipment {
    id: string;
    requestDate: string;
    jobId: string;
    hbl: string;
    mbl: string;
    amount: number;
    requestCurrency: string;
    advanceNo: string;
}


