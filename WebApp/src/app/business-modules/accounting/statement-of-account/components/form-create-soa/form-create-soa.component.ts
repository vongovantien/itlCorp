import { Component, Output, EventEmitter } from '@angular/core';
import { AppPage, IComboGirdConfig } from 'src/app/app.base';
import { SystemRepo } from 'src/app/shared/repositories';
import { GlobalState } from 'src/app/global-state';
import { catchError } from 'rxjs/operators';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import _ from 'lodash';
import { Charge } from 'src/app/shared/models';
import moment from 'moment';

@Component({
    selector: 'form-create-soa',
    templateUrl: './form-create-soa.component.html',
    styleUrls: ['./form-create-soa.component.scss']
})
export class StatementOfAccountFormCreateComponent extends AppPage {

    @Output() onApply: EventEmitter<any> = new EventEmitter<any>();

    configPartner: IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    charges: Charge[] = [];
    configCharge: IComboGirdConfig = {
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
    selectedUser: any = null;

    services: any[] = [];
    selectedService: any[] = [];
    note: string = '';

    items: any[] = [];

    isSubmited: boolean = false;

    constructor(
        private _sysRepo: SystemRepo,
        private _globalState: GlobalState
    ) {

        super();
    }

    ngOnInit() {
        this.initBasicData();
        this.getPartner();
        this.getCurrency();
        this.getUser();
        this.getCharge();
    }

    getPartner() {
        this._sysRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.ALL, inactive: false })
            .pipe(catchError(this.catchError))
            .subscribe(
                (dataPartner: any) => {
                    this.configPartner.dataSource = dataPartner;
                    this.configPartner.displayFields = [
                        { field: 'taxCode', label: 'Taxcode' },
                        { field: 'partnerNameEn', label: 'Name' },
                        { field: 'partnerNameVn', label: 'Customer Name' },
                    ];
                    this.configPartner.selectedDisplayFields = ['partnerNameEn'];
                },
                (errs: any) => {
                    console.log(errs + '');
                    // TODO handle errors
                },
                // complete
                () => { }
            );
    }

    getCurrency() {
        this._sysRepo.getListCurrency()
            .pipe(catchError(this.catchError))
            .subscribe(
                (dataCurrency: any) => {
                    this.currencyList = (dataCurrency).map((item: any) => ({ id: item.id, text: item.id }));
                    this.selectedCurrency = [this.currencyList.filter((curr) => curr.id === "VND")[0]];
                },
                (errs: any) => {
                    console.log(errs + '');
                    // TODO handle errors
                },
                // complete
                () => { }
            );
    }

    getUser() {
        this._sysRepo.getListSystemUser()
            .pipe(catchError(this.catchError))
            .subscribe(
                (dataUser: any) => {
                    this.users = (dataUser || []).map((item: any) => ({ id: item.id, text: item.id }));
                    this.selectedUser = [this.users.filter((i: any) => i.id === 'admin')[0]];
                },
                (errs: any) => {
                    console.log(errs + '');
                    // TODO handle errors
                },
                // complete
                () => { }
            );
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
            },
                (errs: any) => {
                    console.log(errs + '');
                    // TODO handle errors
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
            { text: 'Yes', id: 1 },
            { text: 'No', id: 2 }
        ];
        this.selectedObh = [this.obhs[1]];

        this.services = [
            { text: 'All', id: 'All' },
            { text: 'Logistic (operation)', id: 'LGO' },
            { text: "Air Import", id: "AI" },
            { text: "Air Export", id: "AE" },
            { text: "Sea Import", id: "SI" },
            { text: "Sea Export", id: "SE" },
            { text: "Sea FCL Export", id: "SFE" },
            { text: "Sea FCL Import", id: "SFI" },
            { text: "Sea LCL Export", id: "SLE" },
            { text: "Sea LCL Import", id: "SLI" },
            { text: "Sea Consol Export", id: "SCE" },
            { text: "Sea Consol Import", id: "SCI" },
            { text: "Inland Trucking", id: "IT" },
        ];
        this.selectedService = [this.services[0]];
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type.toLowerCase()) {
            case 'partner':
                this.selectedPartner = { field: data.partnerNameEn, value: data.id };
                break;
            case 'date-mode':
                this.selectedDateMode = [data];
                break;
            case 'type':
                this.selectedType = [data];
                break;
            case 'obh':
                this.selectedObh = [data];
                break;
            case 'office':
                break;
            case 'currency':
                this.selectedCurrency = [data];
                break;
            case 'service':
                // * reset selected charges & dataSource.
                this.selectedCharges = [];
                this.configCharge.dataSource = this.charges;

                if (data.id === 'All') {
                    this.selectedService = [];
                    this.selectedService.push({ id: 'All', text: "All" });

                    this.configCharge.dataSource = this.charges;
                } else {
                    this.selectedService.push(data);
                    this.detectServiceWithAllOption(data);

                    // ? filter charge when add service
                    this.configCharge.dataSource = this.filterChargeWithService(this.configCharge.dataSource, this.selectedService.map((service: any) => service.id));
                    this.configCharge.dataSource.push(new Charge({ code: 'All', id: 'All', chargeNameEn: 'All' }));
                }
                break;
            case 'user':
                this.selectedUser.push(data);
                break;
            case 'charge':
                if (data.id === 'All') {
                    this.selectedCharges = [];
                    this.selectedCharges.push({ id: 'All', code: 'All', chargeNameEn: 'All' });
                } else {
                    this.selectedCharges.push(data);
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

    onSearchCharge() {
        this.isSubmited = true;
        if (this.isSubmited && !this.selectedRangeDate.startDate || !this.selectedPartner.value) {
            return;
        } else {
            const body = {
                currencyLocal: 'VND', //todo: get currency local follow location or login info
                currency: this.selectedCurrency[0].id,
                customerID: this.selectedPartner.value || '',
                dateType: this.selectedDateMode[0].id,
                fromDate: moment(this.selectedRangeDate.startDate).format("YYYY-MM-DD"),
                toDate: moment(this.selectedRangeDate.endDate).format("YYYY-MM-DD"),
                type: this.selectedType[0].text,
                isOBH: this.selectedObh[0].id === 1 ? true : false,
                strCreators: this.selectedUser.map((item: any) => item.id).toString(),
                strCharges: this.selectedCharges.map((item: any) => item.code).toString(),
                note: this.note
            };
            this.onApply.emit(body);
        }

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
                if (_.includes(keys, key)) {
                    result.push(charge);
                }
            }
        }
        return _.uniq(result);
    }
}
