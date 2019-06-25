import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { ActivatedRoute } from '@angular/router';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import find from 'lodash/find';
import { NgForm } from '@angular/forms';

@Component({
    selector: 'app-custom-clearance-edit',
    templateUrl: './custom-clearance-edit.component.html',
    styleUrls: ['./custom-clearance-edit.component.scss']
})
export class CustomClearanceEditComponent implements OnInit {
    customDeclaration: any = {};
    listCustomer: any = [];
    listPort: any = [];
    listCountry: any = [];
    listCommodity: any = [];
    listUnit: any = [];

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU,
        private route: ActivatedRoute) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    async ngOnInit() {
        this.getClearanceType();
        await this.getListCustomer();
        await this.getListPort();
        this.getListCountry();
        await this.getListUnit();
        await this.getListCommodity();
        await this.route.params.subscribe(prams => {
            if (prams.id != undefined) {
                console.log(prams.id);
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
        const _customer = find(this.listCustomer, { 'taxCode': this.customDeclaration.partnerTaxCode });
        this.customerCurrent = _customer != undefined ? _customer.taxCode : '';
        console.log('customerCurrent: ' + this.customerCurrent);

        this.customDeclaration.clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };

        const _serviceType = find(this.serviceTypes, { 'id': this.customDeclaration.serviceType });
        this.serviceTypeCurrent = _serviceType != undefined ? [_serviceType.id] : [];
        console.log('serviceTypeCurrent: ' + this.serviceTypeCurrent);

        const _port = find(this.listPort, { 'code': this.customDeclaration.gateway });
        this.portCurrent = _port != undefined ? _port.code : '';
        console.log('portCurrent: ' + this.portCurrent);

        const _typeClearance = find(this.typeClearance, { 'id': this.customDeclaration.type });
        this.typeClearanceCurrent = _typeClearance != undefined ? [_typeClearance.id] : [];
        console.log('typeClearanceCurrent: ' + this.typeClearanceCurrent);

        const _routeClearance = find(this.routeClearance, { 'id': this.customDeclaration.route });
        this.routeClearanceCurrent = _routeClearance != undefined ? [_routeClearance.id] : [];
        console.log('routeClearanceCurrent: ' + this.routeClearanceCurrent);

        const _cargoType = find(this.cargoTypes, { 'id': this.customDeclaration.cargoType });
        this.cargoTypeCurrent = _cargoType != undefined ? [_cargoType.id] : [];
        console.log('cargoTypeCurrent: ' + this.cargoTypeCurrent);

        const _countryExport = find(this.listCountry, { 'code': this.customDeclaration.exportCountryCode });
        this.countryExportCurrent = _countryExport != undefined ? _countryExport.code : '';
        console.log('countryExportCurrent: ' + this.countryExportCurrent);

        const _countryImport = find(this.listCountry, { 'code': this.customDeclaration.importCountryCode });
        this.countryImportCurrent = _countryImport != undefined ? _countryImport.code : '';
        console.log('countryImportCurrent: ' + this.countryImportCurrent);

        const _commodity = find(this.listCommodity, { 'code': this.customDeclaration.commodityCode });
        this.commodityCurrent = _commodity != undefined ? _commodity.code : '';
        console.log('commodityCurrent: ' + this.commodityCurrent);

        const _unit = find(this.listUnit, { 'code': this.customDeclaration.unitCode });
        this.unitCurrent = _unit != undefined ? _unit.code : '';
        console.log('unitCurrent: ' + this.unitCurrent);
    }

    obj: any = {};
    async updateCustomClearance(form: NgForm) {
        console.log(this.customerCurrent);
        console.log(this.portCurrent);
        if (this.customerCurrent == '' || this.portCurrent == '') return;
        console.log(form.form.status);
        if (form.form.status != "INVALID") {
            this.obj.id = this.customDeclaration.id;
            this.obj.clearanceNo = this.customDeclaration.clearanceNo;
            this.obj.PartnerTaxCode = this.customerCurrent;//
            this.obj.clearanceDate = this.customDeclaration.clearanceDate.endDate._d;
            this.obj.hblid = this.customDeclaration.hblid;
            this.obj.mblid = this.customDeclaration.mblid;
            this.obj.serviceType = this.serviceTypeCurrent[0];//
            this.obj.gateway = this.portCurrent;//
            this.obj.type = this.typeClearanceCurrent[0];//
            this.obj.route = this.routeClearanceCurrent[0];//
            this.obj.cargoType = this.cargoTypeCurrent[0];//
            this.obj.exportCountryCode = this.countryExportCurrent;//
            this.obj.importCountryCode = this.countryImportCurrent;//
            this.obj.commodityCode = this.commodityCurrent;//
            this.obj.grossWeight = this.customDeclaration.grossWeight;
            this.obj.netWeight = this.customDeclaration.netWeight;
            this.obj.cbm = this.customDeclaration.cbm;
            this.obj.qtyCont = this.customDeclaration.qtyCont;
            this.obj.pcs = this.customDeclaration.pcs;
            this.obj.unitCode = this.unitCurrent;//
            this.obj.note = this.customDeclaration.note;
            console.log(this.obj);
            const respone = await this.baseServices.putAsync(this.api_menu.ToolSetting.CustomClearance.update, this.obj, true, true);
            console.log(respone);
        }
    }

    async getListCustomer() {
        //partnerGroup = 3 ~ Customer
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: 3 }, true, true);
        this.listCustomer = res;
    }

    getClearanceType() {
        this.baseServices.get(this.api_menu.ToolSetting.CustomClearance.getClearanceTypes).subscribe((res: any) => {
            this.serviceTypes = res.serviceTypes.map(x => ({ "text": x.displayName, "id": x.value }));
            this.typeClearance = res.types.map(x => ({ "text": x.displayName, "id": x.value }));
            this.routeClearance = res.routes.map(x => ({ "text": x.displayName, "id": x.value }));
            this.cargoTypes = res.cargoTypes.map(x => ({ "text": x.displayName, "id": x.value }));
        });
    }

    async getListPort() {
        //placeType = 8 ~ Port
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, { placeType: 8 }, true, true);
        this.listPort = res;
    }

    getListCountry() {
        this.baseServices.get(this.api_menu.Catalogue.Country.getAll).subscribe((res: any) => {
            this.listCountry = res;
        });
    }

    async getListCommodity() {
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.Commodity.query, {}, true, true);
        this.listCommodity = res;
    }

    async getListUnit() {
        //unitType = Package
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: 'Package' }, true, true);
        this.listUnit = res;
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

    serviceTypes: any = [];
    typeClearance: any = [];
    routeClearance: any = [];
    cargoTypes: any = [];

    customerCurrent: string = '';
    portCurrent: string = '';
    countryImportCurrent: string = '';
    countryExportCurrent: string = '';
    commodityCurrent: string = '';
    unitCurrent: string = '';

    serviceTypeCurrent: any = [];
    typeClearanceCurrent: any = [];
    routeClearanceCurrent: any = [];
    cargoTypeCurrent: any = [];

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

    public selectedServiceType(value: any): void {
        console.log('Selected value is: ', value);
        this.serviceTypeCurrent = [value.id];
        if (this.serviceTypeCurrent[0] == 'Air' || this.serviceTypeCurrent[0] == 'Express') {
            this.cargoTypeCurrent = [];
        }
    }

    public selectedTypeClearance(value: any): void {
        console.log('Selected value is: ', value);
        this.typeClearanceCurrent = [value.id];
    }

    public selectedRouteClearance(value: any): void {
        console.log('Selected value is: ', value);
        this.routeClearanceCurrent = [value.id];
    }

    public selectedCargoType(value: any): void {
        console.log('Selected value is: ', value);
        this.cargoTypeCurrent = [value.id];
    }

    public removedServiceType(value: any): void {
        this.serviceTypeCurrent = [];
    }

    public removedTypeClearance(value: any): void {
        this.typeClearanceCurrent = [];
    }

    public removedRouteClearance(value: any): void {
        this.routeClearanceCurrent = [];
    }

    public removedCargoType(value: any): void {
        this.cargoTypeCurrent = [];
    }
}
