import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { Partner, User } from '@models';
import { AbstractControl, FormGroup, FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';
import { JobConstants } from '@constants';
import { AccAccountingManagementCriteria } from 'src/app/shared/models/accouting/accounting-management';
import { formatDate } from '@angular/common';

@Component({
    selector: 'form-search-vat-voucher',
    templateUrl: './form-search-vat-voucher.component.html'
})

export class AccountingManagementFormSearchVatVoucherComponent extends AppForm implements OnInit {
    @Input() accountType: string = 'Invoice';
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

    voucherTypeList: CommonInterface.INg2Select[] = [
        { id: 'Debt Voucher', text: 'Debt Voucher' },
        { id: 'Bank', text: 'Bank' },
        { id: 'Other', text: 'Orther' },
    ];

    // tslint:disable-next-line: no-any
    invoiceStatusActive: any[] = [];
    // tslint:disable-next-line: no-any
    voucherTypeActive: any[] = [];

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
    ) {
        super();
    }

    ngOnInit() {
        this.initFormSearch();
        this.loadPartnerList();
        this.loadUserList();
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            referenceNo: [],
            partner: [],
            issueDate: [],
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

    // tslint:disable-next-line: no-any
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

    onSubmit() {
        const criteria: AccAccountingManagementCriteria = {
            // tslint:disable-next-line: no-any
            referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            partnerId: this.partner.value,
            issuedDate: this.issueDate.value ? (this.issueDate.value.startDate !== null ? formatDate(this.issueDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            creatorId: this.creator.value,
            invoiceStatus: this.invoiceStatus.value ? (this.invoiceStatus.value.length > 0 ? this.invoiceStatus.value[0].id : null) : null,
            voucherType: this.voucherType.value ? (this.voucherType.value.length > 0 ? this.voucherType.value[0].id : null) : null,
            typeOfAcctManagement: this.accountType,
        };
        this.onSearch.emit(criteria);
    }

    search() {
        this.onSubmit();
    }

    reset() {
        this.formSearch.reset();
        // tslint:disable-next-line: no-any
        this.onSearch.emit(<any>{ typeOfAcctManagement: this.accountType });
    }
}
