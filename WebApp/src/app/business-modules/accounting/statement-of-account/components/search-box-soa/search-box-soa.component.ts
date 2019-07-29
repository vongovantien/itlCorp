import { Component, EventEmitter, Output } from '@angular/core';
import { forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AppPage, IComboGirdConfig } from 'src/app/app.base';
import { GlobalState } from 'src/app/global-state';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { Currency, Partner, User } from 'src/app/shared/models';
import { SystemRepo } from 'src/app/shared/repositories';
import moment from 'moment';
import { BaseService } from 'src/app/shared/services';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'soa-search-box',
    templateUrl: './search-box-soa.component.html',
    styleUrls: ['./search-box-soa.component.scss']
})
export class StatementOfAccountSearchComponent extends AppPage {

    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();

    items: any[];
    selectedRange: any;

    configPartner: IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    partners: Partner[] = [];
    selectedPartner: any = {};

    currencyList: Currency[] = [];
    selectedCurrency: any;

    users: User[] = [];
    userLogged: User;
    currentUser: any;

    statusSOA: any[] = [];
    selectedStatus: any = null;

    reference: string = '';

    constructor(
        private _sysRepo: SystemRepo,
        private _globalState: GlobalState,
        private _baseService: BaseService,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void {
        this.getBasicData();
        this.getStatus();
        this.getUserLogged();

    }

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin();
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
                    // * add all value into partners data
                    this.partners.unshift(new Partner({ taxCode: 'All', shortName: 'All', partnerNameEn: 'All' })); 
                    this.selectedPartner = { field: 'partnerNameEn', value: 'All' };

                    this.currencyList = <any>this.utility.prepareNg2SelectData(this.mapModel(dataCurrency, Currency), 'id', 'id');
                    this.selectedCurrency = [this.currencyList.filter((item: any) => item.id === 'VND')[0]];

                    this.users = <any>this.utility.prepareNg2SelectData(this.mapModel(dataSystemUser, User), 'id', 'username');
                    this.currentUser = [this.users.filter((user: any) => user.id === this.userLogged.id)[0]];

                    // * set config for combogird
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
                (errors: any) => {
                    this.handleError(errors);
                },
                // complete
                () => { }
            );
    }

    getStatus() {
        this.statusSOA = [
            { title: 'New', name: 'New' },
            { title: 'Request Confirmed', name: 'RequestConfirmed' },
            { title: 'Confirmed ', name: 'Confirmed ' },
            { title: 'Need Revise', name: 'NeedRevise' },
            { title: 'Done', name: 'Done' },
        ];
        this.selectedStatus = this.statusSOA[0];
    }

    onSelectDataFormSearch(data: any, key: string) {
        switch (key.toLowerCase()) {
            case 'partner':
                this.selectedPartner = <any>{ field: data.partnerNameEn, value: data.id };
                break;
            case 'currency':
                this.selectedCurrency = [data];
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
            // Todo handle error.
            console.log(error + '');
        }
    }

    search() {
        const body = {
            strCodes: this.reference.replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').split(',').toString().trim(),
            customerID: (this.selectedPartner.value === 'All' ? '' : this.selectedPartner.value) || '',
            soaFromDateCreate: !!this.selectedRange.startDate ? moment(this.selectedRange.startDate).format("YYYY-MM-DD") : null,
            soaToDateCreate: !!this.selectedRange.endDate ? moment(this.selectedRange.endDate).format("YYYY-MM-DD") : null,
            soaStatus: this.selectedStatus.name || null,
            soaCurrency: this.selectedCurrency[0].id || null,
            soaUserCreate: this.currentUser[0].id || '',
        };

        this.onSearch.emit(body);
    }

    // * reset data in form search
    reset() {
        this.reference = '';
        this.selectedPartner = { field: 'partnerNameEn', value: 'All' };
        this.selectedStatus = this.statusSOA[0];
        this.selectedRange = null;

        // ? search again!
        this.onSearch.emit({});
    }

    handleError(errors: any) {
        let message: string = 'Has Error Please Check Again !';
        let title: string = '';
        if (errors instanceof HttpErrorResponse) {
            message = errors.message;
            title = errors.statusText;
        }
        this._toastService.error(message, title, { positionClass: 'toast-bottom-right' });
    }

}
