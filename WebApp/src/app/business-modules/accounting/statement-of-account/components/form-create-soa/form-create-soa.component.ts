import { Component, Output, EventEmitter } from '@angular/core';
import { AppPage, IComboGirdConfig } from 'src/app/app.base';
import { SystemRepo } from 'src/app/shared/repositories';
import { GlobalState } from 'src/app/global-state';
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
        const partners: any[] = [
            { code: 'TAX CODE 1', name: 'Name Abbr 1', nameEn: 'Name EN 1' },
            { code: 'TAX CODE 2', name: 'Name Abbr 2', nameEn: 'Name EN 2' },
            { code: 'TAX CODE 3', name: 'Name Abbr 3', nameEn: 'Name EN 3' },
        ];
        this.configPartner.dataSource.push(...partners);
        this.configPartner.displayFields = [
            { field: 'code', label: 'Taxcode' },
            { field: 'name', label: 'Name' },
            { field: 'nameEn', label: 'Customer Name' },
        ];
        this.configPartner.selectedDisplayFields = ['code'];
    }

    getCharge() {
        const charges: any[] = [
            { code: 'All', name: 'All', id: -1 },
            { code: Math.random(), name: 'Name Abbr 1', id: Math.random() },
            { code: Math.random(), name: 'Name Abbr 2', id: Math.random() },
            { code: Math.random(), name: 'Name Abbr 3', id: Math.random() },
        ];
        this.configCharge.dataSource.push(...charges);
        this.configCharge.displayFields = [
            { field: 'code', label: 'Charge Code' },
            { field: 'name', label: 'Charge Name EN ' },
        ];
        this.configCharge.selectedDisplayFields = ['code'];
    }

    initBasicData() {
        this.dateModes = [
            { text: 'Created Date', id: 1 },
            { text: 'Service Date', id: 2 },
            { text: 'Invoice Issued Date', id: 3 },
        ];

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
            { text: 'All', id: -1 },
            { text: 'Logistic (operation)', id: 1 },
            { text: 'Air Import', id: 2 },
            { text: 'Air Export', id: 3 },
            { text: 'Sea FCL Export', id: 4 },
            { text: 'Sea LCL Export', id: 5 },
            { text: 'Sea LCL Import', id: 6 },
            { text: 'Sea Consol Export', id: 7 },
            { text: 'Sea Consol Import ', id: 9 },
            { text: 'Trucking Inland', id: 8 },
        ];
        this.selectedService = [this.services[0]];
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type.toLowerCase()) {
            case 'partner':
                this.selectedPartner = { field: 'code', value: data.code };
                console.log(this.selectedPartner);
                break;
            case 'date-mode':
                this.selectedDateMode = data;
                break;
            case 'type':
                this.selectedType = [data];
                break;
            case 'obh':
                this.selectedObh = [data];
                break;
            case 'office':
                break;
            case 'service':
                if (data.id < 0) {
                    this.selectedService = [];
                    this.selectedService.push({ "id": -1, "text": "All" });
                } else {
                    this.selectedService.push(data);
                    this.detectServiceWithAllOption(data);
                }
                break;
            case 'user':
                this.selectedUser.push(data);
                break;
            case 'charge':
                this.selectedCharges.push(data);
                this.detectChargeWithAllOption(data);

                break;
            default:
                break;
        }
    }

    onRemoveService(data: any) {
        this.selectedService.splice(this.selectedService.findIndex((item: any) => item.id === data.id), 1);
        this.detectServiceWithAllOption();
    }

    onRemoveUser(data: any) {
        this.selectedUser.splice(this.selectedUser.findIndex((item: any) => item.id === data.id), 1);
    }

    onRemoveCharge(index: number = 0) {
        this.selectedCharges.splice(index, 1);
    }

    getCurrency() {
        this._globalState.subscribe('currency', (data: any) => {
            this.currencyList = (data.data || []).map((item: any) => ({ id: item.id, text: item.id }));
            this.selectedCurrency = [this.currencyList[0]];
        });
    }

    getUser() {
        this._globalState.subscribe('system-user', (data: any) => {
            this.users = (data || []).map((item: any) => ({ id: item.id, text: item.id }));
            this.selectedUser = [this.users[0]];
        });
    }

    detectServiceWithAllOption(data?: any) {
        if (!this.selectedService.every((value: any) => value.id > 0)) {
            this.selectedService.splice(this.selectedService.findIndex((item: any) => item.id < 0), 1);

            this.selectedService = [];
            this.selectedService.push(data);
        }
    }

    detectChargeWithAllOption(data?: any) {
        if (!this.selectedCharges.every((value: any) => value.id > 0)) {
            this.selectedCharges.splice(this.selectedCharges.findIndex((item: any) => item.id < 0), 1);

            this.selectedCharges = [];
            this.selectedCharges.push(data);
        }
    }

    onSearchCharge() {
        console.log(this.selectedRangeDate);
        this.isSubmited = true;

        const body = {
            partner: this.selectedPartner,
            date: this.selectedRangeDate,
            dateMode: this.selectedDateMode,
            type: this.selectedType,
            obh: this.selectedObh,
            currency: this.selectedCurrency,
            services: this.selectedService,
            office: null,
            creator: this.selectedUser,
            charges: this.selectedCharges,
            note: this.note
        };
        this.onApply.emit(body);
    }
}
