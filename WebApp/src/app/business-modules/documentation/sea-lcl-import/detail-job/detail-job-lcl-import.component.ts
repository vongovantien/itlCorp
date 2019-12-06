import { Component, OnInit, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';

import { SeaLCLImportCreateJobComponent } from '../create-job/create-job-lcl-import.component';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { CsTransactionDetail } from 'src/app/shared/models';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent } from 'src/app/shared/common';

import { combineLatest, of } from 'rxjs';
import { switchMap, map, tap, skip, takeUntil, catchError, finalize } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';


type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL';

@Component({
    selector: 'app-sea-lcl-import-detail-job',
    templateUrl: './detail-job-lcl-import.component.html'
})

export class SeaLCLImportDetailJobComponent extends SeaLCLImportCreateJobComponent implements OnInit {

    @ViewChild("deleteConfirmTemplate", { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild("duplicateconfirmTemplate", { static: false }) confirmDuplicatePopup: ConfirmPopupComponent;
    @ViewChild("confirmLockShipment", { static: false }) confirmLockPopup: ConfirmPopupComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;

    jobId: string;

    shipmentDetail: any;

    selectedTab: TAB | string = 'SHIPMENT';
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';

    action: any = {};
    dataReport: any = null;
    constructor(
        protected _router: Router,
        protected _documenRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        private _activedRoute: ActivatedRoute,
        private _store: Store<any>,
        private _ngProgressService: NgProgress
    ) {
        super(_router, _documenRepo, _toastService);
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() { }

    ngAfterViewInit() {
        combineLatest([
            this._activedRoute.params,
            this._activedRoute.queryParams
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
            tap((param: any) => {
                this.selectedTab = !!param.tab ? param.tab.toUpperCase() : 'SHIPMENT';
                this.jobId = !!param.jobId ? param.jobId : '';
                if (param.action) {
                    this.ACTION = param.action.toUpperCase();
                } else {
                    this.ACTION = null;
                }
            }),
            switchMap(() => of(this.jobId)),
        ).subscribe(
            (jobId: string) => {
                if (!!jobId) {
                    this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));
                    this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(jobId));

                    this.getDetailSeaFCLImport();
                }
            }
        );
    }

    getDetailSeaFCLImport() {
        this._store.select<any>(fromShareBussiness.getTransactionDetailCsTransactionState)
            .pipe(
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CsTransactionDetail) => {
                    if (!!res) {
                        this.shipmentDetail = res;

                        // * Update Goods Summary.
                        this.shipmentGoodSummaryComponent.commodities = res.commodity;
                        this.shipmentGoodSummaryComponent.gw = res.grossWeight;
                        this.shipmentGoodSummaryComponent.packageQuantity = res.packageQty;
                        this.shipmentGoodSummaryComponent.cbm = res.cbm;

                        // * reset field duplicate
                        if (this.ACTION === "COPY") {

                            this.resetFormControl(this.formCreateComponent.etd);
                            this.resetFormControl(this.formCreateComponent.mawb);
                            this.resetFormControl(this.formCreateComponent.eta);
                            this.formCreateComponent.getUserLogged();
                        }

                        const packageTypesTemp: CommonInterface.INg2Select[] = this.shipmentDetail.packageType.split(',').map((i: string) => <CommonInterface.INg2Select>({
                            id: i,
                            text: i,
                        }));
                        const packageTypes = [];
                        packageTypesTemp.forEach((type: CommonInterface.INg2Select) => {
                            const dataTempInPackages = this.shipmentGoodSummaryComponent.packages.find((t: CommonInterface.INg2Select) => t.id === type.id);
                            if (!!dataTempInPackages) {
                                packageTypes.push(dataTempInPackages);
                            }
                        });

                        this.shipmentGoodSummaryComponent.packageTypes = packageTypes;
                    }
                },
            );
    }

    onSaveJob() {
        [this.formCreateComponent.isSubmitted, this.shipmentGoodSummaryComponent.isSubmitted] = [true, true];

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const modelAdd = this.onSubmitData();

        //  * Update field
        modelAdd.id = this.jobId;
        modelAdd.transactionType = this.shipmentDetail.transactionType;
        modelAdd.jobNo = this.shipmentDetail.jobNo;
        modelAdd.datetimeCreated = this.shipmentDetail.datetimeCreated;
        modelAdd.userCreated = this.shipmentDetail.userCreated;
        if (this.ACTION === 'COPY') {
            this.duplicateJob(modelAdd);
        } else {
            this.saveJob(modelAdd);
        }
    }
    duplicateJob(body: any) {
        this._documenRepo.importCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.jobId = res.data.id;
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                        this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: this.jobId }));
                        // * get detail & container list.
                        this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                        this.ACTION = 'SHIPMENT';
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    saveJob(body: any) {
        this._documenRepo.updateCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * Dispatch action get detail.
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action) });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            case 'assignment':
                this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
        }
    }

    deleteJob() {
        this.confirmDeletePopup.show();
    }

    onDeleteJob() {
        this._progressRef.start();
        this._documenRepo.deleteMasterBill(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeletePopup.hide();
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

    previewPLsheet(currency: string) {
        this._documenRepo.previewSIFPLsheet(this.jobId, currency)
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

    showDuplicateConfirm() {
        this.confirmDuplicatePopup.show();
    }

    duplicateConfirm() {
        this.action = { action: 'copy' };
        this._router.navigate([`home/documentation/sea-lcl-import/${this.jobId}`], {
            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action)
        });
        this.confirmDuplicatePopup.hide();
    }

    lockShipment() {
        this.confirmLockPopup.show();
    }

    onLockShipment() {
        this.confirmLockPopup.hide();

        const modelAdd = this.onSubmitData();

        //  * Update field
        modelAdd.id = this.jobId;
        modelAdd.transactionType = this.shipmentDetail.transactionType;
        modelAdd.jobNo = this.shipmentDetail.jobNo;
        modelAdd.datetimeCreated = this.shipmentDetail.datetimeCreated;
        modelAdd.userCreated = this.shipmentDetail.userCreated;
        modelAdd.isLocked = true;

        this.saveJob(modelAdd);
    }
}
