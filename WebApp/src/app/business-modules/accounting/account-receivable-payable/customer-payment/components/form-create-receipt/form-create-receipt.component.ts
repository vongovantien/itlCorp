import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { JobConstants, AccountingConstants, RoutingConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, ReceiptModel, } from '@models';
import { CatalogueRepo } from '@repositories';
import { IAppState } from '@store';
import { AppForm } from '@app';
import { ComboGridVirtualScrollComponent } from '@common';

import { Observable } from 'rxjs';
import { ARCustomerPaymentCustomerAgentDebitPopupComponent } from '../customer-agent-debit/customer-agent-debit.popup';
import { ResetInvoiceList, SelectPartnerReceipt, SelectReceiptDate, SelectReceiptAgreement, SelectReceiptClass } from '../../store/actions';
import { ReceiptPaymentMethodState, ReceiptTypeState } from '../../store/reducers';
import { takeUntil } from 'rxjs/operators';
import { Router } from '@angular/router';

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
    referenceNo: AbstractControl;

    $customers: Observable<Partner[]>;
    customers: Partner[] = [];

    agreements: IAgreementReceipt[];
    receipt: ReceiptModel;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldAgreement: CommonInterface.IComboGridDisplayField[] = [
        { field: 'contractType', label: 'Agreement Type' },
        { field: 'contractNo', label: 'Agreement No' },
        { field: 'expiredDate', label: 'Expired Date' },
    ];

    isReadonly = null;
    customerName: string;
    contractNo: string;
    isRequireAgreement: boolean = true;

    classReceipt: string[] = [
        AccountingConstants.RECEIPT_CLASS.CLEAR_DEBIT,
        AccountingConstants.RECEIPT_CLASS.ADVANCE,
        AccountingConstants.RECEIPT_CLASS.COLLECT_OBH,
        AccountingConstants.RECEIPT_CLASS.COLLECT_OBH_OTHER,
        AccountingConstants.RECEIPT_CLASS.PAY_OBH,
        AccountingConstants.RECEIPT_CLASS.NET_OFF];
    partnerTypeState: string;
    receiptReference: string = null;
    isShowGetDebit: boolean = true;
    constructor(
        private readonly _fb: FormBuilder,
        private readonly _store: Store<IAppState>,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _toastService: ToastrService,
        private readonly _router: Router,
    ) {
        super();
    }
    ngOnInit() {
        this.initForm();

        if (!this.receipt || (this.receipt?.status !== AccountingConstants.RECEIPT_STATUS.DRAFT || this.receipt?.status !== AccountingConstants.RECEIPT_STATUS.CANCEL)
            || !this.receipt?.arcbno) {
            this._store.select(ReceiptTypeState)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (partnerGroup) => {
                        if (!!partnerGroup) {
                            this.partnerTypeState = partnerGroup;
                            if (this.partnerTypeState.toUpperCase() === 'CUSTOMER') {
                                this.isRequireAgreement = true;
                            } else {
                                this.getRequireAgreementAgent();
                            }
                        }
                        this.getCustomerAgent();
                    }
                )
        }
    }

    getCustomerAgent() {
        const customersFromService = this._catalogueRepo.getCurrentCustomerSource();

        if (!!customersFromService.data.length) {
            this.customers = customersFromService.data;
            return;
        }
        this._catalogueRepo.getPartnerGroupsWithCriteria({
            partnerGroups: [CommonEnum.PartnerGroupEnum.CUSTOMER, CommonEnum.PartnerGroupEnum.AGENT]
            , partnerType: this.partnerTypeState.toUpperCase() === 'CUSTOMER' ? 'Customer' : 'Agent'
        })
            .subscribe(
                (data) => {
                    this.customers = data;
                }
            );
    }

    getRequireAgreementAgent() {
        if (this.class.value === AccountingConstants.RECEIPT_CLASS.ADVANCE || this.class.value === AccountingConstants.RECEIPT_CLASS.COLLECT_OBH) {
            this.isRequireAgreement = true;
            return;
        }
        this._store.select(ReceiptPaymentMethodState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data) => {
                    this.isRequireAgreement = false;
                    if (!!data) {
                        if (this.class.value === AccountingConstants.RECEIPT_CLASS.CLEAR_DEBIT) {
                            if (data.includes(AccountingConstants.RECEIPT_PAYMENT_METHOD.CLEAR_ADVANCE) || data.includes(AccountingConstants.RECEIPT_PAYMENT_METHOD.COLL_INTERNAL)) {
                                this.isRequireAgreement = true;
                            }
                        }
                        
                    }
                }
            )
    }

    initForm() {
        this.formSearchInvoice = this._fb.group({
            customerId: [null, Validators.required],
            date: [],
            paymentRefNo: [null],
            agreementId: this.isRequireAgreement ? [null, Validators.required] : [null],
            class: [this.classReceipt[0]],
            referenceNo: [{ value: null, disabled: true }]
        });
        this.customerId = this.formSearchInvoice.controls['customerId'];
        this.date = this.formSearchInvoice.controls['date'];
        this.paymentRefNo = this.formSearchInvoice.controls['paymentRefNo'];
        this.agreementId = this.formSearchInvoice.controls['agreementId'];
        this.class = this.formSearchInvoice.controls['class'];
        this.referenceNo = this.formSearchInvoice.controls['referenceNo'];

    }

    getContract() {
        this._catalogueRepo.getAgreement(
            <IQueryAgreementCriteria>{
                partnerId: this.customerId.value, status: true
            }).subscribe(
                (d: IAgreementReceipt[]) => {
                    if (!!d) {
                        this.agreements = d || [];
                        // if (!!this.agreements.length) {
                        //     this.agreementId.setValue(d[0].id);
                        // } else {
                        //     this.combogrid.displaySelectedStr = '';
                        //     this.agreementId.setValue(null);
                        // }
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

                                        // * Check partner group của đối tượng đang chọn có # với đối tượng phiếu thu muốn tạo
                                        // if (data.partnerType === this.partnerTypeState) {
                                        //     this._store.dispatch(SelectPartnerReceipt({ id: data.id, partnerGroup: this.partnerTypeState }));
                                        //     return;
                                        // }
                                        this._store.dispatch(SelectPartnerReceipt({ id: data.id, partnerGroup: data.partnerType.toUpperCase() }));

                                    } else {
                                        this.combogrid.displaySelectedStr = '';
                                        this.agreementId.setValue(null);
                                        if (this.isRequireAgreement) {
                                            this._toastService.warning(`Partner ${data.shortName} does not have any agreement`);
                                        } else {
                                            this._store.dispatch(SelectPartnerReceipt({ id: data.id, partnerGroup: data.partnerType.toUpperCase() }));
                                        }

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
                this.contractNo = (data as IAgreementReceipt).contractType + ' - ' + data.saleManName + ' - ' + data?.contractNo;
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

    onChangeReceiptType(type: string) {
        this._store.dispatch(SelectReceiptClass({ class: type }));
        this.getRequireAgreementAgent();
        if (type === AccountingConstants.RECEIPT_CLASS.CLEAR_DEBIT || type === AccountingConstants.RECEIPT_CLASS.NET_OFF) {
            this.isShowGetDebit = true;
            return;
        }
        this.isShowGetDebit = false;
    }

    goToReceiptCombine() {
        if (!this.receipt.arcbno) {
            return;
        }
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receipt/combine/${this.receipt.arcbno}`]);

    }
}

export interface IAgreementReceipt {
    id: string;
    contractNo: string;
    contractType: string;
    saleManName: string;
    expiredDate: Date;
    customerAdvanceAmountUsd: number;
    customerAdvanceAmountVnd: number;
    creditCurrency: string;
}

export interface IQueryAgreementCriteria {
    partnerId: string;
    status: boolean;
}
