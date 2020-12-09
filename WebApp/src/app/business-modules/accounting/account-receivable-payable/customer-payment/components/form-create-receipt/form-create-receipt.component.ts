import { Component, Output, EventEmitter, OnInit, Input } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Data } from '@angular/router';
import { SystemConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Currency, Customer, Partner, User, ReceiptInvoiceModel } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo, AccountingRepo } from '@repositories';
import { GetCatalogueCurrencyAction, getCatalogueCurrencyState, IAppState } from '@store';
import { Moment } from 'moment';
import { Observable } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'customer-payment-form-create-receipt',
    templateUrl: './form-create-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptComponent extends AppForm implements OnInit {
    @Input() isUpdate: boolean = false;

    form: FormGroup;
    selected: { start: Moment, end: Moment };
    customerId: AbstractControl; // load partner
    date: AbstractControl;
    paymentReferenceNo: AbstractControl;
    agreement: AbstractControl;
    paidAmount: AbstractControl;
    methods: CommonInterface.ICommonTitleValue[];
    userLogged: SystemInterface.IClaimUser;
    type: AbstractControl;
    cusAdvanceAmount: AbstractControl;
    finalPaidAmount: AbstractControl;
    balance: AbstractControl;
    paymentMethod: AbstractControl;
    currency: AbstractControl;
    paymentDate: AbstractControl;
    exchangeRate: AbstractControl;
    bankAcountNo: AbstractControl;

    $currencyList: Observable<Currency[]>;
    $customers: Observable<Partner[]>;
    agreements: IAgreementReceipt[];
    username: AbstractControl;

    paymentMethods: string[] = ['Cash', 'Bank Transfer'];
    receiptTypes: string[] = ['Debit', 'NetOff Adv'];

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldAgreement: CommonInterface.IComboGridDisplayField[] = [
        { field: 'saleManName', label: 'Salesman' },
        { field: 'contractNo', label: 'Contract No' },
        { field: 'contractType', label: 'Contract Type' },
    ];
    constructor(
        private _fb: FormBuilder,
        private _store: Store<IAppState>,
        private _catalogueRepo: CatalogueRepo,
        private _accountingRepo: AccountingRepo,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService

    ) {
        super();
    }
    ngOnInit() {
        this._store.dispatch(new GetCatalogueCurrencyAction());
        this.initForm();

        this.$customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
        this.$currencyList = this._store.select(getCatalogueCurrencyState);

        if (!this.isUpdate) {
            this.generateReceiptNo();
        }

    }

    initForm() {
        this.form = this._fb.group({
            customerId: [null, Validators.required],
            date: [],
            paymentReferenceNo: [],
            agreement: [null, Validators.required],
            paidAmount: [],
            type: [this.receiptTypes[0]],
            cusAdvanceAmount: [{ value: null, disabled: true }],
            finalPaidAmount: [{ value: null, disabled: true }],
            balance: [],
            paymentMethod: [this.paymentMethods[0]],
            currency: ['VND'],
            paymentDate: [],
            exchangeRate: [],
            bankAcountNo: [],
        });
        this.customerId = this.form.controls['customerId'];
        this.date = this.form.controls['date'];
        this.paymentReferenceNo = this.form.controls['paymentReferenceNo'];
        this.agreement = this.form.controls['agreement'];
        this.paidAmount = this.form.controls['paidAmount'];
        this.type = this.form.controls['type'];
        this.cusAdvanceAmount = this.form.controls['cusAdvanceAmount'];
        this.finalPaidAmount = this.form.controls['finalPaidAmount'];
        this.balance = this.form.controls['balance'];
        this.paymentMethod = this.form.controls['paymentMethod'];
        this.paymentDate = this.form.controls['paymentDate'];
        this.exchangeRate = this.form.controls['exchangeRate'];
        this.bankAcountNo = this.form.controls['bankAcountNo'];
    }

    generateReceiptNo() {
        this._accountingRepo.generateReceiptNo().subscribe(
            (data: any) => {
                if (!!data) {
                    const { receiptNo } = data;
                    this.paymentReferenceNo.setValue(receiptNo);
                }
            }
        );
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.customerId.setValue((data as Partner).id);

                this._catalogueRepo.getAgreement(
                    <IQueryAgreementCriteria>{
                        partnerId: this.customerId.value, status: true
                    }).subscribe(
                        (d: IAgreementReceipt[]) => {
                            if (!!d) {
                                this.agreements = d || [];
                                if (!!this.agreements.length) {
                                    this.agreement.setValue(d[0].id);
                                }
                            }
                        }
                    );
                break;
            case 'agreement':
                this.agreement.setValue((data as IAgreementReceipt).id);

                if (!this.cusAdvanceAmount.value) {
                    this.cusAdvanceAmount.setValue((data as IAgreementReceipt).cusAdvanceAmount);
                }
                break;
            default:
                break;
        }
    }

    getInvoiceList() {
        this.isSubmitted = true;
        const body = {
            customerId: this.customerId.value,
            agreementId: this.agreement.value,
            fromDate: !!this.date.value?.startDate ? formatDate(this.date.value?.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: !!this.date.value?.endDate ? formatDate(this.date.value?.endDate, 'yyyy-MM-dd', 'en') : null,
        };
        console.log(body);

        this._accountingRepo.getInvoiceForReceipt(body)
            .pipe()
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        // set invoicelist.
                    } else {
                        this._toastService.warning("Not found invoices");
                    }
                }
            );
    }
}

interface IAgreementReceipt {
    id: string;
    contractNo: string;
    contractType: string;
    saleManName: string;
    expiredDate: Date;
    cusAdvanceAmount: number;
}

interface IQueryAgreementCriteria {
    partnerId: string;
    status: boolean;
}
