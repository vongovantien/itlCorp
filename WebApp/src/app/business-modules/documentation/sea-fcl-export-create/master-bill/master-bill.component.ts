import { Component, OnInit, Input } from '@angular/core';
import * as moment from 'moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as lodash from 'lodash';
import { NgForm } from '@angular/forms';
import * as dataHelper from 'src/helper/data.helper';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';

@Component({
  selector: 'app-master-bill',
  templateUrl: './master-bill.component.html',
  styleUrls: ['./master-bill.component.scss']
})
export class MasterBillComponent implements OnInit {
    @Input() shipment: CsTransaction = new CsTransaction();
    @Input() formAddEdit: NgForm;
    terms: any[];
    shipmentTypes: any[];
    serviceTypes: any[];
    billOfLadingTypes: any[];
    coloaders: any[];
    agents: any[];
    portOfLadings: any[] = [];
    portOfDestinations: any[] = [];
    userInCharges: any[] = [];

    constructor(
    private baseServices: BaseService,
    private api_menu: API_MENU) { }

    async ngOnInit() {
       this.getShipmentCommonData();
       this.getPorIndexs();
       this.getColoaders();
       this.getAgents();
       this.getUserInCharges();
    }

    async getShipmentCommonData(){
        const data = await shipmentHelper.getShipmentCommonData(this.baseServices,this.api_menu);
        this.billOfLadingTypes = dataHelper.prepareNg2SelectData(data.billOfLadings,'value','displayName');
        this.serviceTypes = dataHelper.prepareNg2SelectData(data.serviceTypes,'value','displayName');
        this.terms = dataHelper.prepareNg2SelectData(data.freightTerms,'value','displayName');
        this.shipmentTypes = dataHelper.prepareNg2SelectData(data.shipmentTypes,'value','displayName');
    }
    async getPorIndexs(){
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging+"?page=1&size=20", { placeType: PlaceTypeEnum.Port, modeOfTransport : 'SEA', inactive: false }, false, false);
        if(portIndexs != null){
            this.portOfLadings = portIndexs;
            this.portOfDestinations = portIndexs;
        }
    }

    async getColoaders(){
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging+"?page=1&size=20", { placeType: PartnerGroupEnum.CARRIER, modeOfTransport : 'SEA', inactive: false }, false, false);
        if(partners != null){
            this.coloaders = partners;
        }
    }

    async getAgents(){
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging+"?page=1&size=20", { placeType: PartnerGroupEnum.AGENT, modeOfTransport : 'SEA', inactive: false }, false, false);
        if(partners != null){
            this.agents = partners;
        }
    }

    async getUserInCharges(){
        const users = await this.baseServices.getAsync(this.api_menu.System.User_Management.getAll, false, false);
        if(users != null){
            this.userInCharges = users;
        }
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
