import { PopupBase } from "src/app/popup.base";
import { Component, Input, ViewChild } from "@angular/core";
import { OpsTransaction } from "src/app/shared/models/document/OpsTransaction.model";
import { DocumentationRepo } from "src/app/shared/repositories";
import { catchError, tap } from "rxjs/operators";
import { ConfirmPopupComponent, InfoPopupComponent } from "src/app/shared/common/popup";
@Component({
    selector: 'cd-note-add-popup',
    templateUrl: './add-cd-note.popup.html'
})
export class CdNoteAddPopupComponent extends PopupBase {
    @ViewChild('changePartnerPopup', { static: false }) changePartnerPopup: ConfirmPopupComponent;
    @ViewChild('notExistsChargePopup', { static: false }) notExistsChargePopup: InfoPopupComponent;
    @Input() currentJob: OpsTransaction = null;

    headers: CommonInterface.IHeaderTable[];

    noteTypes = [
        { text: 'DEBIT', id: 'DEBIT' },
        { text: 'CREDIT', id: 'CREDIT' },
        { text: 'INVOICE', id: 'INVOICE' }
    ];

    currentHblID: any = '8F74BE7E-87E9-4D58-9FAF-422EBF24FE18';
    selectedNoteType: any = null;

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
    ) {
        super();
        //this._progressRef = this._progressService.ref();
        this.selectedNoteType = this.noteTypes[0].id;// "DEBIT";
    }

    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'jobNo', sortable: true },
            { title: 'Code', field: 'jobNo', sortable: true },
            { title: 'Charge Name', field: 'mawb', sortable: true },
            { title: 'Quantity', field: 'eta', sortable: true },
            { title: 'Unit', field: 'supplierName', sortable: true },
            { title: 'Unit Price', field: 'agentName', sortable: true },
            { title: 'Currency', field: 'polName', sortable: true },
            { title: 'VAT', field: 'podName', sortable: true },
            { title: "Credit Value", field: 'sumCont', sortable: true },
            { title: "Debit Value", field: 'sumPackage', sortable: true },
            { title: 'Note', field: 'grossWeight', sortable: true }
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

        // await this._documentationRepo.getChargesByPartner('7186BF67-CE8F-406D-9634-D47E803B6248', '1980912739')
        //     .pipe(
        //         catchError(this.catchError),
        //     ).subscribe(
        //         (dataCharges: any) => {
        //             //console.log('list charges')
        //             console.log(dataCharges);
        //             for (const item of dataCharges) {
        //                 this.listChargePartner.push(item);
        //             }
        //             //Tính toán Amount Credit, Debit, Balance
        //             this.calculatorAmount();
        //             console.log(this.listChargePartner)
        //         },
        //     );
    }

    onSelectDataFormInfo(data: any) {
        //console.log('seleted partner ' + data.partnerNameEn);
        this.selectedPartner = { field: "id", value: data.id };

        if (this.partnerCurrent.value !== this.selectedPartner.value) {
            this.changePartnerPopup.show();
        }
    }

    onSubmitChangePartnerPopup() {
        //console.log('confirm change partner')        
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

}