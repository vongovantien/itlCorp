import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { NgxSpinnerService } from 'ngx-spinner';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { SOA } from 'src/app/shared/models';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';

@Component({
    selector: 'app-statement-of-account-detail',
    templateUrl: './detail-soa.component.html',
    styleUrls: ['./detail-soa.component.scss']
})
export class StatementOfAccountDetailComponent extends AppList {

    soaNO: string = '';
    currencyLocal: string = 'VND';

    soa: SOA = new SOA();
    headers: CommonInterface.IHeaderTable[] = [];

    constructor(
        private _activedRoute: ActivatedRoute,
        private _accoutingRepo: AccoutingRepo,
        private _spinner: NgxSpinnerService,
        private _toastService: ToastrService,
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
            if (!!params.no && params.currency) {
                this.soaNO = params.no;
                this.currencyLocal = params.currency;
                this.getDetailSOA(this.soaNO, this.currencyLocal)
            }
        });
    }

    getDetailSOA(soaNO: string, currency: string) {
        this._spinner.show()
        this._accoutingRepo.getDetaiLSOA(soaNO, currency)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._spinner.hide(); })
            )
            .subscribe(
                (res: any) => {
                    this.soa = new SOA(res);
                    this.totalItems = this.soa.chargeShipments.length;
                },
                (errors: any) => {
                    this.handleError(errors);
                },
                () => { }
            );
    }

    sortChargeList(sortField?: string, order?: boolean) {
        this.soa.chargeShipments = this._sortService.sort(this.soa.chargeShipments, sortField, order);
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

    back() {
        this._router.navigate(['home/accounting/statement-of-account']);
    }

}
