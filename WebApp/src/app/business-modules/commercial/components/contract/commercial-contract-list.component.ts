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
import { SortService } from '@services';

@Component({
    selector: 'commercial-contract-list',
    templateUrl: './commercial-contract-list.component.html',
})
export class CommercialContractListComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(FormContractCommercialPopupComponent, { static: false }) formContractPopup: FormContractCommercialPopupComponent;
    @Input() partnerId: string;
    contracts: Contract[] = [];
    //
    isActiveNewContract: boolean = true;
    //
    selectedContract: Contract = new Contract();
    indexToRemove: number = 0;
    indexlstContract: number = null;
    contract: Contract = new Contract();

    constructor(private _router: Router,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _sortService: SortService,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
        this.requestSort = this.sortLocal;

    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Salesman', field: 'username', sortable: true },
            { title: 'Contract No', field: 'contractNo', sortable: true },
            { title: 'Contract Type', field: 'contractType', sortable: true },
            { title: 'Service', field: 'saleService', sortable: true },
            { title: 'Effective Date', field: 'trialEffectDate', sortable: true },
            { title: 'Expired Date', field: 'trialExpiredDate', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
            { title: 'Office', field: 'officeNameAbbr', sortable: true },
            { title: 'Company', field: 'companyNameAbbr', sortable: true },
        ];
    }

    sortLocal(sort: string): void {
        this.contracts = this._sortService.sort(this.contracts, sort, this.order);
    }

    gotoCreateContract() {
        this.formContractPopup.formGroup.patchValue({
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
            saleService: null,
        });
        this.formContractPopup.files = null;
        this.formContractPopup.fileList = null;
        this.formContractPopup.isUpdate = false;
        this.formContractPopup.isSubmitted = false;
        this.formContractPopup.partnerId = this.partnerId;
        this.indexlstContract = null;
        if (!this.partnerId) {
            this.formContractPopup.isCreateNewCommercial = true;
        }
        this.formContractPopup.selectedContract = new Contract();
        const userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
        this.formContractPopup.salesmanId.setValue(userLogged.id);
        this.formContractPopup.formGroup.controls['paymentTerm'].setValue(30);
        this.formContractPopup.formGroup.controls['creditLimitRate'].setValue(120);
        this.formContractPopup.contractType.setValue([<CommonInterface.INg2Select>{ id: 'Trial', text: 'Trial' }]);
        this.formContractPopup.trialEffectDate.setValue(null);
        this.formContractPopup.trialExpiredDate.setValue(null);

        this.formContractPopup.show();
    }

    handleSearchContract($event: any) {
        console.log("keyword: ", $event.target.value);
    }

    getDetailContract(id: string, index: number) {
        this.formContractPopup.isUpdate = true;
        this.formContractPopup.partnerId = this.partnerId;
        this.formContractPopup.selectedContract.id = id;

        this.indexlstContract = index;
        if (this.formContractPopup.selectedContract.id !== SystemConstants.EMPTY_GUID && this.formContractPopup.selectedContract.id !== "") {
            this.formContractPopup.getFileContract();
            this._catalogueRepo.getDetailContract(this.formContractPopup.selectedContract.id)
                .subscribe(
                    (res: Contract) => {
                        this.selectedContract = res;
                        this.formContractPopup.idContract = this.selectedContract.id;
                        this.formContractPopup.selectedContract = res;
                        this.formContractPopup.statusContract = this.formContractPopup.selectedContract.active;
                        this.formContractPopup.pachValueToFormContract();
                        this.formContractPopup.show();
                    }
                );
        } else {
            if (this.contracts.length > 0) {
                this.formContractPopup.selectedContract = this.contracts[this.indexlstContract];
                this.formContractPopup.indexDetailContract = this.indexlstContract;
                this.formContractPopup.fileList = this.formContractPopup.selectedContract.fileList;
            }
            this.formContractPopup.pachValueToFormContract();
            this.formContractPopup.show();
        }
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
