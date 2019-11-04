import { Component, ViewChild } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { AppForm } from 'src/app/app.form';
import { formatDate } from '@angular/common';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { FormAddHouseBillComponent } from '../components/form-add-house-bill/form-add-house-bill.component';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
@Component({
    selector: 'app-create-house-bill',
    templateUrl: './create-house-bill.component.html',
    styleUrls: ['./create-house-bill.component.scss']
})
export class CreateHouseBillComponent extends AppForm {
    @ViewChild(FormAddHouseBillComponent, { static: false }) formHouseBill: FormAddHouseBillComponent;
    constructor(
        private _progressService: NgProgress,
        private _fb: FormBuilder,
        private _documentationRepo: DocumentationRepo,
        private _toastService: ToastrService,

    ) {
        super();
        this._progressRef = this._progressService.ref();
    }
    ngOnInit() {
    }


    create() {
        this.formHouseBill.isSubmited = true;
        if (this.formHouseBill.selectedPortOfLoading.data.id === this.formHouseBill.selectedPortOfDischarge.data.id) {
            this.formHouseBill.PortChargeLikePortLoading = true;
        } else {
            this.formHouseBill.PortChargeLikePortLoading = false;
        }

        if (this.formHouseBill.formGroup.valid && this.formHouseBill.selectedPortOfLoading) {
            const body: ITransactionDetail = {
                jobId: "6A42E788-663A-409D-8F64-7A92582E1679",
                mawb: this.formHouseBill.mtBill.value,
                saleManId: this.formHouseBill.selectedSaleman.data.saleMan_ID,
                shipperId: this.formHouseBill.selectedShipper.data.id,
                shipperDescription: this.formHouseBill.shipperdescriptionModel,
                consigneeId: this.formHouseBill.selectedConsignee.data.id,
                consigneeDescription: this.formHouseBill.consigneedescriptionModel,
                notifyPartyId: this.formHouseBill.selectedNotifyParty.data.id,
                notifyPartyDescription: this.formHouseBill.notifyPartyModel,
                alsoNotifyPartyId: this.formHouseBill.selectedAlsoNotifyParty.data.id,
                alsoNotifyPartyDescription: this.formHouseBill.alsoNotifyPartyDescriptionModel,
                hwbno: this.formHouseBill.hwbno.value,
                hbltype: this.formHouseBill.hbltype.value.value,
                etd: formatDate(this.formHouseBill.etd.value.startDate, 'yyyy-MM-dd', 'en'),
                eta: formatDate(this.formHouseBill.eta.value.startDate, 'yyyy-MM-dd', 'en'),
                pickupPlace: this.formHouseBill.pickupPlace.value,
                pol: this.formHouseBill.selectedPortOfLoading.data.id,
                pod: this.formHouseBill.selectedPortOfDischarge.data.id,
                finalDestinationPlace: this.formHouseBill.finalDestinationPlace.value,
                coloaderId: this.formHouseBill.selectedSupplier.data.id,
                localVessel: this.formHouseBill.localVessel.value,
                localVoyNo: this.formHouseBill.localVoyNo.value,
                oceanVessel: this.formHouseBill.oceanVessel.value,
                documentDate: formatDate(this.formHouseBill.documentDate.value.startDate, 'yyyy-MM-dd', 'en'),
                documentNo: this.formHouseBill.documentNo.value,
                etawarehouse: formatDate(this.formHouseBill.etawarehouse.value.startDate, 'yyyy-MM-dd', 'en'),
                warehouseNotice: this.formHouseBill.warehouseNotice.value,
                shippingMark: this.formHouseBill.shippingMark.value,
                remark: this.formHouseBill.remark.value,
                issueHBLPlace: this.formHouseBill.selectedPlaceOfIssued.data.id,
                issueHBLDate: formatDate(this.formHouseBill.issueHBLDate.value.startDate, 'yyyy-MM-dd', 'en'),
                originBLNumber: this.formHouseBill.originBLNumber.value.value,
                referenceNo: this.formHouseBill.referenceNo.value,
                customerId: this.formHouseBill.selectedCustomer.value
            }
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
}
