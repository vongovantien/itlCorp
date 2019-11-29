import { Component, OnInit, ViewChild } from '@angular/core';
import { Router, Params } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { AppList } from 'src/app/app.list';
import { getParamsRouterState } from 'src/app/store';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { CsTransactionDetail } from 'src/app/shared/models';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

import { catchError, finalize, takeUntil, take } from 'rxjs/operators';

import * as fromShareBussiness from './../../../../share-business/store';


@Component({
    selector: 'app-sea-fcl-export-hbl',
    templateUrl: './sea-fcl-export-hbl.component.html'
})

export class SeaFCLExportHBLComponent extends AppList implements OnInit {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteHBLPopup: ConfirmPopupComponent;
    @ViewChild('confirmDeleteJob', { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;

    jobId: string;
    headers: CommonInterface.IHeaderTable[];
    houseBills: any[] = [];

    selectedHbl: CsTransactionDetail;

    selectedTabSurcharge: string = 'BUY';

    constructor(
        private _router: Router,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this._store.select(getParamsRouterState)
            .pipe(takeUntil(this.ngUnsubscribe), take(1))
            .subscribe((param: Params) => {
                if (param.jobId) {
                    this.jobId = param.jobId;
                    this.getHouseBills(this.jobId);
                }
            });

        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true, width: 100 },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'SaleMan', field: 'saleManName', sortable: true },
            { title: 'Notify Party', field: 'notifyParty', sortable: true },
            { title: 'Destination', field: 'finalDestinationPlace', sortable: true },
            { title: 'Containers', field: 'containers', sortable: true },
            { title: 'Package', field: 'packages', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true }
        ];
    }

    getHouseBills(id: string) {
        this.isLoading = true;
        this._documentRepo.getListHouseBillOfJob({ jobId: this.jobId }).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
        ).subscribe(
            (res: any) => {
                this.houseBills = res;
                if (!!this.houseBills.length) {
                    this.selectHBL(this.houseBills[0]);
                }
            },
        );
    }

    showDeletePopup(hbl: CsTransactionDetail, event: Event) {
        event.preventDefault();
        event.stopImmediatePropagation();
        event.stopPropagation();

        this.confirmDeleteHBLPopup.show();
        this.selectedHbl = hbl;

    }

    onDeleteHbl() {
        this.confirmDeleteHBLPopup.hide();
        this.deleteHbl(this.selectedHbl.id);
    }

    deleteHbl(id: string) {
        this._progressRef.start();
        this._documentRepo.deleteHbl(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.getHouseBills(this.jobId);
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    deleteJob() {
        this.confirmDeleteJobPopup.show();
    }

    onDeleteJob() {
        this._progressRef.start();
        this._documentRepo.deleteMasterBill(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeleteJobPopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {

                        this._toastService.success(respone.message, 'Delete Success !');

                        this.gotoList();
                    }
                },
            );
    }

    gotoList() {
        this._router.navigate(["home/documentation/sea-fcl-export"]);
    }

    gotoCreate() {
        this._router.navigate([`/home/documentation/sea-fcl-export/${this.jobId}/hbl/new`]);
    }

    selectHBL(hbl: CsTransactionDetail) {
        this.selectedHbl = new CsTransactionDetail(hbl);

        // * Get container, Job detail, Surcharge with hbl id, JobId.
        this._store.dispatch(new fromShareBussiness.GetDetailHBLSuccessAction(hbl));
        this._store.dispatch(new fromShareBussiness.GetContainersHBLAction({ hblid: hbl.id }));
        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(hbl.jobId));
        this._store.dispatch(new fromShareBussiness.GetProfitHBLAction(this.selectedHbl.id));

        switch (this.selectedTabSurcharge) {
            case 'BUY':
                this._store.dispatch(new fromShareBussiness.GetBuyingSurchargeAction({ type: 'BUY', hblId: this.selectedHbl.id }));
                break;
            case 'SELL':
                this._store.dispatch(new fromShareBussiness.GetSellingSurchargeAction({ type: 'SELL', hblId: this.selectedHbl.id }));
                break;
            case 'OBH':
                this._store.dispatch(new fromShareBussiness.GetOBHSurchargeAction({ type: 'OBH', hblId: this.selectedHbl.id }));
                break;
            default:
                break;
        }
    }

    onSelectTabSurcharge(tabName: string) {
        this.selectedTabSurcharge = tabName;

        if (!!this.selectedHbl) {
            switch (this.selectedTabSurcharge) {
                case 'BUY':
                    this._store.dispatch(new fromShareBussiness.GetBuyingSurchargeAction({ type: 'BUY', hblId: this.selectedHbl.id }));
                    break;
                case 'SELL':
                    this._store.dispatch(new fromShareBussiness.GetSellingSurchargeAction({ type: 'SELL', hblId: this.selectedHbl.id }));
                    break;
                case 'OBH':
                    this._store.dispatch(new fromShareBussiness.GetOBHSurchargeAction({ type: 'OBH', hblId: this.selectedHbl.id }));
                    break;
                default:
                    break;
            }
        }
    }


    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: { tab: 'SHIPMENT' } });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            case 'assignment':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
        }
    }
}
