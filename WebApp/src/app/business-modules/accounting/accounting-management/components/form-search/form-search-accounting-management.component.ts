import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { User, Customer } from '@models';
import { Observable } from 'rxjs';
import { AbstractControl, FormGroup, FormBuilder } from '@angular/forms';
import { formatDate } from '@angular/common';
import { JobConstants, AccountingConstants } from '@constants';

@Component({
    selector: 'form-search-accounting-management',
    templateUrl: './form-search-accounting-management.component.html'
})

export class AccountingManagementFormSearchComponent extends AppForm implements OnInit {
    @Output() onSearch: EventEmitter<ISearchDataInvoiceCDNote> = new EventEmitter<ISearchDataInvoiceCDNote>();
    @Output() onReset: EventEmitter<ISearchDataInvoiceCDNote> = new EventEmitter<ISearchDataInvoiceCDNote>();

    partners: Observable<Customer[]>;
    creators: Observable<User[]>;

    filterTypes: CommonInterface.INg2Select[] =
        [
            { id: 'Debit', text: 'Debit Note' },
            { id: 'Credit', text: 'Credit Note' },
            { id: 'Invoice', text: 'Invoice' }
        ];
    status: CommonInterface.INg2Select[] = AccountingConstants.STATUS_CD;
    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    displayFieldsCreator: CommonInterface.IComboGridDisplayField[] = [
        { field: 'username', label: 'User Name' },
        { field: 'employeeNameVn', label: 'Full Name' }
    ];

    partner: AbstractControl;
    searchText: AbstractControl;
    creator: AbstractControl;
    issuedDate: AbstractControl;
    filterType: AbstractControl;
    filterStatus: AbstractControl;

    formSearch: FormGroup;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();

        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, null);
        this.creators = this._systemRepo.getSystemUsers();
    }

    initForm() {
        this.formSearch = this._fb.group({
            searchText: [],
            partner: [],
            issuedDate: [],
            creator: [],
            filterType: [],
            filterStatus: []
        });

        this.partner = this.formSearch.controls['partner'];
        this.creator = this.formSearch.controls['creator'];
        this.searchText = this.formSearch.controls['searchText'];
        this.filterType = this.formSearch.controls['filterType'];
        this.issuedDate = this.formSearch.controls['issuedDate'];
        this.filterStatus = this.formSearch.controls['filterStatus'];
    }

    onSelectDataFormInfo(data: any, type: string) {
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

    searchInvoiceCDnote() {
        const s = (!!this.issuedDate.value && !!this.issuedDate.value.startDate) ? formatDate(this.issuedDate.value.startDate, 'yyyy-MM-dd', 'en') : null;
        const body: ISearchDataInvoiceCDNote = {
            referenceNos: this.searchText.value,
            partnerId: this.partner.value,
            issuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.startDate) ? formatDate(this.issuedDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            creatorId: this.creator.value,
            type: !!this.filterType.value && this.filterType.value.length > 0 ? this.filterType.value[0].id : null,
            status: !!this.filterStatus.value && this.filterStatus.value.length > 0 ? this.filterStatus.value[0].id : null
        };
        this.onSearch.emit(body);
    }

    resetSearch() {
        this.resetKeywordSearchCombogrid();
        this.formSearch.reset();
        this.resetFormControl(this.partner);
        this.resetFormControl(this.creator);
        this.onReset.emit(<any>{ transactionType: null });
    }
}
interface ISearchDataInvoiceCDNote {
    referenceNos: string;
    partnerId: string;
    issuedDate: string;
    creatorId: string;
    type: string;
    status: string;
}
