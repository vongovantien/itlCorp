import * as moment from 'moment';
import { Component, OnInit, ViewChild, Output, EventEmitter, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
// import { PaginationComponent } from 'ngx-bootstrap';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import * as lodash from 'lodash';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { NgForm, FormGroup, FormBuilder, Validators, FormControl } from '@angular/forms';
import { MasterBillComponent } from './master-bill/master-bill.component';
import * as dataHelper from 'src/helper/data.helper';
import { Container } from '../../../shared/models/document/container.model';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';


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
    @ViewChild('formAddEditContainer') formAddEditContainer: NgForm;
    @ViewChild(MasterBillComponent) masterBillComponent;

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
            jobId: ['', Validators.required],
            estimatedTimeofDepature: ['', Validators.required],
            estimatedTimeofArrived: [''],
            mawb: ['', Validators.required],
            mbltype: [null, Validators.required],
            coloaderId: [''],
            bookingNo: [''],
            typeOfService: [null, Validators.required],
            flightVesselName: [''],
            agentId: [null],
            pol: [null, Validators.required],
            pod: [null, Validators.required],
            paymentTerm: [''],
            voyNo: [''],
            shipmentType: [null, Validators.required],
            pono: [''],
            personIncharge: [''],
            notes: ['']
        });
    }

    async ngOnInit() {
        this.getContainerTypes();
        this.getPackageTypes();
        this.getComodities();
        this.getWeightTypes();
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
            commodityActive: [],
            packageTypeId: null,
            packageTypeName: '',
            packageTypeActive: [],
            packageQuantity: null,
            description: '',
            gw: null,
            nw: null,
            chargeAbleWeight :null,
            cbm: null,
            allowEdit: true
        };
        if (this.lstMasterContainers.length == 0) {
            this.lstMasterContainers.push(container);
        }
        console.log(this.lstMasterContainers);
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
        this.lstMasterContainers.push(new Container());
    }
    onSubmitContainer() {
        if (this.formAddEditContainer.valid) {
            for (var i = 0; i < this.lstMasterContainers.length; i++) {
                this.lstMasterContainers[i].isSave = true;
                this.totalGrossWeight = this.totalGrossWeight + this.lstMasterContainers[i].grossWeight;
                this.totalNetWeight = this.totalNetWeight + this.lstMasterContainers[i].netWeight;
                this.totalCharWeight = this.totalCharWeight + this.lstMasterContainers[i].chargeAbleWeight;
                this.totalCBM = this.totalCBM + this.lstMasterContainers[i].cbm;
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
