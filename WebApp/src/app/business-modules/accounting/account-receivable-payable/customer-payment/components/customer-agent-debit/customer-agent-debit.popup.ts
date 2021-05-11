import { Component, Output, EventEmitter } from '@angular/core';
import { PopupBase } from '@app';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { Observable } from 'rxjs';
import { Customer, ReceiptInvoiceModel } from '@models';
import { CatalogueRepo, AccountingRepo } from '@repositories';
import { JobConstants, ChargeConstants } from '@constants';
import { formatDate } from '@angular/common';
import { NgProgress } from '@ngx-progressbar/core';
import { finalize, catchError, takeUntil, pluck } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { IAppState } from '@store';
import { GetInvoiceListSuccess, ResetInvoiceList } from '../../store/actions';
import { ToastrService } from 'ngx-toastr';
import { ReceiptCreditListState, ReceiptDebitListState } from '../../store/reducers';
import { ActivatedRoute } from '@angular/router';
import { SortService } from '@services';
import { AgencyReceiptModel } from 'src/app/shared/models/accouting/agency-receipt.model';
import { CommonEnum } from '@enums';

@Component({
    selector: 'customer-agent-debit-popup',
    templateUrl: 'customer-agent-debit.popup.html'
})

export class CustomerAgentDebitPopupComponent extends PopupBase {
    @Output() onAddToReceipt: EventEmitter<any> = new EventEmitter<any>();
    typeSearch: AbstractControl;
    partnerId: AbstractControl;
    referenceNo: AbstractControl;
    date: AbstractControl;
    dateType: AbstractControl;
    service: AbstractControl;

    type: string = null;
    customerFromReceipt: string = null;

    dateFromReceipt: any = null;

    displayFilesPartners: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    formSearch: FormGroup;
    searchOptions: string[] = ['SOA', 'Debit Note/Invoice', 'VAT Invoice', 'JOB NO', 'HBL', 'MBL', 'Customs No'];
    dateTypeList: string[] = ['Invoice Date', 'Service Date', 'Billing Date'];

    listDebit: ReceiptInvoiceModel[] = [];
    listCreditInvoice: ReceiptInvoiceModel[] = [];
    listDebitInvoice: ReceiptInvoiceModel[] = [];
    customers: Observable<Customer[]>;
    headersAgency: CommonInterface.IHeaderTable[];
    agencyDebitModel: AgencyReceiptModel = new AgencyReceiptModel();
    agencyDebitTemp: ReceiptInvoiceModel[] = []
    checkAll = false;
    checkAllAgency = false;

    TYPELIST: string = 'LIST';

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

    constructor(
        private _sortService: SortService,
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _store: Store<IAppState>,
        private _toastService: ToastrService,
        private _activedRoute: ActivatedRoute
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortDebit;


    }

    ngOnInit() {
        this.customers = (this._catalogueRepo.customers$ as Observable<any>).pipe(pluck('data'));

        this.initForm();
        this.headers = [
            { title: 'Reference No', field: 'referenceNo', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Tax Code', field: 'taxCode', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Unpaid', field: 'unpaid', sortable: true },
            { title: 'Unpaid VND', field: 'unpaidAmountVnd', sortable: true },
            { title: 'Unpaid USD', field: 'unpaidAmountUsd', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Payment Term', field: 'paymentTerm', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Payment Status', field: 'paymentStatus', sortable: true },
            { title: 'Bu Handle', field: 'departmentName', sortable: true },
            { title: 'Office', field: 'officeName', sortable: true },
        ];

        this.headersAgency = [
            { title: 'Reference No', field: 'referenceNo', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'JOB', field: 'jobNo', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'PartnerId', field: 'taxCode', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Unpaid Amount', field: 'unpaid', sortable: true },
            { title: 'Unpaid VND', field: 'unpaidAmountVnd', sortable: true },
            { title: 'Unpaid USD', field: 'unpaidAmountUsd', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Payment Term', field: 'paymentTerm', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Payment Status', field: 'paymentStatus', sortable: true },
            { title: 'Bu Handle', field: 'departmentName', sortable: true },
            { title: 'Office', field: 'officeName', sortable: true },
        ];

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
    }

    getCustomer(type: string) {
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
    }

    onSelectDataFormInfo(data: any) {
        this.partnerId.setValue(data.id);
        if (this.partnerId.value !== this.customerFromReceipt) {
            this.listDebit = [];
        }
    }

    checkAllChange() {
        if (this.checkAll) {
            this.listDebit.forEach(x => {
                x.isSelected = true;
            });
        } else {
            this.listDebit.forEach(x => {
                x.isSelected = false;
            });
        }
    }

    onApply() {
        this.isSubmitted = true;
        if (this.formSearch.valid) {
            const body: IAcctCustomerDebitCredit = {
                partnerId: this.partnerId.value,
                searchType: this.typeSearch.value,
                referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
                fromDate: !!this.date.value?.startDate ? formatDate(this.date.value.startDate, 'yyyy-MM-dd', 'en') : null,
                toDate: !!this.date.value?.endDate ? formatDate(this.date.value?.endDate, 'yyyy-MM-dd', 'en') : null,
                dateType: this.dateType.value,
                service: this.service.value[0] === 'All' ? this.mapServiceId() : (this.service.value.length > 0 ? this.service.value.map((item: any) => item.id).toString().replace(/(?:,)/g, ';') : null)
            };
            this._progressRef.start();
            if (this.type === 'Customer') {
                this._accountingRepo.getDataIssueCustomerPayment(body).pipe(
                    catchError(this.catchError),
                    finalize(() => {
                        this._progressRef.complete();
                    })
                ).subscribe(
                    (res: ReceiptInvoiceModel[]) => {
                        if (!!res) {
                            this.listDebit = res || [];
                            this.filterList();
                        }
                    },
                );
            } else {
                this._accountingRepo.getDataIssueAgencyPayment(body).pipe(
                    catchError(this.catchError),
                    finalize(() => {
                        this._progressRef.complete();
                    })
                ).subscribe(
                    (res: AgencyReceiptModel) => {
                        if (!!res) {
                            this.agencyDebitModel = res;

                            // this.agencyDebitTemp = [];
                            // this.listDebit = [...this.agencyDebitModel.invoices];
                            // if (this.agencyDebitTemp.length > 0) {
                            //     this.listDebit = [...this.agencyDebitTemp];
                            // }
                            this.filterList();
                        }
                    },
                );
            }

        }
    }

    filterListCredit() {
        this._store.select(ReceiptCreditListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((result: any) => {
                if (!!result) {
                    this.listCreditInvoice = result || [];
                    this.listDebit = this.listDebit.filter(s =>
                        result.every(t => {
                            var key = Object.keys(t)[0];
                            return s[key] !== t[key];
                        }));
                    this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                        x.invoices.forEach(invoice => {
                            for (let i = 0; i < result.length; i++) {

                                if (result[i].isSelected === true && invoice.refNo === result[i].refNo && invoice.jobNo === result[i].jobNo && invoice.mbl === result[i].mbl && invoice.hbl === result[i].hbl) {
                                    invoice.isSelected = true;
                                }
                                if ((invoice.refNo === result[i].refNo && invoice.jobNo === result[i].jobNo && invoice.mbl === result[i].mbl && invoice.hbl === result[i].hbl) || invoice.refNo === result[i].refNo) {
                                    const index = x.invoices.indexOf(invoice);
                                    x.invoices.splice(index, 1);
                                }
                                if (invoice.isSelected === true && this.checkAllAgency) {
                                    const index = x.invoices.indexOf(invoice);
                                    x.invoices.splice(index, 1);
                                }
                            }
                        });
                    });
                    this.agencyDebitModel.groupShipmentsAgency = this.agencyDebitModel.groupShipmentsAgency.filter(x => x.invoices.length > 0);
                }
            });
    }

    filterListDebit() {
        this._store.select(ReceiptDebitListState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((result: any) => {
                if (!!result) {
                    this.listDebitInvoice = result || [];
                    this.listDebit = this.listDebit.filter(s =>
                        result.every(t => {
                            var key = Object.keys(t)[0];
                            return s[key] !== t[key];
                        }));

                    this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                        x.invoices.forEach(invoice => {
                            result.forEach(t => {
                                if (t.isSelected === true && invoice.refNo === t.refNo && invoice.jobNo === t.jobNo && invoice.mbl === t.mbl && invoice.hbl === t.hbl) {
                                    invoice.isSelected = true;
                                }
                                if (invoice.refNo === t.refNo && invoice.jobNo === t.jobNo && invoice.mbl === t.mbl && invoice.hbl === t.hbl) {
                                    const index = x.invoices.indexOf(invoice);
                                    x.invoices.splice(index, 1);
                                }
                                if (invoice.isSelected === true && this.checkAllAgency) {
                                    const index = x.invoices.indexOf(invoice);
                                    x.invoices.splice(index, 1);
                                }
                            })
                        });
                    });

                    this.agencyDebitModel.groupShipmentsAgency = this.agencyDebitModel.groupShipmentsAgency.filter(x => x.invoices.length > 0);

                }
            })
    }

    filterList() {
        this.filterListCredit();
        this.filterListDebit();
    }

    getAgencyDebitGroup() {
        const arr = [];
        this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
            for (let i = 0; i < x.invoices.length; i++) {
                arr.push(x.invoices[i]);
            }
        });
        return arr;
    }

    addToReceipt() {
        const partnerId = this.partnerId.value;
        let datatoReceiptGroup = [];
        let datatoReceiptList = [];
        let datatoReceipt = this.listDebit;
        if (this.TYPELIST === 'GROUP' && this.type === 'Agency') {
            datatoReceiptGroup = this.getAgencyDebitGroup();
            datatoReceipt = datatoReceiptGroup;
        }
        if (this.TYPELIST === 'LIST' && this.type === 'Agency') {
            datatoReceiptList = this.getAgencyDebitGroup();
            datatoReceipt = datatoReceiptList;
        }
        datatoReceipt = datatoReceipt.filter(x => x.isSelected === true);
        if (datatoReceipt.length === 0) {
            this._toastService.warning('No data to add receipt!');
            return;
        }

        if (this.customerFromReceipt !== this.partnerId.value
            && ((this.listDebitInvoice.length > 0 && this.listDebitInvoice[0].partnerId !== partnerId)
                || (this.listCreditInvoice.length > 0 && this.listCreditInvoice[0].partnerId !== partnerId)
            )) {
            this._store.dispatch(ResetInvoiceList());
        }

        this._store.dispatch(GetInvoiceListSuccess({ invoices: datatoReceipt }));
        this.onAddToReceipt.emit(this.partnerId.value);
        this.hide();
    }

    setDefaultValue() {
        if (this.customerFromReceipt !== this.partnerId.value) {
            this.listDebit = [];
        }
        if (!!this.dateFromReceipt && !!this.dateFromReceipt.startDate) {
            this.date.setValue({ startDate: new Date(this.dateFromReceipt.startDate), endDate: new Date(this.dateFromReceipt.endDate) });
        }
        if (!!this.customerFromReceipt) {
            this.partnerId.setValue(this.customerFromReceipt);
        }
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

    switchToGroup() {
        if (this.TYPELIST === 'GROUP') {
            this.TYPELIST = 'LIST';
        } else {
            this.TYPELIST = 'GROUP';
        }
    }


    sortDebit(sort: string) {
        this.listDebit = this._sortService.sort(this.listDebit, sort, this.order);
    }

    mapServiceId() {
        let serviceId = '';
        const serv = this.services.filter(service => service.id !== 'All');
        serviceId = serv.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
        return serviceId;
    }

    reset() {
        this.formSearch.reset();
        this.listDebit = [];
        this.agencyDebitModel = new AgencyReceiptModel();
        this.service.setValue([this.services[0].id]);
    }

    clearData() {
        this.partnerId.setValue(null);
    }

    removeAllChecked() {
        this.checkAll = false;
    }

    checkAllChangeAgency() {
        if (this.checkAllAgency) {
            //if (this.TYPELIST === 'GROUP') {
            this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                for (let i = 0; i < x.invoices.length; i++) {
                    x.invoices[i].isSelected = true;
                }

            })
        }
        else {
            this.agencyDebitModel.groupShipmentsAgency.forEach(x => {
                for (let i = 0; i < x.invoices.length; i++) {
                    x.invoices[i].isSelected = false;
                }
            })
        }
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
