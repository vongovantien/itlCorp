import { Component, ViewChild } from '@angular/core';
import { Store, ActionsSubject } from '@ngrx/store';

import { Router, ActivatedRoute } from '@angular/router';

import { SeaFCLImportCreateJobComponent } from '../create-job/create-job-fcl-import.component';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SeaFClImportFormCreateComponent } from '../components/form-create/form-create-sea-fcl-import.component';
import { Container } from 'src/app/shared/models/document/container.model';

import { combineLatest, of } from 'rxjs';
import { map, tap, switchMap, skip, catchError, takeUntil } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import * as fromStore from './../store';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';


type TAB = 'SHIPMENT' | 'CDNOTE';

@Component({
    selector: 'app-detail-job-fcl-import',
    templateUrl: './detail-job-fcl-import.component.html',
    styleUrls: ['./../create-job/create-job-fcl-import.component.scss']
})
export class SeaFCLImportDetailJobComponent extends SeaFCLImportCreateJobComponent {

    @ViewChild(SeaFClImportFormCreateComponent, { static: false }) formCreateComponent: SeaFClImportFormCreateComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    id: string;
    selectedTab: TAB = 'SHIPMENT';

    fclImportDetail: any; // TODO Model.
    containers: Container[] = [];

    constructor(
        protected _router: Router,
        protected _documentRepo: DocumentationRepo,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromStore.ISeaFCLImportState>,
        protected _actionStoreSubject: ActionsSubject,
        protected _toastService: ToastrService
    ) {
        super(_router, _documentRepo, _actionStoreSubject, _toastService);
    }

    ngOnInit(): void {

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
            }),
            switchMap(() => of(this.id))
            // switchMap((param: any) => this._documentRepo.getDetailTransaction(param.id)) // ? Using Effects or common way.
        ).subscribe(
            (jobId: string) => {
                this.id = jobId;
                this._store.dispatch(new fromStore.SeaFCLImportGetDetailAction(jobId));
                this._store.dispatch(new fromStore.GetContainerAction({ mblid: jobId }));
                this._store.dispatch(new fromStore.SeaFCLImportGetProfitAction(jobId));


                this.getDetailSeaFCLImport();
                this.getListContainer();
            }
        );
    }

    getDetailSeaFCLImport() {
        this._store.select<any>(fromStore.seaFCLImportTransactionState)
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

                    // console.log("detail sea fcl import", this.fclImportDetail);

                    setTimeout(() => {
                        this.updateForm();
                    }, 200);

                    // this.formCreateComponent.fclImportDetail = this.fclImportDetail;
                    // this.formCreateComponent.initFormUpdate();
                },

            );
    }

    getListContainer() {
        this._store.select<any>(fromStore.getContainerSaveState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (containers: any) => {
                    this.containers = containers || [];
                }
            );
    }

    updateForm() {
        this.formCreateComponent.formCreate.setValue({
            jobId: this.fclImportDetail.jobNo, // * disabled
            mawb: this.fclImportDetail.mawb,
            subColoader: this.fclImportDetail.subColoader,
            flightVesselName: this.fclImportDetail.flightVesselName,
            voyNo: this.fclImportDetail.voyNo,
            pono: this.fclImportDetail.pono,
            notes: this.fclImportDetail.notes,

            etd: !!this.fclImportDetail.etd ? { startDate: new Date(this.fclImportDetail.etd), endDate: new Date(this.fclImportDetail.etd) } : null, // * Date
            eta: !!this.fclImportDetail.eta ? { startDate: new Date(this.fclImportDetail.eta), endDate: new Date(this.fclImportDetail.eta) } : null, // * Date
            serviceDate: !!this.fclImportDetail.serviceDate ? { startDate: new Date(this.fclImportDetail.serviceDate) } : null,

            mbltype: (this.formCreateComponent.ladingTypes || []).filter(type => type.value === this.fclImportDetail.mbltype)[0].value, // * select
            shipmentType: (this.formCreateComponent.shipmentTypes || []).filter(type => type.value === this.fclImportDetail.shipmentType)[0].value, // * select
            typeOfService: (this.formCreateComponent.serviceTypes || []).filter(type => type.value === this.fclImportDetail.typeOfService)[0].value, // * select
            personIncharge: this.fclImportDetail.personIncharge,  // * select
        });

        // * Combo grid
        this.formCreateComponent.selectedPortDestination = { field: 'id', value: this.fclImportDetail.pod };
        this.formCreateComponent.selectedPortDelivery = { field: 'id', value: this.fclImportDetail.deliveryPlace };
        this.formCreateComponent.selectedPortLoading = { field: 'id', value: this.fclImportDetail.pol };
        this.formCreateComponent.selectedAgent = { field: 'id', value: this.fclImportDetail.agentId };
        this.formCreateComponent.selectedSupplier = { field: 'id', value: this.fclImportDetail.coloaderId };

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
        modelUpdate.userCreated = this.fclImportDetail.userCreated;
        modelUpdate.transactionType = this.fclImportDetail.transactionType;
        modelUpdate.jobNo = this.fclImportDetail.jobNo;
        modelUpdate.datetimeCreated = this.fclImportDetail.modifiedDate;

        this.updateJob(modelUpdate);
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
                        this._store.dispatch(new fromStore.SeaFCLImportGetDetailAction(this.id));

                        this._store.dispatch(new fromStore.GetContainerAction({ mblid: this.id }));
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
                this._router.navigate([`home/documentation/sea-fcl-import/${this.id}`], { queryParams: { tab: 'SHIPMENT' } });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.id}`], { queryParams: { tab: 'CDNOTE' } });
                break;
        }
    }

    deleteJob() {
        this.confirmDeletePopup.show();
    }

    onDeleteJob() {
        this.confirmDeletePopup.hide();
    }
}
