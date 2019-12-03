import { Component, OnInit, ViewChild } from '@angular/core';
import { Router, Params } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { AppList } from 'src/app/app.list';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { HouseBill } from 'src/app/shared/models';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { ShareBussinessSellingChargeComponent } from 'src/app/business-modules/share-business';
import { getParamsRouterState } from 'src/app/store';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

import { takeUntil, take, catchError, finalize } from 'rxjs/operators';

import * as fromShareBussiness from './../../../../share-business/store';

@Component({
    selector: 'app-sea-lcl-import-hbl',
    templateUrl: './sea-lcl-import-hbl.component.html'
})

export class SeaLCLImportHBLComponent extends AppList implements OnInit {
    @ViewChild(ShareBussinessSellingChargeComponent, { static: false }) sellingChargeComponent: ShareBussinessSellingChargeComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteHBLPopup: ConfirmPopupComponent;
    @ViewChild('confirmDeleteJob', { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;

    jobId: string;

    houseBills: HouseBill[] = [];
    selectedHbl: HouseBill;

    headers: CommonInterface.IHeaderTable[];

    selectedTabSurcharge: CommonEnum.SurchargeTypeEnum = CommonEnum.SurchargeTypeEnum.BUYING_RATE;

    constructor(
        private _router: Router,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService
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

    selectHBL(hbl: HouseBill) {
        this.selectedHbl = new HouseBill(hbl);

        // * Get container, Job detail, Surcharge with hbl id, JobId.
        this._store.dispatch(new fromShareBussiness.GetDetailHBLSuccessAction(hbl));
        this._store.dispatch(new fromShareBussiness.GetContainersHBLAction({ hblid: hbl.id }));
        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(hbl.jobId));
        this._store.dispatch(new fromShareBussiness.GetProfitHBLAction(this.selectedHbl.id));

        switch (this.selectedTabSurcharge) {
            case CommonEnum.SurchargeTypeEnum.BUYING_RATE:
                this._store.dispatch(new fromShareBussiness.GetBuyingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.BUYING_RATE, hblId: this.selectedHbl.id }));
                break;
            case CommonEnum.SurchargeTypeEnum.SELLING_RATE:
                this._store.dispatch(new fromShareBussiness.GetSellingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.SELLING_RATE, hblId: this.selectedHbl.id }));
                break;
            case CommonEnum.SurchargeTypeEnum.OBH:
                this._store.dispatch(new fromShareBussiness.GetOBHSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.OBH, hblId: this.selectedHbl.id }));
                break;
            default:
                break;
        }
    }

    onSelectTabSurcharge(tabName: any) {
        this.selectedTabSurcharge = tabName;
        if (!!this.selectedHbl) {
            switch (this.selectedTabSurcharge) {
                case 'BUY':
                    this._store.dispatch(new fromShareBussiness.GetBuyingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.BUYING_RATE, hblId: this.selectedHbl.id }));
                    break;
                case 'SELL':
                    this._store.dispatch(new fromShareBussiness.GetSellingSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.SELLING_RATE, hblId: this.selectedHbl.id }));
                    break;
                case 'OBH':
                    this._store.dispatch(new fromShareBussiness.GetOBHSurchargeAction({ type: CommonEnum.SurchargeTypeEnum.OBH, hblId: this.selectedHbl.id }));
                    break;
                default:
                    break;
            }
        }
    }

    showDeletePopup(hbl: HouseBill, event: Event) {
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
        this._router.navigate(["home/documentation/sea-lcl-import"]);
    }

    gotoCreate() {
        this._router.navigate([`/home/documentation/sea-lcl-import/${this.jobId}/hbl/new`]);
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'shipment':
                this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}`], { queryParams: { tab: 'SHIPMENT' } });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            case 'assignment':
                this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
        }
    }
}
