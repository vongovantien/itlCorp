import { Component } from '@angular/core';
import { AppPage, IComboGirdConfig } from 'src/app/app.base';
import { SystemRepo } from 'src/app/shared/repositories';
import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'form-create-soa',
    templateUrl: './form-create-soa.component.html',
    styleUrls: ['./form-create-soa.component.scss']
})
export class StatementOfAccountFormCreateComponent extends AppPage {
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

    constructor(
        private _sysRepo: SystemRepo
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
            { code: 'Charge CODE 1', name: 'Name Abbr 1' },
            { code: 'Charge CODE 2', name: 'Name Abbr 2' },
            { code: 'Charge CODE 3', name: 'Name Abbr 3' },
        ];
        this.configCharge.dataSource.push(...charges);
        this.configCharge.displayFields = [
            { field: 'code', label: 'Charge Code' },
            { field: 'name', label: 'Charge Name EN ' },
        ];
        this.configCharge.selectedDisplayFields = ['code', 'name'];
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
                    this.detectServiceWithAllOption();
                }
                break;
            case 'user':
                this.selectedUser.push(data);
                break;
            case 'charge':
                this.selectedCharge = { field: 'code', value: data.code };
                this.selectedCharges.push(this.selectedCharge);
                console.log(this.selectedCharge);
                break;
            default:
                break;
        }
    }

    onRemoveService(data: any) {
        this.selectedService.splice(this.selectedService.findIndex((item: any) => item.id === data.id), 1);
        this.detectServiceWithAllOption('remove');
    }

    onRemoveUser(data: any) {
        this.selectedUser.splice(this.selectedUser.findIndex((item: any) => item.id === data.id), 1);
    }


    onRemoveCharge(index: number = 0) {
        this.selectedCharges.splice(index, 1);
    }

    getCurrency() {
        this._sysRepo.getListCurrency(1, 20)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((data: any) => {
                this.currencyList = (data.data || []).map((item: any) => ({ id: item.id, text: item.id }));
                this.selectedCurrency = [this.currencyList[0]];
            });
    }

    getUser() {
        this._sysRepo.getListSystemUser()
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((data: any) => {
                this.users = (data || []).map((item: any) => ({ id: item.id, text: item.id }));
                this.selectedUser = [this.users[0]];
            });
    }

    isBelowThreshold(currentValue: any) {
        return currentValue.id > 0;
    }

    detectServiceWithAllOption(type?: string) {
        if (!this.selectedService.every((value: any) => value.id > 0)) {
            this.selectedService.splice(this.selectedService.findIndex((item: any) => item.id < 0), 1);
            this.selectedService = [];
            this.selectedService.push({ "id": 2, "text": "Air Import" });
        }
    }
}
