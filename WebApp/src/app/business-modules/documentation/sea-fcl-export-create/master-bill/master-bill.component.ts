import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectorRef, AfterViewInit, AfterContentChecked, AfterViewChecked } from '@angular/core';
import * as moment from 'moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as lodash from 'lodash';
import { NgForm, FormGroup } from '@angular/forms';
import * as dataHelper from 'src/helper/data.helper';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';

@Component({
    selector: 'app-master-bill',
    templateUrl: './master-bill.component.html',
    styleUrls: ['./master-bill.component.scss']
})
export class MasterBillComponent implements OnInit{

    shipment: CsTransaction = new CsTransaction();
    @Input() formAddEdit: NgForm;
    @Input() submitted: boolean;
    terms: any[];
    shipmentTypes: any[];
    serviceTypes: any[];
    billOfLadingTypes: any[];
    coloaders: any[];
    agents: any[];
    portOfLadings: any[] = [];
    portOfDestinations: any[] = [];
    userInCharges: any[] = [];
    etdSelected: any = null;
    etaSelected: any = null;

    constructor(
        private baseServices: BaseService,
        private cdr: ChangeDetectorRef,
        private api_menu: API_MENU) { }

    async ngOnInit() {
        this.getShipmentCommonData();
        this.getPorIndexs(null, null);
        this.getColoaders(null);
        this.getAgents(null);
        this.getUserInCharges(null);
    }
    changePort(keySearch: any, isLading: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getPorIndexs(keySearch, isLading);
    }
    changeAgent(keySearch: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getAgents(keySearch);
    }
    changeColoader(keySearch: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getColoaders(keySearch);
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getShipmentCommonData(this.baseServices, this.api_menu);
        this.billOfLadingTypes = dataHelper.prepareNg2SelectData(data.billOfLadings, 'value', 'displayName');
        this.serviceTypes = dataHelper.prepareNg2SelectData(data.serviceTypes, 'value', 'displayName');
        this.terms = dataHelper.prepareNg2SelectData(data.freightTerms, 'value', 'displayName');
        this.shipmentTypes = dataHelper.prepareNg2SelectData(data.shipmentTypes, 'value', 'displayName');
    }
    async getPorIndexs(searchText: any, isLading: any) {
        let portSearchIndex = { placeType: PlaceTypeEnum.Port, modeOfTransport: 'SEA', inactive: false, all: searchText };
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=1&size=20", portSearchIndex, false, false);
        if (portIndexs != null) {
            if (isLading == null) {
                this.portOfLadings = portIndexs.data;
                this.portOfDestinations = portIndexs.data;
            }
            else {
                if (isLading) {
                    this.portOfLadings = portIndexs.data;
                }
                else {
                    this.portOfDestinations = portIndexs.data;
                }
            }
            console.log(this.portOfDestinations);
        }
    }

    async getColoaders(searchText: any) {
        let criteriaSearchColoader = { partnerGroup: PartnerGroupEnum.CARRIER, modeOfTransport: 'SEA', all: searchText, inactive: false };
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchColoader, false, false);
        if (partners != null) {
            this.coloaders = partners.data;
            console.log(this.coloaders);
        }
    }
    async getAgents(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.AGENT, modeOfTransport: 'SEA', inactive: false, all: searchText };
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.agents = partners.data;
        }
    }

    async getUserInCharges(searchText: any) {
        const users = await this.baseServices.getAsync(this.api_menu.System.User_Management.getAll, false, false);
        if (users != null) {
            this.userInCharges = users;
        }
    }
    clickColoader(id) {
        console.log(this.shipment);
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
    public items: Array<string> = ['Option 1', 'Option 2', 'Option 3', 'Option 4',
        'Option 5', 'Option 6', 'Option 7', 'Option 8', 'Option 9', 'Option 10',];

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