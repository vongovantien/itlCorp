import { Component, ViewChild } from '@angular/core';
import { StatementOfAccountAddChargeComponent } from '../components/poup/add-charge/add-charge.popup';
import { NgxSpinnerService } from 'ngx-spinner';
import { AccoutingRepo, SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { SOA } from 'src/app/shared/models';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router } from '@angular/router';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';
import { forkJoin } from 'rxjs';
import { formatDate } from '@angular/common';
import moment from 'moment';

@Component({
    selector: 'app-statement-of-account-edit',
    templateUrl: './edit-soa.component.html',
    styleUrls: ['./edit-soa.component.scss']
})
export class StatementOfAccountEditComponent extends AppList {
    @ViewChild(StatementOfAccountAddChargeComponent, { static: false }) addChargePopup: StatementOfAccountAddChargeComponent;

    currencyList: any[];
    selectedCurrency: any = null;

    selectedRange: any;

    soa: SOA = new SOA();
    soaNO: string = '';
    currencyLocal: string = '';

    headers: CommonInterface.IHeaderTable[] = [];
    isCheckAllCharge: boolean = true;
    constructor(
        private _spinner: NgxSpinnerService,
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _activedRoute: ActivatedRoute,
        private _sysRepo: SystemRepo,
        private _sortService: SortService,
        private _router: Router
    ) {
        super();
        this.requestList = this.sortChargeList;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'Debit', field: 'debit', sortable: true },
            { title: 'Credit', field: 'credit', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Services Date', field: 'serviceDate', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
        this._activedRoute.queryParams.subscribe((params: any) => {
            if (!!params.no && !!params.currency) {
                this.soaNO = params.no;
                this.currencyLocal = params.currency;
                this.getCurrency();
                this.getDetailSOA(this.soaNO, this.currencyLocal);
            }
        });
    }

    addCharge() {
        this.addChargePopup.show();
    }

    getDetailSOA(soaNO: string, currency: string) {
        this._spinner.show();
        forkJoin([
            this._accoutingRepo.getDetaiLSOA(soaNO, currency),
            this._sysRepo.getListCurrency()
        ]).pipe(
            catchError(this.catchError),
            finalize(() => { this._spinner.hide(); })
        ).subscribe(
            ([dataSoa, dataCurrency]: any[]) => {
                this.soa = new SOA(dataSoa);
                this.currencyList = dataCurrency;

                // * make all chargeshipment was selected
                for (const item of this.soa.chargeShipments) {
                    item.isSelected = this.isCheckAllCharge;
                }

                // * update currency
                this.selectedCurrency = !!this.soa.currency ? this.currencyList.filter((currencyItem: any) => currencyItem.id === this.soa.currency.trim())[0] : this.currencyList[0];

                // * update range Date
                this.selectedRange = { startDate: new Date(this.soa.soaformDate), endDate: new Date(this.soa.soatoDate) };
            },
            (errors: any) => {
                this.handleError(errors);
            },
            () => { }
        );
    }


    getCurrency() {
        this._sysRepo.getListCurrency()
            .pipe(catchError(this.catchError))
            .subscribe(
                (dataCurrency: any) => {
                    this.currencyList = dataCurrency;
                    console.log(this.currencyList);
                },
                (errors: any) => {
                    this.handleError(errors);
                },
                // complete
                () => { }
            );
    }

    handleError(errors: any) {
        let message: string = 'Has Error Please Check Again !';
        let title: string = '';
        if (errors instanceof HttpErrorResponse) {
            message = errors.message;
            title = errors.statusText;
        }
        this._toastService.error(message, title, { positionClass: 'toast-bottom-right' });
    }


    sortChargeList(sortField?: string, order?: boolean) {
        this.soa.chargeShipments = this._sortService.sort(this.soa.chargeShipments, sortField, order);
    }

    onChangeCheckBoxCharge($event: Event) {
        this.isCheckAllCharge = this.soa.chargeShipments.every((item: any) => item.isSelected);
    }

    checkUncheckAllCharge() {
        for (const charge of this.soa.chargeShipments) {
            charge.isSelected = this.isCheckAllCharge;
        }
    }

    removeAllCharge() {
        this.isCheckAllCharge = false;
        this.checkUncheckAllCharge();
    }

    back() {
        this._router.navigate(['home/accounting/statement-of-account']);
    }

    updateSOA() {
        const chargeChecked = this.soa.chargeShipments.filter((charge: any) => charge.isSelected);
        /*
        * endDate must greater or equal soaToDate
                    * and 
        * fromDate must lower or equal soaFromDate
        */
        if (!(moment(this.soa.soaformDate).isSameOrAfter(moment(this.selectedRange.startDate), 'day') && moment(this.selectedRange.endDate).isSameOrAfter(moment(this.soa.soatoDate), 'day'))) {
            this._toastService.warning(`Range date invalid `, '', { positionClass: 'toast-bottom-right' });
            return;
        }
        if (!chargeChecked.length) {
            this._toastService.warning(`SOA Don't have any charges in this period, Please check it again! `, '', { positionClass: 'toast-bottom-right' });
            return;
        } else {
            const body = {
                surchargeIds: chargeChecked.map((item: any) => item.id),
                id: this.soa.id,
                soano: this.soaNO,
                soaformDate: !!this.selectedRange.startDate ? formatDate(this.selectedRange.startDate, 'yyyy-MM-dd', 'en') : null,
                soatoDate: !!this.selectedRange.endDate ? formatDate(this.selectedRange.endDate, 'yyyy-MM-dd', 'en') : null,
                currency: this.selectedCurrency.id,
                status: this.soa.status,
                note: this.soa.note,
                userCreated: this.soa.userCreated,
                userModified: this.soa.userModified,
                datetimeCreated: this.soa.datetimeCreated,
                datetimeModified: this.soa.datetimeModified
            };
            console.log(body);

            this._accoutingRepo.updateSOA(body)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        console.log(res);
                        if (res.status) {
                            this._toastService.success(res.message, '', { positionClass: 'toast-bottom-right' });

                            // * get detail again
                            this.getDetailSOA(this.soaNO, this.currencyLocal);

                            // * init checkbox all
                            this.isCheckAllCharge = true;
                        }
                    },
                    (errors: any) => {
                        this.handleError(errors);
                    },
                    () => { }
                );
        }
    }
}
