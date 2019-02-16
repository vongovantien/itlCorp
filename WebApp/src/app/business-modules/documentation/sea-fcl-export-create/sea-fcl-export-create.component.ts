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
import { container } from '@angular/core/src/render3';


@Component({
    selector: 'app-sea-fcl-export-create',
    templateUrl: './sea-fcl-export-create.component.html',
    styleUrls: ['./sea-fcl-export-create.component.scss']
})
export class SeaFclExportCreateComponent implements OnInit {
    shipment: CsTransaction = new CsTransaction();
    containerTypes: any[] = [];
    lstMasterContainers: any[] = [];
    packageTypes: any[] = [];
    commodities: any[] = [];
    weightMesurements: any[] = [];
    totalGrossWeight: number = 0;
    totalNetWeight: number = 0;
    totalCharWeight: number = 0;
    totalCBM: number = 0;
    myForm: FormGroup;
    submitted = false;
    @ViewChild('containerMasterForm') containerMasterForm: NgForm;
    @ViewChild(MasterBillComponent) masterBillComponent: any;
    selectedCommodityValue: any;
    numberOfTimeSaveContainer: number = 0;
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };

     /**
        * problem: Bad performance when switch between 'Shipment Detail' tab and 'House Bill List' tab
        * this method imporove performance for web when detecting change 
        * for more informations, check this reference please 
        * https://blog.bitsrc.io/boosting-angular-app-performance-with-local-change-detection-8a6a3afa8d4d
        *
      */
    switchTab(){
        this.cdr.detach();
        setTimeout(() => {
            this.cdr.reattach();
            this.cdr.checkNoChanges();
        }, 1000);
    }

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU, private fb: FormBuilder, private cdr: ChangeDetectorRef) {
        this.myForm = this.fb.group({
            jobId: new FormControl('', Validators.required),
            estimatedTimeofDepature: new FormControl('', Validators.required),
            estimatedTimeofArrived: new FormControl(''),
            mawb: new FormControl('', Validators.required),
            mbltype: new FormControl(null, Validators.required),
            coloaderId: new FormControl(''),
            bookingNo: new FormControl(''),
            typeOfService: new FormControl(null, Validators.required),
            flightVesselName: new FormControl(''),
            agentId: new FormControl(null),
            pol: new FormControl(null, Validators.required),
            pod: new FormControl(null, Validators.required),
            paymentTerm: new FormControl(''),
            voyNo: new FormControl(''),
            shipmentType: new FormControl(null, Validators.required),
            pono: new FormControl(''),
            personIncharge: new FormControl(''),
            notes: new FormControl(''),
            commodity: new FormControl('')
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
            desOfGoods: '',
            allowEdit: true
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
    async onSubmit() {
        this.submitted = true;
        // this.shipment = this.masterBillComponent.shipment;
        // this.shipment.eta = this.shipment["etaSelected"]!= null?this.shipment["etaSelected"]["startDate"]: null;
        // this.shipment.etd = this.shipment["etdSelected"]!= null?this.shipment["etdSelected"]["startDate"]: null;
        // console.log(this.shipment.etd);
        this.shipment = this.myForm.value;
        this.shipment.etd = this.myForm.value.estimatedTimeofDepature != null ? this.myForm.value.estimatedTimeofDepature["startDate"] : null;
        this.shipment.eta = this.myForm.value.estimatedTimeofArrived != null ? this.myForm.value.estimatedTimeofArrived["startDate"] : null;
        console.log(this.shipment);

        if (this.myForm.valid) {
            console.log('abc');
            // this.shipment.csMawbcontainers = this.containers.filter(x => x.isSave == true);
            // await this.baseServices.postAsync(this.api_menu.Documentation.CsTransaction.post, this.shipment, true, false);
        }
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
            this.lstMasterContainers.push(this.initNewContainer());
        }
        else{
            this.baseServices.errorToast("Current container must be save!!!");
        }
    }
    saveNewContainer(index: any){
        if(this.containerMasterForm.invalid) return;
        this.lstMasterContainers[index].allowEdit = false;
        this.lstMasterContainers[index].containerTypeActive = this.lstMasterContainers[index].containerTypeId != null? [{ id: this.lstMasterContainers[index].containerTypeId, text: this.lstMasterContainers[index].containerTypeName }]: [];
        this.lstMasterContainers[index].packageTypeActive = this.lstMasterContainers[index].packageTypeId != null? [{ id: this.lstMasterContainers[index].packageTypeId, text: this.lstMasterContainers[index].packageTypeName }]: [];
        this.lstMasterContainers[index].unitOfMeasureActive = this.lstMasterContainers[index].unitOfMeasureId!= null? [{ id: this.lstMasterContainers[index].unitOfMeasureId, text: this.lstMasterContainers[index].unitOfMeasureName }]: [];
        this.selectedCommodityValue = this.lstMasterContainers[index].commodityName;
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
                if(this.numberOfTimeSaveContainer == 1){
                    this.shipment.commodity = this.shipment.commodity + this.lstMasterContainers[i].commodityName + "; ";
                    this.shipment.desOfGoods = this.shipment.desOfGoods + this.lstMasterContainers[i].desOfGoods + "; ";
                    this.shipment.packageContainer = this.shipment.packageContainer + this.lstMasterContainers[i].packageContainer + "; ";
                }
            }
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

}
