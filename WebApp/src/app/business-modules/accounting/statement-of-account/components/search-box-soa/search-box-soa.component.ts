import { Component, EventEmitter, Output } from '@angular/core';
import { formatDate } from '@angular/common';
import { forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { Currency, Partner, User } from 'src/app/shared/models';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { SortService, DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { AppForm } from '@app';
import { Store } from '@ngrx/store';
import { IAppState } from '@store';
import { SearchListSOA } from '../../store/actions';
import { getDataSearchSOAState, getSOAPagingState } from '../../store/reducers';
@Component({
    selector: 'soa-search-box',
    templateUrl: './search-box-soa.component.html',
})
export class StatementOfAccountSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();
    selectedRange: any;
    configPartner: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [{ field: 'taxCode', label: 'Taxcode' },
        { field: 'shortName', label: 'Name (ABR)' },
        { field: 'partnerNameEn', label: 'Customer Name [EN]' },],
        dataSource: [],
        selectedDisplayFields: ['partnerNameEn'],
    };
    partners: Partner[] = [];
    selectedPartner: any = { field: 'partnerNameEn', value: 'All' };;
    currencyList: any[] = [
        { id: 'VND', value: 'VND' },
        { id: 'USD', value: 'USD' },
    ];
    selectedCurrency: any = null;
    users: User[] = [];
    configUser: CommonInterface.IComboGirdConfig = {
        placeholder: 'Select Personal Handle',
        displayFields: [{ field: 'username', label: 'User Name' },
        { field: 'employeeNameVn', label: 'Full Name' },],
        dataSource: [],
        selectedDisplayFields: ['username'],
    };
    userLogged: User;
    currentUser: any = null;
    statusSOA: any[] = [
        { title: 'New', name: 'New' },
        { title: 'Issued Voucher', name: 'Issued Voucher' },
        { title: 'Issued Invoice', name: 'Issued Invoice' },
        { title: 'Request Confirmed', name: 'Request Confirmed' },
        { title: 'Confirmed ', name: 'Confirmed ' },
        { title: 'Need Revise', name: 'Need Revise' },
        { title: 'Done', name: 'Done' },
    ];
    selectedStatus: any = null;
    reference: string = '';
    constructor(
        private _sysRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
        private _dataService: DataService,
        private _store: Store<IAppState>,
    ) {
        super();
        this.requestReset = this.reset;
    }
    ngOnInit(): void {
        //this.getBasicData();
        this.getPartner();
        //this.getStatus();
        this.getCurrency();
        this.getUser();
        this.getUserLogged();
        this.subscriptionSearchParamState();
    }
    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
    }
    subscriptionSearchParamState() {
        this._store.select(getDataSearchSOAState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    if (!!data) {
                        if (data.dataSearch !== undefined) {
                            if (!!data.dataSearch.soaFromDateCreate && !!data.dataSearch.soaToDateCreate) {
                                this.selectedRange = {};
                                this.selectedRange.startDate = new Date(data.dataSearch?.soaFromDateCreate);
                                this.selectedRange.endDate = new Date(data.dataSearch?.soaToDateCreate);
                            }
                            //this.selectedStatus = { title: data.dataSearch?.soaStatus, name: data.dataSearch?.soaStatus };
                            this.reference = !!data.dataSearch.strCodes ? data.dataSearch.strCodes : "";
                            this.selectedPartner = Object.assign({}, !!!data.dataSearch.customerID ? { field: 'partnerNameEn', value: 'All' } : null);
                            this.selectedStatus = !!data.dataSearch.soaStatus ? this.statusSOA.filter((soa) => soa.name === data.dataSearch.soaStatus)[0] : null;
                            this.selectedCurrency = !!data.dataSearch.soaCurrency ? this.currencyList.filter((cur) => cur.id === data.dataSearch.soaCurrency)[0] : null;

                            if (data.dataSearch.soaUserCreate !== null) {
                                this.currentUser = Object.assign({}, data.dataSearch.soaUserCreate !== null ? { field: 'id', value: data.dataSearch.soaUserCreate } : { field: 'id', value: null });
                            }
                        }
                    }
                }
            );
    }
    getPartner() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.ALL, active: true })
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (dataPartner: any) => {
                    this.partners = this.mapModel(dataPartner, Partner);
                    this.partners.push(new Partner({ taxCode: 'All', shortName: 'All', partnerNameEn: 'All' }));
                    this.partners = this._sortService.sort(this.partners, 'shortName', true);
                    this.configPartner.dataSource = this.partners;
                    this._dataService.setData(SystemConstants.CSTORAGE.PARTNER, dataPartner);
                }
            )
    }
    getCurrency() {
        this._catalogueRepo.getListCurrency()
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (currencies: any) => {
                    //this.currencyList = currencies || [];
                    this._dataService.setData(SystemConstants.CSTORAGE.CURRENCY, currencies);
                    //console.log(currencies);
                }
            )
    }
    getUser() {
        this._sysRepo.getListSystemUser()
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (user: any) => {
                    //this.users = user || [];
                    this.users = this.mapModel(user, User);
                    this.configUser.dataSource = this.users;
                    this._dataService.setData(SystemConstants.CSTORAGE.CURRENCY, user);
                }
            )
    }
    // getBasicData() {
    //     forkJoin([ // ? forkJoin like Promise.All
    //         //this._catalogueRepo.getListCurrency(),
    //         this._sysRepo.getListSystemUser(),
    //         //this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.ALL, active: true })
    //     ]).pipe(takeUntil(this.ngUnsubscribe))
    //         .subscribe(
    //             ([ dataSystemUser]: any) => {
    //                 //this.partners = this.mapModel(dataPartner, Partner);
    //                 // * add all value into partners data
    //                 //this.partners.push(new Partner({ taxCode: 'All', shortName: 'All', partnerNameEn: 'All' }));
    //                 // this.partners = this._sortService.sort(this.partners, 'shortName', true);
    //                 //this.currencyList = dataCurrency || [];
    //                 this.users = dataSystemUser || [];
    //                 // * set config for combogird
    //                 // this.configPartner.dataSource = this.partners;
    //                 // this.configPartner.displayFields = [
    //                 //     { field: 'taxCode', label: 'Taxcode' },
    //                 //     { field: 'shortName', label: 'Name (ABR)' },
    //                 //     { field: 'partnerNameEn', label: 'Customer Name [EN]' },
    //                 // ];
    //                 //this.configPartner.selectedDisplayFields = ['partnerNameEn'];
    //                 // this._dataService.setData(SystemConstants.CSTORAGE.CURRENCY, dataCurrency);
    //                 // this._dataService.setData(SystemConstants.CSTORAGE.PARTNER, dataPartner);
    //                 this._dataService.setData(SystemConstants.CSTORAGE.SYSTEM_USER, dataSystemUser);
    //             },
    //         );
    // }
    onSelectDataFormSearch(data: any, key: string) {
        switch (key.toLowerCase()) {
            case 'partner':
                this.selectedPartner = <any>{ field: data.partnerNameEn, value: data.id };
                break;
            case 'currency':
                this.selectedCurrency = data;
                break;
            case 'user':
                this.currentUser = <any>{ field: data.username, value: data.id };
                break;
            case 'status':
                this.selectedStatus = data;
                break;
            default:
                break;
        }
    }
    mapModel(data: any[], Model: any) {
        try {
            return (data || []).map((item: any) => new Model(item));
        } catch (error) {
            // Todo handle error.
        }
    }
    search() {
        const body = {
            strCodes: this.reference.replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').split(',').toString().trim(),
            customerID: (this.selectedPartner.value === 'All' ? '' : this.selectedPartner.value) || '',
            soaFromDateCreate: !!this.selectedRange.startDate ? formatDate(this.selectedRange.startDate, 'yyyy-MM-dd', 'en') : null,
            soaToDateCreate: !!this.selectedRange.endDate ? formatDate(this.selectedRange.endDate, 'yyyy-MM-dd', 'en') : null,
            soaStatus: !!this.selectedStatus ? this.selectedStatus.name : null,
            soaCurrency: !!this.selectedCurrency ? this.selectedCurrency.value : null,
            soaUserCreate: !!this.currentUser ? this.currentUser.value : null,
            CurrencyLocal: "VND",
            isSearching: true
        };
        this._store.dispatch(SearchListSOA({ dataSearch: body }));

        // this.onSearch.emit(body);
    }
    // * reset data in form search
    reset() {
        this.reference = '';
        this.selectedPartner = { field: 'partnerNameEn', value: 'All' };
        this.currentUser = null;
        this.selectedStatus = null;
        this.selectedRange = null;
        this.selectedCurrency = null;
        this.currentUser = null;
        // ? search again!
        this.onSearch.emit(<any>{ CurrencyLocal: "VND" });
        this.resetStore();
    }
    resetStore() {
        const bodyEmpty = {
            strCodes: '',
            customerID: '',
            soaFromDateCreate: null,
            soaToDateCreate: null,
            soaStatus: null,
            soaCurrency: null,
            soaUserCreate: null,
            CurrencyLocal: "VND"
        };
        this._store.dispatch(SearchListSOA({ dataSearch: bodyEmpty }));
    }
    resetDate() {
        this.selectedRange = null;
    }
    resetPersonalHandle() {
        this.currentUser = null;
    }
}
