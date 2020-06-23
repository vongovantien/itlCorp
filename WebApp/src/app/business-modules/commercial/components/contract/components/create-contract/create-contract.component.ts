import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormContractCommercialCatalogueComponent } from 'src/app/business-modules/share-commercial-catalogue/components/form-contract-commerical-catalogue.component';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { formatDate } from '@angular/common';

@Component({
    selector: 'commercial-create-contract',
    templateUrl: 'create-contract.component.html'
})

export class CommercialCreateContractComponent extends AppForm implements OnInit {

    @ViewChild(FormContractCommercialCatalogueComponent, { static: false }) formContract: FormContractCommercialCatalogueComponent;

    contract: Contract = new Contract();
    partnerId: string = '';
    constructor(protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _activedRoute: ActivatedRoute,
        protected _router: Router,

    ) {
        super();
    }
    ngOnInit() {
        this._activedRoute.params.subscribe((params: Params) => {
            if (params.partnerId) {
                this.partnerId = params.partnerId;
            }
        });
    }

    onSubmit() {
        this.setError(this.formContract.vas);
        this.setError(this.formContract.paymentMethod);
        this.formContract.isSubmitted = true;
        if (!!this.formContract.contractType.value && this.formContract.contractType.value.length > 0) {
            if (this.formContract.contractType.value[0].id === this.formContract.contractTypes[1].id) {
                this.formContract.isRequiredContractNo = true;
                return;
            } else {
                this.formContract.isRequiredContractNo = false;
            }
        }
        if (this.formContract.formGroup.valid) {
            this.asignValueToModel();
            console.log(this.contract);
            if (!this.formContract.isUpdate) {
                this._catalogueRepo.createContract(this.contract)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                if (!!this.formContract.fileList) {
                                    this.formContract.uploadFileContract(res.data);
                                } else {
                                    this.gotoList();
                                }
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            } else {
                this._catalogueRepo.updateContract(this.contract)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);

                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            }

        }
    }

    asignValueToModel() {
        this.contract.saleManId = this.formContract.salesmanId.value;
        this.contract.companyId = this.formContract.companyId.value;
        this.contract.officeId = this.formContract.officeId.value;
        this.contract.contractNo = this.formContract.formGroup.controls['contractNo'].value;
        this.contract.effectiveDate = this.formContract.effectiveDate.value ? (this.formContract.effectiveDate.value.startDate !== null ? formatDate(this.formContract.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null;
        this.contract.expiredDate = !!this.formContract.expiredDate.value && !!this.formContract.expiredDate.value.startDate ? formatDate(this.formContract.expiredDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.contract.contractType = !!this.formContract.contractType.value ? this.formContract.contractType.value[0].id : null;
        this.contract.saleService = !!this.formContract.saleService.value ? this.formContract.saleService.value[0].id : null;
        this.contract.paymentMethod = !!this.formContract.paymentMethod.value ? this.formContract.paymentMethod.value[0].id : null;
        this.contract.vas = !!this.formContract.vas.value ? this.formContract.vas.value[0].id : null;
        this.contract.trialCreditLimited = this.formContract.formGroup.controls['trialCreditLimit'].value;
        this.contract.trialCreditDays = this.formContract.formGroup.controls['trialCreditDays'].value;
        this.contract.trialEffectDate = !!this.formContract.trialEffectDate.value && !!this.formContract.trialEffectDate.value.startDate ? formatDate(this.formContract.trialEffectDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.contract.trialExpiredDate = !!this.formContract.trialExpiredDate.value && !!this.formContract.trialExpiredDate.value.startDate ? formatDate(this.formContract.trialExpiredDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        this.contract.paymentTerm = this.formContract.formGroup.controls['paymentTerm'].value;
        this.contract.creditLimit = this.formContract.formGroup.controls['creditLimit'].value;
        this.contract.creditLimitRate = this.formContract.formGroup.controls['creditLimitRate'].value;
        this.contract.creditAmount = this.formContract.formGroup.controls['creditAmount'].value;
        this.contract.billingAmount = this.formContract.formGroup.controls['billingAmount'].value;
        this.contract.paidAmount = this.formContract.formGroup.controls['paidAmount'].value;
        this.contract.unpaidAmount = this.formContract.formGroup.controls['unpaidAmount'].value;
        this.contract.customerAdvanceAmount = this.formContract.formGroup.controls['customerAmount'].value;
        this.contract.creditRate = this.formContract.formGroup.controls['creditRate'].value;
        this.contract.description = this.formContract.formGroup.controls['description'].value;
        this.contract.partnerId = this.partnerId;
    }

    gotoList() {
        this._router.navigate([`home/commercial/customer/${this.partnerId}`]);
    }

}