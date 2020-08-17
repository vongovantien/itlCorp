import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { SortService, DataService } from '@services';
import { ConfirmPopupComponent } from '@common';
import { timeoutD } from '@decorators';
import { ChargeOfAccountingManagementModel } from '@models';

import { AppList } from 'src/app/app.list';
import { IAccountingManagementState, getAccountingManagementPartnerChargeState } from '../../store';

import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'list-charge-accounting-management',
    templateUrl: './list-charge-accounting-management.component.html',
})
export class AccountingManagementListChargeComponent extends AppList implements OnInit {
    @Input() set type(t: string) {
        this._type = t;
        if (this._type !== 'invoice') {
            this.headers = [
                { title: 'Code', field: 'chargeCode', sortable: true, },
                { title: 'Charge Name', field: 'chargeName', sortable: true, },
                { title: 'Job No', field: 'jobNo', sortable: true },
                { title: 'HBL', field: 'hbl', sortable: true },
                { title: 'Contra Account', field: 'contraAccount', sortable: true },
                { title: 'Org Amount', field: 'orgAmount', sortable: true },
                { title: 'VAT', field: 'vat', sortable: true },
                { title: 'Org VAT Amount', field: 'orgVatAmount', sortable: true },
                { title: 'VAT Account', field: 'vatAccount', sortable: true },
                { title: 'Currency', field: 'currency', sortable: true },
                { title: 'Exchange Rate', field: 'exchangeRate', sortable: true },
                { title: 'Amount(VND)', field: 'amountVnd', sortable: true },
                { title: 'VAT Amount(VND)', field: 'vatAmountVnd', sortable: true },
                { title: 'Invoice No', field: 'invoiceNo', sortable: true },
                { title: 'Serie', field: 'serie', sortable: true },
                { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
                { title: 'OBH Partner', field: 'obhPartner', sortable: true },
                { title: 'VAT Partner ID', field: 'vatPartnerCode', sortable: true },
                { title: 'VAT Partner', field: 'vatPartnerName', sortable: true },
                { title: 'Debit Note', field: 'cdNoteNo', sortable: true },
                { title: 'SOA No', field: 'soaNo', sortable: true },
                { title: 'Settlement No', field: 'settlementCode', sortable: true },
                { title: 'Qty', field: 'qty', sortable: true },
                { title: 'Unit', field: 'unitName', sortable: true },
                { title: 'Unit Price', field: 'unitPrice', sortable: true },
                { title: 'MBL', field: 'mbl', sortable: true },
            ];
        }
    }

    get type() {
        return this._type;
    }

    private _type: string = 'invoice';

    @ViewChild(ConfirmPopupComponent, { static: true }) confirmRemovePopup: ConfirmPopupComponent;

    charges: ChargeOfAccountingManagementModel[] = [];

    // * totalAmountVatVnd = totalAmountVnd + vatAmountVnd
    totalAmountVnd: number;
    totalAmountVat: number;

    contentConfirmRemoveCharge: string;

    headers = [
        { title: 'Code', field: 'chargeCode', sortable: true, },
        { title: 'Charge Name', field: 'chargeName', sortable: true, },
        { title: 'Job No', field: 'jobNo', sortable: true },
        { title: 'HBL', field: 'hbl', sortable: true },
        { title: 'Contra Account', field: 'contraAccount', sortable: true },
        { title: 'Org Amount', field: 'orgAmount', sortable: true },
        { title: 'VAT', field: 'vat', sortable: true },
        { title: 'Org VAT Amount', field: 'orgVatAmount', sortable: true },
        { title: 'VAT Account', field: 'vatAccount', sortable: true },
        { title: 'Currency', field: 'currency', sortable: true },
        { title: 'Exchange Rate', field: 'exchangeRate', sortable: true },
        { title: 'Amount(VND)', field: 'amountVnd', sortable: true },
        { title: 'VAT Amount(VND)', field: 'vatAmountVnd', sortable: true },
        { title: 'VAT Partner ID', field: 'vatPartnerCode', sortable: true },
        { title: 'VAT Partner', field: 'vatPartnerName', sortable: true },
        { title: 'Debit Note', field: 'cdNoteNo', sortable: true },
        { title: 'SOA No', field: 'soaNo', sortable: true },
        { title: 'Qty', field: 'qty', sortable: true },
        { title: 'Unit', field: 'unitName', sortable: true },
        { title: 'Unit Price', field: 'unitPrice', sortable: true },
        { title: 'MBL', field: 'mbl', sortable: true },
    ];

    isReadOnly: boolean = false;

    constructor(
        private _sortService: SortService,
        private _store: Store<IAccountingManagementState>,
        private _toastService: ToastrService,
        private _dataService: DataService,
    ) {
        super();
        this.requestSort = this.sortCharges;
    }

    ngOnInit(): void {
        this._store.select(getAccountingManagementPartnerChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (charges: ChargeOfAccountingManagementModel[]) => {
                    if (!this.detectDuplicateCharge([...this.charges, ...charges])) {
                        this.charges = [...this.charges, ...charges];

                        this.updateTotalAmount();
                    } else {
                        this._toastService.warning("Charge has existed in list");
                        return;
                    }
                }
            );

        this._dataService.currentMessage.pipe(
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            (res: { [key: string]: any }) => {
                if (res.generalExchangeRate) {
                    if (!!this.charges.length) {
                        this.charges.forEach(c => {
                            if (c.currency !== 'VND') { // ! CURENCY LOCAL
                                c.exchangeRate = res.generalExchangeRate; // * for Display
                                c.finalExchangeRate = res.generalExchangeRate; // * for Calculating
                            }
                        });
                        this._toastService.success("Exchange Rate synced successfully");
                    }
                }
            }
        );
    }

    sortCharges(sort: string) {
        this.charges = this._sortService.sort(this.charges, sort, this.order);
    }

    calculateTotalAmountVND(charges: ChargeOfAccountingManagementModel[]): ITotalAmountVatVnd {
        const total: ITotalAmountVatVnd = {
            totalAmountVnd: 0,
            totalAmountVat: 0
        };
        if (charges.length) {
            total.totalAmountVnd = charges.reduce((acc, curr) => (acc += curr.amountVnd), 0);
            total.totalAmountVat = charges.reduce((acc, curr) => (acc += curr.vatAmountVnd), 0);
        }

        return total;
    }

    updateTotalAmount() {
        const totalData: ITotalAmountVatVnd = this.calculateTotalAmountVND(this.charges);
        this.totalAmountVnd = totalData.totalAmountVnd;
        this.totalAmountVat = totalData.totalAmountVat;
    }

    detectDuplicateCharge(charges: ChargeOfAccountingManagementModel[]) {
        if (!charges.length) {
            return false;
        }
        return this.utility.checkDuplicateInObject('surchargeId', charges);
    }

    checkUncheckAllCharge() {
        for (const charge of this.charges) {
            charge.isSelected = this.isCheckAll;
        }
    }

    onChangeCheckBoxCharge(e: any, selectedCharge: ChargeOfAccountingManagementModel) {
        this.isCheckAll = this.charges.every((item: ChargeOfAccountingManagementModel) => item.isSelected);
        if (this.type === 'invoice') {
            if (e.target.checked) {
                if (!!selectedCharge.soaNo) {
                    this.updateChargeOfAcc('soaNo', selectedCharge.soaNo);
                } else {
                    if (!!selectedCharge.cdNoteNo) {
                        this.updateChargeOfAcc('cdNoteNo', selectedCharge.cdNoteNo);
                    }
                }
            } else {
                if (!!selectedCharge.soaNo) {
                    this.updateChargeOfAcc('soaNo', selectedCharge.soaNo, false);
                } else {
                    if (!!selectedCharge.cdNoteNo) {
                        this.updateChargeOfAcc('cdNoteNo', selectedCharge.cdNoteNo, false);
                    }
                }
            }
        } else {
            if (e.target.checked) {
                if (!!selectedCharge.settlementCode) {
                    this.updateChargeOfAcc('settlementCode', selectedCharge.settlementCode);

                } else if (!!selectedCharge.soaNo) {
                    this.updateChargeOfAcc('soaNo', selectedCharge.soaNo);
                } else {
                    if (!!selectedCharge.cdNoteNo) {
                        this.updateChargeOfAcc('cdNoteNo', selectedCharge.cdNoteNo);
                    }
                }
            } else {
                if (!!selectedCharge.settlementCode) {
                    this.updateChargeOfAcc('settlementCode', selectedCharge.settlementCode, false);

                } else if (!!selectedCharge.soaNo) {
                    this.updateChargeOfAcc('soaNo', selectedCharge.soaNo, false);
                } else {
                    if (!!selectedCharge.cdNoteNo) {
                        this.updateChargeOfAcc('cdNoteNo', selectedCharge.cdNoteNo, false);
                    }
                }
            }
        }
    }

    updateChargeOfAcc(key: string, value: string, isCheck: boolean = true) {
        this.charges.forEach(c => {
            if (c[key] === value) {
                c.isSelected = isCheck;
            }
        });
    }

    @timeoutD(500)
    removeCharge() {
        const chargeToDelete: ChargeOfAccountingManagementModel[] = this.charges.filter(x => x.isSelected);
        if (!!chargeToDelete.length) {
            this.contentConfirmRemoveCharge = `Are you sure you want to remove ${this.getSoaNo(chargeToDelete)} ${this.getCdNote(chargeToDelete)} ?`;
            this.confirmRemovePopup.show();
        } else {
            return;
        }
    }

    onRemoveCharge() {
        this.confirmRemovePopup.hide();
        this.charges = this.charges.filter((item: ChargeOfAccountingManagementModel) => !item.isSelected);

        this.isCheckAll = false;

        // * calculate total.
        const totalData: ITotalAmountVatVnd = this.calculateTotalAmountVND(this.charges);
        this.totalAmountVnd = totalData.totalAmountVnd;
        this.totalAmountVat = totalData.totalAmountVat;
    }

    getSoaNo(charges: ChargeOfAccountingManagementModel[]): string {
        const soaNo: string[] = charges.filter(x => Boolean(x.soaNo)).map(i => i.soaNo);
        if (!soaNo.length) {
            return '';
        }
        return soaNo.join(",\n");
    }

    getCdNote(charges: ChargeOfAccountingManagementModel[]) {
        const cdNoteNo: string[] = charges.filter(x => Boolean(x.cdNoteNo)).map(i => i.cdNoteNo);
        if (!cdNoteNo.length) {
            return '';
        }
        return cdNoteNo.join(",\n");
    }
}

interface ITotalAmountVatVnd {
    totalAmountVnd: number;
    totalAmountVat: number;
}
