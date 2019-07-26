import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { ActivatedRoute } from '@angular/router';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import find from 'lodash/find';
import { NgForm } from '@angular/forms';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';

@Component({
    selector: 'app-custom-clearance-edit',
    templateUrl: './custom-clearance-edit.component.html',
    styleUrls: ['./custom-clearance-edit.component.scss']
})
export class CustomClearanceEditComponent implements OnInit {
    customDeclaration: CustomClearance = new CustomClearance();
    listCustomer: any[] = [];
    listPort: any = [];
    listCountry: any = [];
    listCommodity: any = [];
    listUnit: any = [];
    _clearanceDate: any;
    listUser: any[] = [];
    isConvertJob: boolean = false;

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
                this.getCustomCleanranceById(prams.id);
            }
        });
        this.getListUser();
    }
    getListUser() {
        this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
            this.listUser = res.map(x => ({ "text": x.username, "id": x.id }));
        }, err => {
            this.listUser = [];
            this.baseServices.handleError(err);
        });
    }

    async getCustomCleanranceById(id) {
        // this.baseServices.get(this.api_menu.Operation.CustomClearance.details + id).subscribe((res: any) => {
        //     console.log(res);
        //     this.customDeclaration = res;           
        // }, err => {
        //     this.customDeclaration = {};
        //     this.baseServices.handleError(err);
        // });
        const res = await this.baseServices.getAsync(this.api_menu.Operation.CustomClearance.details + id, true, true);
        console.log(res);
        this.customDeclaration = res;
        const _customer = find(this.listCustomer, { 'taxCode': this.customDeclaration.partnerTaxCode });
        this.strCustomerCurrent = _customer != undefined ? _customer.taxCode : '';
        //console.log('strCustomerCurrent: ' + this.strCustomerCurrent);

        //this.customDeclaration.clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };
        this._clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };

        const _serviceType = find(this.serviceTypes, { 'id': this.customDeclaration.serviceType });
        this.serviceTypeCurrent = _serviceType != undefined ? [_serviceType.id] : [];
        //console.log('serviceTypeCurrent: ' + this.serviceTypeCurrent);

        const _port = find(this.listPort, { 'code': this.customDeclaration.gateway });
        this.strPortCurrent = _port != undefined ? _port.code : '';
        //console.log('portCurrent: ' + this.strPortCurrent);

        const _typeClearance = find(this.typeClearance, { 'id': this.customDeclaration.type });
        this.typeClearanceCurrent = _typeClearance != undefined ? [_typeClearance.id] : [];
        //console.log('typeClearanceCurrent: ' + this.typeClearanceCurrent);

        const _routeClearance = find(this.routeClearance, { 'id': this.customDeclaration.route });
        this.routeClearanceCurrent = _routeClearance != undefined ? [_routeClearance.id] : [];
        //console.log('routeClearanceCurrent: ' + this.routeClearanceCurrent);

        const _cargoType = find(this.cargoTypes, { 'id': this.customDeclaration.cargoType });
        this.cargoTypeCurrent = _cargoType != undefined ? [_cargoType.id] : [];
        //console.log('cargoTypeCurrent: ' + this.cargoTypeCurrent);

        const _countryExport = find(this.listCountry, { 'code': this.customDeclaration.exportCountryCode });
        this.strCountryExportCurrent = _countryExport != undefined ? _countryExport.code : '';
        //console.log('strCountryExportCurrent: ' + this.strCountryExportCurrent);

        const _countryImport = find(this.listCountry, { 'code': this.customDeclaration.importCountryCode });
        this.strCountryImportCurrent = _countryImport != undefined ? _countryImport.code : '';
        //console.log('strCountryImportCurrent: ' + this.strCountryImportCurrent);

        const _commodity = find(this.listCommodity, { 'code': this.customDeclaration.commodityCode });
        this.strCommodityCurrent = _commodity != undefined ? _commodity.code : '';
        //console.log('strCommodityCurrent: ' + this.strCommodityCurrent);

        const _unit = find(this.listUnit, { 'code': this.customDeclaration.unitCode });
        this.strUnitCurrent = _unit != undefined ? _unit.code : '';
        //console.log('strUnitCurrent: ' + this.strUnitCurrent);
    }

    async updateCustomClearance(formUpdate: NgForm) {
        if (this.strCustomerCurrent == '' || this.strPortCurrent == '') return;
        if (this.serviceTypeCurrent[0] != 'Air' && this.serviceTypeCurrent[0] != 'Express') {
            if (this.cargoTypeCurrent.length == 0) return;
        }
        if (formUpdate.form.status != "INVALID" && this._clearanceDate.endDate != null) {
            this.customDeclaration.partnerTaxCode = this.strCustomerCurrent;
            //this.customDeclaration.clearanceDate = moment(this.customDeclaration.clearanceDate.endDate._d).format('YYYY-MM-DD');
            this.customDeclaration.clearanceDate = moment(this._clearanceDate.endDate._d).format('YYYY-MM-DD');
            this.customDeclaration.serviceType = this.serviceTypeCurrent[0];
            this.customDeclaration.gateway = this.strPortCurrent;
            this.customDeclaration.type = this.typeClearanceCurrent[0];
            this.customDeclaration.route = this.routeClearanceCurrent[0];
            this.customDeclaration.cargoType = (this.serviceTypeCurrent[0] == 'Air' || this.serviceTypeCurrent[0] == 'Express') ? null : this.cargoTypeCurrent[0];
            this.customDeclaration.exportCountryCode = this.strCountryExportCurrent;
            this.customDeclaration.importCountryCode = this.strCountryImportCurrent;
            this.customDeclaration.commodityCode = this.strCommodityCurrent;
            this.customDeclaration.unitCode = this.strUnitCurrent;
            console.log(this.customDeclaration);

            const respone = await this.baseServices.putAsync(this.api_menu.Operation.CustomClearance.update, this.customDeclaration, true, true);
            console.log(respone);
            if (respone.status) {
                this.getCustomCleanranceById(this.customDeclaration.id);
                this.mapClearanceToShipment();
            } else {
                //reset lại _clearanceDate
                //this.customDeclaration.clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };
                this._clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };
            }
        }
    }
    async convertClearanceToShipment(formUpdate: NgForm) {
        if (this.strCustomerCurrent === '' || this.strPortCurrent === ''
            || this.typeClearanceCurrent.length === 0
            || this.customDeclaration.hblid == null || this.customDeclaration.hblid === ''
            || this.customDeclaration.mblid == null || this.customDeclaration.mblid === '') { return; }
        if (this.serviceTypeCurrent[0] !== 'Air' && this.serviceTypeCurrent[0] !== 'Express') {
            if (this.cargoTypeCurrent.length === 0) { return; }
        }
        if (formUpdate.form.status !== "INVALID" && this._clearanceDate.endDate != null) {
            this.customDeclaration.partnerTaxCode = this.strCustomerCurrent;
            // this.customDeclaration.clearanceDate = moment(this.customDeclaration.clearanceDate.endDate._d).format('YYYY-MM-DD');
            this.customDeclaration.clearanceDate = moment(this._clearanceDate.endDate._d).format('YYYY-MM-DD');
            this.customDeclaration.serviceType = this.serviceTypeCurrent[0];
            this.customDeclaration.gateway = this.strPortCurrent;
            this.customDeclaration.type = this.typeClearanceCurrent[0];
            this.customDeclaration.route = this.routeClearanceCurrent[0];
            this.customDeclaration.cargoType = (this.serviceTypeCurrent[0] === 'Air' || this.serviceTypeCurrent[0] === 'Express') ? null : this.cargoTypeCurrent[0];
            this.customDeclaration.exportCountryCode = this.strCountryExportCurrent;
            this.customDeclaration.importCountryCode = this.strCountryImportCurrent;
            this.customDeclaration.commodityCode = this.strCommodityCurrent;
            this.customDeclaration.unitCode = this.strUnitCurrent;

            const shipment = this.mapClearanceToShipment();
            const response = await this.baseServices.postAsync(this.api_menu.Documentation.Operation.convertClearanceToJob, { opsTransaction: shipment, customsDeclaration: this.customDeclaration }, true, true);
            if (response.status) {
                this.getCustomCleanranceById(this.customDeclaration.id);
            } else {
                // reset lại _clearanceDate
                // this.customDeclaration.clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };
                this._clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: moment(this.customDeclaration.clearanceDate), endDate: moment(this.customDeclaration.clearanceDate) };
            }
        }
    }

    mapClearanceToShipment() {
        let shipment = new OpsTransaction();
        let index = this.listCustomer.findIndex(x => x.taxCode === this.customDeclaration.partnerTaxCode);
        if (index > -1) {
            const customer = this.listCustomer[index];
            shipment.customerId = customer.id;
            shipment.salemanId = customer.salePersonId;
            shipment.serviceMode = this.customDeclaration.type;
            index = this.listPort.findIndex(x => x.code === this.customDeclaration.gateway);
            if (index > -1) {
                if (this.customDeclaration.type === "Export") {
                    shipment.pol = this.listPort[index].id;
                }
                if (this.customDeclaration.type === "Import") {
                    shipment.pod = this.listPort[index].id;
                }
            }
            if (this.customDeclaration.serviceType === "Sea") {
                if (this.customDeclaration.cargoType === "FCL") {
                    shipment.productService = "SeaFCL";
                }
                if (this.customDeclaration.cargoType === "LCL") {
                    shipment.productService = "SeaLCL";
                }
            } else {
                shipment.productService = this.customDeclaration.serviceType;
            }
            shipment.shipmentMode = "External";
            shipment.mblno = this.customDeclaration.mblid;
            shipment.hwbno = this.customDeclaration.hblid;
            shipment.serviceDate = this.customDeclaration.clearanceDate;
            shipment.sumGrossWeight = this.customDeclaration.grossWeight;
            shipment.sumNetWeight = this.customDeclaration.netWeight;
            shipment.sumCbm = this.customDeclaration.cbm;
            const claim = localStorage.getItem('id_token_claims_obj');
            const currenctUser = JSON.parse(claim)["id"];
            shipment.billingOpsId = currenctUser;
            index = this.listUnit.findIndex(x => x.code === this.customDeclaration.unitCode);
            if (index > -1) {
                shipment.packageTypeId = this.listUnit[index].id;
            }
        } else {
            this.baseServices.errorToast("Không có customer để tạo job mới");
            shipment = null;
        }
        if (this.customDeclaration.clearanceDate == null) {
            this.baseServices.errorToast("Không có clearance date để tạo job mới");
            shipment = null;
        }
        return shipment;
    }

    async getListCustomer() {
        //partnerGroup = 3 ~ Customer
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER }, true, true);
        this.listCustomer = res;
    }

    getClearanceType() {
        this.baseServices.get(this.api_menu.Operation.CustomClearance.getClearanceTypes).subscribe((res: any) => {
            this.serviceTypes = res.serviceTypes.map(x => ({ "text": x.displayName, "id": x.value }));
            this.typeClearance = res.types.map(x => ({ "text": x.displayName, "id": x.value }));
            this.routeClearance = res.routes.map(x => ({ "text": x.displayName, "id": x.value }));
            this.cargoTypes = res.cargoTypes.map(x => ({ "text": x.displayName, "id": x.value }));
        });
    }

    async getListPort() {
        //placeType = 8 ~ Port
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Port }, true, true);
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

    strCustomerCurrent: any = '';
    strPortCurrent: any = '';
    strCountryImportCurrent: any = '';
    strCountryExportCurrent: any = '';
    strCommodityCurrent: any = '';
    strUnitCurrent: any = '';

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
        this.serviceTypeCurrent = [value.id];
        if (this.serviceTypeCurrent[0] == 'Air' || this.serviceTypeCurrent[0] == 'Express') {
            this.cargoTypeCurrent = [];
        }
    }

    public selectedTypeClearance(value: any): void {
        this.typeClearanceCurrent = [value.id];
    }

    public selectedRouteClearance(value: any): void {
        this.routeClearanceCurrent = [value.id];
    }

    public selectedCargoType(value: any): void {
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
