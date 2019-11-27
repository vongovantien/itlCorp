import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { SeaFCLImportCreateJobComponent } from '../create-job/create-job-fcl-import.component';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SeaFClImportFormCreateComponent } from '../components/form-create/form-create-sea-fcl-import.component';
import { Container } from 'src/app/shared/models/document/container.model';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent } from 'src/app/shared/common';

import { combineLatest, of } from 'rxjs';
import { map, tap, switchMap, skip, catchError, takeUntil, finalize } from 'rxjs/operators';

import * as fromShareBussiness from './../../../share-business/store';
import { TransactionTypeEnum } from 'src/app/shared/enums';

type TAB = 'SHIPMENT' | 'CDNOTE' | 'ASSIGNMENT' | 'HBL';



@Component({
    selector: 'app-detail-job-fcl-import',
    templateUrl: './detail-job-fcl-import.component.html',
    styleUrls: ['./../create-job/create-job-fcl-import.component.scss']
})
export class SeaFCLImportDetailJobComponent extends SeaFCLImportCreateJobComponent {

    @ViewChild(SeaFClImportFormCreateComponent, { static: false }) formCreateComponent: SeaFClImportFormCreateComponent;
    @ViewChild("deleteConfirmTemplate", { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild("duplicateconfirmTemplate", { static: false }) confirmDuplicatePopup: ConfirmPopupComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;

    id: string;
    selectedTab: TAB | string = 'SHIPMENT';
    ACTION: CommonType.ACTION_FORM | string = 'UPDATE';

    fclImportDetail: any; // TODO Model.
    containers: Container[] = [];
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
                this.id = !!param.id ? param.id : '';
                if (param.action) {
                    this.ACTION = param.action.toUpperCase();
                }

                this.cdr.detectChanges();
            }),
            switchMap(() => of(this.id))
        ).subscribe(
            (jobId: string) => {
                this.id = jobId;
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

                    // * Update Good Summary.
                    this.shipmentGoodSummaryComponent.containerDetail = this.fclImportDetail.packageContainer;
                    this.shipmentGoodSummaryComponent.commodities = this.fclImportDetail.commodity;
                    this.shipmentGoodSummaryComponent.description = this.fclImportDetail.desOfGoods;
                    this.shipmentGoodSummaryComponent.grossWeight = this.fclImportDetail.grossWeight;
                    this.shipmentGoodSummaryComponent.netWeight = this.fclImportDetail.netWeight;
                    this.shipmentGoodSummaryComponent.totalChargeWeight = this.fclImportDetail.chargeWeight;
                    this.shipmentGoodSummaryComponent.totalCBM = this.fclImportDetail.cbm;
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
                }
            );
    }

    onUpdateShipmenetDetail() {
        this.formCreateComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        if (!this.containers.length) {
            this._toastService.warning('Please add container to update job');
            return;
        }

        const modelUpdate = this.onSubmitData();

        //  * Update field
        modelUpdate.csMawbcontainers = this.containers;
        modelUpdate.id = this.id;
        modelUpdate.branchId = this.fclImportDetail.branchId;
        modelUpdate.transactionType = this.fclImportDetail.transactionType;
        modelUpdate.jobNo = this.fclImportDetail.jobNo;
        modelUpdate.datetimeCreated = this.fclImportDetail.datetimeCreated;
        modelUpdate.userCreated = this.fclImportDetail.userCreated;

        if (this.ACTION === 'UPDATE') {
            this.updateJob(modelUpdate);
        } else {
            this.duplicateJob(modelUpdate);
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
                        this.id = res.data.id;
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.id));

                        this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: this.id }));
                        // * get detail & container list.
                        this._router.navigate([`home/documentation/sea-fcl-import/${this.id}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }) });
                        this.ACTION = 'SHIPMENT';
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
                        this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.id));

                        this._store.dispatch(new fromShareBussiness.GetContainerAction({ mblid: this.id }));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.id}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.id}`], { queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action) });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.id}`], { queryParams: { tab: 'CDNOTE', transactionType: TransactionTypeEnum.SeaFCLImport } });
                break;
            case 'assignment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.id}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
        }
    }

    deleteJob() {
        this.confirmDeletePopup.show();
    }

    onDeleteJob() {
        this._progressRef.start();
        this._documenRepo.deleteMasterBill(this.id)
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

    showDuplicateConfirm() {
        this.confirmDuplicatePopup.show();
    }

    duplicateConfirm() {
        this.action = { action: 'copy' };
        this._router.navigate([`home/documentation/sea-fcl-import/${this.id}`], {
            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, this.action)
        });
        this.confirmDuplicatePopup.hide();
    }

    gotoList() {
        this._router.navigate(["home/documentation/sea-fcl-import"]);
    }

    previewPLsheet(currency: string) {
        this._documenRepo.previewSIFPLsheet(this.id, currency)
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
