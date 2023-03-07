import { Component, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { Bank, Partner } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo } from '@repositories';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { FormBankCommercialCatalogueComponent } from 'src/app/business-modules/share-modules/components/form-bank-commercial-catalogue/form-bank-commercial-catalogue.component';
@Component({
    selector: 'app-commercial-bank-list',
    templateUrl: './commercial-bank-list.component.html',
})

export class CommercialBankListComponent extends AppList {

    @ViewChild(FormBankCommercialCatalogueComponent) formUpdateBankPopup: FormBankCommercialCatalogueComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    partnerBanks: Bank[] = [];
    partnerId: string = '';
    partner: Partner;
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
                    this.partnerBanks = res || [];

                }
            );
    }

    showPopupUpdateBank() {
        this.formUpdateBankPopup.isUpdate = false;
        this.formUpdateBankPopup.partnerId = this.partnerId;
        if (!this.formUpdateBankPopup.isUpdate) {
            this.formUpdateBankPopup.formGroup.reset();
            this.formUpdateBankPopup.beneficiaryAddress.setValue(this.partner.addressShippingVn);
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
                            this.formUpdateBankPopup.id = res.id
                            this.formUpdateBankPopup.show();
                        }
                    }
                );
        }
    }

    sortLocal(sort: string): void {
        this.partnerBanks = this._sortService.sort(this.partnerBanks, sort, this.order);
    }


    onDeleteBank(id: string) {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Confirm',
            body: 'Do you want to delete this bank account',
            labelConfirm: 'Ok'
        }, () => { this.handleDeleteBank(id) });
    }


    handleDeleteBank(id: string) {
        this._catalogueRepo.deleteBank(id)
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
            this.getListBank(this.partnerId);
        }
    }
}
