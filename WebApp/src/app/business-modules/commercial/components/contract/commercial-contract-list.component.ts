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
import { SystemConstants } from '@constants';

@Component({
    selector: 'commercial-contract-list',
    templateUrl: './commercial-contract-list.component.html',
})
export class CommercialContractListComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(FormContractCommercialPopupComponent, { static: false }) formContractPopup: FormContractCommercialPopupComponent;
    @Input() partnerId: string;
    contracts: Contract[] = [];
    selectedContract: Contract = new Contract();
    indexToRemove: number = 0;
    indexlstContract: number = null;
    contract: Contract = new Contract();

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
        this.indexlstContract = null;
        if (!this.partnerId) {
            this.formContractPopup.isCreateNewCommercial = true;
        }
        this.formContractPopup.show();
    }

    getDetailContract(id: string, index: number) {
        this.formContractPopup.isUpdate = true;
        this.formContractPopup.partnerId = this.partnerId;
        this.formContractPopup.selectedContract.id = id;
        this.indexlstContract = index;
        if (this.formContractPopup.selectedContract.id !== SystemConstants.EMPTY_GUID) {
            this.formContractPopup.getFileContract();
            this._catalogueRepo.getDetailContract(this.formContractPopup.selectedContract.id)
                .subscribe(
                    (res: Contract) => {
                        this.selectedContract = res;
                        this.formContractPopup.idContract = this.selectedContract.id;
                        this.formContractPopup.selectedContract = res;
                        this.pachValueToFormContract();
                        this.formContractPopup.show();
                    }
                );
        } else {
            if (this.contracts.length > 0) {
                this.formContractPopup.selectedContract = this.contracts[this.indexlstContract];
                this.formContractPopup.indexDetailContract = this.indexlstContract;
                this.formContractPopup.fileList = this.formContractPopup.selectedContract.fileList;
            }
            this.pachValueToFormContract();
            this.formContractPopup.show();
        }
    }

    pachValueToFormContract() {
        this.formContractPopup.formGroup.patchValue({
            salesmanId: !!this.formContractPopup.selectedContract.saleManId ? this.formContractPopup.selectedContract.saleManId : null,
            companyId: !!this.formContractPopup.selectedContract.companyId ? this.formContractPopup.selectedContract.companyId : null,
            officeId: !!this.formContractPopup.selectedContract.officeId ? this.formContractPopup.selectedContract.officeId : null,
            contractNo: this.formContractPopup.selectedContract.contractNo,
            effectiveDate: !!this.formContractPopup.selectedContract.effectiveDate ? { startDate: new Date(this.formContractPopup.selectedContract.effectiveDate), endDate: new Date(this.formContractPopup.selectedContract.effectiveDate) } : null,
            expiredDate: !!this.formContractPopup.selectedContract.expiredDate ? { startDate: new Date(this.formContractPopup.selectedContract.expiredDate), endDate: new Date(this.formContractPopup.selectedContract.expiredDate) } : null,
            contractType: !!this.formContractPopup.selectedContract.contractType ? [this.formContractPopup.contractTypes.find(type => type.id === this.formContractPopup.selectedContract.contractType)] : null,
            saleService: !!this.formContractPopup.selectedContract.saleService ? [this.formContractPopup.serviceTypes.find(type => type.id === this.formContractPopup.selectedContract.saleService)] : null,
            vas: !!this.formContractPopup.selectedContract.vas ? [this.formContractPopup.vaslst.find(type => type.id === this.formContractPopup.selectedContract.vas)] : null,
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
            paymentMethod: !!this.formContractPopup.selectedContract.paymentMethod ? [this.formContractPopup.paymentMethods.find(type => type.id === this.formContractPopup.selectedContract.paymentMethod)] : null
        });
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
        this.selectedContract = contract;
        this.indexToRemove = index;
        if (this.selectedContract.id === SystemConstants.EMPTY_GUID) {
            this.contracts = [...this.contracts.slice(0, index), ...this.contracts.slice(index + 1)];
        } else {
            this.confirmDeletePopup.show();
        }
    }

    onDelete() {
        this.confirmDeletePopup.hide();
        this._catalogueRepo.deleteContract(this.selectedContract.id, this.partnerId)
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
        this.selectedContract = contract;
        this._router.navigate([`/home/commercial/customer/${this.partnerId}/contract/${this.selectedContract.id}`]);
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
        this.contract = $event;
        this.selectedContract = new Contract(this.contract);
        if (!!this.selectedContract && !this.formContractPopup.isCreateNewCommercial) {
            this.getListContract(this.partnerId);
        } else {
            console.log(this.selectedContract);
            const objCheckContract = !!this.selectedContract.contractNo && this.contracts.length >= 1 ? this.contracts.some(x => x.contractNo === this.selectedContract.contractNo) : null;
            if (this.indexlstContract !== null) {
                this.contracts[this.indexlstContract] = this.selectedContract;
                this.formContractPopup.hide();
            } else {
                if (objCheckContract && objCheckContract != null) {
                    this.formContractPopup.isDuplicateContract = true;
                    this._toastService.error('Contract no has been existed!');
                } else {
                    this.formContractPopup.isDuplicateContract = false;
                    this.contracts.push(this.selectedContract);
                }
            }
        }
        this.formContractPopup.contracts = this.contracts;
    }


}
