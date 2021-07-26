import { Component, OnInit, Input, ViewChild } from '@angular/core';
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
import { ARCustomerPaymentCustomerAgentDebitPopupComponent } from '../customer-agent-debit/customer-agent-debit.popup';
import { ResetInvoiceList, SelectPartnerReceipt, SelectReceiptDate, SelectReceiptAgreement } from '../../store/actions';

@Component({
    selector: 'customer-payment-form-create-receipt',
    templateUrl: './form-create-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptComponent extends AppForm implements OnInit {

    @Input() isUpdate: boolean = false;
    @ViewChild('combogridAgreement') combogrid: ComboGridVirtualScrollComponent;
    @ViewChild(ARCustomerPaymentCustomerAgentDebitPopupComponent) debitPopup: ARCustomerPaymentCustomerAgentDebitPopupComponent;

    formSearchInvoice: FormGroup;
    customerId: AbstractControl;
    date: AbstractControl;
    paymentRefNo: AbstractControl;
    agreementId: AbstractControl;
    class: AbstractControl;

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

    classReceipt: string[] = ['Clear Debit', 'Advance', 'Other'];

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
            agreementId: [null, Validators.required],
            class: [this.classReceipt[0]]
        });
        this.customerId = this.formSearchInvoice.controls['customerId'];
        this.date = this.formSearchInvoice.controls['date'];
        this.paymentRefNo = this.formSearchInvoice.controls['paymentRefNo'];
        this.agreementId = this.formSearchInvoice.controls['agreementId'];
        this.class = this.formSearchInvoice.controls['class'];

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
            case 'partnerPopup':
                if (!this.isReadonly) {
                    if (this.customerId.value !== data.id && type === 'partner') {
                        this._store.dispatch(ResetInvoiceList());
                    }
                    this.customerName = (data as Partner).shortName;
                    this.customerId.setValue((data as Partner).id);

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

                                        this._store.dispatch(SelectPartnerReceipt({ id: data.id, partnerGroup: data.partnerGroup }));

                                    } else {
                                        this.combogrid.displaySelectedStr = '';
                                        this.agreementId.setValue(null);
                                        this._toastService.warning(`Partner ${data.shortName} does not have any agreement`);

                                    }
                                }
                            }
                        );
                }
                break;
            case 'agreement':
                this.agreementId.setValue((data as IAgreementReceipt).id);
                // this._store.dispatch(SelectReceiptAgreement({ cusAdvanceAmount: null })) // ! Dispatch action to Trigger new state to call caculateAmountFromDebitList 
                this._store.dispatch(SelectReceiptAgreement({ ...data }));
                break;
            default:
                break;
        }
    }

    getPartnerOnForm($event: any) {
        const partnerId = $event;
        const partner = this.customers.find((x: Partner) => x.id === partnerId);
        if (!!partner) {
            this.onSelectDataFormInfo(partner, 'partnerPopup');
        }
    }


    getDebit() {
        this.debitPopup.show();
        if (!!this.date.value?.startDate) {
            this._store.dispatch(SelectReceiptDate({ date: this.date.value }));
        }
    }

    addToReceipt($event: any) {
        const partnerId = $event;
        if (!!partnerId) {
            this.getPartnerOnForm(partnerId);
        }
    }

}

export interface IAgreementReceipt {
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
