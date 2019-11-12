import { PopupBase } from "src/app/popup.base";
import { Component, ViewChild, EventEmitter, Output } from "@angular/core";
import { DocumentationRepo } from "src/app/shared/repositories";
import { catchError, map } from "rxjs/operators";
import { ConfirmPopupComponent, InfoPopupComponent } from "src/app/shared/common/popup";
import { CdNoteAddRemainingChargePopupComponent } from "../add-remaining-charge/add-remaining-charge.popup";
import { SortService } from "src/app/shared/services";
import { ChargeCdNote } from "src/app/shared/models/document/chargeCdNote.model";
import { ToastrService } from "ngx-toastr";
import { AcctCDNote } from "src/app/shared/models/document/acctCDNote.model";
import { TransactionTypeEnum } from "src/app/shared/enums/transaction-type.enum";
@Component({
    selector: 'cd-note-add-popup',
    templateUrl: './add-cd-note.popup.html'
})
export class CdNoteAddPopupComponent extends PopupBase {
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();
    @Output() onUpdate: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('changePartnerPopup', { static: false }) changePartnerPopup: ConfirmPopupComponent;
    @ViewChild('notExistsChargePopup', { static: false }) notExistsChargePopup: InfoPopupComponent;
    @ViewChild(CdNoteAddRemainingChargePopupComponent, { static: false }) addRemainChargePopup: CdNoteAddRemainingChargePopupComponent;

    headers: CommonInterface.IHeaderTable[];

    noteTypes = [
        { text: 'DEBIT', id: 'DEBIT' },
        { text: 'CREDIT', id: 'CREDIT' },
        { text: 'INVOICE', id: 'INVOICE' }
    ];

    action: string = 'create';
    cdNoteCode: string = '';
    cdNoteId: string = '';

    currentMBLId: string = '';
    selectedNoteType: string = '';

    CDNote: AcctCDNote = new AcctCDNote();
    listChargePartner: ChargeCdNote[] = [];
    initGroup: ChargeCdNote[] = [];

    selectedPartner: any = {};
    partnerCurrent: any = {};
    isHouseBillID: boolean = false;

    configPartner: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    isCheckAllCharge: boolean = false;
    totalCredit: string = '';
    totalDebit: string = '';
    balanceAmount: string = '';

    constructor(
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
    ) {
        super();
        this.selectedNoteType = "DEBIT";
        console.log(this.selectedNoteType);
        this.requestSort = this.sortCdNoteCharge;
    }

    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true },
            { title: 'Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'nameEn', sortable: true },
            { title: 'Quantity', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unit', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: "Credit Value", field: 'total', sortable: true },
            { title: "Debit Value", field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true }
        ];
    }

    closePopup() {
        this.hide();
        //Reset popup về default
        this.selectedNoteType = "DEBIT";
        this.selectedPartner = {};;
        this.partnerCurrent = {};
        this.listChargePartner = [];
        this.initGroup = [];
        console.log(this.selectedPartner);
    }

    getListSubjectPartner(mblId: any) {
        const isHouseBillID = false;
        this._documentationRepo.getPartners(mblId, isHouseBillID)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (dataPartner: any) => {
                    this.getPartnerData(dataPartner);
                },
            );
    }

    getPartnerData(data: any) {
        this.configPartner.dataSource = data;
        this.configPartner.displayFields = [
            { field: 'id', label: 'Partner ID' },
            { field: 'shortName', label: 'Partner ABBR' },
            { field: 'partnerNameEn', label: 'Partner Name' },
            { field: 'taxCode', label: 'Tax Code' },
        ];
        this.configPartner.selectedDisplayFields = ['partnerNameEn'];
    }

    getListCharges(mblId: string, partnerId: string, isHouseBillID: boolean, cdNoteCode: string) {
        this._documentationRepo.getChargesByPartner(mblId, partnerId, isHouseBillID, cdNoteCode)
            .pipe(
                catchError(this.catchError),
                map((data: any) => {
                    return data.map((item: any) => new ChargeCdNote(item));
                })
            ).subscribe(
                (dataCharges: any) => {
                    this.listChargePartner = dataCharges;
                    this.initGroup = dataCharges;
                    console.log(this.listChargePartner)
                    //Tính toán Amount Credit, Debit, Balance
                    this.calculatorAmount();
                },
            );
    }

    onSelectDataFormInfo(data: any) {
        this.selectedPartner = { field: "id", value: data.id };
        console.log(this.selectedPartner)
        if (this.partnerCurrent.value !== this.selectedPartner.value && this.listChargePartner.length > 0) {
            this.changePartnerPopup.show();
        } else {
            this.getListCharges(this.currentMBLId, this.selectedPartner.value, this.isHouseBillID, "");
            this.partnerCurrent = Object.assign({}, this.selectedPartner);
        }
    }

    onSubmitChangePartnerPopup() {
        this.keyword = '';
        this.isCheckAllCharge = false;
        //Gán this.selectedPartner cho this.partnerCurrent
        this.partnerCurrent = Object.assign({}, this.selectedPartner);
        //console.log(this.partnerCurrent)
        this.getListCharges(this.currentMBLId, this.selectedPartner.value, this.isHouseBillID, "");
        this.changePartnerPopup.hide();
    }

    onCancelChangePartnerPopup() {
        //Gán this.partnerCurrent cho this.selectedPartner
        this.selectedPartner = Object.assign({}, this.partnerCurrent);
        //console.log(this.selectedPartner)
    }

    onSubmitNotExistsChargePopup() {
        this.notExistsChargePopup.hide();
    }

    checkUncheckAllCharge() {
        for (const group of this.listChargePartner) {
            group.isSelected = this.isCheckAllCharge;
            for (const item of group.listCharges) {
                item.isSelected = this.isCheckAllCharge;
            }
        }
    }

    onChangeCheckBoxGrpCharge(charges: any) {
        this.isCheckAllCharge = this.listChargePartner.every((item: any) => item.isSelected);
        for (const charge of charges.listCharges) {
            charge.isSelected = this.isCheckAllCharge;
        }
    }

    onChangeCheckBoxItemCharge(chargeGroup: any) {
        chargeGroup.isSelected = chargeGroup.listCharges.every((item: any) => item.isSelected);
        this.isCheckAllCharge = this.listChargePartner.every((item: any) => item.isSelected);
    }

    removeCharge() {
        //console.log(this.listChargePartner.filter(group => group.isSelected));
        let chargesNotSelected = [];
        let grpNotSelectedChargeResult = [];

        if (this.listChargePartner.length > 0) {
            for (const charges of this.listChargePartner) {
                chargesNotSelected = charges.listCharges.filter(group => !group.isSelected);
                if (chargesNotSelected.length > 0) {
                    grpNotSelectedChargeResult.push({ id: charges.id, hwbno: charges.hwbno, listCharges: chargesNotSelected });
                }
            }
        }

        this.listChargePartner = grpNotSelectedChargeResult;
        //Tính toán Amount Credit, Debit, Balance
        this.calculatorAmount();
    }

    saveCDNote() {
        //console.log(this.selectedNoteType)
        console.log(this.listChargePartner)

        //Không được phép create khi chưa có charge
        if (this.listChargePartner.length == 0) {
            this.notExistsChargePopup.show();
        } else {
            this.CDNote.jobId = this.currentMBLId;//"03EA44D1-6DC1-4BD4-AFFD-8C5A5C192D22";
            this.CDNote.partnerId = this.selectedPartner.value;
            this.CDNote.type = this.selectedNoteType;
            this.CDNote.currencyId = "USD" // in the future , this id must be local currency of each country
            this.CDNote.transactionTypeEnum = TransactionTypeEnum.SeaFCLImport;
            var arrayCharges = [];
            for (const charges of this.listChargePartner) {
                for (const charge of charges.listCharges) {
                    arrayCharges.push(charge);
                }
            }
            this.CDNote.listShipmentSurcharge = arrayCharges;
            const _totalCredit = arrayCharges.filter(f => (f.type === 'BUY' || (f.type === 'OBH' && this.selectedPartner.value === f.payerId))).reduce((credit, charge) => credit + charge.total * charge.exchangeRate, 0);
            const _totalDebit = arrayCharges.filter(f => (f.type === 'SELL' || (f.type === 'OBH' && this.selectedPartner.value === f.paymentObjectId))).reduce((debit, charge) => debit + charge.total * charge.exchangeRate, 0);;
            console.log(_totalCredit);
            console.log(_totalDebit);
            const _balance = _totalDebit - _totalCredit;
            this.CDNote.total = _balance;
            console.log(_balance);
            if (_balance > 99999999999999 || _balance < -99999999999999) {
                this._toastService.error('Balance amount field exceeds numeric storage size');
            } else {
                console.log(this.CDNote);
                if (this.action == "create") {
                    this._documentationRepo.addCdNote(this.CDNote)
                        .pipe(catchError(this.catchError))
                        .subscribe(
                            (res: CommonInterface.IResult) => {
                                if (res.status) {
                                    this._toastService.success(res.message);
                                    this.onRequest.emit();
                                    this.closePopup();
                                } else {
                                    this._toastService.error(res.message);
                                }
                            }
                        );
                } else {
                    this._documentationRepo.updateCdNote(this.CDNote)
                        .pipe(catchError(this.catchError))
                        .subscribe(
                            (res: CommonInterface.IResult) => {
                                if (res.status) {
                                    this._toastService.success(res.message);
                                    this.onUpdate.emit();
                                    this.closePopup();
                                } else {
                                    this._toastService.error(res.message);
                                }
                            }
                        );
                }
            }
        }
    }

    calculatorAmount() {
        //List currency có trong listCharges
        const listCurrency = [];
        const listCharge = [];
        for (const charges of this.listChargePartner) {
            for (const currenct of charges.listCharges.map(m => m.currencyId)) {
                listCurrency.push(currenct)
            }
            for (const charge of charges.listCharges) {
                listCharge.push(charge);
            }
        }
        //List currency unique      
        const uniqueCurrency = [...new Set(listCurrency)] // Remove duplicate
        console.log(uniqueCurrency)
        this.totalCredit = '';
        this.totalDebit = '';
        this.balanceAmount = '';
        console.log(listCharge)
        for (const currency of uniqueCurrency) {
            const _credit = listCharge.filter(f => (f.type === 'BUY' || (f.type === 'OBH' && this.selectedPartner.value === f.payerId)) && f.currencyId === currency).reduce((credit, charge) => credit + charge.total, 0);
            const _debit = listCharge.filter(f => (f.type === 'SELL' || (f.type === 'OBH' && this.selectedPartner.value === f.paymentObjectId)) && f.currencyId === currency).reduce((debit, charge) => debit + charge.total, 0);
            const _balance = _debit - _credit;
            this.totalCredit += this.formatNumberCurrency(_credit) + ' ' + currency + ' | ';
            this.totalDebit += this.formatNumberCurrency(_debit) + ' ' + currency + ' | ';
            this.balanceAmount += (_balance > 0 ? this.formatNumberCurrency(_balance) : '(' + this.formatNumberCurrency(Math.abs(_balance)) + ')') + ' ' + currency + ' | ';
        }
        this.totalCredit += "]";
        this.totalDebit += "]";
        this.balanceAmount += "]";
        this.totalCredit = this.totalCredit.replace("| ]", "").replace("]", "");
        this.totalDebit = this.totalDebit.replace("| ]", "").replace("]", "");
        this.balanceAmount = this.balanceAmount.replace("| ]", "").replace("]", "");
    }

    formatNumberCurrency(input: number) {
        return input.toLocaleString(
            undefined, // leave undefined to use the browser's locale, or use a string like 'en-US' to override it.
            { minimumFractionDigits: 3 }
        );
    }

    openPopupAddCharge() {
        this.addRemainChargePopup.partner = this.selectedPartner.value;
        this.addRemainChargePopup.listChargePartner = this.listChargePartner;

        this._documentationRepo.getChargesByPartnerNotExitstCdNote(this.currentMBLId, this.selectedPartner.value, this.isHouseBillID, this.listChargePartner)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (dataCharges: any) => {
                    this.addRemainChargePopup.listChargePartnerAddMore = dataCharges;
                    this.addRemainChargePopup.show();
                },
            );
    }

    onChangeNoteType(noteType: any) {
        this.selectedNoteType = noteType.id;
    }

    sortCdNoteCharge(sort: string): void {
        this.listChargePartner = this._sortService.sort(this.listChargePartner, sort, this.order);
    }

    onAddMoreCharge(data: ChargeCdNote[]) {
        this.listChargePartner = [];
        this.listChargePartner = data;
        this.initGroup = data;
        //Tính toán giá trị amount, balance
        this.calculatorAmount();
    }

    //Charge keyword search
    onChangeKeyWord(keyword: string) {
        this.listChargePartner = this.initGroup;
        //TODO improve search.
        if (!!keyword) {
            keyword = keyword.toLowerCase();
            // Search group
            let dataGrp = this.listChargePartner.filter((item: any) => item.hwbno.toLowerCase().toString().search(keyword) !== -1)
            // Không tìm thấy group thì search tiếp list con của group
            if (dataGrp.length == 0) {
                let arrayCharge = [];
                for (const group of this.listChargePartner) {
                    const data = group.listCharges.filter((item: any) => item.chargeCode.toLowerCase().toString().search(keyword) !== -1 || item.nameEn.toLowerCase().toString().search(keyword) !== -1)
                    if (data.length > 0) {
                        arrayCharge.push({ id: group.id, hwbno: group.hwbno, listCharges: data });
                    }
                }
                dataGrp = arrayCharge;
            }
            return this.listChargePartner = dataGrp;
        } else {
            this.listChargePartner = this.initGroup;
        }
    }
}