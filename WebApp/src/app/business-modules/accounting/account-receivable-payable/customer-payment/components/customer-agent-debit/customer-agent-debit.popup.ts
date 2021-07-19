import { Component, Output, EventEmitter } from '@angular/core';
import { PopupBase } from '@app';
import { ReceiptInvoiceModel } from '@models';
import { AccountingRepo } from '@repositories';
import { catchError, takeUntil } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { IAppState } from '@store';
import { GetInvoiceListSuccess, ResetInvoiceList } from '../../store/actions';
import { ToastrService } from 'ngx-toastr';
import { ReceiptCreditListState, ReceiptDebitListState, ReceiptTypeState, ReceiptPartnerCurrentState } from '../../store/reducers';
import { SortService } from '@services';
import { AgencyReceiptModel } from 'src/app/shared/models/accouting/agency-receipt.model';

@Component({
    selector: 'customer-agent-debit-popup',
    templateUrl: 'customer-agent-debit.popup.html'
})

export class ARCustomerPaymentCustomerAgentDebitPopupComponent extends PopupBase {
    @Output() onAddToReceipt: EventEmitter<any> = new EventEmitter<any>();

    type: string = null;

    listDebit: ReceiptInvoiceModel[] = [];
    listCreditInvoice: ReceiptInvoiceModel[] = [];
    listDebitInvoice: ReceiptInvoiceModel[] = [];

    headersAgency: CommonInterface.IHeaderTable[] = [
        { title: 'Reference No', field: 'referenceNo', sortable: true },
        { title: 'VoucherId', field: 'voucherId', sortable: true },
        { title: 'Type', field: 'type', sortable: true },
        { title: 'Invoice No', field: 'invoiceNo', sortable: true },
        { title: 'JOB', field: 'jobNo', sortable: true },
        { title: 'HBL', field: 'hbl', sortable: true },
        { title: 'MBL', field: 'mbl', sortable: true },
        { title: 'PartnerId', field: 'taxCode', sortable: true },
        { title: 'Partner Name', field: 'partnerName', sortable: true },
        // { title: 'Amount', field: 'amount', sortable: true },
        // { title: 'Unpaid Amount', field: 'unpaid', sortable: true },
        { title: 'Unpaid VND', field: 'unpaidAmountVnd', sortable: true },
        { title: 'Unpaid USD', field: 'unpaidAmountUsd', sortable: true },
        { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
        { title: 'Payment Term', field: 'paymentTerm', sortable: true },
        { title: 'Due Date', field: 'dueDate', sortable: true },
        { title: 'Bu Handle', field: 'departmentName', sortable: true },
        { title: 'Office', field: 'officeName', sortable: true },
    ];

    agencyDebitModel: AgencyReceiptModel = new AgencyReceiptModel();
    checkAll = false;
    checkAllAgency = false;
    checkAllGroup = false;
    TYPELIST: string = 'LIST';
    partnerId: string;

    sumTotalObj = {
        totalDebitVnd: 0,
        totalDebitUsd: 0,
        totalDebitOBHVnd: 0,
        totalDebitOBHUsd: 0,
        totalCreditVnd: 0,
        totalCreditUsd: 0,
        totalBalanceVnd: 0,
        totalBalanceUsd: 0,
    }
    constructor(
        private _sortService: SortService,
        private _accountingRepo: AccountingRepo,
        private _store: Store<IAppState>,
        private _toastService: ToastrService,
    ) {
        super();
        this.requestSort = this.sortDebit;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Reference No', field: 'referenceNo', sortable: true },
            { title: 'VoucherId', field: 'voucherId', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Tax Code', field: 'taxCode', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            // { title: 'Amount', field: 'amount', sortable: true },
            // { title: 'Unpaid', field: 'unpaid', sortable: true },
            { title: 'Unpaid VND', field: 'unpaidAmountVnd', sortable: true },
            { title: 'Unpaid USD', field: 'unpaidAmountUsd', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Payment Term', field: 'paymentTerm', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Bu Handle', field: 'departmentName', sortable: true },
            { title: 'Office', field: 'officeName', sortable: true },
        ];

        this._store.select(ReceiptTypeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((type: string) => {
                const typeArr = (type || '').split(";").filter(x => Boolean(x)).map(x => x.trim());
                if (typeArr.length === 1 && (typeArr.includes("CUSTOMER") || typeArr.includes("AGENT"))) {
                    this.type = type;
                } else {
                    this.type = typeArr[0];
                }
            })

        this._store.select(ReceiptPartnerCurrentState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (id) => {
                    this.partnerId = id;
                }
            )
    }

    checkAllChange() {
        if (this.checkAll) {
            this.listDebit.forEach(x => {
                x.isSelected = true;
            });
        } else {
            this.listDebit.forEach(x => {
                x.isSelected = false;
            });
        }
    }

    onApply(body) {
        if (this.type.includes("CUSTOMER")) {
            this._accountingRepo.getDataIssueCustomerPayment(body)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: ReceiptInvoiceModel[]) => {
                        if (!!res) {
                            this.listDebit = res || [];
                            this.calculateSumDataObject(this.listDebit);
                            this.filterList();
                        }
                    },
                );
        } else {
            this._accountingRepo.getDataIssueAgencyPayment(body)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: AgencyReceiptModel) => {
                        if (!!res) {
                            this.agencyDebitModel = res;
                            this.agencyDebitModel.groupShipmentsAgency.forEach(element => {
                                element.isSelected = false;
                                element.invoices.forEach(x => {
                                    x.isSelected = false;
                                });
                            });
                            this.filterList();
                        }
                    },
                );
        }
    }

    filterListCredit() {
        this._store.select(ReceiptCreditListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((result: any) => {
                if (!!result) {
                    this.listCreditInvoice = result || [];
                    this.listDebit = this.listDebit.filter(s =>
                        result.every((t: { [x: string]: string; }) => {
                            return s["refNo"] !== t["refNo"]
                        }));
                    this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                        x.invoices.forEach(invoice => {
                            for (let i = 0; i < result.length; i++) {

                                if (result[i].isSelected === true && invoice.refNo === result[i].refNo && invoice.jobNo === result[i].jobNo && invoice.mbl === result[i].mbl && invoice.hbl === result[i].hbl) {
                                    invoice.isSelected = true;
                                }
                                if ((invoice.refNo === result[i].refNo && invoice.jobNo === result[i].jobNo && invoice.mbl === result[i].mbl && invoice.hbl === result[i].hbl) || invoice.refNo === result[i].refNo) {
                                    const index = x.invoices.indexOf(invoice);
                                    x.invoices.splice(index, 1);
                                }
                                if (invoice.isSelected === true && this.checkAllAgency) {
                                    const index = x.invoices.indexOf(invoice);
                                    x.invoices.splice(index, 1);
                                }
                            }
                        });
                    });
                    this.agencyDebitModel.groupShipmentsAgency = this.agencyDebitModel.groupShipmentsAgency.filter(x => x.invoices.length > 0);
                }
            });
    }

    filterListDebit() {
        this._store.select(ReceiptDebitListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((result: any) => {
                if (!!result) {
                    this.listDebitInvoice = result || [];
                    this.listDebit = this.listDebit.filter(s =>
                        result.every((t: { [x: string]: string; }) => {
                            return s["refNo"] !== t["refNo"];
                        }));

                    this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                        x.invoices.forEach(invoice => {
                            result.forEach(t => {
                                if (t.isSelected === true && invoice.refNo === t.refNo && invoice.jobNo === t.jobNo && invoice.mbl === t.mbl && invoice.hbl === t.hbl) {
                                    invoice.isSelected = true;
                                }
                                if ((invoice.refNo === t.refNo && invoice.jobNo === t.jobNo && invoice.mbl === t.mbl && invoice.hbl === t.hbl)) {
                                    const index = x.invoices.indexOf(invoice);
                                    x.invoices.splice(index, 1);
                                }
                                if (invoice.isSelected === true && this.checkAllAgency) {
                                    const index = x.invoices.indexOf(invoice);
                                    x.invoices.splice(index, 1);
                                }
                            })
                        });
                    });

                    this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                        if (x.isSelected) {
                            x.invoices = [];
                        }
                    })

                    this.agencyDebitModel.groupShipmentsAgency = this.agencyDebitModel.groupShipmentsAgency.filter(x => x.invoices.length > 0);
                }
            })
    }

    filterList() {
        this.filterListCredit();
        this.filterListDebit();
    }

    getAgencyDebitGroup() {
        const arr = [];
        this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
            for (let i = 0; i < x.invoices.length; i++) {
                arr.push(x.invoices[i]);
            }
        });
        return arr;
    }

    addToReceipt() {
        // const partnerId = this.partnerId.value;
        let datatoReceiptGroup = [];
        let datatoReceiptList = [];
        let datatoReceipt = this.listDebit;
        if (this.TYPELIST === 'GROUP' && this.type === 'AGENT') {
            datatoReceiptGroup = this.getAgencyDebitGroup();
            datatoReceipt = datatoReceiptGroup;
        }
        if (this.TYPELIST === 'LIST' && this.type === 'AGENT') {
            datatoReceiptList = this.getAgencyDebitGroup();
            datatoReceipt = datatoReceiptList;
        }
        datatoReceipt = datatoReceipt.filter(x => x.isSelected === true);
        if (datatoReceipt.length === 0) {
            this._toastService.warning('No data to add receipt!');
            return;
        }

        if (((this.listDebitInvoice.length > 0 && this.listDebitInvoice[0].partnerId !== this.partnerId)
            || (this.listCreditInvoice.length > 0 && this.listCreditInvoice[0].partnerId !== this.partnerId)
        )) {
            this._store.dispatch(ResetInvoiceList());
        }

        datatoReceipt.forEach(element => {
            element.totalPaidVnd = element.paidAmountVnd;
            element.totalPaidUsd = element.paidAmountUsd;
        });

        this._store.dispatch(GetInvoiceListSuccess({ invoices: datatoReceipt }));
        this.onAddToReceipt.emit(this.partnerId);
        // this.hide();
    }

    switchToGroup() {
        if (this.TYPELIST === 'GROUP') {
            this.TYPELIST = 'LIST';
        } else {
            this.TYPELIST = 'GROUP';
        }
    }

    sortDebit(sort: string) {
        this.listDebit = this._sortService.sort(this.listDebit, sort, this.order);
    }


    reset() {
        this.listDebit = [];
        this.agencyDebitModel = new AgencyReceiptModel();
    }

    removeAllChecked(groupShipment: any) {
        this.checkAll = false;
        this.checkAllAgency = false;
        if (this.type === 'AGENT') {
            groupShipment.isSelected = false;
            if (this.TYPELIST === 'GROUP') {
                groupShipment.invoices.forEach(x => {
                    x.isSelected = false;
                })
            }
        }
    }

    checkAllChangeAgency() {
        if (this.checkAllAgency) {
            this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                x.isSelected = true;
                for (let i = 0; i < x.invoices.length; i++) {
                    x.invoices[i].isSelected = true;
                }

            })
        }
        else {
            this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                x.isSelected = false;
                for (let i = 0; i < x.invoices.length; i++) {
                    x.invoices[i].isSelected = false;
                }
            })
        }
    }

    checkAllChangeGroupAgency(groupShipment: any) {
        groupShipment.invoices.forEach(x => {
            if (groupShipment.isSelected) {
                x.isSelected = true;
            }
            else {
                x.isSelected = false;
            }
        })
    }

    calculateSumDataObject(model: ReceiptInvoiceModel[]) {
        if (!model.length) {
            return;
        }
        for (let index = 0; index < model.length; index++) {
            const element = model[index];
            switch (element.type) {
                case 'CREDIT':
                    this.sumTotalObj.totalCreditVnd += element.unpaidAmountVnd;
                    this.sumTotalObj.totalCreditUsd += element.unpaidAmountUsd;
                    break;
                case 'DEBIT':
                    this.sumTotalObj.totalDebitVnd += element.unpaidAmountVnd;
                    this.sumTotalObj.totalDebitUsd += element.unpaidAmountUsd;
                    break;
                case 'OBH':
                    this.sumTotalObj.totalDebitOBHVnd += element.unpaidAmountVnd;
                    this.sumTotalObj.totalDebitOBHUsd += element.unpaidAmountUsd;
                    break;
                default:
                    break;
            }
            this.sumTotalObj.totalBalanceVnd = (this.sumTotalObj.totalDebitOBHVnd + this.sumTotalObj.totalDebitVnd) - this.sumTotalObj.totalCreditVnd;
            this.sumTotalObj.totalBalanceUsd = (this.sumTotalObj.totalDebitOBHUsd + this.sumTotalObj.totalDebitUsd) - this.sumTotalObj.totalCreditUsd;
        }
    }
}


