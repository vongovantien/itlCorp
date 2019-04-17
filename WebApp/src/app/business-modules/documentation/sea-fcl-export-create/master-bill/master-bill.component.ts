import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectorRef, AfterViewInit, AfterContentChecked, AfterViewChecked } from '@angular/core';
import moment from 'moment/moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
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
export class MasterBillComponent implements OnInit, AfterViewInit{
    ngAfterViewInit(): void {
        if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
            this.inEditing = true;
            console.log(this.shipment.etd);
            if(this.isImport == false){
                this.etdSelected = (this.shipment.etd!= null)? { startDate: moment(this.shipment.etd), endDate: moment(this.shipment.etd) }: null;
                this.etaSelected = (this.shipment.eta!= null)? { startDate: moment(this.shipment.eta), endDate: moment(this.shipment.eta) }: null;
            }
            else{
                this.etdSelected = null;
                this.etaSelected = null;
            }
        }
    }

    @Input()shipment: CsTransaction;
    @Input()isImport: boolean;
    @Input() formAddEdit: NgForm;
    @Input() submitted: boolean;
    @Input() isLoaded: boolean;
    @Output() shipmentDetails = new EventEmitter<any>();
    terms: any[];
    shipmentTypes: any[];
    serviceTypes: any[];
    billOfLadingTypes: any[];
    coloaders: any[];
    agents: any[];
    portOfLadings: any[] = [];
    portOfDestinations: any[] = [];
    userInCharges: any[] = [];
    etdSelected: any;
    etaSelected: any;
    billOfLadingTypeActive: any[] = [];
    servicetypeActive: any[] = [];
    paymentTermActive: any[] = [];
    shimentTypeActive: any[] = [];
    inEditing: boolean = false;

    constructor(
        private baseServices: BaseService,
        private cdr: ChangeDetectorRef,
        private api_menu: API_MENU) { }

    async ngOnInit() {
        this.baseServices.spinnerShow();
        await this.getPortLoading(null);
        await this.changePortDestination(null);
        await this.getColoaders(null);
        await this.getAgents(null);
        await this.getUserInCharges(null);
        await this.getShipmentCommonData();
        let index = -1;
        if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
            if(this.isImport == false){
                this.etdSelected = (this.shipment.etd!= null)? { startDate: moment(this.shipment.etd), endDate: moment(this.shipment.etd) }: null;
                this.etaSelected = (this.shipment.eta!= null)? { startDate: moment(this.shipment.eta), endDate: moment(this.shipment.eta) }: null;
            }
            else{
                this.etdSelected = null;
                this.etaSelected = null;
            }
            this.inEditing = true;
            console.log(this.shipment.etd);
            if(this.isImport == true){
                let claim = localStorage.getItem('id_token_claims_obj');
                index = this.userInCharges.findIndex(x => x.id == JSON.parse(claim)["id"]);
                if(index > -1) {
                    this.shipment.personInChargeName = this.userInCharges[index].username;
                    this.shipment.personIncharge = JSON.parse(claim)["id"];
                }
            }
            index = this.billOfLadingTypes.findIndex(x => x.id == this.shipment.mbltype);
            if(index > -1) this.billOfLadingTypeActive = [this.billOfLadingTypes[index]];
            index = this.serviceTypes.findIndex(x => x.id == this.shipment.typeOfService);
            if(index > -1) this.servicetypeActive = [this.serviceTypes[index]];
            index = this.terms.findIndex(x => x.id == this.shipment.paymentTerm);
            if(index > -1) this.paymentTermActive = [this.terms[index]];
            index = this.shipmentTypes.findIndex(x => x.id == this.shipment.shipmentType);
            if(index > -1) this.shimentTypeActive = [this.shipmentTypes[index]];
            if(this.portOfLadings.length > 0){
                index = this.portOfLadings.findIndex(x => x.id == this.shipment.pol);
                if(index > -1) this.shipment.polName = this.portOfLadings[index].nameEN;
                else this.shipment.polName = '';
            }
            if(this.portOfDestinations.length > 0){
                index = this.portOfDestinations.findIndex(x => x.id == this.shipment.pod);
                if(index > -1) this.shipment.podName = this.portOfDestinations[index].nameEN;
                else this.shipment.podName = '';
            }
            if(this.agents.length > 0){
                index = this.agents.findIndex(x => x.id == this.shipment.agentId);
                if(index > -1) this.shipment.agentName = this.agents[index].partnerNameEn;
                else this.shipment.agentName = '';
            }
            if(this.coloaders.length > 0){
                index = this.coloaders.findIndex(x => x.id == this.shipment.coloaderId);
                if(index > -1) this.shipment.coloaderName = this.coloaders[index].partnerNameEn;
                else this.shipment.coloaderName = '';
            }
            if(this.userInCharges.length > 0){
                index = this.userInCharges.findIndex(x => x.id == this.shipment.personIncharge);
                if(index > -1) this.shipment.personInChargeName = this.userInCharges[index].username;
                else this.shipment.personInChargeName = '';
            }
                  
        }
        else{
            index = this.terms.findIndex(x => x.id == "Prepaid");
            if(index > -1){
                this.shipment.paymentTerm = this.terms[index].id;
                this.paymentTermActive = [this.terms[index]];
            } 
            index = this.shipmentTypes.findIndex(x => x.id == "Freehand");
            if(index > -1){
                this.shimentTypeActive = [this.shipmentTypes[index]];
                this.shipment.shipmentType = this.shipmentTypes[index].id;
            } 
        }
        this.shipmentDetails.emit(Object.assign({},this.shipment));    
        this.isLoaded = true;
        this.baseServices.spinnerHide();
    }
    changePortLoading(keySearch: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getPortLoading(keySearch);
    }
    changePortDestination(keySearch: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getDestinations(keySearch);
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
    changePersonInCharge(keySearch: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getUserInCharges(keySearch);
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getShipmentCommonData(this.baseServices, this.api_menu);
        this.billOfLadingTypes = dataHelper.prepareNg2SelectData(data.billOfLadings, 'value', 'displayName');
        this.serviceTypes = dataHelper.prepareNg2SelectData(data.serviceTypes, 'value', 'displayName');
        this.terms = dataHelper.prepareNg2SelectData(data.freightTerms, 'value', 'displayName');
        this.shipmentTypes = dataHelper.prepareNg2SelectData(data.shipmentTypes, 'value', 'displayName');
    }
    
    async getPortLoading(searchText: any) {
        let portSearchIndex = { placeType: PlaceTypeEnum.Port, modeOfTransport: 'SEA', inactive: false, all: searchText };
        if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
            portSearchIndex.inactive = null;
        }
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=1&size=20", portSearchIndex, false, false);
        if (portIndexs != null) {
            this.portOfLadings = portIndexs.data;
            console.log(this.portOfLadings);
        }
        else{
            this.portOfLadings = [];
        }
    }
    async getDestinations(searchText: any) {
        let portSearchIndex = { placeType: PlaceTypeEnum.Port, modeOfTransport: 'SEA', inactive: false, all: searchText };
        if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
            portSearchIndex.inactive = null;
        }
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=1&size=20", portSearchIndex, false, false);
        if (portIndexs != null) {
            this.portOfDestinations = portIndexs.data;
            console.log(this.portOfLadings);
        }
        else{
            this.portOfDestinations = [];
        }
    }

    async getColoaders(searchText: any) {
        let criteriaSearchColoader = { partnerGroup: PartnerGroupEnum.CARRIER, modeOfTransport: 'SEA', all: searchText, inactive: false };
        if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
            criteriaSearchColoader.inactive = null;
        }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchColoader, false, false);
        if (partners != null) {
            this.coloaders = partners.data;
            console.log(this.coloaders);
        }
        else{
            this.coloaders = [];
        }
    }
    async getAgents(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.AGENT, inactive: false, all: searchText };
        if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
            criteriaSearchAgent.inactive = null;
        }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.agents = partners.data;
        }
        else{
            this.agents = [];
        }
    }

    async getUserInCharges(searchText: any) {
        const users = await this.baseServices.getAsync(this.api_menu.System.User_Management.getAll, false, false);
        if (users != null) {
            this.userInCharges = users;
            if(this.shipment.id == "00000000-0000-0000-0000-000000000000"){

                let claim = localStorage.getItem('id_token_claims_obj');
                let index = this.userInCharges.findIndex(x => x.id == JSON.parse(claim)["id"]);
                if(index > -1) {
                    this.shipment.personInChargeName = this.userInCharges[index].username;
                    this.shipment.personIncharge = JSON.parse(claim)["id"];
                }
                else this.shipment.personInChargeName = '';
            }
        }
        else{
            this.userInCharges = [];
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