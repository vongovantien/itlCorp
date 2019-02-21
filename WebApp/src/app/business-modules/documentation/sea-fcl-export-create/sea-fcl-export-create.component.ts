import * as moment from 'moment';
import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
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
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
declare var $: any;


@Component({
    selector: 'app-sea-fcl-export-create',
    templateUrl: './sea-fcl-export-create.component.html',
    styleUrls: ['./sea-fcl-export-create.component.scss']
})
export class SeaFclExportCreateComponent implements OnInit {
    shipment: CsTransaction = new CsTransaction();
    containerTypes: any[] = [];
    lstMasterContainers: any[] = [];
    lstContainerTemp: any[];
    packageTypes: any[] = [];
    commodities: any[] = [];
    weightMesurements: any[] = [];
    totalGrossWeight: number = 0;
    totalNetWeight: number = 0;
    totalCharWeight: number = 0;
    totalCBM: number = 0;
    myForm: FormGroup;
    submitted = false;
    searchcontainer: string = '';
    @ViewChild('containerMasterForm') containerMasterForm: NgForm;
    @ViewChild(MasterBillComponent) masterBillComponent: any;
    selectedCommodityValue: any;
    numberOfTimeSaveContainer: number = 0;
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };
    //
    housebillTabviewHref= '#confirm-create-job-modal';
     /**
        * problem: Bad performance when switch between 'Shipment Detail' tab and 'House Bill List' tab
        * this method imporove performance for web when detecting change 
        * for more informations, check this reference please 
        * https://blog.bitsrc.io/boosting-angular-app-performance-with-local-change-detection-8a6a3afa8d4d
        *
      */
    switchTab(id: string){
        this.cdr.detach();
        setTimeout(() => {
            this.cdr.reattach();
            this.cdr.checkNoChanges();
        }, 1000);
    }

    //open tab by link
    activeTab() {
        $('#masterbill-tablink').removeClass('active');
        $('#masterbill-tabview-tab').removeClass('active show');
        $('#housebill-tablink').addClass('active');
        $('#housebill-tabview-tab').addClass('active show');
    }

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU, private fb: FormBuilder, private cdr: ChangeDetectorRef) {
        this.myForm = this.fb.group({
            jobId: new FormControl({value: '', disabled: true}),
            estimatedTimeofDepature: new FormControl('', Validators.required),
            estimatedTimeofArrived: new FormControl(''),
            mawb: new FormControl('', Validators.required),
            //mbltype: new FormControl(null, Validators.required),
            coloaderId: new FormControl(''),
            bookingNo: new FormControl(''),
            //typeOfService: new FormControl(null, Validators.required),
            flightVesselName: new FormControl(''),
            agentId: new FormControl(null),
            pol: new FormControl(null, Validators.required),
            pod: new FormControl(null),
            //paymentTerm: new FormControl(''),
            voyNo: new FormControl(''),
            //shipmentType: new FormControl(null, Validators.required),
            pono: new FormControl(''),
            personIncharge: new FormControl(''),
            notes: new FormControl(''),
            commodity: new FormControl(''),
            packageContainer: new FormControl(''),
            desOfGoods: new FormControl('')
        });
    }

    async ngOnInit() {
        this.getContainerTypes();
        this.getPackageTypes();
        this.getComodities();
        this.getWeightTypes();
        
        if (this.lstMasterContainers.length == 0) {
            this.lstMasterContainers.push(this.initNewContainer());
        }
        console.log(this.lstMasterContainers);
    }
    initNewContainer(){
        var container = {
            containerTypeId: null,
            containerTypeName: '',
            containerTypeActive: [],
            quantity: null,
            containerNo: '',
            sealNo: '',
            markNo: '',
            unitOfMeasureId: null,
            unitOfMeasureName: '',
            unitOfMeasureActive: [],
            commodityId: null,
            commodityName: '',
            packageTypeId: null,
            packageTypeName: '',
            packageTypeActive: [],
            packageQuantity: null,
            description: '',
            gw: null,
            nw: null,
            chargeAbleWeight :null,
            cbm: null,
            packageContainer: '',
            //desOfGoods: '',
            allowEdit: true,
            isNew: true,
            verifying:false
        };
        return container;
    }
    async getContainerTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Container", inactive: false }, false, false);
        if (responses != null) {
            this.containerTypes = dataHelper.prepareNg2SelectData(responses, 'id', 'unitNameEn');
        }
    }
    async getWeightTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Weight Measurement", inactive: false }, false, false);
        if (responses != null) {
            this.weightMesurements = dataHelper.prepareNg2SelectData(responses, 'id', 'unitNameEn');
            console.log(this.weightMesurements);
        }
    }
    async getPackageTypes() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Package", inactive: false }, false, false);
        if (responses != null) {
            this.packageTypes = dataHelper.prepareNg2SelectData(responses, 'id', 'unitNameEn');
            console.log(this.packageTypes);
        }
    }
    async getComodities() {
        let responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Commodity.query, { inactive: false }, false, false);
        this.commodities = responses;
        console.log(this.commodities);
    }
    confirmCreateJob(){
        $('#confirm-create-job-modal').modal('show');
    }
    async onSubmit() {
        this.submitted = true;
        this.shipment = this.myForm.value;
        this.shipment.etd = this.myForm.value.estimatedTimeofDepature != null ? this.myForm.value.estimatedTimeofDepature["startDate"] : null;
        this.shipment.eta = this.myForm.value.estimatedTimeofArrived != null ? this.myForm.value.estimatedTimeofArrived["startDate"] : null;
        console.log(this.shipment);

        if (this.myForm.valid) {
            console.log('abc');
            this.shipment.csMawbcontainers = this.lstMasterContainers.filter(x => x.isNew == false);
            await this.saveJob();
            this.activeTab();
            //$('#confirm-create-job-modal').modal('show');
        }
    }
    async saveJob(){
        var response = await this.baseServices.postAsync(this.api_menu.Documentation.CsTransaction.post, this.shipment, true, false);
        if(response != null){
            if(response.result.success){
                this.shipment = response.model;
                this.shipment.csMawbcontainers = this.lstMasterContainers;
            }
        }
        this.housebillTabviewHref = "#housebill-tabview-tab";
    }
    cancelSaveJob(){
        $('#confirm-create-job-modal').modal('hide');
    }
    addNewContainer() {
        let hasItemEdited = false;
        for(let i=0; i< this.lstMasterContainers.length; i++){
            if(this.lstMasterContainers[i].allowEdit == true){
                hasItemEdited = true;
                break;
            }
        }
        if(hasItemEdited == false){
            console.log(this.containerMasterForm);
            //this.containerMasterForm.onReset();
            this.lstMasterContainers.push(this.initNewContainer());
        }
        else{
            this.baseServices.errorToast("Current container must be save!!!");
        }
    }
    removeAContainer(index: number){
        this.lstMasterContainers.splice(index, 1);
    }
    saveNewContainer(index: any){
        this.lstMasterContainers[index].verifying = true;
        if(this.containerMasterForm.invalid) return;
        //Cont Type, Cont Q'ty, Container No, Package Type
        let existedItem = this.lstMasterContainers.filter(x => x.containerTypeId == this.lstMasterContainers[index].containerTypeId 
            && x.quantity == this.lstMasterContainers[index].quantity
            && x.containerNo == this.lstMasterContainers[index].containerNo
            && x.packageTypeId == this.lstMasterContainers[index].packageTypeId);
        if(existedItem.length > 1) { 
            this.baseServices.errorToast("This container has been existed");
            return;
        }
        else{
            if(this.lstMasterContainers[index].isNew == true) this.lstMasterContainers[index].isNew = false;
            this.lstMasterContainers[index].allowEdit = false;
            this.lstMasterContainers[index].containerTypeActive = this.lstMasterContainers[index].containerTypeId != null? [{ id: this.lstMasterContainers[index].containerTypeId, text: this.lstMasterContainers[index].containerTypeName }]: [];
            this.lstMasterContainers[index].packageTypeActive = this.lstMasterContainers[index].packageTypeId != null? [{ id: this.lstMasterContainers[index].packageTypeId, text: this.lstMasterContainers[index].packageTypeName }]: [];
            this.lstMasterContainers[index].unitOfMeasureActive = this.lstMasterContainers[index].unitOfMeasureId!= null? [{ id: this.lstMasterContainers[index].unitOfMeasureId, text: this.lstMasterContainers[index].unitOfMeasureName }]: [];
            this.selectedCommodityValue = this.lstMasterContainers[index].commodityName;
        }
        this.lstContainerTemp = Object.assign([], this.lstMasterContainers);
    }

    
    cancelNewContainer(index: number){
        if(this.lstMasterContainers[index].isNew == true){
            this.lstMasterContainers.splice(index, 1);
        }
        else{
            this.lstMasterContainers[index].allowEdit = false;
        }
    }
    changeEditStatus(index: any){
        if(this.lstMasterContainers[index].allowEdit == false){
            this.lstMasterContainers[index].allowEdit = true;
        }
        else{
            this.lstMasterContainers[index].allowEdit = false;
        }
    }
    onSubmitContainer() {
        this.numberOfTimeSaveContainer = this.numberOfTimeSaveContainer + 1;
        if (this.containerMasterForm.valid) {
            this.totalGrossWeight = 0;
            this.totalNetWeight = 0;
            this.totalCharWeight = 0;
            this.totalCBM = 0;
            this.shipment.commodity = '';
            this.shipment.desOfGoods = '';
            this.shipment.packageContainer = '';
            for (var i = 0; i < this.lstMasterContainers.length; i++) {
                this.lstMasterContainers[i].isSave = true;
                this.totalGrossWeight = this.totalGrossWeight + this.lstMasterContainers[i].gw;
                this.totalNetWeight = this.totalNetWeight + this.lstMasterContainers[i].nw;
                this.totalCharWeight = this.totalCharWeight + this.lstMasterContainers[i].chargeAbleWeight;
                this.totalCBM = this.totalCBM + this.lstMasterContainers[i].cbm;
                this.shipment.packageContainer = this.shipment.packageContainer + (this.lstMasterContainers[i].quantity == ""?"": this.lstMasterContainers[i].quantity + "x" +this.lstMasterContainers[i].containerTypeName + ", ");
                if(this.numberOfTimeSaveContainer == 1){
                    this.shipment.commodity = this.shipment.commodity + (this.lstMasterContainers[i].commodityName== ""?"": this.lstMasterContainers[i].commodityName + ", ");
                    this.shipment.desOfGoods = this.shipment.desOfGoods + (this.lstMasterContainers[i].description== ""?"": this.lstMasterContainers[i].description + ", ");
                }
            }
                 this.shipment.csMawbcontainers = this.lstMasterContainers;
        }
    }
    searchContainer(keySearch: any){
        keySearch = keySearch!=null? keySearch.trim().toLowerCase(): "";
        this.lstMasterContainers = Object.assign([], this.lstContainerTemp).filter(
            item => (item.containerTypeName.toLowerCase().includes(keySearch)
                ||  (item.quantity!= null? item.quantity.toString(): "").includes(keySearch)
                ||  (item.containerNo!= null? item.containerNo.toLowerCase(): "").includes(keySearch)
                ||  (item.sealNo!= null? item.sealNo.toLowerCase(): "").includes(keySearch)
                ||  (item.markNo != null? item.markNo.toLowerCase(): "").includes(keySearch)
                ||  (item.commodityName != null? item.commodityName.toLowerCase(): "").includes(keySearch)
                ||  (item.packageTypeName != null? item.packageTypeName.toLowerCase(): "").includes(keySearch)
                ||  (item.packageQuantity != null? item.packageQuantity.toString().toLowerCase(): "").includes(keySearch)
                ||  (item.description!= null? item.description.toLowerCase(): "").includes(keySearch)
                ||  (item.nw!= null? item.nw.toString().toLowerCase():"").includes(keySearch)
                ||  (item.chargeAbleWeight!= null? item.chargeAbleWeight.toString():"").toLowerCase().includes(keySearch)
                ||  (item.gw != null? item.gw.toString().toLowerCase(): "").includes(keySearch)
                ||  (item.unitOfMeasureName != null? item.unitOfMeasureName.toLowerCase(): "").includes(keySearch)
                ||  (item.cbm != null? item.cbm.toString().toLowerCase(): "").includes(keySearch)
                    )
         );
         console.log(this.lstMasterContainers);
    }
    closeContainerPopup(){
        let index = this.lstMasterContainers.findIndex(x => x.isNew == true);
        if(index> -1){
            this.lstMasterContainers.splice(index, 1);
        }
        if(this.numberOfTimeSaveContainer == 0){
            this.lstMasterContainers = [];
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

    private _disabledV: string = '0';
    public disabled: boolean = false;


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
    }

    public houseBillList:any= [];
    public houseBillCatcher(e:CsTransactionDetail){
        console.log(e);
        this.houseBillList.push(e);

    }

}
