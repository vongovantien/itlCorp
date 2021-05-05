import { Component, OnInit, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { formatDate } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, ReceiptInvoiceModel, } from '@models';
import { CatalogueRepo, SystemRepo, AccountingRepo } from '@repositories';
import { IAppState } from '@store';
import { AppForm } from '@app';
import { DataService } from '@services';
import { ComboGridVirtualScrollComponent } from '@common';

import { GetInvoiceListSuccess, GetInvoiceList } from '../../store/actions';
import { Observable } from 'rxjs';
import { CustomerAgentDebitPopupComponent } from '../customer-agent-debit/customer-agent-debit.popup';
import { takeUntil } from 'rxjs/operators';
import { ReceiptDebitListState } from '../../store/reducers';
@Component({
    selector: 'customer-payment-form-create-receipt',
    templateUrl: './form-create-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptComponent extends AppForm implements OnInit {
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();
    @Input() isUpdate: boolean = false;
    @ViewChild('combogridAgreement') combogrid: ComboGridVirtualScrollComponent;
    @ViewChild(CustomerAgentDebitPopupComponent, { static: true }) debitPopup: CustomerAgentDebitPopupComponent;


    formSearchInvoice: FormGroup;
    customerId: AbstractControl;
    date: AbstractControl;
    paymentRefNo: AbstractControl;
    agreementId: AbstractControl;

    $customers: Observable<Partner[]>;
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

        if (!this.isUpdate) {
            this.$customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
            this.generateReceiptNo();
        }
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

    getDebit() {
        this.debitPopup.show();
        this.debitPopup.customerFromReceipt = this.customerId.value;
        this.debitPopup.dateFromReceipt = this.date.value;
        if (!this.debitPopup.partnerId.value) {
            this.debitPopup.setDefaultValue();
        }
    }

    addToReceipt($event: any) {
        const partnerId = $event;
        if (!!partnerId) {
            this.$customers.pipe(takeUntil(this.ngUnsubscribe))
                .subscribe((x: Partner[]) => {
                    const partner = x.filter((x: Partner) => x.id === partnerId).shift();
                    if (!!partner) {
                        this.onSelectDataFormInfo(partner, 'partner');
                    }
                })
        }
        this.onRequest.emit(true);
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
