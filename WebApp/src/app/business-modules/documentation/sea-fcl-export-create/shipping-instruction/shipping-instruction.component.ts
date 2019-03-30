import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import { ActivatedRoute } from '@angular/router';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';

@Component({
    selector: 'app-shipping-instruction',
    templateUrl: './shipping-instruction.component.html',
    styleUrls: ['./shipping-instruction.component.scss']
})
export class ShippingInstructionComponent implements OnInit {
    shipment: CsTransaction = new CsTransaction();
    userInCharges: any[] = [];
    suppliers: any[] = [];
    listConsignees: any[] = [];
    consignees: any[] = [];
    realConsignees: any[] = [];
    listShippers: any[] = [];
    shippers: any[] = [];
    realShippers: any [] = [];
    paymentTerms: any[] = [];

    constructor(private baseServices: BaseService,
        private route: ActivatedRoute,
        private api_menu: API_MENU) { }

    async ngOnInit() {
        await this.getUserInCharges(null);
        await this.getSuppliers(null);
        await this.getConsignees(null);
        await this.getShippers(null);
        await this.getShipmentCommonData();
        if(this.listConsignees != null){
            this.consignees = this.listConsignees;
            this.realConsignees = this.listConsignees;
        }
        if(this.listShippers != null){
            this.shippers = this.listShippers;
            this.realShippers = this.listShippers;
        }
        await this.route.params.subscribe(async prams => {
            if(prams.id != undefined){          
                await this.getShipmentDetail(prams.id);
                this.getNewShipmentDetail();
            }
        });
    }
    getNewShipmentDetail(): any {
        throw new Error("Method not implemented.");
    }
    async getShipmentDetail(id: String) {
        this.shipment = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransaction.getById + id, false, true);
        console.log({"THIS":this.shipment});
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getShipmentCommonData(this.baseServices, this.api_menu);
        this.paymentTerms = dataHelper.prepareNg2SelectData(data.freightTerms, 'value', 'displayName');
    }
    async getUserInCharges(searchText: any) {
        const users = await this.baseServices.getAsync(this.api_menu.System.User_Management.getAll, false, false);
        if (users != null) {
            this.userInCharges = users;
            console.log(this.userInCharges);
        }
        else{
            this.userInCharges = [];
        }
    }
    async getSuppliers(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.CARRIER, inactive: false, all: searchText };
        // if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
        //     criteriaSearchAgent.inactive = null;
        // }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.suppliers = partners.data;
            console.log(this.suppliers);
        }
        else{
            this.suppliers = [];
        }
    }
    async getConsignees(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.CONSIGNEE, inactive: false, all: searchText };
        // if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
        //     criteriaSearchAgent.inactive = null;
        // }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.listConsignees = partners.data;
            console.log(this.listConsignees);
        }
        else{
            this.listConsignees = [];
        }
    }
    async getShippers(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.SHIPPER, inactive: false, all: searchText };
        // if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
        //     criteriaSearchAgent.inactive = null;
        // }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.listShippers = partners.data;
            console.log(this.listShippers);
        }
        else{
            this.listShippers = [];
        }
    }
    
    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    public get disabledV(): string {
        return this._disabledV;
    }

    public set disabledV(value: string) {
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
