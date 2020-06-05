import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormBuilder, FormGroup, AbstractControl } from '@angular/forms';
import { Store } from '@ngrx/store';

import { CatalogueRepo } from '@repositories';
import { JobConstants, AccountingConstants } from '@constants';
import { ChartOfAccounts, Partner, PartnerOfAcctManagementResult } from '@models';
import { CommonEnum } from '@enums';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction } from '@store';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
    selector: 'form-create-vat-invoice',
    templateUrl: './form-create-vat-invoice.component.html'
})

export class AccountingManagementFormCreateVATInvoiceComponent extends AppForm implements OnInit {

    @Output() onSearchInputRef: EventEmitter<PartnerOfAcctManagementResult[]> = new EventEmitter<PartnerOfAcctManagementResult[]>();

    formGroup: FormGroup;

    partnerId: AbstractControl;
    personalName: AbstractControl;
    partnerAddress: AbstractControl;
    description: AbstractControl;
    voucherId: AbstractControl;
    date: AbstractControl;
    invoiceNoTempt: AbstractControl;
    invoiceNoReal: AbstractControl;
    serie: AbstractControl;
    paymentMethod: AbstractControl;
    accountNo: AbstractControl;
    totalAmount: AbstractControl;
    currency: AbstractControl;
    status: AbstractControl;
    attachDocInfo: AbstractControl;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    paymentMethods: CommonInterface.INg2Select[] = AccountingConstants.PaymentMethod;

    displayFieldsChartAccount: CommonInterface.IComboGridDisplayField[] = [
        { field: 'accountCode', label: 'Account Code' },
        { field: 'accountNameLocal', label: 'Account Name Local' },
    ];

    partners: Observable<Partner[]>;
    listCurrency: Observable<CommonInterface.INg2Select[]>;
    chartOfAccounts: Observable<ChartOfAccounts[]>;

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<IAppState>
    ) {
        super();
    }

    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());

        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
        this.listCurrency = this._store.select(getCatalogueCurrencyState).pipe(map(data => this.utility.prepareNg2SelectData(data, 'id', 'id')));
        this.chartOfAccounts = this._catalogueRepo.getListChartOfAccounts();

        this.initForm();
    }

    initForm() {
        this.formGroup = this._fb.group({
            partnerId: [],

            personalName: [],
            partnerAddress: [],
            description: [],
            attachDocInfo: [],

            voucherId: [],
            date: [{ startDate: new Date(), endDate: new Date() }],
            invoiceNoTempt: [],
            invoiceNoReal: [],
            serie: [],
            paymentMethod: [],
            accountNo: [],
            totalAmount: [],
            currency: [[{ id: 'VND', text: 'VND' }]],
            status: ['New'],
        });

        this.partnerId = this.formGroup.controls['partnerId'];
        this.date = this.formGroup.controls['date'];
        this.paymentMethod = this.formGroup.controls['paymentMethod'];
        this.currency = this.formGroup.controls['currency'];
        this.accountNo = this.formGroup.controls['accountNo'];
    }

    onSelectDataFormInfo(data, type: string) {
        switch (type) {
            case 'partner':
                this.partnerId.setValue((data as Partner).id);
                console.log(data);
                break;
            case 'account':
                this.accountNo.setValue((data as ChartOfAccounts).id);
                break;
            default:
                break;
        }
    }

    onSearchRefNo(data) {
        this.onSearchInputRef.emit(data);
    }
}
