import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { SortService, DataService } from '@services';
import { ConfirmPopupComponent } from '@common';
import { delayTime } from '@decorators';
import { ChargeOfAccountingManagementModel } from '@models';

import { AppList } from 'src/app/app.list';
import { IAccountingManagementState, getAccountingManagementPartnerChargeState } from '../../store';

import { switchMap, takeUntil } from 'rxjs/operators';
import { formatDate } from '@angular/common';
import { AccountingRepo } from '@repositories';
import { of } from 'rxjs';
import cloneDeep from 'lodash/cloneDeep';

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
                { title: 'Invoice No', field: 'invoiceNo', sortable: true, width: 100 },
                { title: 'Serie', field: 'serie', sortable: true, width: 100 },
                { title: 'Invoice Date', field: 'invoiceDate', sortable: true, width: 150 },
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

    headers: CommonInterface.IHeaderTable[] = [
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
        { title: 'Amount(VND)', field: 'amountVnd', sortable: true, width: 150 },
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
        private _accountingRepo: AccountingRepo
    ) {
        super();
        this.requestSort = this.sortCharges;
    }

    ngOnInit(): void {

        this._store.select(getAccountingManagementPartnerChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (charges: ChargeOfAccountingManagementModel[]) => {
                    console.log(charges);
                    if (!this.detectDuplicateCharge([...this.charges, ...charges])) {
                        this.charges = [...this.charges, ...cloneDeep(charges)]; // * CloneDeep to avoid shadow copy after call fn => refreshListCharge()

                        this.updateTotalAmount();

                        if (this._type !== 'invoice') {
                            this.charges.forEach(c => {
                                if (!!c.invoiceDate) {
                                    c.invoiceDate = formatDate(new Date(c.invoiceDate), 'dd/MM/yyyy', 'en');
                                }
                            });
                        }

                    } else {
                        this._toastService.warning("Charge has existed in list");
                        return;
                    }
                }
            );

        this._dataService.currentMessage
            .pipe(
                takeUntil(this.ngUnsubscribe),
                switchMap((res: { [key: string]: any }) => {
                    console.log(res);
                    if (res.generalExchangeRate) {
                        if (!!this.charges.length) {
                            this.charges.forEach(c => {
                                if (c.currency !== 'VND') { // ! CURENCY LOCAL
                                    c.exchangeRate = res.generalExchangeRate; // * for Display
                                    c.finalExchangeRate = res.generalExchangeRate; // * for Calculating
                                }
                            });
                            return this._accountingRepo.calculateListChargeAccountingMngt(this.charges);
                        }
                        return of(false);
                    }
                    return of(false);
                })
            ).subscribe(
                (data: IChargeAccountingMngtTotal) => {
                    if (!!data) {
                        this.charges = [...data.charges];
                        this.totalAmountVnd = data.totalAmountVnd;
                        this.totalAmountVat = data.totalAmountVat;
                        this._toastService.success("Exchange Rate synced successfully");

                    } else {
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

    @delayTime(500)
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

    refreshListCharge() {
        this._store.select(getAccountingManagementPartnerChargeState)
            .pipe(takeUntil(this.ngUnsubscribe),
                switchMap(
                    (listChargeInStore: ChargeOfAccountingManagementModel[]) => {
                        console.log("list charge in store: ", listChargeInStore);
                        if (!!listChargeInStore.length) {
                            return this._accountingRepo.calculateListChargeAccountingMngt(listChargeInStore);
                        }
                        return of(false);
                    }
                )
            ).subscribe(
                (data: IChargeAccountingMngtTotal | any) => {
                    if (!!data) {
                        this.charges = cloneDeep([...(data.charges)]);
                        console.log("list charge after calculate: ", this.charges);

                        this.totalAmountVnd = data.totalAmountVnd;
                        this.totalAmountVat = data.totalAmountVat;
                    }
                }
            );
    }

    onBlurAnyCharge(e: any) {
        this.updateForChargerByFieldName(e.target.name, e.target.value);
    }

    updateForChargerByFieldName(field: string, value: string) {
        this.charges.forEach(ele => {
            if (!!ele[field]) {
                // continue
            } else {
                ele[field] = value;
            }
        });
    }

}

interface ITotalAmountVatVnd {
    totalAmountVnd: number;
    totalAmountVat: number;
}

interface IChargeAccountingMngtTotal extends ITotalAmountVatVnd {
    charges: ChargeOfAccountingManagementModel[];
    totalAmount: number;
}

