import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import { ActivatedRoute, Router } from '@angular/router';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
import { CsShippingInstruction } from 'src/app/shared/models/document/shippingInstruction.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import  moment from 'moment/moment';
import { NgForm } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
declare var $: any;

@Component({
    selector: 'app-shipping-instruction',
    templateUrl: './shipping-instruction.component.html',
    styleUrls: ['./shipping-instruction.component.scss']
})
export class ShippingInstructionComponent implements OnInit {
    shipment: CsTransaction = new CsTransaction();
    shippingIns: CsShippingInstruction = new CsShippingInstruction();
    userIssues: any[] = [];
    suppliers: any[] = [];
    //listConsignees: any[] = [];
    consignees: any[] = [];
    realConsignees: any[] = [];
    //listShippers: any[] = [];
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
    previewSIReportLink = '';
    dataLocalSIUrl = null;
    previewOCLReportLink = '';
    dataLocalOCLUrl = null;
    previewModalId = "preview-modal";
    dataReport: any;

    constructor(private baseServices: BaseService,
        private route: ActivatedRoute,
        private api_menu: API_MENU,
        private sanitizer: DomSanitizer,
        private router: Router) {
            this.issueDate = { startDate: moment(), endDate: moment()};
         }

    async ngOnInit() {
        await this.loadReferenceData();
        await this.route.params.subscribe(async prams => {
            if(prams.id != undefined){         
                await this.getShipmentDetail(prams.id); 
                await this.getShippingInstruction(prams.id);
                if(this.shippingIns == null){
                    this.shippingIns = new CsShippingInstruction();
                    this.getNewInstructionDetail();
                }
                else{
                    this.getInstructionDetail();
                }
                await this.getHouseBillList(prams.id);
                this.getContainerInfos();
                this.getContainerList(prams.id);
                this.shippingIns.refNo = this.shipment.jobNo;
                this.isLoad = true;
                console.log(this.housebills);
            }
        });
    }
    async getContainerList(id: any) {
        let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsMawbcontainer.query, { mblid: id }, false, false);
        this.shippingIns.csMawbcontainers = responses;
    }
    async loadReferenceData() {
        await this.getUserInCharges(null);
        await this.getSuppliers(null);
        await this.getConsignees(null);
        await this.getShippers(null);
        await this.getPortOfLading(null);
        await this.getPortOfDestination(null);
        await this.getRealConsignees(null);
        await this.getRealShippers(null);
        await this.getShipmentCommonData();
    }
    getInstructionDetail(): any {
        let index = this.paymentTerms.findIndex(x => x.id == this.shippingIns.paymenType);
        if(index > -1){
            this.paymentTermActive = [this.paymentTerms[index]];
        }
        this.issueDate = { startDate: moment(this.shippingIns.invoiceDate), endDate: moment(this.shippingIns.invoiceDate) };
        this.loadingDate = { startDate: moment(this.shippingIns.invoiceDate), endDate: moment(this.shippingIns.loadingDate) };
    }
    async getShippingInstruction(id: any){
        this.shippingIns = await this.baseServices.getAsync(this.api_menu.Documentation.CsShippingInstruction.get + id, false, true);
        console.log(this.shippingIns);
    }
    async previewSIReport(){
        this.dataReport = null;
        this.shippingIns.jobId = this.shipment.id;
        this.shippingIns.csTransactionDetails = this.housebills;
        var response = await this.baseServices.postAsync(this.api_menu.Documentation.CsShippingInstruction.previewSI, this.shippingIns, false, true);
        console.log(response);
        this.dataReport = response;
        var id = this.previewModalId;
        setTimeout(function(){ 
            $('#' + id).modal('show');
        }, 100);
    }
    async previewOCLReport(){
        this.dataReport = null;
        this.shippingIns.jobId = this.shipment.id;
        this.shippingIns.csTransactionDetails = this.housebills;
        var response = await this.baseServices.postAsync(this.api_menu.Documentation.CsShippingInstruction.previewOCL, this.shippingIns, false, true);
        console.log(response);
        this.dataReport = response;
        var id = this.previewModalId;
        setTimeout(function(){ 
            $('#' + id).modal('show');
        }, 200);
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
    closeSIModal(event){
        console.log(event);
        this.router.navigate(["/home/documentation/shipping-instruction", { id: this.shipment.id }]);
    }
    async save(form: NgForm){
        this.shippingIns.jobId = this.shipment.id;
        this.shippingIns.invoiceDate = dataHelper.dateTimeToUTC(this.issueDate["startDate"]);
        this.shippingIns.loadingDate = dataHelper.dateTimeToUTC(this.loadingDate["startDate"]);
        console.log(this.shippingIns);
        if(form.valid && this.shippingIns.supplier != null 
                      && this.shippingIns.issuedUser != null 
                      && this.shippingIns.consigneeId != null
                      && this.shippingIns.paymenType != null
                      && this.shippingIns.pol != null
                      && this.shippingIns.pod != null
                      && this.loadingDate != null
                      && this.issueDate != null){
            let response = await this.baseServices.postAsync(this.api_menu.Documentation.CsShippingInstruction.update, this.shippingIns, true, true);
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
        this.shippingIns.csTransactionDetails = this.housebills;
        console.log(this.shippingIns);
        //console.log(this.shipment.csTransactionDetails);
    }
    refreshShippingInstruction(){
        this.shippingIns.invoiceNoticeRecevier = null;
        this.shippingIns.shipper = null;
        this.shippingIns.invoiceNoticeRecevier = null;
        this.shippingIns.actualShipperId = null;
        this.shippingIns.actualShipperName = null;
        this.shippingIns.actualConsigneeName = null;
        this.shippingIns.actualShipperDescription = null;
        this.shippingIns.actualConsigneeId = null;
        this.shippingIns.actualConsigneeName = null;
        this.shippingIns.actualConsigneeDescription = null;
        this.shippingIns.cargoNoticeRecevier = null;
        this.shippingIns.remark = null;
        this.shippingIns.poDelivery = null;
        this.getNewInstructionDetail();
        this.getContainerInfos();
    }
    getNewInstructionDetail(): any {
        this.shippingIns.bookingNo = this.shipment.bookingNo;
        let index = this.suppliers.findIndex(x => x.id == this.shipment.coloaderId);
        if(index > -1){
            this.shippingIns.supplier = this.suppliers[index].id;
            this.shippingIns.supplierName = this.suppliers[index].partnerNameEn;
        }
        let claim = localStorage.getItem('id_token_claims_obj');
        index = this.userIssues.findIndex(x => x.id == JSON.parse(claim)["id"]);
        if(index > -1) {
            this.shippingIns.issuedUserName = this.userIssues[index].username;
            this.shippingIns.issuedUser = JSON.parse(claim)["id"];
        }
        index = this.consignees.findIndex(x => x.id == this.shipment.agentId);
        if(index > -1)
        {
            this.shippingIns.consigneeId = this.consignees[index].id;
            this.shippingIns.consigneeName = this.consignees[index].partnerNameEn;
            this.shippingIns.consigneeDescription = this.consignees[index].partnerNameEn;
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
            this.shippingIns.poDelivery = this.portOfDestinations[index].nameEN;
        }
        this.paymentTermActive = ["Prepaid"];
        this.shippingIns.paymenType = this.paymentTermActive[0];
        this.loadingDate = { startDate: moment(this.shipment.etd), endDate: moment(this.shipment.etd) };
        this.shippingIns.voyNo = this.shipment.flightVesselName + " & " + this.shipment.voyNo;
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
            this.userIssues = users;
            console.log(this.userIssues);
        }
        else{
            this.userIssues = [];
        }
    }
    changeSupplier(keySearch: string) {
        keySearch = keySearch != null?keySearch.trim(): null;
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getSuppliers(keySearch);
    }
    changeConsignee(keySearch: string) {
        keySearch = keySearch != null?keySearch.trim(): null;
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getConsignees(keySearch);
    }
    changeRealShipper(keySearch: string) {
        keySearch = keySearch != null?keySearch.trim(): null;
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getRealShippers(keySearch);
    }
    changeRealConsignee(keySearch: string) {
        keySearch = keySearch != null?keySearch.trim(): null;
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getRealConsignees(keySearch);
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
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.ALL, inactive: false, all: searchText };
        // if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
        //     criteriaSearchAgent.inactive = null;
        // }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.consignees = partners.data;
        }
        else{
            this.consignees = [];
        }
    }
    async getRealConsignees(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.ALL, inactive: false, all: searchText };
        // if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
        //     criteriaSearchAgent.inactive = null;
        // }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.realConsignees = partners.data;
            console.log(this.realConsignees);
        }
        else{
            this.realConsignees = [];
        }
    }
    async getShippers(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.SHIPPER, inactive: false, all: searchText };
        // if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
        //     criteriaSearchAgent.inactive = null;
        // }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.shippers = partners.data;
            console.log(this.shippers);
        }
        else{
            this.shippers = [];
        }
    }
    async getRealShippers(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.SHIPPER, inactive: false, all: searchText };
        // if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
        //     criteriaSearchAgent.inactive = null;
        // }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.realShippers = partners.data;
            console.log(this.realShippers);
        }
        else{
            this.realShippers = [];
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
