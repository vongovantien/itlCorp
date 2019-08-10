import { Component, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { SystemRepo, AccoutingRepo } from 'src/app/shared/repositories';
import { SortService, DataService } from 'src/app/shared/services';
import { StatementOfAccountAddChargeComponent } from '../components/poup/add-charge/add-charge.popup';
import { ToastrService } from 'ngx-toastr';
import { Charge, SOASearchCharge } from 'src/app/shared/models';
import { formatDate } from '@angular/common';
import _includes from 'lodash/includes';
import _uniq from 'lodash/uniq';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-statement-of-account-new',
    templateUrl: './add-new-soa.component.html',
    styleUrls: ['./add-new-soa.component.scss'],
})
export class StatementOfAccountAddnewComponent extends AppList {

    @ViewChild(StatementOfAccountAddChargeComponent, { static: false }) addChargePopup: StatementOfAccountAddChargeComponent;

    configPartner: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    charges: Charge[] = [];
    configCharge: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    selectedRangeDate: any = null;

    selectedPartner: any = {};
    selectedCharge: any = {};
    selectedCharges: any[] = []; // for multiple select

    dateModes: any[] = [];
    selectedDateMode: any = null;

    types: any = [];
    selectedType: any = null;

    obhs: any = [];
    selectedObh: any = null;

    currencyList: any[] = [];
    selectedCurrency: any = null;

    users: any = [];
    selectedUser: any = [];

    services: any[] = [];
    selectedService: any[] = [];

    note: string = '';

    dataSearch: SOASearchCharge = new SOASearchCharge();

    listCharges: any[] = [];
    headers: CommonInterface.IHeaderTable[];

    totalShipment: number = 0;
    totalCharge: number = 0;

    isCollapsed: boolean = true;
    isApplied: boolean = false;
    isCheckAllCharge: boolean = false;

    dataCharge: any = null;

    constructor(
        private _sysRepo: SystemRepo,
        private _sortService: SortService,
        private _accountRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _dataService: DataService
    ) {
        super();
        this.requestList = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'Debit', field: 'debit', sortable: true },
            { title: 'Credit', field: 'credit', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Services Date', field: 'serviceDate', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
        this.initBasicData();
        this.getPartner();
        this.getCurrency();
        this.getUser();
        this.getCharge();
        this.getService();
    }

    addMoreCharge() {
        this.isApplied = true;
        if (this.isApplied && !this.selectedRangeDate.startDate || !this.selectedPartner.value) {
            return;
        } else {
            this.addChargePopup.searchInfo = this.dataSearch;
            this.addChargePopup.getListShipmentAndCDNote(this.dataSearch);

            this.addChargePopup.charges = this.charges;
            this.addChargePopup.configCharge = this.configCharge;

            this.addChargePopup.show({ backdrop: 'static' });
            
        }
    }

    getPartner() {
        this._dataService.getDataByKey(SystemConstants.CSTORAGE.PARTNER)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError)
            )
            .subscribe(
                (data: any) => {
                    if (!data) {
                        this._sysRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.ALL, inactive: false })
                            .pipe(catchError(this.catchError))
                            .subscribe(
                                (dataPartner: any) => {
                                    this.getPartnerData(dataPartner)
                                },
                                (errors: any) => {
                                    this.handleError(errors);
                                },
                                // complete
                                () => { }
                            );
                    } else {
                        this.getPartnerData(data);
                    }
                }
            );
    }

    getPartnerData(data: any) {
        this.configPartner.dataSource = data;
        this.configPartner.displayFields = [
            { field: 'taxCode', label: 'Taxcode' },
            { field: 'partnerNameEn', label: 'Name' },
            { field: 'partnerNameVn', label: 'Customer Name' },
        ];
        this.configPartner.selectedDisplayFields = ['partnerNameEn'];
    }

    getCurrencyData(data: any) {
        this.currencyList = (data).map((item: any) => ({ id: item.id, text: item.id }));
        this.selectedCurrency = this.currencyList.filter((curr) => curr.id === "VND")[0];
        this.updateDataSearch('currency', this.selectedCurrency.id);
        this.updateDataSearch('currencyLocal', 'VND');
    }

    getCurrencyUser(data: any) {
        this.users = (data || []).map((item: any) => ({ id: item.id, text: item.id }));
        this.selectedUser = [this.users.filter((i: any) => i.id === 'admin')[0]];

        this.updateDataSearch('strCreators', this.selectedUser.map((item: any) => item.id).toString());
    }

    getService() {
        this._sysRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {

                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        this.services.unshift({ id: 'All', text: 'All' });

                        this.selectedService = [this.services[0]];
                    } else {
                        this.handleError();
                    }
                },
                (errors: any) => {
                    this.handleError(errors);
                },
                () => { }
            );
    }

    getCurrency() {
        this._dataService.getDataByKey(SystemConstants.CSTORAGE.CURRENCY)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError)
            )
            .subscribe(
                (data: any) => {
                    if (!!data) {
                        this.getCurrencyData(data);
                    } else {
                        this._sysRepo.getListCurrency()
                            .pipe(catchError(this.catchError))
                            .subscribe(
                                (dataCurrency: any) => {
                                    this.getCurrencyData(dataCurrency)
                                },
                                (errors: any) => {
                                    this.handleError(errors);
                                },
                                // complete
                                () => { }
                            );
                    }
                }
            );
    }

    getUser() {
        this._dataService.getDataByKey(SystemConstants.CSTORAGE.SYSTEM_USER)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError)
            )
            .subscribe(
                (data: any) => {
                    if (!!data) {
                        this.getCurrencyUser(data);
                    } else {
                        this._sysRepo.getListSystemUser()
                            .pipe(catchError(this.catchError))
                            .subscribe(
                                (dataUser: any) => {
                                    this.getCurrencyUser(dataUser);
                                },
                                (errors: any) => {
                                    this.handleError(errors);
                                },
                                // complete
                                () => { }
                            );
                    }
                }
            )
        
    }

    getCharge() {
        this._sysRepo.getListCharge()
            .pipe(catchError(this.catchError))
            .subscribe((data) => {
                this.charges = data;
                this.charges.push(new Charge({ code: 'All', id: 'All', chargeNameEn: 'All' }));

                this.configCharge.dataSource = data || [];
                this.configCharge.displayFields = [
                    { field: 'code', label: 'Charge Code' },
                    { field: 'chargeNameEn', label: 'Charge Name EN ' },
                ];
                this.configCharge.selectedDisplayFields = ['code'];

                this._dataService.setData(SystemConstants.CSTORAGE.CHARGE, data || []);
            },
                (errors: any) => {
                    this.handleError(errors);
                },
                // complete
                () => { }
            );
    }

    initBasicData() {
        this.dateModes = [
            { text: 'Created Date', id: 'CreatedDate' },
            { text: 'Service Date', id: 'ServiceDate' },
            { text: 'Invoice Issued Date', id: 'InvoiceIssuedDate' },
        ];
        this.selectedDateMode = [this.dateModes[0]];

        this.types = [
            { id: 1, text: 'All' },
            { text: 'Debit', id: 2 },
            { text: 'Credit', id: 3 },
        ];
        this.selectedType = [this.types[0]];

        this.obhs = [
            { text: 'Yes', id: true },
            { text: 'No', id: false }
        ];
        this.selectedObh = this.obhs[1];
        this.updateDataSearch('isOBH', this.selectedObh.id);
        this.updateDataSearch('dateType', this.selectedDateMode[0].id);
        this.updateDataSearch('type', this.selectedType[0].text);
    }

    updateDataSearch(key: string, data: any) {
        this.dataSearch[key] = data;
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type.toLowerCase()) {
            case 'partner':
                this.selectedPartner = { field: data.partnerNameEn, value: data.id };
                this.updateDataSearch('customerID', this.selectedPartner.value);
                break;
            case 'date-mode':
                this.selectedDateMode = [data];
                this.updateDataSearch('dateType', this.selectedDateMode[0].id);
                break;
            case 'type':
                this.selectedType = [data];
                this.updateDataSearch('type', this.selectedType[0].text);
                break;
            case 'obh':
                this.selectedObh = data;
                this.updateDataSearch('isOBH', this.selectedObh.id);
                break;
            case 'currency':
                this.selectedCurrency = data;
                this.updateDataSearch('currency', this.selectedCurrency.id);
                break;
            case 'service':
                // * reset selected charges & dataSource.
                this.selectedCharges = [];
                this.configCharge.dataSource = this.charges;

                if (data.id === 'All') {
                    this.selectedService = [];
                    this.selectedService.push({ id: 'All', text: "All" });

                    this.configCharge.dataSource = this.charges;
                    this.updateDataSearch('serviceTypeId', "");

                } else {
                    this.selectedService.push(data);
                    this.detectServiceWithAllOption(data);

                    // ? filter charge when add service
                    this.configCharge.dataSource = this.filterChargeWithService(this.configCharge.dataSource, this.selectedService.map((service: any) => service.id));
                    this.configCharge.dataSource.push(new Charge({ code: 'All', id: 'All', chargeNameEn: 'All' }));

                    this.updateDataSearch('serviceTypeId', this.selectedService.map((service: any) => service.id));
                }

                break;
            case 'user':
                this.selectedUser = [];
                this.selectedUser.push(...data);
                this.updateDataSearch('strCreators', this.selectedUser.map((item: any) => item.id).toString());
                break;
            case 'charge':
                if (data.id === 'All') {
                    this.selectedCharges = [];
                    this.selectedCharges.push({ id: 'All', code: 'All', chargeNameEn: 'All' });
                } else {
                    this.selectedCharges.push(data);
                    this.selectedCharges = [...new Set(this.selectedCharges)];

                    this.detectChargeWithAllOption(data);
                }
                break;
            default:
                break;
        }
    }

    onRemoveService(data: any) {
        this.selectedService.splice(this.selectedService.findIndex((item: any) => item.id === data.id), 1);
        this.detectServiceWithAllOption();

        // ! filter charge when delete service
        this.configCharge.dataSource = this.filterChargeWithService(this.configCharge.dataSource, this.selectedService.map((service: any) => service.id));

    }

    onRemoveUser(data: any) {
        this.selectedUser.splice(this.selectedUser.findIndex((item: any) => item.id === data.id), 1);
    }

    onRemoveCharge(index: number = 0) {
        this.selectedCharges.splice(index, 1);
    }

    detectServiceWithAllOption(data?: any) {
        if (!this.selectedService.every((value: any) => value.id !== 'All')) {
            this.selectedService.splice(this.selectedService.findIndex((item: any) => item.id === 'All'), 1);

            this.selectedService = [];
            this.selectedService.push(data);
        }
    }

    detectChargeWithAllOption(data?: any) {
        if (!this.selectedCharges.every((value: any) => value.id !== 'All')) {
            this.selectedCharges.splice(this.selectedCharges.findIndex((item: any) => item.id === 'All'), 1);

            this.selectedCharges = [];
            this.selectedCharges.push(data);
        }
    }

    onApplySearchCharge() {
        this.isApplied = true;
        if (this.isApplied && !this.selectedRangeDate.startDate || !this.selectedPartner.value) {
            return;
        } else {
            const body = {
                currencyLocal: 'VND', // Todo: get currency local follow location or login info
                currency: this.selectedCurrency.id,
                customerID: this.selectedPartner.value || '',
                dateType: this.selectedDateMode[0].id,
                fromDate: formatDate(this.selectedRangeDate.startDate, 'yyyy-MM-dd', 'vi'),
                toDate: formatDate(this.selectedRangeDate.endDate, 'yyyy-MM-dd', 'vi'),
                type: this.selectedType[0].text,
                isOBH: this.selectedObh.id,
                strCreators: this.selectedUser.map((item: any) => item.id).toString(),
                strCharges: this.selectedCharges.map((item: any) => item.code).toString(),
                note: this.note,
                serviceTypeId: !!this.selectedService.length ? this.mapServiceId(this.selectedService[0].id) : this.mapServiceId('All'),
            };
            this.dataSearch = new SOASearchCharge(body);
            this.searchChargeWithDataSearch(this.dataSearch);
        }
    }

    onChangeNote(note: string) {
        this.dataSearch.note = note;
        this.updateDataSearch('note', note);
    }

    filterChargeWithService(charges: any[], keys: any[]) {
        const result: any[] = [];
        for (const charge of charges) {
            if (charge.hasOwnProperty('serviceTypeId')) {
                if (typeof (charge.serviceTypeId) !== 'object') {
                    charge.serviceTypeId = charge.serviceTypeId.split(";").filter((i: string) => Boolean(i));
                }
            }
            for (const key of charge.serviceTypeId) {
                if (_includes(keys, key)) {
                    result.push(charge);
                }
            }
        }
        return _uniq(result);
    }

    sortLocal(sortField?: string, order?: boolean) {
        this.listCharges = this._sortService.sort(this.listCharges, sortField, order);
    }

    onChangeCheckBoxCharge($event: Event) {
        this.isCheckAllCharge = this.listCharges.every((item: any) => item.isSelected);
    }

    checkUncheckAllCharge() {
        for (const charge of this.listCharges) {
            charge.isSelected = this.isCheckAllCharge;
        }
    }

    onCreateSOA() {
        if (!this.listCharges.length) {
            this._toastService.warning(`SOA Don't have any charges in this period, Please check it again! `, '', { positionClass: 'toast-bottom-right' });
            return;
        } else {

            const body = {
                surchargeIds: this.listCharges.map((item: any) => item.id),
                soaformDate: this.dataSearch.fromDate,
                soatoDate: this.dataSearch.toDate,
                currency: this.dataSearch.currency,
                note: this.dataSearch.note,
                dateType: this.dataSearch.dateType,
                serviceTypeId: !!this.selectedService.length ? this.mapServiceId(this.selectedService[0].id) : this.mapServiceId('All'),
                customer: this.dataSearch.customerID,
                type: this.dataSearch.type,
                obh: this.dataSearch.isOBH,
                creatorShipment: this.dataSearch.strCreators
            };
            this._accountRepo.createSOA(body)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.status) {
                            this._toastService.success(res.message, '', { positionClass: 'toast-bottom-right' });
                            //  * go to detail page
                            this._router.navigate(['home/accounting/statement-of-account/detail'], { queryParams: { no: res.data.soano, currency: 'VND' } });

                        } else {
                            this._toastService.error(res, '', { positionClass: 'toast-bottom-right' });
                        }
                    },
                    (errors: any) => {
                        this.handleError(errors);
                    },
                    () => { }
                );
        }

    }

    searchChargeWithDataSearch(dataSearch: any) {
        this.isLoading = true;
        this.dataSearch = dataSearch;
        this._accountRepo.getListChargeShipment(dataSearch)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (res: any) => {
                    this.dataCharge = res;
                    this.listCharges = res.chargeShipments || [];
                    this.totalCharge = res.totalCharge;
                    this.totalShipment = res.totalShipment;

                    this.updateDataSearch('chargeShipments', this.listCharges);
                },
                (errors: any) => {
                    this.handleError(errors);
                },
                () => { }
            );
    }

    onChangeRangeDate(rangeDate: any) {
        if (!!rangeDate.startDate) {
            this.updateDataSearch('fromDate', formatDate(rangeDate.startDate, 'yyyy-MM-dd', 'vi'));
        }
        if (!!rangeDate.endDate) {
            this.updateDataSearch('toDate', formatDate(rangeDate.endDate, 'yyyy-MM-dd', 'vi'));
        }
    }

    removeCharge() {
        this.listCharges = this.listCharges.filter((charge: any) => !charge.isSelected);
    }

    handleError(errors?: any) {
        let message: string = 'Has Error Please Check Again !';
        let title: string = '';
        if (errors instanceof HttpErrorResponse) {
            message = errors.message;
            title = errors.statusText;
        }
        this._toastService.error(message, title, { positionClass: 'toast-bottom-right' });
    }

    onUpdateMoreSOA(data: any) {
        this.dataCharge = data;
        this.listCharges = data.chargeShipments || [];

        this.totalCharge = data.totalCharge;
        this.totalShipment = data.shipment;
    }

    mapServiceId(service: any = 'All') {
        let serviceTypeId = '';
        if (!!service) {
            if (service === 'All') {
                this.services.shift(); // * remove item with value 'All'
                serviceTypeId = this.services.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
            } else {
                serviceTypeId = this.selectedService.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
            }
        } else {
            this.services.shift(); // * remove item with value 'All'
            serviceTypeId = this.services.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
        }

        return serviceTypeId;
    }
}




