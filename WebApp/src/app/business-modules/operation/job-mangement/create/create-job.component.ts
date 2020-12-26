import { Component, ViewChild } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { Router } from "@angular/router";
import { formatDate } from "@angular/common";
import { NgxSpinnerService } from "ngx-spinner";
import { InfoPopupComponent } from "@common";

import { OpsTransaction } from "@models";
import { AppForm } from "@app";
import { DocumentationRepo } from "@repositories";
import { RoutingConstants } from "@constants";
import { JobManagementFormCreateComponent } from "../components/form-create/form-create-job.component";

import { takeUntil, catchError, finalize } from "rxjs/operators";
import _merge from 'lodash/merge';


@Component({
    selector: "app-job-mangement-create",
    templateUrl: "./create-job.component.html",
})
export class JobManagementCreateJobComponent extends AppForm {

    @ViewChild(JobManagementFormCreateComponent) formCreateComponent: JobManagementFormCreateComponent;
    @ViewChild(InfoPopupComponent) infoPoup: InfoPopupComponent;

    isSaveLink: boolean = false;

    constructor(
        private spinner: NgxSpinnerService,
        private _toaster: ToastrService,
        private _documentRepo: DocumentationRepo,
        private _router: Router
    ) {
        super();
    }

    ngOnInit() {
    }

    onSubmitData() {
        const form: any = this.formCreateComponent.formCreate.getRawValue();
        const formData = {
            serviceDate: !!form.serviceDate && !!form.serviceDate.startDate ? formatDate(form.serviceDate.startDate, 'yyyy-MM-dd', 'en') : null,
            commodityGroupId: form.commodity,
        };
        const opsTransaction: OpsTransaction = new OpsTransaction(Object.assign(_merge(form, formData)));
        opsTransaction.salemanId = form.salemansId;

        if (form.shipmentMode === 'Internal' && (form.productService.indexOf('Sea') > -1 || form.productService === 'Air')) {
            this.isSaveLink = true;
        }
        return opsTransaction;
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formCreateComponent.formCreate.valid || (!!this.formCreateComponent.serviceDate.value && !this.formCreateComponent.serviceDate.value.startDate)) {
            valid = false;
        }
        return valid;
    }

    onSave() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.infoPoup.show();
            return;
        }

        const modelAdd: OpsTransaction = this.onSubmitData();
        this.saveJob(modelAdd);
    }

    saveJob(model: OpsTransaction) {
        if (this.isSaveLink) {
            this._documentRepo.getASTransactionInfo(model.hwbno, model.mblno, model.productService, model.serviceMode)
                .subscribe((res: any) => {
                    if (!!res) {
                        model.serviceNo = res.jobNo === '' ? null : res.jobNo;
                        model.serviceHblId = res.jobNo === '' ? null : res.id;
                        this._documentRepo.addOPSJob(model).pipe(
                            takeUntil(this.ngUnsubscribe),
                            catchError(this.catchError),
                            finalize(() => { this.spinner.hide(); })
                        ).subscribe(
                            (res: CommonInterface.IResult) => {
                                if (!res.status) {
                                    this._toaster.error(res.message);
                                } else {
                                    this._toaster.success(res.message);
                                    this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_DETAIL}/${res.data}`]);
                                }
                            }
                        );
                    }
                });
        } else {
            this._documentRepo.addOPSJob(model).pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError),
                finalize(() => { this.spinner.hide(); })
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (!res.status) {
                        this._toaster.error(res.message);
                    } else {
                        this._toaster.success(res.message);
                        this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_DETAIL}/${res.data}`]);
                    }
                }
            );
        }
    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.LOGISTICS.JOB_MANAGEMENT}`]);
    }
}
