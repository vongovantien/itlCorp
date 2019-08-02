import { Component, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { takeUntil, catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { GlobalState } from 'src/app/global-state';
import { SystemRepo, AccoutingRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';
import { StatementOfAccountAddChargeComponent } from '../components/poup/add-charge/add-charge.popup';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-statement-of-account-new',
    templateUrl: './add-new-soa.component.html',
    styleUrls: ['./add-new-soa.component.scss'],
})
export class StatementOfAccountAddnewComponent extends AppList {

    @ViewChild(StatementOfAccountAddChargeComponent, { static: false }) addChargePopup: StatementOfAccountAddChargeComponent;

    charges: any[] = [];
    headers: CommonInterface.IHeaderTable[];

    totalShipment: number = 0;
    totalCharge: number = 0;

    isCollapsed: boolean = true;

    isCheckAllCharge: boolean = false;

    dataCharge: any = null;
    dataSearch: any = {};

    constructor(
        private _sysRepo: SystemRepo,
        private _globalState: GlobalState,
        private _sortService: SortService,
        private _accountRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _router: Router
    ) {
        super();
        this.requestList = this.sortLocal;
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
    }

    addCharge() {
        this.addChargePopup.show();
    }

    getBasicData() {
        forkJoin([
            this._sysRepo.getListCurrency(1, 20),
            this._sysRepo.getListSystemUser()
        ])
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                ([dataCurrency, dataSystemUser]: any) => {
                    this._globalState.notifyDataChanged('currency', dataCurrency);
                    this._globalState.notifyDataChanged('system-user', dataSystemUser);
                },
                (errors: any) => {
                    this.handleError(errors);
                },
                // complete
                () => {
                }
            );
    }

    sortLocal(sortField?: string, order?: boolean) {
        this.charges = this._sortService.sort(this.charges, sortField, order);
    }

    onChangeCheckBoxCharge($event: Event) {
        this.isCheckAllCharge = this.charges.every((item: any) => item.isSelected);
    }

    checkUncheckAllCharge() {
        for (const charge of this.charges) {
            charge.isSelected = this.isCheckAllCharge;
        }
    }

    onCreateSOA() {
        const chargeChecked = this.charges.filter((charge: any) => charge.isSelected);
        if (!chargeChecked.length) {
            this._toastService.warning(`SOA Don't have any charges in this period, Please check it again! `, '', { positionClass: 'toast-bottom-right' });
            return;
        } else {
            const body = {
                surchargeIds: chargeChecked.map((item: any) => item.id),
                soaformDate: this.dataSearch.fromDate,
                soatoDate: this.dataSearch.toDate,
                currency: this.dataSearch.currency,
                note: this.dataSearch.note,
                dateType: this.dataSearch.dateType,
                serviceTypeId: this.dataSearch.serviceTypeId,
                customer: this.dataSearch.customerID,
                type: this.dataSearch.type,
                obh: this.dataSearch.isOBH,
                creatorShipment: this.dataSearch.strCreators
            };

            this._accountRepo.createSOA(body)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.status) {
                            this._toastService.success(res.message, '', { positionClass: 'toast-bottom-right' });
                            // this.onSearchCharge(this.dataSearch); // ? Charge search again to remove charge was created by current SOA
                            // this.isCheckAllCharge = false; // ? reset checkbox all

                            //  * go to detail page
                            this._router.navigate(['home/accounting/statement-of-account/detail'], { queryParams: { no: res.data.soano, currency: 'VND' } });

                        } else {
                            this._toastService.error(res, '', { positionClass: 'toast-bottom-right' });
                        }
                    },
                    (errors: any) => {
                        this.handleError(errors);
                    },
                    () => {

                    }
                );
        }

    }

    onSearchCharge(dataSearch: any) {
        this.isLoading = true;
        this.dataSearch = dataSearch;
        this._accountRepo.getListChargeShipment(dataSearch)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (res: any) => {
                    this.dataCharge = res;
                    this.charges = res.chargeShipments || [];
                    this.totalCharge = res.totalCharge;
                    this.totalShipment = res.totalShipment;
                },
                (errors: any) => {
                    this.handleError(errors);
                },
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

}
