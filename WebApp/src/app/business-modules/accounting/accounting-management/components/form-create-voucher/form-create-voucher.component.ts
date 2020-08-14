import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { AbstractControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { AccountingConstants } from '@constants';
import { Partner, ChartOfAccounts } from '@models';

import { CatalogueRepo } from '@repositories';
import { Store } from '@ngrx/store';
import { IAppState, GetCatalogueCurrencyAction, getCatalogueCurrencyState } from '@store';
import { CommonEnum } from '@enums';
import { DataService } from '@services';

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
    totalExchangeRate: AbstractControl;
    description: AbstractControl;
    attachDocInfo: AbstractControl;
    paymentTerm: AbstractControl;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'partnerNameVn', label: 'Name Local' },
        { field: 'taxCode', label: 'Tax Code' },
    ];
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
        private _dataService: DataService

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
                        if (!this.partnerId.value) {
                            this.partnerId.setValue(res.partnerId);
                            if (!this.formGroup.controls['personalName'].value) {
                                this.formGroup.controls['personalName'].setValue(res.partnerName);
                            }
                            this.formGroup.controls['partnerAddress'].setValue(res.partnerAddress);
                        }

                    } else {
                        if (!this.formGroup.controls['personalName'].value) {
                            if (!this.formGroup.controls['personalName'].value) {
                                this.formGroup.controls['personalName'].setValue(res.settlementRequester);
                            }
                        }
                    }

                    this.attachDocInfo.setValue(null);
                    this.attachDocInfo.setValue(this.updateAttachInfo(this.attachDocInfo.value, !!res.inputRefNo ? res.inputRefNo : ''));
                    this.description.setValue(`Hoạch Toán Phí : ${this.attachDocInfo.value}`);
                    if (!this.paymentTerm.value) {
                        this.paymentTerm.setValue(res.paymentTerm);
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
            totalExchangeRate: [],

            voucherId: [null, Validators.required],
            date: [{ startDate: new Date(), endDate: new Date() }],
            invoiceNoTempt: [],
            invoiceNoReal: [],

            voucherType: [],
            paymentMethod: [[this.paymentMethods[2]]],
            accountNo: [],
            totalAmount: [{ value: null, disabled: true }],
            currency: [[{ id: 'VND', text: 'VND' }]],
            status: [],
            paymentTerm: [null, Validators.compose([
                Validators.required,
                Validators.max(31),
                Validators.min(1)
            ])],
        });

        this.partnerId = this.formGroup.controls['partnerId'];
        this.voucherId = this.formGroup.controls['voucherId'];

        this.date = this.formGroup.controls['date'];
        this.paymentMethod = this.formGroup.controls['paymentMethod'];
        this.currency = this.formGroup.controls['currency'];
        this.accountNo = this.formGroup.controls['accountNo'];
        this.voucherType = this.formGroup.controls['voucherType'];
        this.totalExchangeRate = this.formGroup.controls['totalExchangeRate'];
        this.description = this.formGroup.controls['description'];
        this.attachDocInfo = this.formGroup.controls['attachDocInfo'];
        this.paymentTerm = this.formGroup.controls['paymentTerm'];
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

    syncExchangeRateCharge() {
        if (!!this.totalExchangeRate.value) {
            this._dataService.setData("generalExchangeRate", this.totalExchangeRate.value);
        }
    }

    updateAttachInfo(pre: string, cur: string) {
        if (!pre) {
            return cur;
        }
        return `${pre}, ${cur}`;
    }
}
