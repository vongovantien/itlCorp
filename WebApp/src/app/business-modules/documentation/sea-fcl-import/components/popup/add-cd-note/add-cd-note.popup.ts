import { PopupBase } from "src/app/popup.base";
import { Component, Input, ViewChild, EventEmitter, Output } from "@angular/core";
import { OpsTransaction } from "src/app/shared/models/document/OpsTransaction.model";
import { DocumentationRepo } from "src/app/shared/repositories";
import { catchError, tap } from "rxjs/operators";
import { ConfirmPopupComponent, InfoPopupComponent } from "src/app/shared/common/popup";
import { CdNoteAddRemainingChargePopupComponent } from "../add-remaining-charge/add-remaining-charge.popup";
import { SortService } from "src/app/shared/services";
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
    @Input() currentJob: OpsTransaction = null;

    action: string = 'create';

    headers: CommonInterface.IHeaderTable[];

    noteTypes = [
        { text: 'DEBIT', id: 'DEBIT' },
        { text: 'CREDIT', id: 'CREDIT' },
        { text: 'INVOICE', id: 'INVOICE' }
    ];

    currentHblID: any = '8F74BE7E-87E9-4D58-9FAF-422EBF24FE18';
    selectedNoteType: string = '';

    listChargePartner: any[] = [];

    selectedPartner: any = {};
    partnerCurrent: any = {};

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
    ) {
        super();
        //this._progressRef = this._progressService.ref();
        this.selectedNoteType = "DEBIT";
        console.log(this.selectedNoteType);
        this.requestList = this.getListCharges;
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
        // if (this.currentJob != null) {
        //     this.currentHblID = this.currentJob.hblid;
        //     this.getListSubjectPartner();
        // }
        this.getListSubjectPartner();
    }

    closePopup() {
        this.hide();
        //this.resetForm();
        this.selectedNoteType = "DEBIT";
        this.selectedPartner = {};
        this.listChargePartner = [];
    }

    getListSubjectPartner() {
        this._documentationRepo.getPartners(this.currentHblID)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (dataPartner: any) => {
                    //console.log('list partner')
                    //console.log(dataPartner);
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

    getListCharges(partnerId: string) {
        console.log(partnerId);
        this._documentationRepo.getChargesByPartner(this.currentHblID, partnerId)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (dataCharges: any) => {
                    //console.log('list charges')
                    console.log(dataCharges);
                    this.listChargePartner = dataCharges;

                    //Tính toán Amount Credit, Debit, Balance
                    this.calculatorAmount();
                },
            );
    }

    onSelectDataFormInfo(data: any) {
        this.selectedPartner = { field: "id", value: data.id };
        console.log(this.selectedPartner)
        if (this.partnerCurrent.value !== this.selectedPartner.value) {
            this.changePartnerPopup.show();
        }
    }

    onSubmitChangePartnerPopup() {
        this.isCheckAllCharge = false;
        //Gán this.selectedPartner cho this.partnerCurrent
        this.partnerCurrent = Object.assign({}, this.selectedPartner);
        console.log(this.partnerCurrent)
        this.getListCharges(this.partnerCurrent.value);
        this.changePartnerPopup.hide();
    }

    onCancelChangePartnerPopup() {
        console.log('cancel')
        console.log(this.partnerCurrent)
        //Gán this.partnerCurrent cho this.selectedPartner
        this.selectedPartner = Object.assign({}, this.partnerCurrent);
        console.log(this.selectedPartner)
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
        console.log(this.listChargePartner.filter(group => group.isSelected));
        let chargesResult = [];
        let grpResult = [];

        if (this.listChargePartner.length > 0) {
            for (const charges of this.listChargePartner) {
                console.log(charges.listCharges.filter(group => group.isSelected))
                chargesResult = charges.listCharges.filter(group => !group.isSelected);
                if (chargesResult.length > 0) {
                    grpResult.push({ id: charges.id, hwbno: charges.hwbno, listCharges: chargesResult });
                }
            }
        }

        this.listChargePartner = grpResult;
        //Tính toán Amount Credit, Debit, Balance
        this.calculatorAmount();
    }

    createCDNote() {
        console.log(this.selectedNoteType)
        console.log(this.listChargePartner)
        if (this.listChargePartner.length == 0) {
            this.notExistsChargePopup.show();
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
            const _credit = listCharge.filter(f => (f.type === 'BUY' || (f.type === 'OBH' && this.partnerCurrent.value === f.payerId)) && f.currencyId === currency).reduce((debit, charge) => debit + charge.total, 0);
            const _debit = listCharge.filter(f => (f.type === 'SELL' || (f.type === 'OBH' && this.partnerCurrent.value === f.paymentObjectId)) && f.currencyId === currency).reduce((credit, charge) => credit + charge.total, 0);
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
        this.addRemainChargePopup.show();
    }

    onChangeNoteType(noteType: any) {
        this.selectedNoteType = noteType.id;
    }

    sortCdNoteCharge(sort: string): void {
        this.listChargePartner = this._sortService.sort(this.listChargePartner, sort, this.order);
    }
}