import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { ActionsSubject } from '@ngrx/store';

import { AppForm } from 'src/app/app.form';
import { InfoPopupComponent } from 'src/app/shared/common/popup';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { CsTransaction } from 'src/app/shared/models';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { Container } from 'src/app/shared/models/document/container.model';
import {
    ShareBussinessFormCreateSeaExportComponent,
    ShareBussinessShipmentGoodSummaryComponent,
    ShareBusinessImportJobDetailPopupComponent,
    ShareBusinessFormCreateAirComponent
} from 'src/app/business-modules/share-business';

import * as fromShareBussiness from '../../../share-business/store';

import { catchError, takeUntil } from 'rxjs/operators';

@Component({
    selector: 'app-create-job-air-export',
    templateUrl: './create-job-air-export.component.html'
})

export class AirExportCreateJobComponent extends AppForm implements OnInit {

    @ViewChild(ShareBusinessFormCreateAirComponent, { static: false }) formCreateComponent: ShareBusinessFormCreateAirComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ShareBussinessShipmentGoodSummaryComponent, { static: false }) shipmentGoodSummaryComponent: ShareBussinessShipmentGoodSummaryComponent;
    @ViewChild(ShareBusinessImportJobDetailPopupComponent, { static: false }) formImportJobDetailPopup: ShareBusinessImportJobDetailPopupComponent;

    containers: Container[] = [];
    isImport: boolean = false;
    selectedJob: any = {}; // TODO model.

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
                (action: fromShareBussiness.ContainerAction) => {
                    if (action.type === fromShareBussiness.ContainerActionTypes.SAVE_CONTAINER) {
                        this.containers = action.payload;
                    }
                });
    }

    ngAfterViewInit() {
        // * Init container
        // this.shipmentGoodSummaryComponent.initContainer();
        // this.shipmentGoodSummaryComponent.containerPopup.isAdd = true;
        this._cdr.detectChanges();
    }

    onSubmitData() {
        const form: any = this.formCreateComponent.formGroup.getRawValue();
        console.log(form);

        const formData = {
            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            etd: !!form.etd && !!form.etd.startDate ? formatDate(form.etd.startDate, 'yyyy-MM-dd', 'en') : null,
            serviceDate: !!form.serviceDate ? formatDate(form.serviceDate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightDate: !!form.flightDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,

            shipmentType: !!form.shipmentType ? form.shipmentType[0].id : null,
            typeOfService: !!form.typeOfService ? form.typeOfService[0].id : null,
            paymentTerm: !!form.paymentTerm ? form.paymentTerm[0].id : null,
            packageType: !!form.packageType ? form.packageType[0].id : null,
            commodity: !!form.commodity ? form.commodity.map(i => i.id).toString() : null,

            agentId: form.agentId,
            pol: form.pol,
            pod: form.pod,
            coloaderId: form.coloaderId,
        };

        const fclExportAddModel: CsTransaction = new CsTransaction(Object.assign({}, form, formData));
        fclExportAddModel.transactionTypeEnum = CommonEnum.TransactionTypeEnum.AirExport;

        return fclExportAddModel;
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formCreateComponent.formGroup.valid || (!!this.formCreateComponent.etd.value && !this.formCreateComponent.etd.value.startDate)) {
            valid = false;
        }
        return valid;
    }

    onSaveJob() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }

        const modelAdd = this.onSubmitData();

        if (this.isImport === true) {
            modelAdd.id = this.selectedJob.id;
            this.importJob(modelAdd);
        } else {
            console.log(modelAdd);
            // this.saveJob(modelAdd);
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

                        this._router.navigate([`home/documentation/sea-fcl-export/${res.model.id}`]);
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
    }

    showImportPopup() {
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
                        // TODO goto detail.
                        this._router.navigate([`home/documentation/sea-fcl-export/${res.data.id}`]);
                    } else {
                        this._toastService.error("Opps", "Something getting error!");
                    }
                }
            );
    }
}
