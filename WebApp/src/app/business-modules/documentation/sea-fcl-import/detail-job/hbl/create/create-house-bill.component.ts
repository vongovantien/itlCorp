import { Component, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { AppForm } from 'src/app/app.form';
import { formatDate } from '@angular/common';
import { catchError, takeUntil } from 'rxjs/operators';
import { finalize } from 'rxjs/internal/operators/finalize';
import { FormAddHouseBillComponent } from '../components/form-add-house-bill/form-add-house-bill.component';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { FCLImportAddModel } from 'src/app/shared/models';
import { ActionsSubject } from '@ngrx/store';
import * as fromStore from '../../../store/index';
import { InfoPopupComponent, ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'app-create-house-bill',
    templateUrl: './create-house-bill.component.html',
    styleUrls: ['./create-house-bill.component.scss']
})
export class CreateHouseBillComponent extends AppForm {
    @ViewChild(FormAddHouseBillComponent, { static: false }) formHouseBill: FormAddHouseBillComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmCreatePopup: ConfirmPopupComponent;

    fclImportAddModel: FCLImportAddModel = new FCLImportAddModel();
    jobId: string = '';
    constructor(
        private _progressService: NgProgress,
        private _fb: FormBuilder,
        private _documentationRepo: DocumentationRepo,
        private _toastService: ToastrService,
        protected _activedRoute: ActivatedRoute,
        protected _actionStoreSubject: ActionsSubject,
        private _router: Router

    ) {
        super();
        this._progressRef = this._progressService.ref();
    }
    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.id) {
                this.jobId = param.id;
            }
        });
        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: fromStore.ContainerAction) => {
                    if (action.type === fromStore.ContainerActionTypes.SAVE_CONTAINER) {
                        this.fclImportAddModel.csMawbcontainers = action.payload;

                        // * Update model object to integer.
                        for (const container of <any>this.fclImportAddModel.csMawbcontainers) {
                            container.containerTypeId = container.containerTypeId.id;
                            container.commodityId = !!container.commodityId ? container.commodityId.id : null;
                            container.packageTypeId = !!container.packageTypeId ? container.packageTypeId.id : null;
                        }

                        console.log("list container add success", this.fclImportAddModel.csMawbcontainers);
                    }
                });
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formHouseBill.formGroup.valid) {
            valid = false;
        }
        return valid;
    }

    oncreate() {
        this.confirmCreatePopup.hide();
        this.create();
    }

    showCreatePpoup() {
        this.confirmCreatePopup.show();
    }

    combackToHBLList() {
        this._router.navigate([`/home/documentation/sea-fcl-import/${this.jobId}/hbl`]);

    }


    create() {
        this.formHouseBill.isSubmited = true;
        if (!!this.formHouseBill.selectedPortOfLoading && this.formHouseBill.selectedPortOfDischarge) {
            if (this.formHouseBill.selectedPortOfLoading.data.id === this.formHouseBill.selectedPortOfDischarge.data.id) {
                this.formHouseBill.PortChargeLikePortLoading = true;
            } else {
                this.formHouseBill.PortChargeLikePortLoading = false;
            }
        }

        if (!this.checkValidateForm()) {
            this.infoPopup.show();
        }
        if (this.formHouseBill.formGroup.valid) {
            const body: ITransactionDetail = {
                jobId: this.jobId,
                mawb: this.formHouseBill.mtBill.value,
                saleManId: this.formHouseBill.selectedSaleman.data.saleMan_ID,
                shipperId: !!this.formHouseBill.selectedShipper.value ? this.formHouseBill.selectedShipper.data.id : null,
                shipperDescription: this.formHouseBill.shipperdescriptionModel,
                consigneeId: this.formHouseBill.selectedConsignee.data.id,
                consigneeDescription: this.formHouseBill.consigneedescriptionModel,
                notifyPartyId: !!this.formHouseBill.selectedNotifyParty.value ? this.formHouseBill.selectedNotifyParty.data.id : null,
                notifyPartyDescription: this.formHouseBill.notifyPartyModel,
                alsoNotifyPartyId: !!this.formHouseBill.selectedAlsoNotifyParty.value ? this.formHouseBill.selectedAlsoNotifyParty.data.id : null,
                alsoNotifyPartyDescription: this.formHouseBill.alsoNotifyPartyDescriptionModel,
                hwbno: this.formHouseBill.hwbno.value,
                hbltype: this.formHouseBill.hbltype.value.value,
                etd: formatDate(!!this.formHouseBill.etd.value ? this.formHouseBill.etd.value.startDate : null, 'yyyy-MM-dd', 'en'),
                eta: formatDate(this.formHouseBill.eta.value.startDate, 'yyyy-MM-dd', 'en'),
                pickupPlace: this.formHouseBill.pickupPlace.value,
                pol: this.formHouseBill.selectedPortOfLoading.data.id,
                pod: this.formHouseBill.selectedPortOfDischarge.data.id,
                finalDestinationPlace: this.formHouseBill.finalDestinationPlace.value,
                coloaderId: !!this.formHouseBill.selectedNotifyParty.value ? this.formHouseBill.selectedSupplier.data.id : null,
                localVessel: this.formHouseBill.localVessel.value,
                localVoyNo: this.formHouseBill.localVoyNo.value,
                oceanVessel: this.formHouseBill.oceanVessel.value,
                documentDate: formatDate(!!this.formHouseBill.documentDate.value ? this.formHouseBill.documentDate.value.startDate : null, 'yyyy-MM-dd', 'en'),
                documentNo: this.formHouseBill.documentNo.value,
                etawarehouse: formatDate(!!this.formHouseBill.etawarehouse.value ? this.formHouseBill.etawarehouse.value.startDate : null, 'yyyy-MM-dd', 'en'),
                warehouseNotice: this.formHouseBill.warehouseNotice.value,
                shippingMark: this.formHouseBill.shippingMark.value,
                remark: this.formHouseBill.remark.value,
                issueHBLPlace: !!this.formHouseBill.selectedPlaceOfIssued.value ? this.formHouseBill.selectedPlaceOfIssued.data.id : null,
                issueHBLDate: formatDate(!!this.formHouseBill.issueHBLDate.value ? this.formHouseBill.issueHBLDate.value.startDate : null, 'yyyy-MM-dd', 'en'),
                originBLNumber: this.formHouseBill.originBLNumber.value.value,
                referenceNo: this.formHouseBill.referenceNo.value,
                customerId: this.formHouseBill.selectedCustomer.value,
                csMawbcontainers: this.fclImportAddModel.csMawbcontainers
            };

            this._documentationRepo.createHousebill(body)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                        } else {

                        }
                    }
                );
        }
    }

}


export interface ITransactionDetail {
    jobId: string;
    mawb: string;
    saleManId: string;
    shipperId: string;
    shipperDescription: string;
    consigneeId: string;
    consigneeDescription: string;
    notifyPartyId: string;
    notifyPartyDescription: string;
    alsoNotifyPartyId: string;
    alsoNotifyPartyDescription: string;
    hwbno: string;
    hbltype: string;
    etd: string;
    eta: string;
    pickupPlace: string;
    pol: string;
    pod: string;
    finalDestinationPlace: string;
    coloaderId: string;
    localVessel: string;
    localVoyNo: string;
    oceanVessel: string;
    documentDate: string;
    documentNo: string;
    etawarehouse: string;
    warehouseNotice: string;
    shippingMark: string;
    remark: string;
    issueHBLPlace: string;
    issueHBLDate: string;
    originBLNumber: number;
    referenceNo: string;
    customerId: string;
    csMawbcontainers: any[];
}
