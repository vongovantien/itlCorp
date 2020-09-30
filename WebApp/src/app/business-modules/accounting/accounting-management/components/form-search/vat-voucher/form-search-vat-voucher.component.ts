import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { AccountingConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';

import { AccAccountingManagementCriteria } from 'src/app/shared/models/accouting/accounting-management';
import { accountingManagementDataSearchState, IAccountingManagementState, SearchListAccountingMngt } from '../../../store';

@Component({
    selector: 'form-search-vat-voucher',
    templateUrl: './form-search-vat-voucher.component.html'
})

export class AccountingManagementFormSearchVatVoucherComponent extends AppForm implements OnInit {

    @Input() accountType: string = AccountingConstants.ISSUE_TYPE.INVOICE;
    @Output() onSearch: EventEmitter<AccAccountingManagementCriteria> = new EventEmitter<AccAccountingManagementCriteria>();

    partner: AbstractControl;
    creator: AbstractControl;
    referenceNo: AbstractControl;
    issueDate: AbstractControl;
    invoiceStatus: AbstractControl;
    voucherType: AbstractControl;

    partners: Observable<Partner[]>;
    creators: Observable<User[]>;
    formSearch: FormGroup;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    displayFieldsCreator: CommonInterface.IComboGridDisplayField[] = [
        { field: 'username', label: 'User Name' },
        { field: 'employeeNameVn', label: 'Full Name' }
    ];

    invoiceStatusList: CommonInterface.INg2Select[] = [
        { id: 'New', text: 'New' },
        { id: 'Updated Invoice', text: 'Updated Invoice' }
    ];

    voucherTypeList: CommonInterface.INg2Select[] = AccountingConstants.VOUCHER_TYPE;
    dataSearch: AccAccountingManagementCriteria;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _store: Store<IAccountingManagementState>
    ) {
        super();
    }

    ngOnInit() {
        this.initFormSearch();
        this.loadPartnerList();
        this.loadUserList();

        // * Update form search from store
        this._store.select(accountingManagementDataSearchState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (dataSearch: AccAccountingManagementCriteria) => {
                    console.log(dataSearch);
                    if (!!dataSearch && dataSearch.typeOfAcctManagement && dataSearch.typeOfAcctManagement === this.accountType) {
                        this.dataSearch = dataSearch;

                        let formData: any = {
                            partner: this.dataSearch.partnerId,
                            issueDate: !!this.dataSearch.fromIssuedDate && !!this.dataSearch.toIssuedDate ? {
                                startDate: new Date(this.dataSearch.fromIssuedDate), endDate: new Date(this.dataSearch.toIssuedDate)
                            } : null,
                            creator: this.dataSearch.creatorId,
                        };
                        if (this.dataSearch.referenceNos) {
                            formData = { ...formData, referenceNo: this.dataSearch.referenceNos.toString().replace(/[,]/g, "\n") || "" };
                        }

                        if (this.dataSearch.typeOfAcctManagement === AccountingConstants.ISSUE_TYPE.VOUCHER && this.dataSearch.voucherType) {
                            formData = { ...formData, voucherType: [AccountingConstants.VOUCHER_TYPE.find(x => x.id === this.dataSearch.voucherType)] };
                        }
                        if (this.dataSearch.typeOfAcctManagement === AccountingConstants.ISSUE_TYPE.INVOICE && this.dataSearch.invoiceStatus) {
                            formData = { ...formData, invoiceStatus: [this.invoiceStatusList.find(x => x.id === this.dataSearch.invoiceStatus)] };
                        }

                        // * Update form search.
                        this.formSearch.patchValue(formData);
                    }
                }
            );
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            referenceNo: [],
            partner: [],
            issueDate: [{ startDate: new Date(), endDate: new Date() }],
            creator: [],
            invoiceStatus: [],
            voucherType: []
        });
        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.partner = this.formSearch.controls['partner'];
        this.issueDate = this.formSearch.controls['issueDate'];
        this.creator = this.formSearch.controls['creator'];
        this.invoiceStatus = this.formSearch.controls['invoiceStatus'];
        this.voucherType = this.formSearch.controls['voucherType'];
    }

    loadPartnerList() {
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, true);
    }

    loadUserList() {
        this.creators = this._systemRepo.getSystemUsers({});
    }

    onSelectDataFormInfo(data: { id: any; }, type: string) {
        switch (type) {
            case 'partner':
                this.formSearch.controls['partner'].setValue(data.id);
                break;
            case 'creator':
                this.formSearch.controls['creator'].setValue(data.id);
                break;
            default:
                break;
        }
    }

    onSubmit() {
        const criteria: AccAccountingManagementCriteria = {
            referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            partnerId: this.partner.value,
            fromIssuedDate: this.issueDate.value ? (this.issueDate.value.startDate !== null ? formatDate(this.issueDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            toIssuedDate: this.issueDate.value ? (this.issueDate.value.endDate !== null ? formatDate(this.issueDate.value.endDate, 'yyyy-MM-dd', 'en') : null) : null,
            creatorId: this.creator.value,
            invoiceStatus: this.invoiceStatus.value ? (this.invoiceStatus.value.length > 0 ? this.invoiceStatus.value[0].id : null) : null,
            voucherType: this.voucherType.value ? (this.voucherType.value.length > 0 ? this.voucherType.value[0].id : null) : null,
            typeOfAcctManagement: this.accountType,
        };
        this.onSearch.emit(criteria);

        // * Dispatch an action.
        this._store.dispatch(SearchListAccountingMngt(criteria));
    }

    search() {
        this.onSubmit();
    }

    reset() {
        this.formSearch.reset();
        this.issueDate.setValue({ startDate: new Date(), endDate: new Date() });

        // * Dispatch an action.
        this._store.dispatch(SearchListAccountingMngt({}));

    }
}
