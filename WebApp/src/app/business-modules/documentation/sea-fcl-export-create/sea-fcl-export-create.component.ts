import moment from 'moment/moment'
import { Component, OnInit, ViewChild, ChangeDetectorRef, Output, EventEmitter, ElementRef } from '@angular/core';
// import { PaginationComponent } from 'ngx-bootstrap';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { NgForm, FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';
import { MasterBillComponent } from './master-bill/master-bill.component';
import * as dataHelper from 'src/helper/data.helper';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
declare var $: any;
import { ActivatedRoute, Router } from '@angular/router';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import {ExtendData} from '../extend-data';
import { SortService } from 'src/app/shared/services/sort.service';
import * as stringHelper from 'src/helper/string.helper';
import { TransactionTypeEnum } from 'src/app/shared/enums/transaction-type.enum';


export class FirstLoadData {
    lstPartner: any[] = null;
    lstUnit: any[] = null;
    lstCurrency: any[] = null;
    lstCharge: any[] = null;
}

@Component({
    selector: 'app-sea-fcl-export-create',
    templateUrl: './sea-fcl-export-create.component.html',
    styleUrls: ['./sea-fcl-export-create.component.scss']
})


export class SeaFclExportCreateComponent implements OnInit {


    _firstLoadData:FirstLoadData = new FirstLoadData();
    
    shipment: CsTransaction;
    containerTypes: any[] = [];
    lstMasterContainers: any[] = [];
    lstContainerTemp: any[];
    packageTypes: any[] = [];
    commodities: any[] = [];
    weightMesurements: any[] = [];
    listContainerType: any[] = [];
    listPackageTypes: any[] = [];
    listWeightMesurement: any[] = [];
    myForm: FormGroup;
    submitted = false;
    searchcontainer: string = '';
    @ViewChild('containerMasterForm') containerMasterForm: NgForm;
    @ViewChild(MasterBillComponent) masterBillComponent: any;
    @ViewChild('containerSelect') containerSelect: ElementRef;
    selectedCommodityValue: any;
    numberOfTimeSaveContainer: number = 0;
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };
    //
    housebillTabviewHref = '#';//'#confirm-create-job-modal';
    housebillRoleToggle = 'modal';
    indexItemConDelete = null;
    isLoaded = true;
    inEditing: boolean = false;
    isImport: boolean = false;
     /**
        * problem: Bad performance when switch between 'Shipment Detail' tab and 'House Bill List' tab
        * this method imporove performance for web when detecting change 
        * for more informations, check this reference please 
        * https://blog.bitsrc.io/boosting-angular-app-performance-with-local-change-detection-8a6a3afa8d4d
        *
      */
    
    isShipment:boolean = true;
    isHouseBill:boolean = false;
    isCDnote:boolean = false;
    switchTab(tab:string){

        if(tab==="shipment"){
            this.isShipment = true;
            this.isHouseBill = false;
            this.isCDnote = false;
            if(this.inEditing == true){
                this.isLoaded = false;
                setTimeout(() => {
                    this.isLoaded = true;
                    this.inEditing = true;
                }, 100);
                this.myForm.patchValue({
                    estimatedTimeofDepature: { startDate: moment(this.shipment.etd), endDate: moment(this.shipment.etd) },
                    polName: this.shipment.polName,
                    podName: this.shipment.podName,
                    coloaderName: this.shipment.coloaderName,
                    agentName: this.shipment.agentName,
                    personInChargeName: this.shipment.personInChargeName
                });
            }
        }
        if(tab==="housebilllist"){
            if(this.inEditing == false){
                this.validateShipmentForm();
            }
            else{
                this.isShipment = false;
                this.isHouseBill = true;
                this.isCDnote = false;
            }
        }
        if(tab==="cdnote"){
            this.isShipment = false;
            this.isHouseBill = false;
            this.isCDnote = true;
        }
        this.cdr.detectChanges(); 
    }

    //open tab by link
    activeTab() {
        $('#masterbill-tablink').removeClass('active');
        $('#masterbill-tabview-tab').removeClass('active show');
        $('#housebill-tablink').addClass('active');
        $('#housebill-tabview-tab').addClass('active show');
    }

    constructor(private baseServices: BaseService,
        private sortService: SortService,
        private router:Router,
        private route: ActivatedRoute,
        private api_menu: API_MENU, private fb: FormBuilder, private cdr: ChangeDetectorRef) {
        this.initNewShipmentForm();
    }

    async ngOnInit() {
        this.baseServices.spinnerShow();
        await this.route.params.subscribe(async prams => {
            this.shipment = new CsTransaction();
            this.shipment.transactionTypeEnum = TransactionTypeEnum.SeaFCLExport;
            if(prams.id != undefined){
                this.inEditing = true;
                this.shipment.id = prams.id;         
                await this.getTotalProfit();      
                //await this.getShipmentDetail(this.shipment.id);
                await this.getShipmentContainer(this.shipment.id);
                this.housebillTabviewHref = "#housebill-tabview-tab";
                this.housebillRoleToggle = "tab";
                console.log(this.myForm.controls.estimatedTimeofDepature);
                if(this.isImport){
                    this.submitted = false;
                    this.isImport = false;
                }
            }
        });
        this.getContainerTypes();
        this.getPackageTypes();
        this.getComodities(null);
        this.getWeightTypes();
        this.getListPartner();
        this.getListUnits();
        this.getListCurrency();
        console.log(this.lstMasterContainers);
    }
    async getShipmentContainer(id: String) {
        let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsMawbcontainer.query, { mblid: id }, false, false);
        this.shipment.csMawbcontainers = this.lstMasterContainers = responses;

        if (this.lstMasterContainers != null) {
            if(this.isImport){
                this.lstMasterContainers.forEach(function(element) {
                    element.containerNo = '';
                    element.sealNo = '';
                    element.markNo = '';
                  });
            }
            else{
                this.shipment.packageContainer = '';
                this.getShipmentContainerDescription(this.lstMasterContainers);
            }
        }
        this.lstContainerTemp = this.lstMasterContainers;
    }
    getShipmentContainerDescription(listContainers: any[]){
        for (var i = 0; i < listContainers.length; i++) {
            listContainers[i].isSave = true;
            // if(this.shipment.id == "00000000-0000-0000-0000-000000000000"){
                this.shipment.grossWeight = this.shipment.grossWeight + listContainers[i].gw;
                this.shipment.netWeight = this.shipment.netWeight + listContainers[i].nw;
                this.shipment.chargeWeight = this.shipment.chargeWeight + listContainers[i].chargeAbleWeight;
                this.shipment.cbm = this.shipment.cbm + listContainers[i].cbm;
                
                if((listContainers[i].quantity != null && listContainers[i].containerTypeName!=null)){
                    let temp = listContainers[i].containerTypeName + "x" + listContainers[i].quantity;
                    if(listContainers[i].packageTypeName != null && listContainers[i].packageQuantity != null){
                        temp = temp + ": " + listContainers[i].packageTypeName + "x" + listContainers[i].packageQuantity;
                    }
                    temp = temp + ", ";
                    if(!this.shipment.packageContainer.includes(temp)){
                        this.shipment.packageContainer = this.shipment.packageContainer + temp;
                    }
                }
                if(this.numberOfTimeSaveContainer == 1 && this.shipment.id == "00000000-0000-0000-0000-000000000000"){
                    //this.shipment.packageContainer = this.shipment.packageContainer + ((listContainers[i].quantity == null && listContainers[i].containerTypeName==null)?"": (listContainers[i].quantity + "x" + listContainers[i].containerTypeName + ", "));
                    if(listContainers[i].commodityName != "" || listContainers[i].commodityName != null){
                        if(!this.shipment.commodity.includes(listContainers[i].commodityName)){
                            this.shipment.commodity = this.shipment.commodity + listContainers[i].commodityName + ", ";
                        }
                    }
                    if(listContainers[i].description != null){
                        if(!this.shipment.desOfGoods.includes(listContainers[i].description)){
                            this.shipment.desOfGoods = this.shipment.desOfGoods + listContainers[i].description + "\n";
                        }
                    }
                    //this.shipment.commodity = this.shipment.commodity + ((listContainers[i].commodityName== "" || listContainers[i].commodityName == null)?"": (listContainers[i].commodityName + ", "));
                    //this.shipment.desOfGoods = this.shipment.desOfGoods + (listContainers[i].description== null?"": (listContainers[i].description + ", "));
                }
            // }
        }
        this.removeEndComma();
    }
    removeEndComma(){
        this.shipment.commodity = stringHelper.subStringComma(this.shipment.commodity);
        this.shipment.desOfGoods = stringHelper.subStringComma(this.shipment.desOfGoods);
        this.shipment.packageContainer = stringHelper.subStringComma(this.shipment.packageContainer);
    }
    async getShipmentDetail(id: String) {
        this.shipment = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransaction.getById + id, false, true);
        ExtendData.currentJobID = this.shipment.id;
        console.log({"THIS":this.shipment});
    }
    initNewShipmentForm() {
        this.myForm = this.fb.group({
            jobId: new FormControl({ value: '', disabled: true }),
            estimatedTimeofDepature: new FormControl('', Validators.required),
            estimatedTimeofArrived: new FormControl(''),
            mawb: new FormControl('', Validators.required),
            //mbltype: new FormControl(null, Validators.required),
            coloaderId: new FormControl(''),
            coloaderName: new FormControl(''),
            bookingNo: new FormControl(''),
            //typeOfService: new FormControl(null, Validators.required),
            flightVesselName: new FormControl(''),
            agentId: new FormControl(null),
            agentName: new FormControl(null),
            pol: new FormControl(null),
            polName: new FormControl(null),
            pod: new FormControl(null),
            podName: new FormControl(null),
            //paymentTerm: new FormControl(''),
            voyNo: new FormControl(''),
            //shipmentType: new FormControl(null, Validators.required),
            pono: new FormControl(''),
            personIncharge: new FormControl(''),
            personInChargeName: new FormControl(''),
            notes: new FormControl(''),
            commodity: new FormControl(''),
            packageContainer: new FormControl(''),
            desOfGoods: new FormControl('')
        });
    }
    async getContainerTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Container", inactive: false }, false, false);
        this.listContainerType = responses;
        // if (responses != null) {
        //     this.containerTypes = dataHelper.prepareNg2SelectData(responses, 'id', 'unitNameEn');
        // }
    }
    async getWeightTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Weight Measurement", inactive: false }, false, false);
        this.listWeightMesurement = responses;
    }
    async getPackageTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Package", inactive: false }, false, false);
        this.listPackageTypes = responses;
    }
    async getComodities(searchText: any) {
        let criteriaSearchCommodity = { inactive: false, all: searchText };
        if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
            criteriaSearchCommodity.inactive = null;
        }
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Commodity.paging + "?page=1&size=20", criteriaSearchCommodity, false, false);
        if(responses != null){
            this.commodities = this.sortService.sort(responses.data, 'commodityNameEn', true);
            console.log(this.commodities);
        }
        else{
            this.commodities = [];
        }
    }
    
    changeComodity(keySearch: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getComodities(keySearch);
    }
    validateShipmentForm(){
        if(this.lstMasterContainers.find(x => x.isNew == false) != null){
            this.shipment.csMawbcontainers = this.lstMasterContainers.filter(x => x.isNew == false);
        }
        if(this.myForm.value.estimatedTimeofDepature != null){
            this.shipment.etd = dataHelper.dateTimeToUTC(this.myForm.value.estimatedTimeofDepature["startDate"]);
        }
        if(this.myForm.value.estimatedTimeofArrived != null){
            this.shipment.eta = dataHelper.dateTimeToUTC(this.myForm.value.estimatedTimeofArrived["startDate"]);
        }
        if(this.myForm.invalid || this.shipment.pol == null 
            || this.shipment.mbltype == null
            || this.shipment.typeOfService == null
            || this.shipment.paymentTerm == null
            || this.shipment.shipmentType == null
            || (this.shipment.etd >= this.shipment.eta && this.shipment.eta != null)
            || this.shipment.pol == this.shipment.pod){
                $('#confirm-can-not-create-job-modal').modal('show');
        }
        else{
            if(this.shipment.csMawbcontainers == null){
                $('#confirm-not-create-job-misscont-modal').modal('show');
            }
            else{
                $('#confirm-create-job-modal').modal('show');
            }
        }
    }
    confirmCreateJob(){
        this.validateShipmentForm();
    }
    async onSubmit() {
        this.submitted = true;
        let validDate = true;
        if(this.shipment.eta != null){
            if(this.shipment.etd >= this.shipment.eta){
                validDate = false;
            }
        }
        if (this.myForm.valid && this.shipment.pol != null 
            && this.shipment.mbltype != null
            && this.shipment.typeOfService != null
            && this.shipment.paymentTerm != null
            && this.shipment.shipmentType != null
            && this.shipment.etd != null
            && this.shipment.pod != this.shipment.pol
            && validDate == true) {
            console.log('abc');
            await this.saveJob();
        }
    }
    
    showListContainer(){
        this.visible = true;
        //container-list-of-job-modal-master
        if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
            for(let i=0; i< this.lstMasterContainers.length; i++){
                this.lstMasterContainers[i].allowEdit = false;
                this.lstMasterContainers[i].isNew = false;
                this.lstMasterContainers[i].containerTypeActive = this.lstMasterContainers[i].containerTypeId != null? [{ id: this.lstMasterContainers[i].containerTypeId, text: this.lstMasterContainers[i].containerTypeName }]: [];
                this.lstMasterContainers[i].packageTypeActive = this.lstMasterContainers[i].packageTypeId != null? [{ id: this.lstMasterContainers[i].packageTypeId, text: this.lstMasterContainers[i].packageTypeName }]: [];
                this.lstMasterContainers[i].unitOfMeasureActive = this.lstMasterContainers[i].unitOfMeasureID!= null? [{ id: this.lstMasterContainers[i].unitOfMeasureID, text: this.lstMasterContainers[i].unitOfMeasureName }]: [];
            }
        }
        else{
            if(this.lstMasterContainers.length == 0){
                this.lstMasterContainers.push(this.initNewContainer());
            }
        }
        $('#container-list-of-job-modal-master').modal('show');
        $("#containerSelect").click();
    }
    visible = false;
    async saveJob(){
        if(this.inEditing == false){
            if(this.isImport){
                this.importShipment();
            }
            else{
                this.addShipment();
            }
        }
        else{
            this.updateShipment();
        }
    }
    async importShipment(){
        var response = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransaction.import, this.shipment, true, true);
        if(response != null){
            if(response.result.success){
                this.shipment = response.model;
                this.shipment.transactionTypeEnum = TransactionTypeEnum.SeaFCLExport;
                this.isShipment = true;
                this.isHouseBill = false;
                this.isCDnote = false;
                this.router.navigate(["/home/documentation/sea-fcl-export-create/",{ id: this.shipment.id }]);
                this.isLoaded = false;
                // if(this.inEditing == false){
                //     this.activeTab();
                // }
                this.inEditing = true;
                setTimeout(() => {
                    this.isLoaded = true;
                  }, 300);
            }
        }
    }
    async updateShipment(){
        var response = await this.baseServices.putAsync(this.api_menu.Documentation.CsTransaction.update, this.shipment, true, true);
        if(response != null){
            if(response.status){
                this.shipment.csMawbcontainers = this.lstMasterContainers;
            }
        }
    }
    async addShipment(){
        var response = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransaction.post, this.shipment, true, true);
        if(response != null){
            if(response.result.success){
                this.shipment = response.model;
                this.shipment.transactionTypeEnum = TransactionTypeEnum.SeaFCLExport;
                this.isShipment = true;
                this.isHouseBill = false;
                this.isCDnote = false;
                this.router.navigate(["/home/documentation/sea-fcl-export-create/",{ id: this.shipment.id }]);
                this.isLoaded = false;
                // if(this.inEditing == false){
                //     this.activeTab();
                // }
                this.inEditing = true;
                setTimeout(() => {
                    this.isLoaded = true;
                  }, 300);
            }
        }
        // this.housebillTabviewHref = "#housebill-tabview-tab";
        // this.housebillRoleToggle = "tab";
    }
    cancelSaveJob() {
        $('#confirm-create-job-modal').modal('hide');
    }
    async showShipment(event){
        await this.getShipmentDetail(event);
        this.isLoaded = false;
        //this.shipment.jobNo = null;
        this.shipment.mawb = null;
        this.isImport = true;
        await this.getShipmentContainer(event);
        this.getHouseBillList(event);
        setTimeout(() => {
            this.isLoaded = true;
          }, 300);
          this.inEditing = false;
        console.log(this.shipment);
    }
    async getHouseBillList(jobId: String){
        var responses = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + jobId, false, false);
        this.shipment.csTransactionDetails = responses;
    }
    duplicateJob(){
        let id = this.shipment.id;
        this.shipment.jobNo = null;
        this.shipment.mawb = null;
        this.isImport = true;
        this.shipment.createdDate = null;
        this.shipment.userCreated = null;
        this.shipment.modifiedDate = null;
        this.shipment.userModified = null;
        this.housebillTabviewHref = '#';//'#confirm-create-job-modal';
        this.housebillRoleToggle = 'modal';
        this.isLoaded = false;
        setTimeout(() => {
            this.isLoaded = true;
            this.inEditing = false;
          }, 300);
        this.myForm.patchValue({
            estimatedTimeofDepature: null,
            estimatedTimeofArrived: null,
            polName: this.shipment.pol,
            podName: this.shipment.podName,
            coloaderName: this.shipment.coloaderName,
            agentName: this.shipment.agentName,
            personInChargeName: this.shipment.personInChargeName
          });
    }
    /**
     * Container
     */
    
    initNewContainer() {
        var container = {
            mawb: this.shipment.id,
            containerTypeId: null,
            containerTypeName: '',
            containerTypeActive: [],
            quantity: 1,
            containerNo: '',
            sealNo: '',
            markNo: '',
            unitOfMeasureId: 37,
            unitOfMeasureName: 'Kilogam',
            unitOfMeasureActive: [{ "id": 37, "text": "Kilogam"}],
            commodityId: null,
            commodityName: '',
            packageTypeId: null,
            packageTypeName: '',
            packageTypeActive: [],
            packageQuantity: null,
            description: null,
            gw: null,
            nw: null,
            chargeAbleWeight: null,
            cbm: null,
            packageContainer: '',
            allowEdit: true,
            isNew: true,
            verifying: false
        };
        this.filterContainer(this.lstMasterContainers.length);
        return container;
    }
    saveContainerSuccess = false;
    addNewContainer() {
        let hasItemEdited = false;
        let index = -1;
        for (let i = 0; i < this.lstMasterContainers.length; i++) {
            if (this.lstMasterContainers[i].allowEdit == true) {
                hasItemEdited = true;
                index = i;
                break;
            }
        }
        if (hasItemEdited == false) {
            console.log(this.containerMasterForm);
            this.lstMasterContainers.push(this.initNewContainer());
        }
        else {
            this.saveNewContainer(index);
            if(this.saveContainerSuccess){
                this.lstMasterContainers.push(this.initNewContainer());
            }
        }
    }
    removeAContainer(index: number){
        this.indexItemConDelete = index;
    }
    removeContainer(){
        this.lstMasterContainers.splice(this.indexItemConDelete, 1);
        this.shipment.csMawbcontainers = this.lstMasterContainers;
        $('#confirm-accept-delete-container-modal').modal('hide');
        this.resetSumContainer();
    }
    async saveNewContainer(index: any){
        
        if(this.lstMasterContainers[index].containerNo.length > 0 || this.lstMasterContainers[index].sealNo.length > 0 || this.lstMasterContainers[index].markNo.length > 0){
            this.lstMasterContainers[index].quantity = 1;
        }
        console.log(this.containerMasterForm.submitted && this.lstMasterContainers[index].containerTypeId == null && this.lstMasterContainers[index].verifying);
        this.lstMasterContainers[index].verifying = true;
        if(this.lstMasterContainers[index].quantity == null || this.lstMasterContainers[index].containerTypeId == null){
            this.saveContainerSuccess = false;
            return;
        } 
        //Cont Type, Cont Q'ty, Container No, Package Type
        let existedItems = this.lstMasterContainers.filter(x => x.containerTypeId == this.lstMasterContainers[index].containerTypeId
            && x.quantity == this.lstMasterContainers[index].quantity);
        if(this.lstMasterContainers[index].containerNo.length != 0 && this.lstMasterContainers[index].packageTypeId != null){
            existedItems = existedItems.filter(x => x.containerNo == this.lstMasterContainers[index].containerNo
                && x.packageTypeId == this.lstMasterContainers[index].packageTypeId);
        }
        else
        {
            existedItems = [];
        }
        if(existedItems.length > 1) { 
            this.lstMasterContainers[index].inValidRow = true;
            this.saveContainerSuccess = false;
        }
        else{
            if(this.lstMasterContainers[index].isNew == true){
                this.lstMasterContainers[index].isNew = false;
            } 
            else{
                this.lstMasterContainers[index].inValidRow = false;
                this.lstMasterContainers[index].containerTypeActive = this.lstMasterContainers[index].containerTypeId != null? [{ id: this.lstMasterContainers[index].containerTypeId, text: this.lstMasterContainers[index].containerTypeName }]: [];
                this.lstMasterContainers[index].packageTypeActive = this.lstMasterContainers[index].packageTypeId != null? [{ id: this.lstMasterContainers[index].packageTypeId, text: this.lstMasterContainers[index].packageTypeName }]: [];
                this.lstMasterContainers[index].unitOfMeasureActive = this.lstMasterContainers[index].unitOfMeasureId!= null? [{ id: this.lstMasterContainers[index].unitOfMeasureId, text: this.lstMasterContainers[index].unitOfMeasureName }]: [];
            }
            this.saveContainerSuccess = true;
            this.lstMasterContainers[index].allowEdit = false;
        }
        this.lstContainerTemp = Object.assign([], this.lstMasterContainers);
    }


    cancelNewContainer(index: number) {
        if (this.lstMasterContainers[index].isNew == true) {
            this.lstMasterContainers.splice(index, 1);
        }
        else {
            this.lstMasterContainers[index].allowEdit = false;
        }
    }
    filterContainer(index: number){
        if (this.listContainerType != null) {
            if(this.lstMasterContainers[index] == null){
                this.containerTypes = dataHelper.prepareNg2SelectData(this.listContainerType, 'id', 'unitNameEn');
                this.packageTypes = dataHelper.prepareNg2SelectData(this.listPackageTypes, 'id', 'unitNameEn');
                this.weightMesurements = dataHelper.prepareNg2SelectData(this.listWeightMesurement, 'id', 'unitNameEn');
            }
            else{
                if(this.lstMasterContainers[index]["id"] != null){
                    let conts = this.listContainerType.filter(x => x.inactive == false);
                    let packs = this.listPackageTypes.filter(x => x.inactive == false);
                    let weights = this.listWeightMesurement.filter(x => x.inactive == false);
                    this.containerTypes = dataHelper.prepareNg2SelectData(conts, 'id', 'unitNameEn');
                    this.packageTypes = dataHelper.prepareNg2SelectData(packs, 'id', 'unitNameEn');
                    this.weightMesurements = dataHelper.prepareNg2SelectData(weights, 'id', 'unitNameEn');
                }
            }
        }
        else{
            this.containerTypes = [];
        }
    }
    changeEditStatus(index: any) {
        this.filterContainer(index);
        let hasItemEdited = false;
        for(let i =0; i< this.lstMasterContainers.length; i++){
            if(i != index && this.lstMasterContainers[i].allowEdit){
                hasItemEdited = true;
                break;
            }
        }
        if(hasItemEdited){
            this.baseServices.errorToast("You must save current container");
        }
        else{
            if (this.lstMasterContainers[index].allowEdit == false) {
                this.lstMasterContainers[index].allowEdit = true;
                this.lstMasterContainers[index].containerTypeActive = this.lstMasterContainers[index].containerTypeId != null? 
                                                [{ id: this.lstMasterContainers[index].containerTypeId, text: this.lstMasterContainers[index].containerTypeName }]: [];
                this.lstMasterContainers[index].packageTypeActive = this.lstMasterContainers[index].packageTypeId != null? 
                                                [{ id: this.lstMasterContainers[index].packageTypeId, text: this.lstMasterContainers[index].packageTypeName }]: [];
                this.lstMasterContainers[index].unitOfMeasureActive = this.lstMasterContainers[index].unitOfMeasureID != null? 
                                                [{ id: this.lstMasterContainers[index].unitOfMeasureID, text: this.lstMasterContainers[index].unitOfMeasureName }]: [];
                for(let i =0; i< this.lstMasterContainers.length; i++){
                    if(i != index){
                        this.lstMasterContainers[i].allowEdit = false;
                    }
                }
            }
            else {
                this.lstMasterContainers[index].allowEdit = false;
            }
        }
    }
    async onSubmitContainer() {
        
        for(let i=0; i< this.lstMasterContainers.length; i++){
            this.lstMasterContainers[i].verifying = true;
        }
        if (this.containerMasterForm.valid) {
            let hasItemEdited = false;
            for(let i=0; i< this.lstMasterContainers.length; i++){
                if(this.lstMasterContainers[i].allowEdit == true){
                    hasItemEdited = true;
                    break;
                }
            }
            if(hasItemEdited == false){
                if(this.inEditing == false || this.isImport == true){
                    this.shipment.id = "00000000-0000-0000-0000-000000000000";
                    this.numberOfTimeSaveContainer = this.numberOfTimeSaveContainer + 1;
                    this.shipment.grossWeight = 0;
                    this.shipment.netWeight = 0;
                    this.shipment.chargeWeight = 0;
                    this.shipment.cbm = 0;
                }
                if (this.numberOfTimeSaveContainer == 1 && this.inEditing == false) {
                    this.shipment.commodity = '';
                    this.shipment.desOfGoods = '';
                }
                this.shipment.packageContainer = '';
                this.getShipmentContainerDescription(this.lstMasterContainers);
                this.shipment.csMawbcontainers = this.lstMasterContainers;
            
                if(this.shipment.id != "00000000-0000-0000-0000-000000000000" && this.isImport == false && this.inEditing == true){
                    var response = await this.baseServices.putAsync(this.api_menu.Documentation.CsMawbcontainer.update, { csMawbcontainerModels: this.shipment.csMawbcontainers, masterId: this.shipment.id}, true, false);
                    let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsMawbcontainer.query, {mblid: this.shipment.id}, false, false);
                }
                $('#container-list-of-job-modal-master').modal('hide');
            }
            else{
                this.baseServices.errorToast("Current container must be save!!!");
            }
        }
    }
    searchContainer(keySearch: any) {
        keySearch = keySearch != null ? keySearch.trim().toLowerCase() : "";
        this.lstMasterContainers = Object.assign([], this.lstContainerTemp).filter(
            item => (item.containerTypeName.toLowerCase().includes(keySearch)
                || (item.quantity != null ? item.quantity.toString() : "").includes(keySearch)
                || (item.containerNo != null ? item.containerNo.toLowerCase() : "").includes(keySearch)
                || (item.sealNo != null ? item.sealNo.toLowerCase() : "").includes(keySearch)
                || (item.markNo != null ? item.markNo.toLowerCase() : "").includes(keySearch)
                || (item.commodityName != null ? item.commodityName.toLowerCase() : "").includes(keySearch)
                || (item.packageTypeName != null ? item.packageTypeName.toLowerCase() : "").includes(keySearch)
                || (item.packageQuantity != null ? item.packageQuantity.toString().toLowerCase() : "").includes(keySearch)
                || (item.description != null ? item.description.toLowerCase() : "").includes(keySearch)
                || (item.nw != null ? item.nw.toString().toLowerCase() : "").includes(keySearch)
                || (item.chargeAbleWeight != null ? item.chargeAbleWeight.toString() : "").toLowerCase().includes(keySearch)
                || (item.gw != null ? item.gw.toString().toLowerCase() : "").includes(keySearch)
                || (item.unitOfMeasureName != null ? item.unitOfMeasureName.toLowerCase() : "").includes(keySearch)
                || (item.cbm != null ? item.cbm.toString().toLowerCase() : "").includes(keySearch)
            )
        );
        console.log(this.lstMasterContainers);
    }
    closeContainerPopup() {
        let index = this.lstMasterContainers.findIndex(x => x.isNew == true);
        if (index > -1 && this.lstMasterContainers.length > 1) {
            this.lstMasterContainers.splice(index, 1);
        }
        $('#container-list-of-job-modal-master').modal('hide');
        // if(this.shipment.csMawbcontainers == null){
        //     this.lstMasterContainers.push(this.initNewContainer());
        // }
        this.shipment.csMawbcontainers = this.lstMasterContainers;
    }
    resetSumContainer(){
        this.shipment.grossWeight = 0;
        this.shipment.netWeight = 0;
        this.shipment.chargeWeight = 0;
        this.shipment.cbm = 0;
        this.shipment.packageContainer = '';
        this.shipment.commodity = '';
        this.shipment.desOfGoods = '';
        if(this.shipment.csMawbcontainers == null) return;
        for (var i = 0; i < this.shipment.csMawbcontainers.length; i++) {
            this.shipment.grossWeight = this.shipment.grossWeight + this.shipment.csMawbcontainers[i].gw;
            this.shipment.netWeight = this.shipment.netWeight + this.shipment.csMawbcontainers[i].nw;
            this.shipment.chargeWeight = this.shipment.chargeWeight + this.shipment.csMawbcontainers[i].chargeAbleWeight;
            this.shipment.cbm = this.shipment.cbm + this.shipment.csMawbcontainers[i].cbm;
            // this.shipment.packageContainer = this.shipment.packageContainer + ((this.shipment.csMawbcontainers[i].quantity == null && this.shipment.csMawbcontainers[i].containerTypeName == "")?"": (this.shipment.csMawbcontainers[i].quantity + "x" + this.shipment.csMawbcontainers[i].containerTypeName + ", "));
            // this.shipment.commodity = this.shipment.commodity + ((this.shipment.csMawbcontainers[i].commodityName== "" || this.shipment.csMawbcontainers[i].commodityName== null)?"": (this.shipment.csMawbcontainers[i].commodityName + ", "));
            // this.shipment.desOfGoods = this.shipment.desOfGoods + (this.shipment.csMawbcontainers[i].description== null?"": (this.shipment.csMawbcontainers[i].description + ", "));
            if((this.shipment.csMawbcontainers[i].quantity != null && this.shipment.csMawbcontainers[i].containerTypeName!=null)){
                let temp = this.shipment.csMawbcontainers[i].containerTypeName + "x" + this.shipment.csMawbcontainers[i].quantity;
                if(this.shipment.csMawbcontainers[i].packageTypeName != null &&this.shipment.csMawbcontainers[i].packageQuantity != null){
                    temp = temp + ": " + this.shipment.csMawbcontainers[i].packageTypeName + "x" + this.shipment.csMawbcontainers[i].packageQuantity;
                }
                temp = temp + ", ";
                if(!this.shipment.packageContainer.includes(temp)){
                    this.shipment.packageContainer = this.shipment.packageContainer + temp;
                }
            }
            if(this.shipment.csMawbcontainers[i].commodityName != null){
                if(!this.shipment.commodity.includes(this.shipment.csMawbcontainers[i].commodityName) && this.shipment.csMawbcontainers[i].commodityName!= "" ){
                    this.shipment.commodity = this.shipment.commodity + this.shipment.csMawbcontainers[i].commodityName + ", ";
                }
            }
            if(this.shipment.csMawbcontainers[i].description!= null){
                if(!this.shipment.desOfGoods.includes(this.shipment.csMawbcontainers[i].description)){
                    this.shipment.desOfGoods = this.shipment.desOfGoods + this.shipment.csMawbcontainers[i].description + "\n";
                }
            }
        }
        this.removeEndComma();
    }
    shipmentTodelete: any = null;
    async confirmDeleteShipment(item){
        this.shipmentTodelete = item;
        let respone = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransaction.checkAllowDelete + item.id, false, true);
        if(respone == true){
            $('#confirm-delete-job-modal').modal('show');
        }
        else{
            $('#confirm-can-not-delete-job-modal').modal('show');
        }
    }
    async deleteJob(){
        let respone = await this.baseServices.deleteAsync(this.api_menu.Documentation.CsTransaction.delete + this.shipmentTodelete.id, false, true);
        this.router.navigate(["/home/documentation/sea-fcl-export"]);
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


    public houseBillList: Array<object> = [];
    public houseBillCatcher(e:any) {
      this.houseBillList = e ;
      console.log({"AAAA":this.houseBillList});
    }
     /**
   * ng2-select
   */
  
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
  }



    /**
     * PREPARE DATA
     */

    getListPartner(search_key: string = null) {
        var key = "";
        if (search_key !== null && search_key.length < 3) {
            return;
        } else {
            key = search_key;
        }
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.paging + "?page=" + 1 + "&size=" + 20, { partnerGroup: PartnerGroupEnum.ALL, inactive: false, all: key }).subscribe(res => {
            var data = res['data'];
            this._firstLoadData.lstPartner = data;
            console.log({"firstLoadData":this._firstLoadData})
        });
    }
    getListUnits() {
        this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, { all: "", inactive: false }).subscribe((data: any) => {
          this._firstLoadData.lstUnit = data;
        });
    }
    getListCurrency(){
        this.baseServices.get(this.api_menu.Catalogue.Currency.getAll).subscribe((res:any)=>{
            this._firstLoadData.lstCurrency = prepareNg2SelectData(res, "id", "currencyName");
        });
    }



    shipmentDetailCatcher(shipmentDetails:any){
        this.shipment = shipmentDetails;
        this.shipment.csMawbcontainers = this.lstMasterContainers;
        ExtendData.currentJobID = shipmentDetails.id;
        this.baseServices.spinnerHide();
    }

    currentHouseBill:any = null;
    currentHouseBillCatcher(currentHouseBill:any){
        this.currentHouseBill = currentHouseBill;
        console.log({"Current_HouseBill":this.currentHouseBill});
    }

    listTotalHouseBill:any[]=[];
    totalUSD : number = 0;
    totalVND : number = 0;
    async getTotalProfit(){
        this.listTotalHouseBill = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransaction.getTotalProfit+"?JobId="+this.shipment.id);
        if(this.listTotalHouseBill){
            this.listTotalHouseBill.forEach(ele => {
                this.totalUSD += ele.totalUSD ;
                this.totalVND += ele.totalVND ;
            });
        }
    }

}
