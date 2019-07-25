import { Component, ViewChild } from '@angular/core';
import { forkJoin } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
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
    headers: any = null;

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
        private _toastService: ToastrService
    ) {
        super();
        this.requestList = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Code', field: 'code', sortable: true },
            { title: 'Charge Name', field: 'name', sortable: true },
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Custom No', field: 'custom', sortable: true },
            { title: 'Debit', field: 'debit', sortable: true },
            { title: 'Credit', field: 'credit', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice No', field: 'invoice', sortable: true },
            { title: 'Services Date', field: 'serviceDate', sortable: true },
            { title: 'Note', field: 'action', sortable: true },
        ];
        // this.getBasicData();
        this.getListCharge();
    }

    addCharge() {
        this.addChargePopup.show();
        this._globalState.notifyDataChanged('system-user', []);
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
                (err: any) => {
                },
                // complete
                () => {
                }
            );
    }

    getListCharge() {
        const results: any[] = [];
        this.charges = results;
    }

    sortLocal(sortField?: string, order?: boolean) {
        console.log(this.sort, this.order);
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
                note: this.dataSearch.note
            };
            console.log(body);

            this._accountRepo.createSOA(body)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.status) {
                            this._toastService.success(res.message, '', { positionClass: 'toast-bottom-right' });

                            // ? search charge
                            this.onSearchCharge(this.dataSearch);

                            // ? reset checkbox
                            this.isCheckAllCharge = false;

                        } else {
                            // TODO: handle error
                        }
                    },
                    (errs: any) => {

                    },
                    () => {

                    }
                );
        }

    }

    onSearchCharge(dataSearch: any) {
        this.dataSearch = dataSearch;
        this._accountRepo.getListChargeShipment(dataSearch)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataCharge = res;
                    this.charges = res.chargeShipments || [];
                    this.totalCharge = res.totalCharge;
                    this.totalShipment = res.totalShipment;
                    console.log(this.charges);
                },
                (errs: any) => {
                    console.log(errs);
                    // Todo: handle error
                },
                () => { }
            );
    }

}
