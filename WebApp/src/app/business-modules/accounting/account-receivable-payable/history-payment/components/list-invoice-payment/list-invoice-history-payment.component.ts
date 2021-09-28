import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import { Store } from '@ngrx/store';
import { getMenuUserSpecialPermissionState, IAppState } from '@store';
import { SystemConstants } from 'src/constants/system.const';
import { catchError, finalize, concatMap, withLatestFrom, takeUntil, map } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { ConfirmPopupComponent, InfoPopupComponent } from '@common';

import { PaymentModel, AccountingPaymentModel } from '@models';
import { RoutingConstants } from '@constants';
import { NgxSpinnerService } from 'ngx-spinner';
import { ARHistoryPaymentUpdateExtendDayPopupComponent } from '../popup/update-extend-day/update-extend-day.popup';
import { getDataSearchHistoryPaymentState, getHistoryPaymentListState, getHistoryPaymentPagingState, getHistoryPaymentLoadingListState } from '../../store/reducers';
import { LoadListHistoryPayment } from '../../store/actions';



@Component({
    selector: 'list-invoice-history-payment',
    templateUrl: './list-invoice-history-payment.component.html',
})
export class ARHistoryPaymentListInvoiceComponent extends AppList implements OnInit {

    @ViewChild(ARHistoryPaymentUpdateExtendDayPopupComponent) updateExtendDayPopup: ARHistoryPaymentUpdateExtendDayPopupComponent;
    @ViewChild(ConfirmPopupComponent) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) infoNotAllowDelete: InfoPopupComponent;
    @ViewChild('confirmInvoicePaymentPopup') confirmInvoicePaymentPopup: ConfirmPopupComponent;
    @Output() onUpdateExtendDateOfInvoice: EventEmitter<any> = new EventEmitter<any>();


    refPayments: AccountingPaymentModel[] = [];
    payments: PaymentModel[] = [];
    paymentHeaders: CommonInterface.IHeaderTable[];

    selectedPayment: PaymentModel;
    confirmMessage: string = '';
    refId: string;
    action: string;
    isClickSubMenu: boolean = false;
    constructor(
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _store: Store<IAppState>,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _exportRepo: ExportRepo,
        private _progressService: NgProgress,
        private _spinner: NgxSpinnerService) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortAccPayment;
        this.requestList = this.getPagingData;

    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Reference No', field: 'refNo', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Invoice No', field: 'invoiceNoReal', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Amount VND', field: 'totalAmountVnd', sortable: true },
            { title: 'Amount USD', field: 'totalAmountUsd', sortable: true },
            { title: 'Paid Amount VND', field: 'paidAmountVnd', sortable: true },
            { title: 'Paid Amount USD', field: 'paidAmountUsd', sortable: true },
            { title: 'Unpaid Amount VND', field: 'unpaidAmountVnd', sortable: true },
            { title: 'Unpaid Amount USD', field: 'unpaidAmountUsd', sortable: true },
            { title: 'Payment Status', field: 'status', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Overdue Days', field: 'overdueDays', sortable: true },
            { title: 'Extends date', field: 'extendDays', sortable: true },
            { title: 'Invoice Date', field: 'issuedDate', sortable: true },
            { title: 'Serie No', field: 'serie', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Paid Amount', field: 'paidAmount', sortable: true },
            { title: 'Unpaid Amount', field: 'unpaidAmount', sortable: true },
        ];
        this.paymentHeaders = [
            { title: 'No', field: '', sortable: true },
            { title: 'Receipt No', field: 'receiptNo', sortable: true },
            { title: 'Paid Amount', field: 'paymentAmount', sortable: true },
            { title: 'Balance', field: 'balance', sortable: true },
            { title: 'Paid Date', field: 'paidDate', sortable: true },
            { title: 'Payment Method', field: 'paymentMethod', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];

        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this._store.select(getDataSearchHistoryPaymentState)
            .pipe(
                withLatestFrom(this._store.select(getHistoryPaymentPagingState)),
                takeUntil(this.ngUnsubscribe),
                map(([dataSearch, pagingData]) => ({ page: pagingData.page, pageSize: pagingData.pageSize, dataSearch: dataSearch }))
            )
            .subscribe(
                (data) => {
                    this.dataSearch = data.dataSearch;
                    this.page = data.page;
                    this.pageSize = data.pageSize;
                }
            );

        this.isLoading = this._store.select(getHistoryPaymentLoadingListState);
    }

    ngAfterViewInit() {
    }

    getPagingData() {
        this._store.dispatch(LoadListHistoryPayment({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
        this._store.select(getHistoryPaymentListState)
            .pipe(
                catchError(this.catchError),
                map((data: any) => {
                    return {
                        data: !!data.data ? data.data.map((item: any) => new AccountingPaymentModel(item)) : [],
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.refPayments = res.data || [];
                    this.totalItems = res.totalItems;
                },
            );
    }

    import() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/history-payment/import`]);
    }

    exportExcel() {
        this._exportRepo.exportAcountingPaymentShipment(this.dataSearch)
            .subscribe(
                (res: Blob) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'invoice-payment.xlsx');
                }
            );
    }

    exportStatementReceivableCustomer() {
        if (!this.refPayments.length) {
            this._toastService.warning('No Data To View, Please Re-Apply Filter');
            return;
        } else {
            this._exportRepo.exportStatementReceivableCustomer(this.dataSearch)
                .subscribe(
                    (res: Blob) => {
                        this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'Statement of Receivable Customer - eFMS.xlsx');
                    }
                );
        }
    }

    exportAdvanceReceipt() {
        if (!this.dataSearch || !this.dataSearch.partnerId) {
            this._toastService.warning('No Data To View, Please Select Partner');
            return;
        }
        const body = {
            customerId: this.dataSearch.partnerId,
            status: "Done",
        };
        this._exportRepo.exportAdvanceReceipt(body)
            .subscribe(
                (res: ArrayBuffer) => {
                    if (!res) {
                        this._toastService.warning('No Data To View');
                        return;
                    }
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'eFms Receipt_Advance.xlsx');
                }
            );
    }

    getPayments(refNo: string, type: string) {
        this._accountingRepo.getPaymentByrefNo(refNo, type)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: []) => {
                    this.payments = res || [];
                },
            );
    }

    sortAccPayment(sortField: string, order: boolean) {
        this.refPayments = this._sortService.sort(this.refPayments, sortField, order);
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

    showExtendDateModel(refNo: string) {
        this._accountingRepo.getInvoiceExtendedDate(refNo)
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
                        this.getPagingData();
                        // this.getPayments(this.selectedPayment.refId, this.selectedPayment.receiptId);
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    confirmSync(refId: string, refNo: string, action: string) {
        this.refId = refId;
        this.action = action;
        this._accountingRepo.getPaymentByrefNo(refId, refNo)
            .pipe(
                catchError(this.catchError)
            ).subscribe(
                (res: []) => {
                    if (res.length > 0) {
                        // this._toastService.success("Tính năng đang phát triển");
                        this.confirmMessage = `Are you sure you want to sync data to accountant system?`;
                        this.confirmInvoicePaymentPopup.show();
                    } else {
                        this._toastService.error("Not found payment to sync");
                    }
                },
            );
    }

    onConfirmInvoicePayment() {
        this.confirmInvoicePaymentPopup.hide();
        const invoicePaymentIds: AccountingInterface.IRequestGuid[] = [];
        const invoicePaymentId: AccountingInterface.IRequestGuid = {
            Id: this.refId,
            action: this.action
        };
        invoicePaymentIds.push(invoicePaymentId);
        this._spinner.show();
        this._accountingRepo.getListInvoicePaymentToSync(invoicePaymentIds)
            .pipe(
                finalize(() => this._spinner.hide()),
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (((res as CommonInterface.IResult).status)) {
                        this._toastService.success("Sync Data to Accountant System Successful");
                    } else {
                        this._toastService.error("Sync Data Fail");
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
    }

    exportStatementReceivableAgency() {
        if (!this.refPayments.length) {
            this._toastService.warning('No Data To View, Please Re-Apply Filter');
            return;
        } else {
            this._exportRepo.exportStatementReceivableAgency(this.dataSearch)
                .subscribe(
                    (res: Blob) => {
                        this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'Statement of Receivable Agency - eFMS.xlsx');
                    }
                );
        }
    }
}
