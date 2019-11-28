import { Component, OnInit } from '@angular/core';
import { Router, Params } from '@angular/router';
import { Store } from '@ngrx/store';

import { AppList } from 'src/app/app.list';
import { getParamsRouterState } from 'src/app/store';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, finalize, takeUntil, take } from 'rxjs/operators';
import { CsTransactionDetail } from 'src/app/shared/models';

import * as fromShareBussiness from './../../../../share-business/store';
import * as fromStore from './../../store';
import { Container } from 'src/app/shared/models/document/container.model';
import { Observable } from 'rxjs';
import { CommonEnum } from 'src/app/shared/enums/common.enum';


@Component({
    selector: 'app-sea-fcl-export-hbl',
    templateUrl: './sea-fcl-export-hbl.component.html'
})

export class SeaFCLExportHBLComponent extends AppList implements OnInit {

    jobId: string;
    headers: CommonInterface.IHeaderTable[];
    houseBills: any[] = [];

    selectedHbl: CsTransactionDetail;

    selectedTabSurcharge: string = 'BUY';

    containers: Observable<Container[]>;
    selectedShipment: Observable<any>;

    constructor(
        private _router: Router,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        private _documentRepo: DocumentationRepo
    ) {
        super();
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

        this.containers = this._store.select(fromShareBussiness.getHBLContainersState);
        this.selectedShipment = this._store.select(fromStore.getSeaFCLShipmentDetail);
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

    selectHBL(hbl: CsTransactionDetail) {
        this.selectedHbl = new CsTransactionDetail(hbl);

        // * Get container, Job detail, Surcharge with hbl id, JobId.
        this._store.dispatch(new fromShareBussiness.GetContainerAction({ hblid: hbl.id }));
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
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: { tab: 'CDNOTE', transactionType: CommonEnum.TransactionTypeEnum.SeaFCLExport } });
                break;
            case 'assignment':
                this._router.navigate([`home/documentation/sea-fcl-export/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
        }
    }
}
