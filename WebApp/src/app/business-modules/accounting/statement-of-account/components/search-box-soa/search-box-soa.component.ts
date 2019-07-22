import { Component } from '@angular/core';
import { forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AppPage, IComboGirdConfig } from 'src/app/app.base';
import { GlobalState } from 'src/app/global-state';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { Currency, Partner, User } from 'src/app/shared/models';
import { SystemRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'soa-search-box',
    templateUrl: './search-box-soa.component.html',
    styleUrls: ['./search-box-soa.component.scss']
})
export class StatementOfAccountSearchComponent extends AppPage {
    items: any[];
    selectedRange: any;
    maxDate: any;

    configPartner: IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    partners: Partner[] = [];
    selectedPartner: Partial<Partner> = {};

    currencyList: Currency[] = [];
    selectedCurrency: Currency;

    users: User[] = [];
    currentUser: any;

    statusSOA: any[] = [];
    selectedStatus: any = null;

    constructor(
        private _sysRepo: SystemRepo,
        private _globalState: GlobalState) {
        super();
    }

    ngOnInit(): void {
        this.getBasicData();
        this.getStatus();

    }

    getBasicData() {
        forkJoin([ // ? forkJoin like Promise.All
            this._sysRepo.getListCurrency(),
            this._sysRepo.getListSystemUser(),
            this._sysRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.ALL, inactive: false })
        ]).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                ([dataCurrency, dataSystemUser, dataPartner]: any) => {
                    this.partners = this.mapModel(dataPartner, Partner);

                    this.currencyList = <any>this.utility.prepareNg2SelectData(this.mapModel(dataCurrency, Currency), 'id', 'currencyName');

                    this.users = <any>this.utility.prepareNg2SelectData(this.mapModel(dataSystemUser, User), 'id', 'username');
                    this.currentUser = [this.users[0]];

                    // set config for combogird
                    this.configPartner.dataSource = this.partners;
                    this.configPartner.displayFields = [
                        { field: 'taxCode', label: 'Taxcode' },
                        { field: 'shortName', label: 'Name (ABR)' },
                        { field: 'partnerNameEn', label: 'Customer Name [EN]' },
                    ];
                    this.configPartner.selectedDisplayFields = ['partnerNameEn'];

                    // * Share data for another components/Pages
                    this._globalState.notifyDataChanged('partner', this.partners);
                    this._globalState.notifyDataChanged('currency', dataCurrency);
                    this._globalState.notifyDataChanged('system-user', dataSystemUser);

                },
                (errs: any) => {
                    console.log(errs + '');
                    // TODO handle errors
                },
                // complete
                () => { }
            );
    }
    
    getStatus() {
        this.statusSOA = [
            { title: 'New', name: 'New' },
            { title: 'Request Confirmed', name: 'Request Confirmed' },
            { title: 'Confirmed ', name: 'Confirmed ' },
            { title: 'Need Revise', name: 'Need Revise' },
            { title: 'Done', name: 'Done' },
        ];
        this.selectedStatus = this.statusSOA[0];
    }

    onSelectDataFormSearch(data: any, key: string) {
        console.log(data);
        switch (key.toLowerCase()) {
            case 'partner':
                this.selectedPartner = <any>{ field: 'code', value: data.partnerNameEn };
                console.log(this.selectedPartner);
                break;
            case 'currency':
                this.selectedCurrency = new Currency(data);
                break;
            case 'user':
                this.currentUser = [data];
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
            console.log(error + '');
        }
    }
}
