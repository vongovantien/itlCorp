import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import { ActivatedRoute } from '@angular/router';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
import { CsShippingInstruction } from 'src/app/shared/models/document/shippingInstruction.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import moment from 'moment/moment';
import { NgForm } from '@angular/forms';

@Component({
    selector: 'app-shipping-instruction',
    templateUrl: './shipping-instruction.component.html',
    styleUrls: ['./shipping-instruction.component.scss']
})
export class ShippingInstructionComponent implements OnInit {
    shipment: CsTransaction = new CsTransaction();
    shippingIns: CsShippingInstruction = new CsShippingInstruction();
    userInCharges: any[] = [];
    suppliers: any[] = [];
    listConsignees: any[] = [];
    consignees: any[] = [];
    realConsignees: any[] = [];
    listShippers: any[] = [];
    shippers: any[] = [];
    realShippers: any [] = [];
    paymentTerms: any[] = [];
    housebills: any[] = [];
    isLoad: boolean = false;
    paymentTermActive: any[] = [];
    portOfLadings: any[] = [];
    portOfDestinations: any[] = [];
    issueDate: any;
    loadingDate: any;
    totalGW = 0;
    totalCBM = 0;

    constructor(private baseServices: BaseService,
        private route: ActivatedRoute,
        private api_menu: API_MENU) {
            this.issueDate = { startDate: moment(), endDate: moment()};
         }

    async ngOnInit() {
        await this.getUserInCharges(null);
        await this.getSuppliers(null);
        await this.getConsignees(null);
        await this.getShippers(null);
        await this.getPortOfLading(null);
        await this.getPortOfDestination(null);
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
                this.shippingIns.jobId = this.shipment.id;
                await this.getShippingInstruction(prams.id);
                if(this.shippingIns == null){
                    this.getNewInstructionDetail();
                }
                await this.getHouseBillList(prams.id);
                this.getContainerInfos();
                this.shippingIns.refNo = this.shipment.jobNo;
                this.isLoad = true;
                console.log(this.housebills);
            }
        });
    }
    async getShippingInstruction(id: any){
        this.shippingIns = await this.baseServices.getAsync(this.api_menu.Documentation.CsShippingInstruction.get + id, false, true);
        console.log(this.shippingIns);
    }
    getContainerInfos(){
        this.totalCBM = 0;
        this.totalGW = 0;
        this.shippingIns.goodsDescription = '';
        this.shippingIns.containerNote = '';
        this.shipment.packageContainer = '';
        this.housebills.forEach(x =>{
                this.totalGW = this.totalGW + x.gw;
                this.totalCBM = this.totalCBM + x.cbm;
                this.shippingIns.goodsDescription += (x.desOfGoods!= null|| x.desOfGoods == '')? (x.desOfGoods + "\n"): '';
                this.shippingIns.containerNote += (x.containerNames != null || x.containerNames =='')? (x.containerNames + "\n"): '';
                this.shippingIns.packagesNote += (x.packageTypes != null || x.packageTypes == '')? (x.packageTypes + "\n"): '';
        });
        this.shippingIns.grossWeight = this.totalGW;
        this.shippingIns.volume= this.totalCBM;
    }
    async save(form: NgForm){
        this.shippingIns.invoiceDate = dataHelper.dateTimeToUTC(this.issueDate["startDate"]);
        if(form.valid){
            let response = await this.baseServices.postAsync(this.api_menu.Documentation.CsShippingInstruction.update, this.shippingIns, true, true);
            await this.getShippingInstruction(this.shipment.id);
            this.shippingIns.refNo = this.shipment.jobNo;
        }
    }
    getConsigneeDescription(consignee: any) {
        console.log(consignee);
        this.shippingIns.consigneeDescription =
            consignee.shortName + "\n" +
            "Address: " + consignee.addressEn;
    }
    getRealConsigneeDescription(consignee: any) {
        console.log(consignee);
        this.shippingIns.actualConsigneeDescription =
            consignee.shortName + "\n" +
            "Address: " + consignee.addressEn;
    }
    getRealShipperDescription(shipper: any) {
        console.log(shipper);
        this.shippingIns.actualShipperDescription =
        shipper.shortName + "\n" +
            "Address: " + shipper.addressEn;
    }
    async getHouseBillList(jobId: String){
        var responses = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + jobId, false, false);
        if(responses != null){
            this.housebills = responses;
        }
        else{
            this.housebills = [];
        }
        console.log(this.shipment.csTransactionDetails);
    }
    getNewInstructionDetail(): any {
        this.shippingIns.bookingNo = this.shipment.bookingNo;
        let index = this.suppliers.findIndex(x => x.id == this.shipment.coloaderId);
        if(index > -1){
            this.shippingIns.supplier = this.suppliers[index].id;
            this.shippingIns.supplierName = this.suppliers[index].partnerNameEn;
        }
        let claim = localStorage.getItem('id_token_claims_obj');
        index = this.userInCharges.findIndex(x => x.id == JSON.parse(claim)["id"]);
        if(index > -1) {
            this.shippingIns.issuedUserName = this.userInCharges[index].username;
            this.shippingIns.issuedUser = JSON.parse(claim)["id"];
        }
        index = this.consignees.findIndex(x => x.id == this.shipment.agentId);
        if(index > -1)
        {
            this.shippingIns.consigneeId = this.consignees[index].id;
            this.shippingIns.consigneeName = this.consignees[index].partnerNameEn;
        }
        index = this.portOfLadings.findIndex(x => x.id == this.shipment.pol);
        if(index > -1)
        {
            this.shippingIns.pol = this.portOfLadings[index].id;
            this.shippingIns.polName = this.portOfLadings[index].nameEN;
        }
        index = this.portOfDestinations.findIndex(x => x.id == this.shipment.pod);
        if(index > -1){
            this.shippingIns.pod = this.portOfDestinations[index].id;
            this.shippingIns.podName = this.portOfDestinations[index].nameEN;
        }
        this.paymentTermActive = ["Prepaid"];
        this.shippingIns.paymenType = "Prepaid";
        this.loadingDate = { startDate: moment(this.shipment.etd), endDate: moment(this.shipment.etd) };
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
    async getPortOfLading(searchText: any) {
        let portSearchIndex = { placeType: PlaceTypeEnum.Port, modeOfTransport: 'SEA', all: searchText };
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=1&size=20", portSearchIndex, false, false);
        if (portIndexs != null) {
            this.portOfLadings = portIndexs.data;
            console.log(this.portOfLadings);
        }
        else{
            this.portOfLadings = [];
        }
    }
    async getPortOfDestination(searchText: any) {
        let portSearchIndex = { placeType: PlaceTypeEnum.Port, modeOfTransport: 'SEA', all: searchText };
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=1&size=20", portSearchIndex, false, false);
        if (portIndexs != null) {
            this.portOfDestinations = portIndexs.data;
            console.log(this.portOfLadings);
        }
        else{
            this.portOfDestinations = [];
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
