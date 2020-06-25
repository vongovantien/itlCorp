import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { AccountingRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { SortService } from '@services';
import { PaymentModel } from 'src/app/shared/models/accouting/payment.model';
import { AccountingPaymentModel } from 'src/app/shared/models/accouting/accounting-payment.model';
import { AccountReceivablePayableUpdateExtendDayPopupComponent } from '../popup/update-extend-day/update-extend-day.popup';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';



@Component({
    selector: 'list-invoice-account-receivable-payable',
    templateUrl: './list-invoice-account-receivable-payable.component.html',
})
export class AccountReceivablePayableListInvoicePaymentComponent extends AppList implements OnInit {
    @ViewChild(AccountReceivablePayableUpdateExtendDayPopupComponent, { static: false }) updateExtendDayPopup: AccountReceivablePayableUpdateExtendDayPopupComponent;
    @Output() onUpdateExtendDateOfInvoice: EventEmitter<any> = new EventEmitter<any>();

    refPaymens: AccountingPaymentModel[] = [];
    payments: PaymentModel[] = [];
    paymentHeaders: CommonInterface.IHeaderTable[];


    constructor(
        private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _router: Router,
        private _accountingRepo: AccountingRepo,
        private _sortService: SortService) {
        super();
        // this.requestList = this.requestSearchShipment;
        this.requestSort = this.sortAccPayment;
        this._progressRef = this._progressService.ref();
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
            { title: 'Update Person', field: 'userModifiedName', sortable: true },
            { title: 'Update Date', field: 'datetimeModified', sortable: true }
        ];
    }

    ngAfterViewInit() {
        //this.updateExtendDayPopup.show();

    }
    import() {
        this._router.navigate(["home/accounting/account-receivable-payable/payment-import"], { queryParams: { type: 'Invoice' } });
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
    sortAccPayment(sortField: string, order: boolean) {
        this.refPaymens = this._sortService.sort(this.refPaymens, sortField, order);
    }
    sortPayment(sortField: string, order: boolean) {
        this.payments = this._sortService.sort(this.payments, sortField, order);
    }
    showConfirmDelete(item) { }

    showExtendDateModel(refId: string) {
        //console.log(refId);
        this._accountingRepo.getInvoiceExtendedDate(refId)
            .pipe(
                catchError(this.catchError)
            ).subscribe((res: any) => {

                this.updateExtendDayPopup.refId = res.refId;
                this.updateExtendDayPopup.numberDaysExtend.setValue(res.numberDaysExtend);
                this.updateExtendDayPopup.note.setValue(res.note);
                this.updateExtendDayPopup.paymentType = res.paymentType;
                this.updateExtendDayPopup.show();

            })

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
}
