import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
@Component({
    selector: 'app-ops-module-billing-job-create',
    templateUrl: './ops-module-billing-job-create.component.html',
    styleUrls: ['./ops-module-billing-job-create.component.scss']
})
export class OpsModuleBillingJobCreateComponent implements OnInit {
    DataStorage: Object = null;
    productServices: any[] = [];
    serviceModes: any[] = [];
    shipmentModes: any[] = [];
    listCustomers: any[] = [];
    listSuppliers: any[] = [];
    listAgents: any[] = [];
    listPort: any[] = [];
    listBillingOps: any[] = [];
    OpsTransactionToAdd: OpsTransaction = new OpsTransaction();

    constructor(private baseServices: BaseService, private api_menu: API_MENU, private router:Router) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };

        this.baseServices.dataStorage.subscribe(data => {
            this.DataStorage = data;
        });
    }

    ngOnInit() {
        this.getShipmentCommonData();
        this.getListCustomers();
        this.getListPorts();
        this.getListSupplier();
        this.getListAgent();
        this.getListBillingOps();
    }

    async getShipmentCommonData() {
        const data = await shipmentHelper.getOPSShipmentCommonData(this.baseServices, this.api_menu);
        this.productServices = dataHelper.prepareNg2SelectData(data.productServices, 'value', 'displayName');
        this.serviceModes = dataHelper.prepareNg2SelectData(data.serviceModes, 'value', 'displayName');
        this.shipmentModes = dataHelper.prepareNg2SelectData(data.shipmentModes, 'value', 'displayName');
    }

    private getListCustomers() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER, inactive: false, all: null }).subscribe((res: any) => {
            this.listCustomers = res;
        });
    }

    private getListPorts() {
        this.baseServices.post(this.api_menu.Catalogue.CatPlace.query, { inactive: false }).subscribe((res: any) => {
            this.listPort = res;
            console.log(this.listPort)
        });
    }

    private getListSupplier(){
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CARRIER, inactive: false, all: null }).subscribe((res: any) => {
            this.listSuppliers = res;
            console.log({"Supplier":this.listSuppliers});
        });
    }
    private getListAgent(){
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.AGENT, inactive: false, all: null }).subscribe((res: any) => {
            this.listAgents = res;
            console.log({"Agents":this.listAgents});
        });
    }
    
    private getListBillingOps(){
        this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
            this.listBillingOps = res;
            console.log({"Billing Ops":this.listBillingOps});  
        });
    }

    public submitNewOps(form: NgForm) {
        console.log(this.OpsTransactionToAdd);
        setTimeout(async() => {
            if(form.submitted){
                var error = $('#add-new-ops-job-form').find('div.has-danger');
                if (error.length === 0) {
                    //this.OpsTransactionToAdd.serviceDate = this.OpsTransactionToAdd.serviceDate.startDate;
                    var res = await this.baseServices.postAsync(this.api_menu.Documentation.Operation.addNew, this.OpsTransactionToAdd);
                    if (res.status) {
                        console.log(res);
                        this.router.navigate(["/home/operation/job-edit",{id:res.data}]);
                        this.OpsTransactionToAdd = new OpsTransaction();
                        this.resetDisplay();
                        form.onReset();
                    }
                }
            }
        }, 300);
    }

    isDisplay: boolean = true;
    resetDisplay() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 300);
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
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4',
        'option 5', 'option 6', 'option 7'];

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
