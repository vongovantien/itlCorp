import { Component, OnInit, Input } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormBuilder, FormGroup, AbstractControl, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';

import { CatalogueRepo, AccountingRepo } from '@repositories';
import { AccountingConstants } from '@constants';
import { ChartOfAccounts, Partner } from '@models';
import { CommonEnum } from '@enums';
import { IAppState, getCatalogueCurrencyState, GetCatalogueCurrencyAction } from '@store';
import { DataService } from '@services';

import { getAccoutingManagementPartnerState, IAccountingManagementPartnerState } from '../../store';

import { Observable, forkJoin } from 'rxjs';
import { map, debounceTime, takeUntil, distinctUntilChanged } from 'rxjs/operators';

@Component({
    selector: 'form-create-vat-invoice',
    templateUrl: './form-create-vat-invoice.component.html'
})

export class AccountingManagementFormCreateVATInvoiceComponent extends AppForm implements OnInit {
    @Input() set update(u: boolean) {
        this._isUpdate = u;

    }
    get update() {
        return this._isUpdate;
    }
    private _isUpdate: boolean = false;

    formGroup: FormGroup;

    partnerId: AbstractControl;
    personalName: AbstractControl;
    totalExchangeRate: AbstractControl;
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

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'partnerNameVn', label: 'Name Local' },
        { field: 'taxCode', label: 'Tax Code' },
    ];
    paymentMethods: CommonInterface.INg2Select[] = AccountingConstants.PAYMENT_METHOD;

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
        private _accountingRepo: AccountingRepo,
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
                        if (!this.formGroup.controls['partnerId'].value) {
                            this.formGroup.controls['partnerId'].setValue(res.partnerId);
                            this.formGroup.controls['personalName'].setValue(res.partnerName);
                            this.formGroup.controls['partnerAddress'].setValue(res.partnerAddress);
                            this.formGroup.controls['attachDocInfo'].setValue(res.inputRefNo);
                        }
                    }
                }
            );

        if (!this.update) {
            this.generateVoucherId();
        }
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
            invoiceNoTempt: [null, Validators.required],
            invoiceNoReal: [{ value: null, disabled: true }],
            serie: [null, Validators.required],
            paymentMethod: [],
            accountNo: [],
            totalAmount: [{ value: null, disabled: true }],
            currency: [[{ id: 'VND', text: 'VND' }]],
            status: ['New'],
        });

        this.partnerId = this.formGroup.controls['partnerId'];
        this.voucherId = this.formGroup.controls['voucherId'];
        this.invoiceNoTempt = this.formGroup.controls['invoiceNoTempt'];
        this.invoiceNoReal = this.formGroup.controls['invoiceNoReal'];
        this.date = this.formGroup.controls['date'];
        this.serie = this.formGroup.controls['serie'];
        this.paymentMethod = this.formGroup.controls['paymentMethod'];
        this.currency = this.formGroup.controls['currency'];
        this.accountNo = this.formGroup.controls['accountNo'];
        this.status = this.formGroup.controls['status'];
        this.totalExchangeRate = this.formGroup.controls['totalExchangeRate'];


        if (!this.update) {
            this.invoiceNoTempt.valueChanges
                .pipe(
                    debounceTime(400),
                    distinctUntilChanged()
                )
                .subscribe(
                    (res) => {
                        this.invoiceNoReal.setValue(res);
                    }
                );
        }
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

    generateVoucherId() {
        forkJoin([
            this._accountingRepo.generateVoucherId(),
            this._accountingRepo.generateInvoiceNoTemp(),
        ])
            .subscribe(
                (res: any[]) => {
                    this.voucherId.setValue(res[0].voucherId);
                    this.invoiceNoTempt.setValue(res[1].invoiceNoTemp);
                    this.invoiceNoReal.setValue(res[1].invoiceNoTemp);
                }
            );
    }

    syncExchangeRateCharge() {
        if (!!this.totalExchangeRate.value) {
            this._dataService.setData("generalExchangeRate", this.totalExchangeRate.value);
        }
    }

}
