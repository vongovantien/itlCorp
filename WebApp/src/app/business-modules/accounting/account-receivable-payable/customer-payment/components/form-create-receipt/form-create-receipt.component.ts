import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { formatDate } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, } from '@models';
import { CatalogueRepo, SystemRepo, AccountingRepo } from '@repositories';
import { IAppState } from '@store';
import { AppForm } from '@app';
import { DataService } from '@services';
import { ComboGridVirtualScrollComponent } from '@common';

import { GetInvoiceListSuccess, GetInvoiceList } from '../../store/actions';
import { Observable } from 'rxjs';
@Component({
    selector: 'customer-payment-form-create-receipt',
    templateUrl: './form-create-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptComponent extends AppForm implements OnInit {
    @Input() isUpdate: boolean = false;
    @ViewChild('combogridAgreement') combogrid: ComboGridVirtualScrollComponent;

    formSearchInvoice: FormGroup;
    customerId: AbstractControl;
    date: AbstractControl;
    paymentRefNo: AbstractControl;
    agreementId: AbstractControl;

    $customers: Observable<Partner[]>;
    agreements: IAgreementReceipt[];

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    displayFieldAgreement: CommonInterface.IComboGridDisplayField[] = [
        { field: 'saleManName', label: 'Salesman' },
        { field: 'contractNo', label: 'Contract No' },
        { field: 'contractType', label: 'Contract Type' },
    ];
    isReadonly = null;
    customerName: string;

    constructor(
        private _fb: FormBuilder,
        private _store: Store<IAppState>,
        private _catalogueRepo: CatalogueRepo,
        private _accountingRepo: AccountingRepo,
        private _systemRepo: SystemRepo,
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
                break;
            default:
                break;
        }
    }

    getInvoiceList() {
        this.isSubmitted = true;
        if (!this.formSearchInvoice.valid) {
            return;
        }
        const body = {
            customerId: this.customerId.value,
            agreementId: this.agreementId.value,
            fromDate: !!this.date.value?.startDate ? formatDate(this.date.value?.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: !!this.date.value?.endDate ? formatDate(this.date.value?.endDate, 'yyyy-MM-dd', 'en') : null,
        };

        this._store.dispatch(GetInvoiceList());
        this._accountingRepo.getInvoiceForReceipt(body)
            .pipe()
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._store.dispatch(GetInvoiceListSuccess({ invoices: res.data }));
                        return;
                    }

                    this._store.dispatch(GetInvoiceListSuccess({ invoices: [] }));
                    this._toastService.warning("Not found invoices");
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
