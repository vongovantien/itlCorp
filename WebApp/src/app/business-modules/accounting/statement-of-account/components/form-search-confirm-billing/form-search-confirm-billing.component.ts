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

@Component({
    selector: 'form-search-confirm-billing',
    templateUrl: './form-search-confirm-billing.component.html'
})

export class ConfirmBillingFormSearchComponent extends AppForm implements OnInit {

    @Input() accountType: string = AccountingConstants.ISSUE_TYPE.INVOICE;
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();

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

    invoiceStatusList: string[] = ['New', 'Updated Invoice'];

    voucherTypeList: string[] = AccountingConstants.VOUCHER_TYPE.map(i => i.id);
    dataSearch: any;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder
    ) {
        super();
        this.requestReset = this.reset;
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
        const criteria: any = {
            referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            partnerId: this.partner.value,
            fromIssuedDate: this.issueDate.value ? (this.issueDate.value.startDate !== null ? formatDate(this.issueDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            toIssuedDate: this.issueDate.value ? (this.issueDate.value.endDate !== null ? formatDate(this.issueDate.value.endDate, 'yyyy-MM-dd', 'en') : null) : null,
            creatorId: this.creator.value,
            invoiceStatus: this.invoiceStatus.value,
            voucherType: this.voucherType.value,
            typeOfAcctManagement: this.accountType,
        };
        this.onSearch.emit(criteria);
    }

    search() {
        this.onSubmit();
    }

    reset() {
        this.formSearch.reset();
        this.issueDate.setValue({ startDate: new Date(), endDate: new Date() });
    }
}
