import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormAddHouseBillComponent } from '../components/form-add-house-bill/form-add-house-bill.component';
import { ITransactionDetail } from '../create/create-house-bill.component';
import { ActivatedRoute, Params } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, finalize, tap } from 'rxjs/operators';


@Component({
    selector: 'app-detail-house-bill',
    templateUrl: './detail-house-bill.component.html',
    styleUrls: ['./detail-house-bill.component.scss']
})
export class DetailHouseBillComponent extends AppForm {
    @ViewChild(FormAddHouseBillComponent, { static: false }) formHouseBill: FormAddHouseBillComponent;
    hblId: string;
    formData: ITransactionDetail = {
        jobId: '',
        mawb: '',
        saleManId: '',
        shipperId: '',
        shipperDescription: '',
        consigneeId: '',
        consigneeDescription: '',
        notifyPartyId: '',
        notifyPartyDescription: '',
        alsoNotifyPartyId: '',
        alsoNotifyPartyDescription: '',
        hwbno: '',
        hbltype: '',
        etd: '',
        eta: '',
        pickupPlace: '',
        pol: '',
        pod: '',
        finalDestinationPlace: '',
        coloaderId: '',
        localVessel: '',
        localVoyNo: '',
        oceanVessel: '',
        documentDate: '',
        documentNo: '',
        etawarehouse: '',
        warehouseNotice: '',
        shippingMark: '',
        remark: '',
        issueHBLPlace: '',
        issueHBLDate: '',
        originBLNumber: 0,
        referenceNo: '',
        customerId: '',
        oceanVoyNo: '',
        csMawbcontainers: []
    };
    constructor(
        private _activedRouter: ActivatedRoute,
        private _progressService: NgProgress,
        private _documentationRepo: DocumentationRepo
    ) {
        super();
        this._progressRef = this._progressService.ref();

    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: Params) => {
            if (param.id) {
                this.hblId = param.id;
                this.getDetailHbl(this.hblId);
            } else {

            }
        });
    }



    getDetailHbl(id: any) {
        this._progressRef.start();
        this._documentationRepo.getDetailHbl(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                tap(
                    (res: CommonInterface.IResult) => {
                        if (res) {
                            console.log(res);
                            this.formHouseBill.etd.setValue(res.data.eta);
                            this.formHouseBill.getListSaleman(res.data.customerId);
                            this.formHouseBill.mtBill.setValue(res.data.mawb);
                            this.formHouseBill.shipperdescriptionModel = res.data.shipperDescription;
                            this.formHouseBill.consigneeDescription.setValue(res.data.consigneeDescription);
                            this.formHouseBill.hwbno.setValue(res.data.hwbno);
                            this.formHouseBill.pickupPlace.setValue(res.data.pickupPlace);
                            this.formHouseBill.eta.setValue(res.data.eta);
                            this.formHouseBill.finalDestinationPlace.setValue(res.data.finalDestinationPlace);

                            this.formHouseBill.selectedShipper = { field: 'shortName', value: res.data.shipperId };

                            this.formHouseBill.hbltype.setValue(this.formHouseBill.hbOfladingTypes.filter(i => i.value === res.data.hbltype)[0]);
                            this.formHouseBill.localVessel.setValue(res.data.localVessel);
                            this.formHouseBill.localVoyNo.setValue(res.data.localVoyNo);
                            this.formHouseBill.oceanVessel.setValue(res.data.oceanVessel);
                            this.formHouseBill.oceanVoyNo.setValue(res.data.oceanVoyNo);
                            this.formHouseBill.documentDate.setValue(res.data.documentDate);
                            this.formHouseBill.documentNo.setValue(res.data.documentNo);
                            this.formHouseBill.etawarehouse.setValue(res.data.etaWarehouse);
                            this.formHouseBill.warehouseNotice.setValue(res.data.warehouseNotice);
                            this.formHouseBill.shippingMark.setValue(res.data.shippingMark);
                            this.formHouseBill.remark.setValue(res.data.remark);
                            this.formHouseBill.issueHBLDate.setValue(res.data.issueHbldate);

                            this.formHouseBill.originBLNumber.setValue(this.formHouseBill.numberOfOrigins.filter(i => i.value === res.data.originBlNumber)[0]);







                            setTimeout(() => {

                                this.formHouseBill.selectedCustomer = { field: 'id', value: res.data.customerId };
                                this.formHouseBill.selectedSaleman = { field: 'id', value: res.data.saleManId };
                                this.formHouseBill.selectedShipper = { field: 'id', value: res.data.shipperId };
                                this.formHouseBill.selectedConsignee = { field: 'id', value: res.data.consigneeId };
                                this.formHouseBill.selectedNotifyParty = { field: 'id', value: res.data.notifyPartyId };
                                this.formHouseBill.selectedAlsoNotifyParty = { field: 'id', value: res.data.alsoNotifyPartyId };
                                this.formHouseBill.selectedPortOfLoading = { field: 'id', value: res.data.pol };
                                this.formHouseBill.selectedPortOfDischarge = { field: 'id', value: res.data.pod };
                                this.formHouseBill.selectedSupplier = { field: 'id', value: res.data.supplierId };
                                this.formHouseBill.selectedPlaceOfIssued = { field: 'id', value: res.data.IssueHBLPlace };


                                this.formHouseBill.update(this.formData);
                            }, 500);

                        }
                    })
            )

            .subscribe(
                (res: any) => {
                    // if (res) {
                    //     console.log(res);
                    //     this.formData.mawb = res.mawb;
                    //     this.formHouseBill.mtBill.setValue(res.mawb);
                    //     this.formHouseBill.selectedCustomer = { field: 'shortName', value: res.customerId };
                    //     this.formHouseBill.selectedSaleman = { field: 'saleMan_ID', value: res.saleManId };
                    //     this.formHouseBill.selectedShipper = { field: 'shortName', value: res.shipperId };
                    //     this.formHouseBill.formGroup.patchValue(this.formData);
                    // }
                },
            );
    }

}
