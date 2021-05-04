import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { ActionsSubject } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';

import {
    ShareBussinessShipmentGoodSummaryComponent,
    ShareBusinessImportJobDetailPopupComponent,
    ContainerAction, ContainerActionTypes
} from '@share-bussiness';
import { InfoPopupComponent } from '@common';
import { DocumentationRepo } from '@repositories';
import { CsTransaction, Container } from '@models';
import { CommonEnum } from '@enums';
import { AppForm } from '@app';
import { RoutingConstants } from '@constants';

import { ShareSeaServiceFormCreateSeaExportComponent } from '../../share-sea/components/form-create-sea-export/form-create-sea-export.component';

import { takeUntil, catchError } from 'rxjs/operators';
import _merge from 'lodash/merge';

@Component({
    selector: ' app-create-job-consol-export',
    templateUrl: './create-job-consol-export.component.html',
})
export class SeaConsolExportCreateJobComponent extends AppForm implements OnInit {

    @ViewChild(ShareSeaServiceFormCreateSeaExportComponent) formCreateComponent: ShareSeaServiceFormCreateSeaExportComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(ShareBussinessShipmentGoodSummaryComponent) shipmentGoodSummaryComponent: ShareBussinessShipmentGoodSummaryComponent;
    @ViewChild(ShareBusinessImportJobDetailPopupComponent) formImportJobDetailPopup: ShareBusinessImportJobDetailPopupComponent;

    containers: Container[] = [];
    isImport: boolean = false;
    selectedJob: Partial<CsTransaction> | any = {}; // TODO model.

    constructor(
        protected _toastService: ToastrService,
        protected _documenRepo: DocumentationRepo,
        protected _router: Router,
        protected _actionStoreSubject: ActionsSubject,
        protected _cdr: ChangeDetectorRef,
    ) {
        super();
    }

    ngOnInit() {
        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: ContainerAction) => {
                    if (action.type === ContainerActionTypes.SAVE_CONTAINER) {
                        this.containers = action.payload;
                    }
                });

    }

    ngAfterViewInit() {
        // * Init container
        this.shipmentGoodSummaryComponent.initContainer();
        this.shipmentGoodSummaryComponent.containerPopup.isAdd = true;
        this.shipmentGoodSummaryComponent.shipment.permission.allowUpdate = true;
        this._cdr.detectChanges();
    }

    onSubmitData() {
        const form: any = this.formCreateComponent.formGroup.getRawValue();

        const formData = {
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd && !!form.etd.startDate ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            serviceDate: !!form.serviceDate && !!form.serviceDate.startDate ? formatDate(form.serviceDate.startDate, 'yyyy-MM-dd', 'en') : null,
            ata: !!form.ata && !!form.ata.startDate ? formatDate(form.ata.startDate, 'yyyy-MM-dd', 'en') : null,
            atd: !!form.atd && !!form.atd.startDate ? formatDate(form.atd.startDate, 'yyyy-MM-dd', 'en') : null,

            personIncharge: form.personalIncharge,
            agentId: form.agent,
            coloaderId: form.coloader,
            paymentTerm: form.term,
            polDescription: form.polDescription,
            podDescription: form.podDescription,

            // * containers summary
            commodity: this.shipmentGoodSummaryComponent.commodities,
            desOfGoods: this.shipmentGoodSummaryComponent.description,
            packageContainer: this.shipmentGoodSummaryComponent.containerDetail,
            netWeight: this.shipmentGoodSummaryComponent.netWeight,
            grossWeight: this.shipmentGoodSummaryComponent.grossWeight,
            chargeWeight: this.shipmentGoodSummaryComponent.totalChargeWeight,
            cbm: this.shipmentGoodSummaryComponent.totalCBM,
        };

        const seaConsolExportModel: CsTransaction = new CsTransaction(Object.assign(_merge(form, formData)));
        seaConsolExportModel.transactionTypeEnum = CommonEnum.TransactionTypeEnum.SeaConsolExport;

        return seaConsolExportModel;
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formCreateComponent.formGroup.valid
            || (!!this.formCreateComponent.etd.value && !this.formCreateComponent.etd.value.startDate)
            || (!!this.formCreateComponent.podDescription.value && !this.formCreateComponent.pod.value)
            || (!this.formCreateComponent.polDescription.value)
        ) {
            valid = false;
        }
        return valid;
    }

    onSaveJob() {
        this.formCreateComponent.isSubmitted = true;
        if (Object.keys(this.selectedJob).length > 0) {
            this.containers = this.selectedJob.containers;
        }
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        const modelAdd = this.onSubmitData();
        modelAdd.csMawbcontainers = this.containers; // * Update containers model

        if (this.isImport === true) {
            modelAdd.id = this.selectedJob.id;
            this.importJob(modelAdd);
        } else {
            this.saveJob(modelAdd);
        }
    }

    saveJob(body: any) {
        this._documenRepo.createTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: any) => {
                    if (res.result.success) {
                        this._toastService.success("New data added");

                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_EXPORT}/${res.model.id}`]);
                    } else {
                        this._toastService.error("Opps", "Something getting error!");
                    }
                }
            );
    }

    onImport(selectedData: any) {
        this.selectedJob = selectedData;
        this.isImport = true;
        this.shipmentGoodSummaryComponent.commodities = this.selectedJob.commodity;
        this.shipmentGoodSummaryComponent.containerDetail = this.selectedJob.packageContainer;
        this.shipmentGoodSummaryComponent.description = this.selectedJob.desOfGoods;
        this.shipmentGoodSummaryComponent.grossWeight = this.selectedJob.grossWeight;
        this.shipmentGoodSummaryComponent.netWeight = this.selectedJob.netWeight;
        this.shipmentGoodSummaryComponent.totalChargeWeight = this.selectedJob.chargeWeight;
        this.shipmentGoodSummaryComponent.totalCBM = this.selectedJob.cbm;

        this.formCreateComponent.formGroup.controls['personIncharge'].setValue(this.formCreateComponent.userLogged.id);
    }

    showImportPopup() {
        this.formImportJobDetailPopup.transactionType = CommonEnum.TransactionTypeEnum.SeaConsolExport;
        this.formImportJobDetailPopup.getShippments();
        this.formImportJobDetailPopup.show();
    }

    importJob(body: any) {
        this._documenRepo.importCSTransaction(body)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_EXPORT}/${res.data.id}`]);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_EXPORT}`]);
    }
}
