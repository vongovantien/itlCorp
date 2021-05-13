import { Component, OnInit, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, ReceiptInvoiceModel, } from '@models';
import { CatalogueRepo, AccountingRepo } from '@repositories';
import { IAppState } from '@store';
import { AppForm } from '@app';
import { DataService } from '@services';
import { ComboGridVirtualScrollComponent } from '@common';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { CustomerAgentDebitPopupComponent } from '../customer-agent-debit/customer-agent-debit.popup';
import { ReceiptTypeState } from '../../store/reducers';

@Component({
    selector: 'customer-payment-form-create-receipt',
    templateUrl: './form-create-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptComponent extends AppForm implements OnInit {

    @Input() isUpdate: boolean = false;
    @ViewChild('combogridAgreement') combogrid: ComboGridVirtualScrollComponent;
    @ViewChild(CustomerAgentDebitPopupComponent) debitPopup: CustomerAgentDebitPopupComponent;
    @Output() onChangeReceipt: EventEmitter<boolean> = new EventEmitter<boolean>();

    formSearchInvoice: FormGroup;
    customerId: AbstractControl;
    date: AbstractControl;
    paymentRefNo: AbstractControl;
    agreementId: AbstractControl;

    $customers: Observable<Partner[]>;
    customers: Partner[] = [];

    agreements: IAgreementReceipt[];
    listReceipts: ReceiptInvoiceModel[] = [];

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldAgreement: CommonInterface.IComboGridDisplayField[] = [
        { field: 'contractType', label: 'Agreement Type' },
        { field: 'contractNo', label: 'Agreement No' },
        { field: 'expiredDate', label: 'Expired Date' },
    ];

    isReadonly = null;
    customerName: string;
    contractNo: string;

    constructor(
        private _fb: FormBuilder,
        private _store: Store<IAppState>,
        private _catalogueRepo: CatalogueRepo,
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _dataService: DataService

    ) {
        super();
    }
    ngOnInit() {
        this.initForm();
        this.getCustomerAgent();

        if (!this.isUpdate) {
            this.generateReceiptNo();
        }
    }

    getCustomerAgent() {
        const customersFromService = this._catalogueRepo.getCurrentCustomerSource();

        if (!!customersFromService.data.length) {
            this.customers = customersFromService.data;
            return;
        }
        this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CUSTOMER, CommonEnum.PartnerGroupEnum.AGENT])
            .subscribe(
                (data) => {
                    this._catalogueRepo.customersSource$.next({ data }); // * Update service.
                    this.customers = data;
                }
            );
    }

    initForm() {
        this.formSearchInvoice = this._fb.group({
            customerId: new FormControl(null, Validators.required),
            date: [],
            paymentRefNo: new FormControl(null, Validators.required),
            agreementId: [null, Validators.required]
        });
        this.customerId = this.formSearchInvoice.controls['customerId'];
        this.date = this.formSearchInvoice.controls['date'];
        this.paymentRefNo = this.formSearchInvoice.controls['paymentRefNo'];
        this.agreementId = this.formSearchInvoice.controls['agreementId'];

    }

    generateReceiptNo() {
        this._accountingRepo.generateReceiptNo().subscribe(
            (data: any) => {
                if (!!data) {
                    const { receiptNo } = data;
                    this.paymentRefNo.setValue(receiptNo);
                }
            }
        );
    }

    getContract() {
        this._catalogueRepo.getAgreement(
            <IQueryAgreementCriteria>{
                partnerId: this.customerId.value, status: true
            }).subscribe(
                (d: IAgreementReceipt[]) => {
                    if (!!d) {
                        this.agreements = d || [];
                        if (!!this.agreements.length) {
                            this.agreementId.setValue(d[0].id);
                        } else {
                            this.combogrid.displaySelectedStr = '';
                            this.agreementId.setValue(null);
                        }
                    }
                }
            );
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.customerName = (data as Partner).shortName;
                this.customerId.setValue((data as Partner).id);
                this._dataService.setData('customer', data);

                this._catalogueRepo.getAgreement(
                    <IQueryAgreementCriteria>{
                        partnerId: this.customerId.value, status: true
                    }).subscribe(
                        (d: IAgreementReceipt[]) => {
                            if (!!d) {
                                this.agreements = d || [];
                                if (!!this.agreements.length) {
                                    this.agreementId.setValue(d[0].id);


                                    this.onSelectDataFormInfo(d[0], 'agreement');
                                } else {
                                    this.combogrid.displaySelectedStr = '';
                                    this.agreementId.setValue(null);
                                }
                            }
                        }
                    );
                break;
            case 'agreement':
                this.agreementId.setValue((data as IAgreementReceipt).id);
                this._dataService.setData('cus-advance', (data as IAgreementReceipt).cusAdvanceAmount);
                this._dataService.setData('currency', (data as IAgreementReceipt).creditCurrency);
                break;
            default:
                break;
        }
    }

    getPartnerOnForm($event: any) {
        const partnerId = $event;
        const partner = this.customers.find((x: Partner) => x.id === partnerId);
        if (!!partner) {
            this.onSelectDataFormInfo(partner, 'partner');
        }
    }


    getDebit() {
        this.debitPopup.show();
        this._store.select(ReceiptTypeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(x => this.debitPopup.type = x);
        this.debitPopup.customerFromReceipt = this.customerId.value;
        this.debitPopup.dateFromReceipt = this.date.value;
        if (!this.debitPopup.partnerId.value) {
            this.debitPopup.setDefaultValue();
        }
    }

    addToReceipt($event: any) {
        const partnerId = $event;
        if (!!partnerId) {
            this.getPartnerOnForm(partnerId);
            this.onChangeReceipt.emit(true);
        }
    }

}

interface IAgreementReceipt {
    id: string;
    contractNo: string;
    contractType: string;
    saleManName: string;
    expiredDate: Date;
    cusAdvanceAmount: number;
    creditCurrency: string;
}

interface IQueryAgreementCriteria {
    partnerId: string;
    status: boolean;
}
