import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { ActivatedRoute } from '@angular/router';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import _ from 'lodash';

@Component({
    selector: 'app-custom-clearance-edit',
    templateUrl: './custom-clearance-edit.component.html',
    styleUrls: ['./custom-clearance-edit.component.scss']
})
export class CustomClearanceEditComponent implements OnInit {
    customDeclaration: any = {};
    listCustomer: any = [];

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU,
        private route: ActivatedRoute) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    async ngOnInit() {
        await this.route.params.subscribe(prams => {
            if (prams.id != undefined) {
                console.log(prams.id);
                this.getClearanceType();
                this.getListCustomer();
                this.getCustomCleanranceById(prams.id);
            }
        });
    }

    async getCustomCleanranceById(id) {
        // this.baseServices.get(this.api_menu.ToolSetting.CustomClearance.details + id).subscribe((res: any) => {
        //     console.log(res);
        //     this.customDeclaration = res;
        // }, err => {
        //     this.customDeclaration = {};
        //     this.baseServices.handleError(err);
        // });
        const res = await this.baseServices.getAsync(this.api_menu.ToolSetting.CustomClearance.details + id, true, true);
        console.log(res);
        this.customDeclaration = res;

        console.log(this.listCustomer);
        const _customer = _.find(this.listCustomer, { 'taxCode': this.customDeclaration.partnerTaxCode });
        this.customerCurrent = _customer != undefined ? _customer.taxCode : '';
        console.log('customerCurrent: ' + this.customerCurrent);

        this.customDeclaration.clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };
        
        const _serviceType = _.find(this.serviceTypes, { 'id': this.customDeclaration.serviceType });
        this.serviceTypeCurrent = _serviceType != undefined ? [_serviceType.id] : [];
        console.log('serviceTypeCurrent: ' + this.serviceTypeCurrent);

        const _typeClearance = _.find(this.typeClearance, { 'id': this.customDeclaration.type });
        this.typeClearanceCurrent = _typeClearance != undefined ? [_typeClearance.id] : [];
        console.log('typeClearanceCurrent: ' + this.typeClearanceCurrent);
        
        const _routeClearance = _.find(this.routeClearance, { 'id' : this.customDeclaration.route});
        this.routeClearanceCurrent = _routeClearance != undefined ? [_routeClearance.id] : [];
        console.log('routeClearanceCurrent: ' + this.routeClearanceCurrent);

        const _cargoType = _.find(this.cargoTypes, { 'id' : this.customDeclaration.cargoType});
        this.cargoTypeCurrent = _cargoType != undefined ? [_cargoType.id] : [];
        console.log('cargoTypeCurrent: ' + this.cargoTypeCurrent);

    }

    updateCustomClearance() {
        console.log(this.customDeclaration.clearanceNo)
    }

    async getListCustomer() {
        //partnerGroup = 3 ~ Customer
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: 3 });
        console.log(res);
        this.listCustomer = res;
    }

    getClearanceType() {
        this.baseServices.get(this.api_menu.ToolSetting.CustomClearance.getClearanceTypes).subscribe((res: any) => {
            console.log(res);
            this.serviceTypes = res.serviceTypes.map(x => ({ "text": x.displayName, "id": x.value }));
            this.typeClearance = res.types.map(x => ({ "text": x.displayName, "id": x.value }));
            this.routeClearance = res.routes.map(x => ({ "text": x.displayName, "id": x.value }));
            this.cargoTypes = res.cargoTypes.map(x => ({ "text": x.displayName, "id": x.value }));
        });
    }

    /**
      * Daterange picker
      */
    selectedRange: any;
    selectedDate: any;
    keepCalendarOpeningWithRange: true;
    maxDate: moment.Moment = moment();
    ranges: any = {
        Today: [moment(), moment()],
        Yesterday: [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
        'Last 7 Days': [moment().subtract(6, 'days'), moment()],
        'Last 30 Days': [moment().subtract(29, 'days'), moment()],
        'This Month': [moment().startOf('month'), moment().endOf('month')],
        'Last Month': [
            moment()
                .subtract(1, 'month')
                .startOf('month'),
            moment()
                .subtract(1, 'month')
                .endOf('month')
        ]
    };

    /**
  * ng2-select
  */
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4', 'option 5', 'option 6', 'option 7'];

    statusClearance: Array<string> = ['All', 'Imported', 'Not imported'];

    serviceTypes: any = [];
    typeClearance: any = [];
    routeClearance: any = [];
    cargoTypes: any = [];
    
    customerCurrent: string = '';

    serviceTypeCurrent: any = [];
    typeClearanceCurrent: any = [];
    routeClearanceCurrent: any = [];
    cargoTypeCurrent: any = [];
    
    packagesUnit: Array<string> = ['PKG', 'PCS', 'BOX', 'CNTS'];
    packagesUnitActive = ['PKG'];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any): void {
        console.log('Selected value is: ', value);
    }

    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }

}
