import { Component, Output, EventEmitter, OnInit, Input, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Data } from '@angular/router';
import { SystemConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Currency, Customer, Partner, User, ReceiptInvoiceModel } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo, AccountingRepo } from '@repositories';
import { IAppState } from '@store';
import { Observable } from 'rxjs';
import { AppForm } from 'src/app/app.form';
import { formatDate } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { GetInvoiceListSuccess, GetInvoiceList } from '../../store/actions';
import { DataService } from '@services';
import { ComboGridVirtualScrollComponent } from '@common';

@Component({
    selector: 'customer-payment-form-create-receipt',
    templateUrl: './form-create-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptComponent extends AppForm implements OnInit {
    @Input() isUpdate: boolean = false;
    @ViewChild('combogridAgreement') combogrid: ComboGridVirtualScrollComponent;

    formSearchInvoice: FormGroup;
    customerId: AbstractControl; // load partner
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

        this.$customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);

        if (!this.isUpdate) {
            this.generateReceiptNo();
        }

    }

    initForm() {
        this.formSearchInvoice = this._fb.group({
            customerId: [null, Validators.required],
            date: [],
            paymentRefNo: [null, Validators.required],
            agreementId: [null]
            // agreement: [null, Validators.required]
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
