import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormGroup, Validators, FormBuilder } from '@angular/forms';
import { ChargeConstants, JobConstants } from '@constants';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { AppForm } from '@app';
import { Partner } from '@models';
import { Store } from '@ngrx/store';
import { formatDate } from '@angular/common';

import { ReceiptPartnerCurrentState, ReceiptDateState } from '../../store/reducers';
import { ARCustomerPaymentCustomerAgentDebitPopupComponent } from '../customer-agent-debit/customer-agent-debit.popup';
import { IReceiptState } from '../../store/reducers/customer-payment.reducer';

import { Observable } from 'rxjs';
import { takeUntil, pluck } from 'rxjs/operators';
import { SelectPartnerReceipt } from '../../store/actions';

@Component({
    selector: 'form-search-customer-agent-cd-invoice',
    templateUrl: './form-search-customer-agent-cd-invoice.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARCustomerPaymentFormSearchCustomerAgentCDInvoiceComponent extends AppForm implements OnInit {

    formSearch: FormGroup;
    typeSearch: AbstractControl;
    partnerId: AbstractControl;
    referenceNo: AbstractControl;
    date: AbstractControl;
    dateType: AbstractControl;
    service: AbstractControl;

    searchOptions: string[] = ['SOA', 'Debit Note/Invoice', 'VAT Invoice', 'JOB NO', 'HBL', 'MBL', 'Customs No'];
    dateTypeList: string[] = ['Invoice Date', 'Service Date', 'Billing Date'];
    services: CommonInterface.INg2Select[] = [
        { text: 'All', id: 'All' },
        { text: ChargeConstants.IT_DES, id: ChargeConstants.IT_CODE },
        { text: ChargeConstants.AI_DES, id: ChargeConstants.AI_CODE },
        { text: ChargeConstants.AE_DES, id: ChargeConstants.AE_CODE },
        { text: ChargeConstants.SFE_DES, id: ChargeConstants.SFE_CODE },
        { text: ChargeConstants.SFI_DES, id: ChargeConstants.SFI_CODE },
        { text: ChargeConstants.SLE_DES, id: ChargeConstants.SLE_CODE },
        { text: ChargeConstants.SLI_DES, id: ChargeConstants.SLI_CODE },
        { text: ChargeConstants.SCE_DES, id: ChargeConstants.SCE_CODE },
        { text: ChargeConstants.SCI_DES, id: ChargeConstants.SCI_CODE },
        { text: ChargeConstants.CL_DES, id: ChargeConstants.CL_CODE }
    ];
    displayFilesPartners: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;


    customers: Observable<Partner[]>;
    customerFromReceipt: string;

    constructor(
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _toastService: ToastrService,
        private readonly poupParentComponent: ARCustomerPaymentCustomerAgentDebitPopupComponent,
        private readonly _store: Store<IReceiptState>

    ) { super(); }

    ngOnInit(): void {
        this.customers = (this._catalogueRepo.customers$ as Observable<any>).pipe(pluck('data'));

        this.initForm();

        this.initSubmitClickSubscription(() => this.searchData());

    }

    initForm() {
        this.formSearch = this._fb.group({
            partnerId: [null, Validators.required],
            typeSearch: [this.searchOptions[1]],
            referenceNo: [],
            date: [],
            dateType: [this.dateTypeList[0]],
            service: [[this.services[0].id]],
        });

        this.typeSearch = this.formSearch.controls['typeSearch'];
        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.date = this.formSearch.controls['date'];
        this.dateType = this.formSearch.controls['dateType'];
        this.service = this.formSearch.controls['service'];
        this.partnerId = this.formSearch.controls['partnerId'];


        // * Listen partner current state.
        this._store.select(ReceiptPartnerCurrentState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (partnerId) => {
                    if (!!partnerId) {
                        this.partnerId.setValue(partnerId);
                        this.customerFromReceipt = partnerId;
                    }
                }
            )
        // * Listen Receipt Date
        this._store.select(ReceiptDateState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (date) => {
                    if (!!date) {
                        this.date.setValue(date);
                    }
                }
            )
    }

    onSelectDataFormInfo(data: any) {
        this._catalogueRepo.getAgreement(
            { partnerId: data.id, status: true })
            .subscribe(
                (d: any[]) => {
                    if (!!d && !!d.length) {
                        this.partnerId.setValue(data.id);

                        //*  Check đối tượng đang search có khác với đối tượng bên ngoài receipt
                        if (this.partnerId.value !== this.customerFromReceipt) {
                            this.poupParentComponent.listDebit = [];
                            this.poupParentComponent.customerFromReceipt = this.customerFromReceipt;
                        }

                        this._store.dispatch(SelectPartnerReceipt({ id: data.id, partnerGroup: data.partnerGroup }));
                    } else {
                        this.partnerId.setValue(null);
                        this._toastService.warning(`Partner ${data.shortName} does not have any agreement`);
                        return false;
                    }
                }
            );
    }

    selelectedService(event: any) {
        if (event.length > 0) {
            if (event[event.length - 1].id === 'All') {
                this.service.setValue([{ id: 'All', text: 'All' }]);
            } else {
                const arrNotIncludeAll = event.filter(x => x.id !== 'All');
                this.service.setValue(arrNotIncludeAll);
            }
        }
    }

    mapServiceId() {
        let serviceId = '';
        const serv = this.services.filter(service => service.id !== 'All');
        serviceId = serv.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
        return serviceId;
    }

    searchData() {
        this.isSubmitted = true;
        if (this.formSearch.valid) {
            const body: IAcctCustomerDebitCredit = {
                partnerId: this.partnerId.value,
                searchType: this.typeSearch.value,
                referenceNos: !!this.referenceNo.value ?
                    this.referenceNo.value.trim()
                        .replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',')
                        .trim()
                        .split(',')
                        .map((item: any) => item.trim()) : null,

                fromDate: !!this.date.value?.startDate ? formatDate(this.date.value.startDate, 'yyyy-MM-dd', 'en') : null,
                toDate: !!this.date.value?.endDate ? formatDate(this.date.value?.endDate, 'yyyy-MM-dd', 'en') : null,
                dateType: this.dateType.value,
                service: this.service.value[0] === 'All' ? this.mapServiceId() : (this.service.value.length > 0 ? this.service.value.map((item: any) => item.id).toString().replace(/(?:,)/g, ';') : null)
            };
            this.poupParentComponent.onApply(body);
        }
    }

    reset() {
        this.formSearch.reset();
        this.initForm();
        this.date.setValue(null);
        this.isSubmitted = false;
    }
}

interface IAcctCustomerDebitCredit {
    partnerId: string;
    searchType: string;
    referenceNos: string[];
    fromDate: string;
    toDate: string;
    dateType: string;
    service: string;
}
