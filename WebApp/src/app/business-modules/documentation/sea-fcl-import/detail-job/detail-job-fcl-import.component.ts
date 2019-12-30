import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { SeaFCLImportCreateJobComponent } from '../create-job/create-job-fcl-import.component';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent, SubHeaderComponent } from 'src/app/shared/common';

import { combineLatest, of } from 'rxjs';
import { map, tap, switchMap, skip, catchError, takeUntil, finalize } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL';


@Component({
    selector: 'app-detail-job-fcl-import',
    templateUrl: './detail-job-fcl-import.component.html',
})
export class SeaFCLImportDetailJobComponent extends SeaFCLImportCreateJobComponent {
    @ViewChild(SubHeaderComponent, { static: false }) headerComponent: SubHeaderComponent;
    @ViewChild("deleteConfirmTemplate", { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild("duplicateconfirmTemplate", { static: false }) confirmDuplicatePopup: ConfirmPopupComponent;
    @ViewChild("confirmLockShipment", { static: false }) confirmLockShipmentPopup: ConfirmPopupComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;

    jobId: string;
    selectedTab: TAB | string = 'SHIPMENT';
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';

    fclImportDetail: any; // TODO Model.
    action: any = {};

    dataReport: any = null;

    constructor(
        protected _router: Router,
        protected _documentRepo: DocumentationRepo,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.ITransactionState>,
        protected _actionStoreSubject: ActionsSubject,
        protected _toastService: ToastrService,
        protected cdr: ChangeDetectorRef,
        private _ngProgressService: NgProgress
    ) {
        super(_router, _documentRepo, _actionStoreSubject, _toastService, cdr);

        this._progressRef = this._ngProgressService.ref();
    }

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

                this.cdr.detectChanges();
            }),
            switchMap(() => of(this.jobId))
        ).subscribe(
            (jobId: string) => {
                this.jobId = jobId;
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(jobId));
                this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: jobId }));
                this._store.dispatch(new fromShareBussiness.TransactionGetProfitAction(jobId));


                this.getDetailSeaFCLImport();
                this.getListContainer();
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
                (res: any) => {
                    this.fclImportDetail = res; // TODO Model.
                    // * reset field duplicate
                    if (this.ACTION === "COPY") {
                        this.formCreateComponent.getUserLogged();
                        this.headerComponent.resetBreadcrumb("Create Job");
                    }
                },
            );
    }

    getListContainer() {
        this._store.select<any>(fromShareBussiness.getContainerSaveState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (containers: any) => {
                    this.containers = containers || [];

                    this.shipmentGoodSummaryComponent.containers = this.containers;
                    if (this.ACTION === 'COPY') {
                        this.containers.forEach(item => {
                            item.sealNo = null;
                            item.containerNo = null;
                            item.markNo = null;
                        });
                    }
                }
            );
    }

    onUpdateShipmentDetail() {
        this.formCreateComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        const modelUpdate = this.onSubmitData();

        //  * Update field
        modelUpdate.csMawbcontainers = this.containers;
        modelUpdate.id = this.jobId;
        modelUpdate.branchId = this.fclImportDetail.branchId;
        modelUpdate.transactionType = this.fclImportDetail.transactionType;
        modelUpdate.jobNo = this.fclImportDetail.jobNo;
        modelUpdate.datetimeCreated = this.fclImportDetail.datetimeCreated;
        modelUpdate.userCreated = this.fclImportDetail.userCreated;

        if (this.ACTION === 'COPY') {
            this.duplicateJob(modelUpdate);
        } else {
            this.updateJob(modelUpdate);
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
                        this.formCreateComponent.formCreate.controls['jobId'].setValue(this.jobId);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    updateJob(body: any) {
        this._documenRepo.updateCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * get detail & container list.
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));

                        this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: this.jobId }));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action) });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            case 'assignment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
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

    lockShipment() {
        this.confirmLockShipmentPopup.show();
    }

    onLockShipment() {
        this.confirmLockShipmentPopup.hide();

        const modelUpdate = this.onSubmitData();

        //  * Update field
        modelUpdate.csMawbcontainers = this.containers;
        modelUpdate.id = this.jobId;
        modelUpdate.branchId = this.fclImportDetail.branchId;
        modelUpdate.transactionType = this.fclImportDetail.transactionType;
        modelUpdate.jobNo = this.fclImportDetail.jobNo;
        modelUpdate.datetimeCreated = this.fclImportDetail.datetimeCreated;
        modelUpdate.userCreated = this.fclImportDetail.userCreated;
        modelUpdate.isLocked = true;
        this.updateJob(modelUpdate);
    }

    showDuplicateConfirm() {
        this.confirmDuplicatePopup.show();
    }

    duplicateConfirm() {
        this.action = { action: 'copy' };
        this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], {
            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action)
        });
        this.confirmDuplicatePopup.hide();
    }

    gotoList() {
        this._router.navigate(["home/documentation/sea-fcl-import"]);
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
}
