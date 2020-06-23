import { Component, OnInit, ViewChild } from '@angular/core';
import { CommercialCreateContractComponent } from '../create-contract/create-contract.component';
import { ToastrService } from 'ngx-toastr';
import { CatalogueRepo } from '@repositories';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { FormContractCommercialCatalogueComponent } from 'src/app/business-modules/share-commercial-catalogue/components/form-contract-commerical-catalogue.component';
import { catchError, finalize } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { Store } from '@ngrx/store';
import { IAppState, getMenuUserSpecialPermissionState } from '@store';
import { Observable } from 'rxjs';

@Component({
    selector: 'commercial-detail-contract',
    templateUrl: 'detail-contract.component.html'
})

export class CommercialDetailContractComponent extends CommercialCreateContractComponent implements OnInit {

    @ViewChild(FormContractCommercialCatalogueComponent, { static: false }) formContract: FormContractCommercialCatalogueComponent;

    contract: Contract = new Contract();

    menuSpecialPermission: Observable<any[]>;

    constructor(protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _activedRoute: ActivatedRoute,
        private _ngProgressService: NgProgress,
        protected _router: Router,
        private _store: Store<IAppState>
    ) {
        super(_catalogueRepo, _toastService, _activedRoute, _router)
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this._activedRoute.params.subscribe((params: Params) => {
            if (params.partnerId) {
                this.partnerId = params.partnerId;
                this.contract.id = params.contractId;
            }

        });
        this.getDetailContract(this.contract.id);
    }
    ngAfterViewInit() {
        this.formContract.partnerId = this.partnerId;
        this.formContract.selectedContract.id = this.contract.id;
        this.getFileContract();
    }

    getDetailContract(id: string) {
        this._catalogueRepo.getDetailContract(id)
            .subscribe(
                (res: Contract) => {
                    this.contract = res;
                    this.formContract.selectedContract = res;
                    this.formContract.formGroup.patchValue({
                        salesmanId: this.formContract.selectedContract.saleManId,
                        companyId: this.formContract.selectedContract.companyId,
                        officeId: this.formContract.selectedContract.officeId,
                        contractNo: this.formContract.selectedContract.contractNo,
                        effectiveDate: !!this.formContract.selectedContract.effectiveDate ? { startDate: new Date(this.formContract.selectedContract.effectiveDate), endDate: new Date(this.formContract.selectedContract.effectiveDate) } : null,
                        expiredDate: !!this.formContract.selectedContract.expiredDate ? { startDate: new Date(this.formContract.selectedContract.expiredDate), endDate: new Date(this.formContract.selectedContract.expiredDate) } : null,
                        contractType: [this.formContract.contractTypes.find(type => type.id === this.formContract.selectedContract.contractType)],
                        saleService: [this.formContract.serviceTypes.find(type => type.id === this.formContract.selectedContract.saleService)],
                        vas: [this.formContract.vaslst.find(type => type.id === this.formContract.selectedContract.vas)],
                        paymentTerm: this.formContract.selectedContract.paymentTerm,
                        creditLimit: this.formContract.selectedContract.creditLimit,
                        creditLimitRate: this.formContract.selectedContract.creditLimitRate,
                        trialCreditLimit: this.formContract.selectedContract.trialCreditLimited,
                        trialCreditDays: this.formContract.selectedContract.trialCreditDays,
                        trialEffectDate: !!this.formContract.selectedContract.trialEffectDate ? { startDate: new Date(this.formContract.selectedContract.trialEffectDate), endDate: new Date(this.formContract.selectedContract.trialEffectDate) } : null,
                        trialExpiredDate: !!this.formContract.selectedContract.trialExpiredDate ? { startDate: new Date(this.formContract.selectedContract.trialExpiredDate), endDate: new Date(this.formContract.selectedContract.trialExpiredDate) } : null,
                        creditAmount: this.formContract.selectedContract.creditAmount,
                        billingAmount: this.formContract.selectedContract.billingAmount,
                        paidAmount: this.formContract.selectedContract.paidAmount,
                        unpaidAmount: this.formContract.selectedContract.unpaidAmount,
                        customerAmount: this.formContract.selectedContract.customerAdvanceAmount,
                        creditRate: this.formContract.selectedContract.creditRate,
                        description: this.formContract.selectedContract.description,
                        paymentMethod: [this.formContract.paymentMethods.find(type => type.id === this.formContract.selectedContract.paymentMethod)]
                    });

                }
            );
    }

    getFileContract() {
        this.isLoading = true;
        this._catalogueRepo.getContractFilesAttach(this.partnerId, this.contract.id).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: any = []) => {
                    this.formContract.files = res;
                    // this.files.forEach(f => f.extension = f.name.split("/").pop().split('.').pop());
                    console.log(this.formContract.files);
                }
            );
    }

    activeInactiveContract(id: string) {
        this._catalogueRepo.activeInactiveContract(id)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.formContract.selectedContract.active = !this.formContract.selectedContract.active;
                        this._toastService.success(res.message);

                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

}

