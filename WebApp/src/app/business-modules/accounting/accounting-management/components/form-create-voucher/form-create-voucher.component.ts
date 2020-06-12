import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { AbstractControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { JobConstants, AccountingConstants } from '@constants';
import { Partner, ChartOfAccounts } from '@models';

import { CatalogueRepo } from '@repositories';
import { Store } from '@ngrx/store';
import { IAppState, GetCatalogueCurrencyAction, getCatalogueCurrencyState } from '@store';
import { CommonEnum } from '@enums';
import { getAccoutingManagementPartnerState, IAccountingManagementPartnerState } from '../../store';

import { Observable } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';


@Component({
    selector: 'form-create-voucher',
    templateUrl: './form-create-voucher.component.html',
})
export class AccountingManagementFormCreateVoucherComponent extends AppForm implements OnInit {

    formGroup: FormGroup;

    partnerId: AbstractControl;
    personalName: AbstractControl;

    voucherId: AbstractControl;
    date: AbstractControl;
    paymentMethod: AbstractControl;
    accountNo: AbstractControl;
    totalAmount: AbstractControl;
    currency: AbstractControl;
    voucherType: AbstractControl;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    paymentMethods: CommonInterface.INg2Select[] = AccountingConstants.PAYMENT_METHOD;
    voucherTypes: CommonInterface.INg2Select[] = AccountingConstants.VOUCHER_TYPE;

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
        private _store: Store<IAppState>,
    ) {
        super();
    }

    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());

        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
        this.listCurrency = this._store.select(getCatalogueCurrencyState).pipe(map(data => this.utility.prepareNg2SelectData(data, 'id', 'id')));
        this.chartOfAccounts = this._catalogueRepo.getListChartOfAccounts();

        this.initForm();

        this._store.select(getAccoutingManagementPartnerState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: IAccountingManagementPartnerState) => {
                    if (!!res.partnerId) {
                        if (!this.formGroup.controls['partnerId'].value) {
                            this.formGroup.controls['partnerId'].setValue(res.partnerId);
                            this.formGroup.controls['personalName'].setValue(res.settlementRequester ? res.settlementRequester : res.partnerName);
                            this.formGroup.controls['partnerAddress'].setValue(res.partnerAddress);
                            this.formGroup.controls['attachDocInfo'].setValue(res.inputRefNo);
                        }
                    }
                }
            );
    }

    initForm() {
        this.formGroup = this._fb.group({
            partnerId: [null, Validators.required],

            personalName: [],
            partnerAddress: [],
            description: [],
            attachDocInfo: [],

            voucherId: [null, Validators.required],
            date: [{ startDate: new Date(), endDate: new Date() }],
            invoiceNoTempt: [],
            invoiceNoReal: [],

            voucherType: [],
            paymentMethod: [],
            accountNo: [],
            totalAmount: [{ value: null, disabled: true }],
            currency: [[{ id: 'VND', text: 'VND' }]],
            status: [],
        });

        this.partnerId = this.formGroup.controls['partnerId'];
        this.voucherId = this.formGroup.controls['voucherId'];

        this.date = this.formGroup.controls['date'];
        this.paymentMethod = this.formGroup.controls['paymentMethod'];
        this.currency = this.formGroup.controls['currency'];
        this.accountNo = this.formGroup.controls['accountNo'];
        this.voucherType = this.formGroup.controls['voucherType'];
    }


    onSelectDataFormInfo(data, type: string) {
        switch (type) {
            case 'partner':
                this.partnerId.setValue((data as Partner).id);
                break;
            case 'account':
                this.accountNo.setValue((data as ChartOfAccounts).accountCode);
                break;
            default:
                break;
        }
    }
}
