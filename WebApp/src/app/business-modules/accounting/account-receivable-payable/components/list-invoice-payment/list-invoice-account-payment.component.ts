import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import { Store } from '@ngrx/store';
import { getMenuUserSpecialPermissionState, IAppState } from '@store';
import { SystemConstants } from 'src/constants/system.const';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { ConfirmPopupComponent, InfoPopupComponent } from '@common';

import { AccountPaymentUpdateExtendDayPopupComponent } from '../popup/update-extend-day/update-extend-day.popup';
import { PaymentModel, AccountingPaymentModel } from '@models';



@Component({
    selector: 'list-invoice-account-payment',
    templateUrl: './list-invoice-account-payment.component.html',
})
export class AccountPaymentListInvoicePaymentComponent extends AppList implements OnInit {
    @ViewChild(AccountPaymentUpdateExtendDayPopupComponent, { static: false }) updateExtendDayPopup: AccountPaymentUpdateExtendDayPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoNotAllowDelete: InfoPopupComponent;

    @Output() onUpdateExtendDateOfInvoice: EventEmitter<any> = new EventEmitter<any>();


    refPaymens: AccountingPaymentModel[] = [];
    payments: PaymentModel[] = [];
    paymentHeaders: CommonInterface.IHeaderTable[];

    selectedPayment: PaymentModel;

    constructor(
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _store: Store<IAppState>,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _exportRepo: ExportRepo,
        private _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortAccPayment;
        this.requestList = this.getPagingData;

    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Reference No', field: 'invoiceNoReal', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Invoice Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice Date', field: 'issuedDate', sortable: true },
            { title: 'Serie No', field: 'serie', sortable: true },
            { title: 'Paid Amount', field: 'paidAmount', sortable: true },
            { title: 'Unpaid Amount', field: 'unpaidAmount', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Overdue Days', field: 'overdueDays', sortable: true },
            { title: 'Payment Status', field: 'status', sortable: true },
            { title: 'Extends date', field: 'extendDays', sortable: true },
            { title: 'Notes', field: 'extendNote', sortable: true },
        ];
        this.paymentHeaders = [
            { title: 'Payment No', field: 'paymentNo', sortable: true },
            { title: 'Payment Amount', field: 'paymentAmount', sortable: true },
            { title: 'Balance', field: 'balance', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'Paid Date', field: 'paidDate', sortable: true },
            { title: 'Payment Type', field: 'paymentType', sortable: true },
            { title: 'Payment Method', field: 'paymentMethod', sortable: true },
            { title: 'Exchange Rate', field: 'exchangeRate', sortable: true },
            { title: 'Update Person', field: 'userModifiedName', sortable: true },
            { title: 'Update Date', field: 'datetimeModified', sortable: true }
        ];

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
    }

    ngAfterViewInit() {
    }

    getPagingData() {
        this._progressRef.start();
        this.isLoading = true;
        this._accountingRepo.paymentPaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.refPaymens = res.data || [];
                    this.totalItems = res.totalItems;
                },
            );
    }

    import() {
        this._router.navigate(["home/accounting/account-receivable-payable/payment-import"]);
    }

    exportExcel() {
        this._exportRepo.exportAcountingPaymentShipment(this.dataSearch)
            .subscribe(
                (res: Blob) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'invoice-payment.xlsx');
                }
            );
    }

    getPayments(refId: string) {
        this._accountingRepo.getPaymentByrefId(refId)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: []) => {
                    this.payments = res || [];
                },
            );
    }

    sortAccPayment(sortField: string, order: boolean) {
        this.refPaymens = this._sortService.sort(this.refPaymens, sortField, order);
    }

    sortPayment(sortField: string, order: boolean) {
        this.payments = this._sortService.sort(this.payments, sortField, order);
    }

    showConfirmDelete(item, index) {
        if (index !== this.payments.length - 1) {
            this.infoNotAllowDelete.show();
        } else {
            this.selectedPayment = item;
            this.confirmDeletePopup.show();
        }
    }

    showExtendDateModel(refId: string) {
        this._accountingRepo.getInvoiceExtendedDate(refId)
            .pipe(
                catchError(this.catchError)
            ).subscribe((res: any) => {

                this.updateExtendDayPopup.refId = res.refId;
                this.updateExtendDayPopup.numberDaysExtend.setValue(res.numberDaysExtend);
                this.updateExtendDayPopup.note.setValue(res.note);
                this.updateExtendDayPopup.paymentType = res.paymentType;
                this.updateExtendDayPopup.show();

            });
    }

    handleUpdateExtendDate($event: any) {
        console.log("result: ", $event);
        this._progressRef.start();
        const body: any = {
            refId: $event.refId,
            numberDaysExtend: $event.numberDaysExtend,
            note: $event.note,
            paymentType: $event.paymentType,
        };
        this._accountingRepo.updateExtendDate(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            ).subscribe((res: any) => {
                if (res.status) {
                    this._toastService.success(res.message);
                    this.onUpdateExtendDateOfInvoice.emit();
                } else {
                    this._toastService.error(res.message);
                }
            });
    }

    onDeletePayment() {
        this.isLoading = true;
        this._accountingRepo.deletePayment(this.selectedPayment.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.confirmDeletePopup.hide();
                }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.getPayments(this.selectedPayment.refNo);
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }
}
