import { Component, OnInit, ViewChild, Input, ChangeDetectorRef } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { catchError, finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { Contract } from 'src/app/shared/models/catalogue/catContract.model';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { ConfirmPopupComponent } from '@common';
import { FormContractCommercialPopupComponent } from 'src/app/business-modules/share-commercial-catalogue/components/form-contract-commercial-catalogue.popup';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'commercial-contract-list',
    templateUrl: './commercial-contract-list.component.html',
})
export class CommercialContractListComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(FormContractCommercialPopupComponent, { static: false }) formContractPopup: FormContractCommercialPopupComponent;
    @Input() partnerId: string;
    contracts: Contract[] = [];
    selectecContract: Contract;
    indexToRemove: number = 0;

    constructor(private _router: Router,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Salesman', field: 'username', sortable: true },
            { title: 'Contract No', field: 'username', sortable: true },
            { title: 'Contract Type', field: 'username', sortable: true },
            { title: 'Service', field: 'username', sortable: true },
            { title: 'Effective Date', field: 'username', sortable: true },
            { title: 'Expired Date', field: 'username', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'Office', field: 'officeName', sortable: true },
            { title: 'Company', field: 'companyName', sortable: true },
        ];
    }

    gotoCreateContract() {
        this.formContractPopup.formGroup.patchValue({
            salesmanId: null,
            officeId: null,
            contractNo: null,
            effectiveDate: null,
            expiredDate: null,
            paymentTerm: null,
            creditLimit: null,
            creditLimitRate: null,
            trialCreditLimit: null,
            trialCreditDays: null,
            trialEffectDate: null,
            trialExpiredDate: null,
            creditAmount: null,
            billingAmount: null,
            paidAmount: null,
            unpaidAmount: null,
            customerAmount: null,
            creditRate: null,
            description: null,
            vas: null,
            saleService: null
        });
        this.formContractPopup.files = null;
        this.formContractPopup.fileList = null;
        this.formContractPopup.isUpdate = false;
        this.formContractPopup.isSubmitted = false;
        if (!this.partnerId) {
            this.formContractPopup.isCreateNewCommercial = true;
        }
        this.formContractPopup.show();
    }

    getDetailContract(id: string) {
        this.formContractPopup.isUpdate = true;
        this.formContractPopup.partnerId = this.partnerId;
        this.formContractPopup.selectedContract.id = id;
        this.formContractPopup.getFileContract();
        this._catalogueRepo.getDetailContract(id)
            .subscribe(
                (res: Contract) => {
                    this.selectecContract = res;

                    this.formContractPopup.selectedContract = res;
                    this.formContractPopup.formGroup.patchValue({
                        salesmanId: this.formContractPopup.selectedContract.saleManId,
                        companyId: this.formContractPopup.selectedContract.companyId,
                        officeId: this.formContractPopup.selectedContract.officeId,
                        contractNo: this.formContractPopup.selectedContract.contractNo,
                        effectiveDate: !!this.formContractPopup.selectedContract.effectiveDate ? { startDate: new Date(this.formContractPopup.selectedContract.effectiveDate), endDate: new Date(this.formContractPopup.selectedContract.effectiveDate) } : null,
                        expiredDate: !!this.formContractPopup.selectedContract.expiredDate ? { startDate: new Date(this.formContractPopup.selectedContract.expiredDate), endDate: new Date(this.formContractPopup.selectedContract.expiredDate) } : null,
                        contractType: [this.formContractPopup.contractTypes.find(type => type.id === this.formContractPopup.selectedContract.contractType)],
                        saleService: [this.formContractPopup.serviceTypes.find(type => type.id === this.formContractPopup.selectedContract.saleService)],
                        vas: [this.formContractPopup.vaslst.find(type => type.id === this.formContractPopup.selectedContract.vas)],
                        paymentTerm: this.formContractPopup.selectedContract.paymentTerm,
                        creditLimit: this.formContractPopup.selectedContract.creditLimit,
                        creditLimitRate: this.formContractPopup.selectedContract.creditLimitRate,
                        trialCreditLimit: this.formContractPopup.selectedContract.trialCreditLimited,
                        trialCreditDays: this.formContractPopup.selectedContract.trialCreditDays,
                        trialEffectDate: !!this.formContractPopup.selectedContract.trialEffectDate ? { startDate: new Date(this.formContractPopup.selectedContract.trialEffectDate), endDate: new Date(this.formContractPopup.selectedContract.trialEffectDate) } : null,
                        trialExpiredDate: !!this.formContractPopup.selectedContract.trialExpiredDate ? { startDate: new Date(this.formContractPopup.selectedContract.trialExpiredDate), endDate: new Date(this.formContractPopup.selectedContract.trialExpiredDate) } : null,
                        creditAmount: this.formContractPopup.selectedContract.creditAmount,
                        billingAmount: this.formContractPopup.selectedContract.billingAmount,
                        paidAmount: this.formContractPopup.selectedContract.paidAmount,
                        unpaidAmount: this.formContractPopup.selectedContract.unpaidAmount,
                        customerAmount: this.formContractPopup.selectedContract.customerAdvanceAmount,
                        creditRate: this.formContractPopup.selectedContract.creditRate,
                        description: this.formContractPopup.selectedContract.description,
                        paymentMethod: [this.formContractPopup.paymentMethods.find(type => type.id === this.formContractPopup.selectedContract.paymentMethod)]
                    });
                    this.formContractPopup.show();
                }
            );
    }

    getFileContract() {
        this.formContractPopup.isLoading = true;
        this._catalogueRepo.getContractFilesAttach(this.partnerId, this.formContractPopup.selectedContract.id).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.formContractPopup.isLoading = false;
            }))
            .subscribe(
                (res: any = []) => {
                    this.formContractPopup.files = res;
                    console.log(this.formContractPopup.files);
                }
            );
    }

    showConfirmDelete(contract: Contract, index: number) {
        this.selectecContract = contract;
        this.indexToRemove = index;
        this.confirmDeletePopup.show();
    }

    onDelete() {
        this.confirmDeletePopup.hide();
        this._catalogueRepo.deleteContract(this.selectecContract.id, this.partnerId)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.contracts.splice(this.indexToRemove, 1);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    showDetail(contract: Contract) {
        this.selectecContract = contract;
        this._router.navigate([`/home/commercial/customer/${this.partnerId}/contract/${this.selectecContract.id}`]);
    }

    getListContract(partneId: string) {
        this.isLoading = true;
        this._catalogueRepo.getListContract(partneId)
            .pipe(
                finalize(() => this.isLoading = false)
            )
            .subscribe(
                (res: any[]) => {
                    this.contracts = res || [];
                }
            );
    }

    onRequestContract($event: any) {
        console.log($event);
        if (!!$event && !this.formContractPopup.isCreateNewCommercial) {
            this.getListContract(this.partnerId);
        } else {
            this.contracts.push($event);
        }
    }


}
