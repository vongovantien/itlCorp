import { Component, ViewChild } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { AppList } from 'src/app/app.list';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

import { Container } from 'src/app/shared/models/document/container.model';
import { CsShipmentSurcharge } from 'src/app/shared/models';

import * as fromStore from './../../store';
import * as fromShareBussiness from './../../../../share-business/store';

import { catchError, finalize, takeUntil, switchMap, tap } from 'rxjs/operators';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-sea-fcl-import-hbl',
    templateUrl: './sea-fcl-import-hbl.component.html',
})
export class SeaFCLImportHBLComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    jobId: string = '';
    headers: CommonInterface.IHeaderTable[];
    houseBill: CsTransactionDetail[] = [];
    goodSummary: any = {};


    containers: Container[] = new Array<Container>();
    selectedShipment: any; // TODO model.
    selectedHbl: CsTransactionDetail;

    charges: CsShipmentSurcharge[] = new Array<CsShipmentSurcharge>();

    selectedTabSurcharge: string = 'BUY';

    constructor(
        private _router: Router,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _activedRoute: ActivatedRoute,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
        private _store: Store<fromStore.ISeaFCLImportState>,
    ) {
        super();
        this.requestSort = this.sortLocal;
        this._progressRef = this._progressService.ref();

    }

    ngOnInit(): void {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.id) {
                this.jobId = param.id;
                this.getHourseBill(this.jobId);

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

        this._store.select(fromStore.getContainerSaveState)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                tap(
                    (containers: Container[]) => {
                        this.containers = (containers || []).map(contaienr => new Container(contaienr));
                    }
                ),
                switchMap(
                    () => this._store.select(fromStore.seaFCLImportTransactionState)
                        .pipe(
                            takeUntil(this.ngUnsubscribe),
                        )
                )
            )
            .subscribe(
                (shipment: any) => {
                    this.selectedShipment = shipment;
                }
            );

        this.getGoodSumaryOfHbl();
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'SHIPMENT' } });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
        }
    }

    sortLocal(sort: string): void {
        this.houseBill = this._sortService.sort(this.houseBill, sort, this.order);
    }


    gotoCreateHouseBill() {
        this._router.navigate([`/home/documentation/sea-fcl-import/${this.jobId}/hbl/new`]);
    }


    showDeletePopup(hbl: CsTransactionDetail) {
        this.confirmDeletePopup.show();
        this.selectedHbl = hbl;

    }

    deleteHbl(id: string) {
        this.isLoading = true;
        this._progressRef.start();
        this._documentRepo.deleteHbl(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.getHourseBill(this.jobId);
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }


    onDeleteHbl() {
        this.confirmDeletePopup.hide();
        this.deleteHbl(this.selectedHbl.id);
    }


    getGoodSumaryOfHbl() {
        this.isLoading = true;
        this._documentRepo.getGoodSummaryOfAllHbl(this.jobId).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
        ).subscribe(
            (res: any) => {

                this.goodSummary = res;
            },
        );
    }


    getHourseBill(id: string) {
        this.isLoading = true;
        this._documentRepo.getListHourseBill({ jobId: this.jobId }).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
        ).subscribe(
            (res: any) => {
                this.houseBill = res;
                if (!!this.houseBill.length) {
                    this.selectHBL(this.houseBill[0])
                }
            },
        );
    }

    selectHBL(hbl: CsTransactionDetail) {
        this.selectedHbl = new CsTransactionDetail(hbl);

        // * Get container, Job detail, Surcharge with hbl id, JobId.
        this._store.dispatch(new fromStore.GetContainerAction({ hblid: hbl.id }));
        this._store.dispatch(new fromStore.SeaFCLImportGetDetailAction(hbl.jobId));
        this._store.dispatch(new fromStore.GetProfitHBLAction(this.selectedHbl.id));

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
            this._store.dispatch(new fromStore.GetContainerAction({ hblid: this.selectedHbl.id }));
            this._store.dispatch(new fromStore.SeaFCLImportGetDetailAction(this.selectedHbl.jobId));
            this._store.dispatch(new fromStore.GetProfitHBLAction(this.selectedHbl.id));

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
}
