import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingPaymentModel } from 'src/app/shared/models/accouting/accounting-payment.model';
import { AccountingRepo } from '@repositories';
import { PaymentModel } from 'src/app/shared/models/accouting/payment.model';
import { SortService } from '@services';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserSpecialPermissionState } from '@store';

import { ToastrService } from 'ngx-toastr';
import { InfoPopupComponent, ConfirmPopupComponent } from '@common';

import { catchError, finalize } from 'rxjs/operators';

import { AccountReceivablePayableUpdateExtendDayPopupComponent } from '../popup/update-extend-day/update-extend-day.popup';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'list-obh-account-receivable-payable',
    templateUrl: './list-obh-account-receivable-payable.component.html',
})
export class AccountReceivablePayableListOBHPaymentComponent extends AppList implements OnInit {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoNotAllowDelete: InfoPopupComponent;
    @ViewChild(AccountReceivablePayableUpdateExtendDayPopupComponent, { static: false }) updateExtendDayPopup: AccountReceivablePayableUpdateExtendDayPopupComponent;

    @Output() onUpdateExtendDateOfOBH: EventEmitter<any> = new EventEmitter<any>();

    refPaymens: AccountingPaymentModel[] = [];
    payments: PaymentModel[] = [];
    paymentHeaders: CommonInterface.IHeaderTable[];
    selectedPayment: PaymentModel;

    constructor(private _router: Router,
        private _progressService: NgProgress,
        private _accountingRepo: AccountingRepo,
        private _store: Store<IAppState>,
        private _sortService: SortService,
        private _toastService: ToastrService) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.getPagingData;
        this.requestSort = this.sortRefPayment;
    }

    ngOnInit(): void {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);

        this.headers = [
            { title: 'Reference No', field: 'soaNo', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'OBH Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Issue Date', field: 'issuedDate', sortable: true },
            { title: 'Paid Amount', field: 'paidAmount', sortable: true },
            { title: 'Unpaid Amount', field: 'unpaidAmount', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Overdue Days', field: 'overdueDays', sortable: true },
            { title: 'Payment Status', field: 'status', sortable: true },
            { title: 'Extend days', field: 'extendDays', sortable: true },
            { title: 'Notes', field: 'extendNote', sortable: true },
        ];

        this.paymentHeaders = [
            { title: 'Payment No', field: 'paymentNo', sortable: true },
            { title: 'Payment Amount', field: 'paymentAmount', sortable: true },
            { title: 'Balance', field: 'balance', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'Paid Date', field: 'paidDate', sortable: true },
            { title: 'Payment Type', field: 'paymentType', sortable: true },
            { title: 'Update Person', field: 'userModifiedName', sortable: true },
            { title: 'Update Date', field: 'datetimeModified', sortable: true }
        ];
    }

    getPagingData() {
        this._progressRef.start();
        this._accountingRepo.paymentPaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.refPaymens = res.data || [];
                    this.totalItems = res.totalItems;
                },
            );
    }

    import() {
        this._router.navigate(["home/accounting/account-receivable-payable/import-obh"]);
    }

    getPayments(refId: string) {
        this._accountingRepo.getPaymentByrefId(refId)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: []) => {
                    this.payments = res;
                    console.log(this.payments);
                },
            );
    }

    sortPayment(sortField: string, order: boolean) {
        this.payments = this._sortService.sort(this.payments, sortField, order);
    }

    sortRefPayment(sortField: string, order: boolean) {
        this.refPaymens = this._sortService.sort(this.refPaymens, sortField, order);
    }

    showExtendDateModel(refId: string) {
        console.log(refId);
        this._accountingRepo.getOBHSOAExtendedDate(refId)
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

    handleUpdateExtendDate($event) {
        console.log($event);
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
                    this.onUpdateExtendDateOfOBH.emit();
                } else {
                    this._toastService.error(res.message);
                }
            });
    }

    showConfirmDelete(item, index) {
        if (index !== this.payments.length - 1) {
            this.infoNotAllowDelete.show();
        } else {
            this.selectedPayment = item;
            this.confirmDeletePopup.show();
        }
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
                        this.getPayments(this.selectedPayment.refId);
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }
}

