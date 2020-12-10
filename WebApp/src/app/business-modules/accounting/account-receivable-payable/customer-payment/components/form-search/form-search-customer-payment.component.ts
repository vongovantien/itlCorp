import { AppForm } from 'src/app/app.form';
import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { formatDate } from '@angular/common';
// import { getAdvancePaymentSearchParamsState, IAdvancePaymentState } from '../../store/reducers';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Partner, Currency, SOASearchCharge, User } from '@models';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';


import { forkJoin, Observable } from 'rxjs';
import { Moment } from 'moment';

import { Store } from '@ngrx/store';
import { catchError, map, takeUntil } from 'rxjs/operators';
import { DataService } from 'src/app/shared/services/data.service';
import { SortService } from '@services';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { data } from 'jquery';



enum OverDueDays {
    All,
    Between1_15,
    Between16_30,
    Between31_60,
    Between61_90
}
@Component({
    selector: 'customer-payment-form-search',
    templateUrl: './form-search-customer-payment.component.html',
})
export class ARCustomerPaymentFormSearchComponent extends AppForm implements OnInit {
    // @Output() onSearch:  EventEmitter<Partial<ISearchAccPayment>> = new EventEmitter<Partial<ISearchAccPayment>>();
    @Output() onChange: EventEmitter<any> = new EventEmitter<any>();

    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();
    selectedRange: any;

    reference: string = '';

    paymentRefNo: any[] = [];
    selectedPaymentRefNo: any = null;

    configPartner: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    partners: Partner[] = [];
    selectedPartner: any = {};

    users: User[] = [];
    userLogged: User;
    currentUser: User = null;

    dateType: any[] = [];
    selectedDateType: any = null;

    currencyList: Currency[] = [];
    selectedCurrency: any = null;

    syncStatus: any[] = [];
    selectedSyncStatus: any = null;



    constructor(
        private _sysRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
        private _dataService: DataService
    ) {
        super();
        this.requestReset = this.reset;
    }
    ngOnInit(): void {
        this.getBasicData();
        this.getDateType();
        this.getSyncStatus();
        this.getPaymentRefNo();
        this.getUserLogged();
    }



    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
    }

    getBasicData() {
        forkJoin([ // ? forkJoin like Promise.All
            this._catalogueRepo.getListCurrency(),
            this._sysRepo.getListSystemUser(),
            this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.ALL, active: true })
        ]).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                ([dataCurrency, dataSystemUser, dataPartner]: any) => {
                    this.partners = this.mapModel(dataPartner, Partner);
                    // * add all value into partners data
                    this.partners.push(new Partner({ taxCode: 'All', shortName: 'All', partnerNameEn: 'All' }));
                    this.selectedPartner = { field: 'partnerNameEn', value: 'All' };

                    this.partners = this._sortService.sort(this.partners, 'shortName', true);
                    this.currencyList = dataCurrency || [];
                    this.users = dataSystemUser || [];
                    // * set config for combogird
                    this.configPartner.dataSource = this.partners;
                    this.configPartner.displayFields = [
                        { field: 'taxCode', label: 'Taxcode' },
                        { field: 'shortName', label: 'Name (ABR)' },
                        { field: 'partnerNameEn', label: 'Customer Name [EN]' },
                    ];
                    this.configPartner.selectedDisplayFields = ['partnerNameEn'];

                    this._dataService.setData(SystemConstants.CSTORAGE.CURRENCY, dataCurrency);
                    this._dataService.setData(SystemConstants.CSTORAGE.PARTNER, dataPartner);
                    this._dataService.setData(SystemConstants.CSTORAGE.SYSTEM_USER, dataSystemUser);
                },
            );
        // console.log(this.getBasicData());
    }
    onSelectDataFormSearch(data: any, key: string) {
        switch (key.toLowerCase()) {
            case 'partner':
                this.selectedPartner = <any>{ field: data.partnerNameEn, value: data.id };
                break;
            case 'currency':
                this.selectedCurrency = data;
                break;
            case 'user':
                this.currentUser = data;
                break;
            case 'paymentrefno':
                this.selectedPaymentRefNo = data;
                break;
            case 'datetype':
                this.selectedDateType = data;
                break;
            case 'syncstatus':
                this.selectedSyncStatus = data;
                break;
            default:
                break;
        }

    }


    getPaymentRefNo() {
        this.paymentRefNo = [
            { title: 'Payment', name: 'Payment' },
            { title: 'Invoice', name: 'Invoice' }
        ];
    }
    getDateType() {
        this.dateType = [
            { title: 'Create Date', name: 'Create Date' },
            { title: 'Paid Date', name: 'Paid Date' },
            { title: 'Last Sync', name: 'Last Sync' },
        ];
    }
    getSyncStatus() {
        this.syncStatus = [
            { title: 'Synced', name: 'Synced' },
            { title: 'Rejected', name: 'Rejected' }
        ];
    }

    mapModel(data: any[], Model: any) {
        try {
            return (data || []).map((item: any) => new Model(item));
        } catch (error) {
            // Todo handle error.
            console.log(error + '');
        }
    }
    search() {
        const body = {
            strCodes: this.reference.replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').split(',').toString().trim(),
            customerID: (this.selectedPartner.value === 'All' ? '' : this.selectedPartner.value) || '',
            cpFromDateCreate: !!this.selectedRange.startDate ? formatDate(this.selectedRange.startDate, 'yyyy-MM-dd', 'en') : null,
            cpToDateCreate: !!this.selectedRange.endDate ? formatDate(this.selectedRange.endDate, 'yyyy-MM-dd', 'en') : null,
            cpCurrency: !!this.selectedCurrency ? this.selectedCurrency.id : null,
            cpPaymentRefNo: ! !this.selectedPaymentRefNo ? this.selectedPaymentRefNo.name : null,
            cpDateTpe: !!this.selectedDateType ? this.selectedDateType.name : null,
            cpSyncStatus: !!this.selectedSyncStatus ? this.selectedSyncStatus.name : null,
            // cpUserCreate: !!this.currentUser ? this.currentUser.id : null,
            CurrencyLocal: "VND"
        };
        this.onSearch.emit(body);
        console.log(body);

    }

    reset() {
        this.reference = '';
        this.selectedPartner = { field: 'partnerNameEn', value: 'All' };
        // this.currentUser = null;
        this.selectedRange = null;
        this.selectedCurrency = null;
        this.selectedPaymentRefNo = null;
        this.selectedDateType = null;
        this.selectedSyncStatus = null;
        // ? search again!
        this.onSearch.emit(<any>{ CurrencyLocal: "VND" });
    }
    // formSearch: FormGroup;

    // partnerId: AbstractControl;
    // referenceNo: AbstractControl;
    // issuedDate: AbstractControl;
    // Currency: AbstractControl;
    // etd: AbstractControl;
    // syncStatus: AbstractControl;
    // paymentRefNo: AbstractControl;
    // // userLogged: User;
    // currencies: Currency[] = [];
    // requesters: Currency[] = [];
    // currencyList: any[] = [];
    // selectedCurrency: any = null;
    // dataSearch: SOASearchCharge = new SOASearchCharge();


    // partners: Observable<Partner[]>;



    // selected: { startDate: Moment, endDate: Moment };
    // displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = [
    //     { field: 'accountNo', label: 'ID' },
    //     { field: 'shortName', label: 'Name ABBR' },
    //     { field: 'partnerNameVn', label: 'Name Local' },
    //     { field: 'taxCode', label: 'Tax Code' },
    // ];

    // paymentRef: string[] = ['Payment ', 'Invoice'];
    // dateType: string[] = ['Create Date', 'Paid Date', 'Last Sync'];
    // syncstatus: string[] = ['Synced', 'Rejected'];
    // currency: string[] = ['combobox'];
    // // overDueDays: CommonInterface.INg2Select[] = [
    // //     { id: '0', text: 'All' },
    // //     { id: 1, text: '01-15 days' },
    // //     { id: 2, text: '16-30 days' },
    // //     { id: 3, text: '31-60 days' },
    // //     { id: 4, text: '60-90 days' },
    // // ];

    // constructor(
    //     private _fb: FormBuilder,
    //     private _dataService: DataService,
    //     private _catalogueRepo: CatalogueRepo,
    //     private _systemRepo: SystemRepo,
    //     // private _store: Store<IAdvancePaymentState>,
    // ) {
    //     super();
    //     this.requestSearch = this.submitSearch;
    //     // this.requestReset = this.resetSearch;
    // }

    // ngOnInit(): void {
    //     this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
    //     this.initForm();
    //     this.getCurrency();
    // }

    // initForm() {
    //     this.formSearch = this._fb.group({
    //         referenceNo: [],
    //         partnerId: [],
    //         etd: [],
    //         Currency: [[this.currency[1]]],
    //         syncStatus: [[this.syncstatus[1], this.syncstatus[2]]],
    //         Date: [[this.dateType[1], this.dateType[2], this.dateType[3]]],
    //         paymentRefNo: [[this.paymentRef[1], this.paymentRef[2]]]
    //         //   etd: !!shipment.etd ? { startDate: new Date(shipment.etd), endDate: new Date(shipment.etd) } : null,

    //     });

    //     this.partnerId = this.formSearch.controls["partnerId"];
    //     this.issuedDate = this.formSearch.controls["issuedDate"];
    //     this.Currency = this.formSearch.controls["Currency"];
    //     this.etd = this.formSearch.controls["Date"];
    //     this.syncStatus = this.formSearch.controls["syncStatus"];
    //     this.paymentRefNo = this.formSearch.controls["paymentRefNo"];
    // }

    // getCurrencyAndUsers() {
    //     combineLatest([
    //         this._catalogueRepo.getListCurrency(),
    //         this._systemRepo.getSystemUsers({}),
    //         this._store.select(getAdvancePaymentSearchParamsState)
    //     ]).pipe(
    //         map((cur, param) => ({ ...cur, param }))
    //     ).subscribe(
    //         (res) => {
    //             this.currencies = res[0] || [];
    //             this.requesters = res[1];

    //             if (Object.keys(res[2]).length === 0 && res[2].constructor === Object) {
    //                 this.requester.setValue(this.userLogged.id);
    //                 this.currencyId.setValue(null);
    //             } else {
    //                 const requesterTemp = this.requesters.find(e => e.id === res[2].requester);
    //                 const currencyTemp = !this.currencies.find(e => e.id === res[2].currencyId) ? null
    //                     : this.currencies.find(e => e.id === res[2].currencyId);

    //                 this.requester.setValue(requesterTemp.id);
    //                 this.currencyId.setValue(currencyTemp);

    //             }
    //         }
    //     );
    // }


    // getCurrency() {
    //     if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY)) {
    //         this.getCurrencyData(this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY));
    //     } else {
    //         this._catalogueRepo.getListCurrency()
    //             .pipe(catchError(this.catchError))
    //             .subscribe(
    //                 (dataCurrency: any) => {
    //                     this.getCurrencyData(dataCurrency);
    //                 },
    //             );
    //     }
    // }

    // getCurrencyData(data: any) {
    //     this.currencyList = (data).map((item: any) => ({ id: item.id, text: item.id }));
    //     this.selectedCurrency = this.currencyList.filter((curr) => curr.id === "VND")[0];
    //     this.updateDataSearch('currency', this.selectedCurrency.id);
    //     this.updateDataSearch('currencyLocal', 'VND');
    // }
    // updateDataSearch(key: string, data: any) {
    //     this.dataSearch[key] = data;
    //     this.onChange.emit({ key: key, data: data });
    // }


    // // tslint:disable-next-line:no-any
    // onSelectDataFormInfo(data: any, type: string) {
    //     switch (type) {
    //         case 'partner':
    //             this.partnerId.setValue((data as Partner).id);
    //             break;
    //         default:
    //             break;
    //     }
    // }

    // submitSearch() {

    //     // const dataForm: { [key: string]: any } = this.formSearch.getRawValue();
    //     // const status = !!dataForm.paymentStatus ? this.getSearchStatus(dataForm.paymentStatus) : null;
    //     // const body: ISearchAccPayment = {
    //     //     referenceNos: !!dataForm.referenceNo ? dataForm.referenceNo.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: any) => item.trim()) : null,
    //     //     partnerId: dataForm.partnerId,
    //     //     paymentStatus: status,
    //     //     fromIssuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.startDate) ? formatDate(this.issuedDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
    //     //     toIssuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.endDate) ? formatDate(this.issuedDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
    //     //     fromUpdatedDate: (!!dataForm.updatedDate && !!dataForm.updatedDate.startDate) ? formatDate(dataForm.updatedDate.startDate, 'yyyy-MM-dd', 'en') : null,
    //     //     toUpdatedDate: (!!dataForm.updatedDate && !!dataForm.updatedDate.endDate) ? formatDate(dataForm.updatedDate.endDate, 'yyyy-MM-dd', 'en') : null,
    //     //     fromDueDate: (!!dataForm.dueDate && !!dataForm.dueDate.startDate) ? formatDate(dataForm.dueDate.startDate, 'yyyy-MM-dd', 'en') : null,
    //     //     toDueDate: (!!dataForm.dueDate && !!dataForm.dueDate.endDate) ? formatDate(dataForm.dueDate.endDate, 'yyyy-MM-dd', 'en') : null,
    //     //     // paymentType: PaymentType.Invoice
    //     // };

    //     // this.onSearch.emit(body);
    // }
    // getSearchStatus(paymentStatus: []) {
    //     let strStatus = null;
    //     if (!!paymentStatus) {
    //         strStatus = [];

    //         paymentStatus.forEach(element => {
    //             if (element !== 'All') {
    //                 strStatus.push(element);
    //             } else {
    //                 return [];
    //             }
    //         });
    //     }
    //     return strStatus;

    // }

    // resetSearch() {
    //     this.formSearch.reset();
    //     this.initForm();
    //     // this.onSearch.emit({ paymentStatus: this.getSearchStatus(this.paymentStatus.value), paymentType: PaymentType.Invoice, overDueDays: OverDueDays.All });
    // }

    // // selelectedStatus(event: string) {
    // //     const currStatus = this.paymentRefNo.value;
    // //     if (currStatus.filter(x => x === 'Payment').length > 0 && event !== 'Payment') {
    // //         currStatus.splice(0);
    // //         currStatus.push(event);
    // //         this.paymentRefNo.setValue(currStatus);

    // //     }
    // //     if (event === 'Payment') {
    // //         this.paymentRefNo.setValue(['Payment']);
    // //     }

    // // }
}
