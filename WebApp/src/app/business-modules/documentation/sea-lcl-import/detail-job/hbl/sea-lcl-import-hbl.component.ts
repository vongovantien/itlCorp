import { Component, OnInit, ViewChild } from '@angular/core';
import { Router, Params } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { AppList } from 'src/app/app.list';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { HouseBill, CsTransaction } from 'src/app/shared/models';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { ShareBussinessSellingChargeComponent } from 'src/app/business-modules/share-business';
import { getParamsRouterState } from 'src/app/store';
import { ConfirmPopupComponent, Permission403PopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent } from 'src/app/shared/common';

import { takeUntil, take, catchError, finalize, skip } from 'rxjs/operators';

import * as fromShareBussiness from './../../../../share-business/store';
import { SortService } from '@services';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
    selector: 'app-sea-lcl-import-hbl',
    templateUrl: './sea-lcl-import-hbl.component.html'
})

export class SeaLCLImportHBLComponent extends AppList implements OnInit {
    @ViewChild(ShareBussinessSellingChargeComponent, { static: false }) sellingChargeComponent: ShareBussinessSellingChargeComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeleteHBLPopup: ConfirmPopupComponent;
    @ViewChild('confirmDeleteJob', { static: false }) confirmDeleteJobPopup: ConfirmPopupComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;
    @ViewChild(Permission403PopupComponent, { static: false }) info403Popup: Permission403PopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) canNotDeleteJobPopup: InfoPopupComponent;

    jobId: string;

    houseBills: HouseBill[] = [];
    selectedHbl: HouseBill;
    shipmentDetail: CsTransaction;

    headers: CommonInterface.IHeaderTable[];

    selectedTabSurcharge: CommonEnum.SurchargeTypeEnum = CommonEnum.SurchargeTypeEnum.BUYING_RATE;

    totalCBM: number;
    totalGW: number;

    dataReport: any = null;

    spinnerSurcharge: string = 'spinnerSurcharge';

    constructor(
        private _router: Router,
        private _store: Store<fromShareBussiness.IShareBussinessState>,
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _spinner: NgxSpinnerService

    ) {
        super();

        this.requestSort = this.sortHBL;
    }

    ngOnInit() {
        this._store.select(getParamsRouterState)
            .pipe(takeUntil(this.ngUnsubscribe), take(1))
            .subscribe((param: Params) => {
                if (param.jobId) {
                    this.jobId = param.jobId;
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                    this.getDetailShipment();
                    this._store.dispatch(new fromShareBussiness.GetListHBLAction({ jobId: this.jobId }));
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                    this.getDetailShipment();
                    this.getHouseBills(this.jobId);
                }
            });

        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true, width: 100 },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'SaleMan', field: 'saleManName', sortable: true },
            { title: 'Notify Party', field: 'notifyParty', sortable: true },
            { title: 'Destination', field: 'finalDestinationPlace', sortable: true },
            { title: 'Package', field: 'packages', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true }
        ];

        this.isLocked = this._store.select(fromShareBussiness.getTransactionLocked);

        this._store.select(fromShareBussiness.getSurchargeLoadingState).subscribe(
            (loading: boolean) => {
                if (loading) {
                    this._spinner.show(this.spinnerSurcharge);
                } else {
                    this._spinner.hide(this.spinnerSurcharge);
                }
            }
        );
    }

    getHouseBills(id: string) {
        this._store.select(fromShareBussiness.getHBLSState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (hbls: any[]) => {
                    this.houseBills = hbls;
                    if (!!this.houseBills.length) {
                        this.totalGW = this.houseBills.reduce((acc: number, curr: HouseBill) => acc += curr.gw, 0);
                        this.totalCBM = this.houseBills.reduce((acc: number, curr: HouseBill) => acc += curr.cbm, 0);
                        this.selectHBL(this.houseBills[0]);
                    } else {
                        this.selectedHbl = null;
                    }
                }
            );
    }

    sortHBL() {
        this.houseBills = this._sortService.sort(this.houseBills, this.sort, this.order);
    }

    selectHBL(hbl: HouseBill) {
        if (!this.selectedHbl || !!this.selectedHbl && this.selectedHbl.id !== hbl.id) {
            this.selectedHbl = new HouseBill(hbl);

            // * Get container, Job detail, Surcharge with hbl id, JobId.
            this._store.dispatch(new fromShareBussiness.GetDetailHBLSuccessAction(hbl));
            this._store.dispatch(new fromShareBussiness.GetContainersHBLAction({ hblid: hbl.id }));
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

    getDetailShipment() {
        this._store.select<any>(fromShareBussiness.getTransactionDetailCsTransactionState)
            .pipe(
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.shipmentDetail = res;
                    }
                },
            );
    }

    gotoDetail(id: string) {
        this._documentRepo.checkDetailShippmentPermission(this.shipmentDetail.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this._router.navigate([`/home/documentation/sea-lcl-import/${this.jobId}/hbl/${id}`]);
                    } else {
                        this.info403Popup.show();
                    }
                },
            );
    }

    prepareDeleteJob() {
        this._documentRepo.checkPermissionAllowDeleteShipment(this.jobId)
            .subscribe((value: boolean) => {
                if (value) {
                    this.deleteJob();
                } else {
                    this.info403Popup.show();
                }
            });
    }

    deleteJob() {
        this._progressRef.start();
        this._documentRepo.checkMasterBillAllowToDelete(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.confirmDeleteJobPopup.show();
                    } else {
                        this.canNotDeleteJobPopup.show();
                    }
                },
            );
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

    duplicateConfirm() {
        this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}`], {
            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, { action: 'copy' })
        });
    }

    previewPLsheet(currency: string) {
        this._documentRepo.previewSIFPLsheet(this.jobId, currency)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        setTimeout(() => {
                            this.previewPopup.frm.nativeElement.submit();
                            this.previewPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }
}
