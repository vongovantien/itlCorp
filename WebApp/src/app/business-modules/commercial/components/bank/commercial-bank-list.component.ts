import { ToastrService } from 'ngx-toastr';
import { Component, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from '@common';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo } from '@repositories';
import { SortService } from '@services';
import { catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { FormBankCommercialCatalogueComponent } from '../../../share-modules/components/form-bank-commercial-catalogue/form-bank-commercial-catalogue.component';
import { Bank } from './../../../../shared/models/catalogue/catBank.model';

@Component({
    selector: 'app-commercial-bank-list',
    templateUrl: './commercial-bank-list.component.html',
})
export class CommercialBankListComponent extends AppList {
    @ViewChild(ConfirmPopupComponent) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(FormBankCommercialCatalogueComponent) formUpdateBankPopup: FormBankCommercialCatalogueComponent;

    partnerBanks: Bank[] = [];
    partnerId: string = '';
    isUpdate: Boolean = false;
    id: string = '';
    indexLstBank: number = null;

    constructor(
        private _ngProgressService: NgProgress,
        private _sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Bank Account No', field: 'bankAccountNo', sortable: true },
            { title: 'Bank Account Name', field: 'bankAccountName', sortable: true },
            { title: 'Bank Address', field: 'bankAddress', sortable: true },
            { title: 'Swift Code', field: '', sortable: false },
            { title: 'Bank Name', field: '', sortable: false },
            { title: 'Bank Code', field: '', sortable: false },
            { title: 'Source', field: '', sortable: false },
            { title: 'Note', field: '', sortable: false },
        ];
    }

    getListBank(partnerId: string) {
        this.isLoading = true;
        this._catalogueRepo.getListBankByPartnerById(partnerId)
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoading = false;
            })).subscribe(
                (res: Bank[]) => {
                    console.log(res)
                    this.partnerBanks = res || [];
                }
            );
    }

    showPopupUpdateBank() {
        this.formUpdateBankPopup.isUpdate = false;
        console.log(this.formUpdateBankPopup.isUpdate)
        this.formUpdateBankPopup.partnerId = this.partnerId;
        if (!this.formUpdateBankPopup.isUpdate) {
            this.formUpdateBankPopup.formGroup.reset();
        }
        this.formUpdateBankPopup.show();
    }

    gotoDetailBank(id: string, index: number = null) {
        this.formUpdateBankPopup.isUpdate = true;
        this.formUpdateBankPopup.partnerId = this.partnerId;
        !!this.formUpdateBankPopup.partnerId ? this.indexLstBank = null : this.indexLstBank = index;
        if (!!this.formUpdateBankPopup.partnerId) {
            this._catalogueRepo.getDetailBankById(id)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: Bank) => {
                        if (!!res) {
                            this.formUpdateBankPopup.updateFormValue(res);
                            this.formUpdateBankPopup.show();
                        }
                    }
                );
        }
    }

    sortLocal(sort: string): void {
        this.partnerBanks = this._sortService.sort(this.partnerBanks, sort, this.order);
    }

    showConfirmDelete(id: string, index: number) {
        this.id = id;
        if (!!this.id) {
            this.confirmDeletePopup.show();
        } else {
            this.partnerBanks = [...this.partnerBanks.slice(0, index), ...this.partnerBanks.slice(index + 1)];
        }
    }

    onDelete() {
        this.confirmDeletePopup.hide();
        this._catalogueRepo.deleteBank(this.id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.getListBank(this.partnerId);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onRequestBank($event: any) {
        const data = $event;
        if (data === true) {
            this.formUpdateBankPopup.hide();
            this.getListBank(this.partnerId);
        }
    }
}
