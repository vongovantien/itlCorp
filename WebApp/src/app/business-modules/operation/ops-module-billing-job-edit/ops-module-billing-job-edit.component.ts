import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import * as shipmentHelper from 'src/helper/shipment.helper';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as dataHelper from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { CsShipmentSurcharge } from 'src/app/shared/models/document/csShipmentSurcharge';
import { ActivatedRoute } from '@angular/router';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { NgForm } from '@angular/forms';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import filter from 'lodash/filter';
import cloneDeep from 'lodash/cloneDeep';
import { SurchargeTypeEnum } from 'src/app/shared/enums/csShipmentSurchargeType-enum';
import { async } from 'rxjs/internal/scheduler/async';
declare var $: any;

@Component({
    selector: 'app-ops-module-billing-job-edit',
    templateUrl: './ops-module-billing-job-edit.component.html',
    styleUrls: ['./ops-module-billing-job-edit.component.scss']
})
export class OpsModuleBillingJobEditComponent implements OnInit {
    opsTransaction: OpsTransaction = new OpsTransaction();
    productServices: any[] = [];
    serviceDate: any;
    finishDate: any;
    serviceModes: any[] = [];
    shipmentModes: any[] = [];
    customers: any[] = [];
    ports: any[] = [];
    suppliers: any[] = [];
    agents: any[] = [];
    billingOps: any[] = [];
    warehouses: any[] = [];
    salemans: any[] = [];
    productServiceActive: any[] = [];
    serviceModeActive: any[] = [];
    shipmentModeActive: any[] = [];   
    isSubmited = false;

    lstBuyingRateChargesComboBox: any[] = [];
    lstSellingRateChargesComboBox: any[] = [];
    lstOBHChargesComboBox: any[] = [];
    lstPartners: any[] = [];
    lstUnits: any[] = [];
    lstCurrencies: any[] = [];

    ListBuyingRateCharges: any[] = [];
    ConstListBuyingRateCharges: any = [];

    ListSellingRateCharges: any[] = [];
    ConstListSellingRateCharges: any[] = [];

    ListOBHCharges: any[] = [];
    ConstListOBHCharges: any[] = [];

    BuyingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
    SellingRateChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();
    OBHChargeToAdd: CsShipmentSurcharge = new CsShipmentSurcharge();

    isDisplay: boolean = true;
    BuyingRateChargeToEdit: any = null;
    SellingRateChargeToEdit: any = null
    OBHChargeToEdit: any = null;

    totalSellingUSD: number = 0;
    totalSellingLocal: number = 0;

    totalProfitUSD: number = 0;
    totalProfitLocal: number = 0;

    totalLogisticChargeUSD: number = 0;
    totalLogisticChargeLocal: number = 0;

    totalBuyingUSD: number = 0;
    totalBuyingLocal: number = 0;

    totalOBHUSD: number = 0;
    totalOBHLocal: number = 0;

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU,
        private route: ActivatedRoute) {
        this.keepCalendarOpeningWithRange = true;
        // this.selectedDate = Date.now();
        // this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }
    async ngOnInit() {
        this.getUnits();
        this.getPartners();
        this.getCurrencies();
        this.getListBuyingRateCharges();
        this.getListSellingRateCharges();
        this.getListOBHCharges();
        this.getCustomers();
        this.getPorts();
        this.getSuppliers();
        this.getAgents();
        this.getBillingOps();
        this.getWarehouses();
        await this.getShipmentCommonData();
        await this.route.params.subscribe(async prams => {
            if (prams.id != undefined) {
                await this.getShipmentDetails(prams.id);
                if(this.opsTransaction != null){
                    this.serviceDate = (this.opsTransaction.serviceDate!= null)? { startDate: moment(this.opsTransaction.serviceDate), endDate: moment(this.opsTransaction.serviceDate) }: null;
                    this.finishDate = this.opsTransaction.finishDate != null? { startDate: moment(this.opsTransaction.finishDate), endDate: moment(this.opsTransaction.finishDate) }: null;
                    let index = this.productServices.findIndex(x => x.id == this.opsTransaction.productService);
                    if (index > -1) this.productServiceActive = [this.productServices[index]];
                    index = this.serviceModes.findIndex(x => x.id == this.opsTransaction.serviceMode);
                    if (index > -1) this.serviceModeActive = [this.serviceModes[index]];
                    index = this.shipmentModes.findIndex(x => x.id == this.opsTransaction.shipmentMode);
                    if (index > -1) this.shipmentModeActive = [this.shipmentModes[index]];
                    this.getAllSurCharges();
                }
                else{
                    this.serviceDate = null;
                    this.finishDate = null;
                }
            }
        });
    }
    async saveShipment() {
        console.log(this.opsTransaction);
        this.opsTransaction.serviceDate = this.serviceDate != null?dataHelper.dateTimeToUTC(this.serviceDate.startDate): null;
        this.opsTransaction.finishDate = this.finishDate != null? dataHelper.dateTimeToUTC(this.finishDate.startDate): null;
        var error = $('#edit-ops-job-form').find('div.has-danger');
        if (error.length === 0 && this.isSubmited == true) {
            var response = await this.baseServices.putAsync(this.api_menu.Documentation.Operation.update, this.opsTransaction, true, true);
            if(response.success){
                this.isSubmited = false;
            }
        }
    }
    async getWarehouses() {
        this.baseServices.post(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Warehouse, inactive: false }).subscribe((res: any) => {
            this.warehouses = res;
        });
    }
    async getShipmentDetails(id: any) {
        this.opsTransaction = await this.baseServices.getAsync(this.api_menu.Documentation.Operation.getById + "?id=" + id, false, true);
        console.log({ SHIPMENT: this.opsTransaction });
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getOPSShipmentCommonData(this.baseServices, this.api_menu);
        this.productServices = dataHelper.prepareNg2SelectData(data.productServices, 'value', 'displayName');
        this.serviceModes = dataHelper.prepareNg2SelectData(data.serviceModes, 'value', 'displayName');
        this.shipmentModes = dataHelper.prepareNg2SelectData(data.shipmentModes, 'value', 'displayName');
    }
    private getListBillingOps() {
        this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
            this.billingOps = res;
        });
    }
    private getPorts() {
        this.baseServices.post(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Port, inactive: false }).subscribe((res: any) => {
            this.ports = res;
            console.log(this.ports)
        });
    }

    private getCustomers() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER, all: null }).subscribe((res: any) => {
            this.customers = res;
        });
    }
    private getSuppliers() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CARRIER, inactive: false, all: null }).subscribe((res: any) => {
            this.suppliers = res;
        });
    }
    private getAgents() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.AGENT, inactive: false, all: null }).subscribe((res: any) => {
            this.agents = res;
        });
    }
    private getBillingOps() {
        this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
            this.billingOps = res;
            this.salemans = res;
        });
    }

    public getListBuyingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'CREDIT', serviceTypeId: 'SEF' }).subscribe(res => {
            this.lstBuyingRateChargesComboBox = res['data'];
        });

    }

    public getListSellingRateCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=0", { inactive: false, type: 'DEBIT', serviceTypeId: 'SEF' }).subscribe(res => {
            this.lstSellingRateChargesComboBox = res['data'];
        });
    }

    public getListOBHCharges() {
        this.baseServices.post(this.api_menu.Catalogue.Charge.paging + "?pageNumber=1&pageSize=20", { inactive: false, type: 'OBH', serviceTypeId: 'SEF' }).subscribe(res => {
            this.lstOBHChargesComboBox = res['data'];
        });
    }

    public getPartners() {
        this.baseServices.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.ALL, inactive: false }).subscribe((res: any) => {
            this.lstPartners = res;
            console.log({ PARTNERS: this.lstPartners });
        });
    }

    public getUnits() {
        this.baseServices.post(this.api_menu.Catalogue.Unit.getAllByQuery, { inactive: false }).subscribe((data: any) => {
            this.lstUnits = data;
        });
    }

    public getCurrencies() {
        this.baseServices.get(this.api_menu.Catalogue.Currency.getAll).subscribe((res: any) => {
            this.lstCurrencies = prepareNg2SelectData(res, "id", "currencyName");
        });
    }

    calculateTotalEachBuying(isEdit: boolean = false) {
        if (isEdit) {
            if (this.BuyingRateChargeToEdit.vatrate >= 0) {
                this.BuyingRateChargeToEdit.total = this.BuyingRateChargeToEdit.quantity * this.BuyingRateChargeToEdit.unitPrice * (1 + (this.BuyingRateChargeToEdit.vatrate / 100));
            } else {
                this.BuyingRateChargeToEdit.total = this.BuyingRateChargeToEdit.quantity * this.BuyingRateChargeToEdit.unitPrice + Math.abs(this.BuyingRateChargeToEdit.vatrate);
            }
        }
        else {
            if (this.BuyingRateChargeToAdd.vatrate >= 0) {
                this.BuyingRateChargeToAdd.total = this.BuyingRateChargeToAdd.quantity * this.BuyingRateChargeToAdd.unitPrice * (1 + (this.BuyingRateChargeToAdd.vatrate / 100));
            } else {
                this.BuyingRateChargeToAdd.total = this.BuyingRateChargeToAdd.quantity * this.BuyingRateChargeToAdd.unitPrice + Math.abs(this.BuyingRateChargeToAdd.vatrate);
            }
        }
    }

    calculateTotalEachSelling(isEdit: boolean = false) {
        if (isEdit) {
            if (this.SellingRateChargeToEdit.vatrate >= 0) {
                this.SellingRateChargeToEdit.total = this.SellingRateChargeToEdit.quantity * this.SellingRateChargeToEdit.unitPrice * (1 + (this.SellingRateChargeToEdit.vatrate / 100));
            } else {
                this.SellingRateChargeToEdit.total = this.SellingRateChargeToEdit.quantity * this.SellingRateChargeToEdit.unitPrice + Math.abs(this.SellingRateChargeToEdit.vatrate);
            }
        } else {
            if (this.SellingRateChargeToAdd.vatrate >= 0) {
                this.SellingRateChargeToAdd.total = this.SellingRateChargeToAdd.quantity * this.SellingRateChargeToAdd.unitPrice * (1 + (this.SellingRateChargeToAdd.vatrate / 100));
            } else {
                this.SellingRateChargeToAdd.total = this.SellingRateChargeToAdd.quantity * this.SellingRateChargeToAdd.unitPrice + Math.abs(this.SellingRateChargeToAdd.vatrate);
            }
        }
    }


    calculateTotalEachOBH(isEdit: boolean = false) {
        if (isEdit) {
            if (this.OBHChargeToEdit.vatrate >= 0) {
                this.OBHChargeToEdit.total = this.OBHChargeToEdit.quantity * this.OBHChargeToEdit.unitPrice * (1 + (this.OBHChargeToEdit.vatrate / 100));
            } else {
                this.OBHChargeToEdit.total = this.OBHChargeToEdit.quantity * this.OBHChargeToEdit.unitPrice + Math.abs(this.OBHChargeToEdit.vatrate);
            }
        } else {
            if (this.OBHChargeToAdd.vatrate >= 0) {
                this.OBHChargeToAdd.total = this.OBHChargeToAdd.quantity * this.OBHChargeToAdd.unitPrice * (1 + (this.OBHChargeToAdd.vatrate / 100));
            } else {
                this.OBHChargeToAdd.total = this.OBHChargeToAdd.quantity * this.OBHChargeToAdd.unitPrice + Math.abs(this.OBHChargeToAdd.vatrate);
            }
        }
    }

    resetDisplay() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 50);
    }

    saveNewCharge(id_form: string, form: NgForm, data: CsShipmentSurcharge, isContinue: boolean) {
        setTimeout(async () => {
            var error = $('#' + id_form).find('div.has-danger');
            if (error.length == 0) {
                data.hblid = this.opsTransaction.hblid;
                var res = await this.baseServices.postAsync(this.api_menu.Documentation.CsShipmentSurcharge.addNew, data);
                if (res.status) {
                    form.onReset();
                    this.resetDisplay();
                    this.getAllSurCharges();
                    this.BuyingRateChargeToAdd = new CsShipmentSurcharge();
                    this.SellingRateChargeToAdd = new CsShipmentSurcharge();
                    this.OBHChargeToAdd = new CsShipmentSurcharge();
                    if (!isContinue)
                        $('#' + id_form).modal('hide');
                }
            }
        }, 300);
    }



    private totalProfit() {
        this.totalProfitUSD = this.totalSellingUSD - this.totalBuyingUSD - this.totalLogisticChargeUSD;
        this.totalProfitLocal = this.totalSellingLocal - this.totalBuyingLocal - this.totalLogisticChargeLocal;
    }

    /**
     * Calculate total cost for all buying charges 
     */
    private totalBuyingCharge() {
        this.totalBuyingUSD = 0;
        this.totalBuyingLocal = 0;
        if (this.ListBuyingRateCharges.length > 0) {

            this.ListBuyingRateCharges.forEach(element => {

                this.totalBuyingLocal += element.total * element.exchangeRate;
                this.totalBuyingUSD += this.totalBuyingLocal / element.exchangeRateUSDToVND;
                this.totalProfit();
            });
        }
    }

    /**
    * Calculate total cost for all selling charges 
    */
    private totalSellingCharge() {
        this.totalSellingUSD = 0;
        this.totalSellingLocal = 0;
        if (this.ListSellingRateCharges.length > 0) {

            this.ListSellingRateCharges.forEach(element => {
                this.totalSellingLocal += element.total * element.exchangeRate;
                this.totalSellingUSD += this.totalSellingLocal / element.exchangeRateUSDToVND;
                this.totalProfit();

            });

        }
    }

    /**
    * Calculate total cost for all obh charges 
    */
    private totalOBHCharge() {
        this.totalOBHUSD = 0;
        this.totalOBHLocal = 0;
        if (this.ListOBHCharges.length > 0) {

            this.ListOBHCharges.forEach(element => {

                this.totalOBHLocal += element.total * element.exchangeRate;
                this.totalOBHUSD += this.totalOBHLocal / element.exchangeRateUSDToVND;
                this.totalProfit();
            });

        }
    }

    getSurCharges(type: 'BUY' | 'SELL' | 'OBH') {
        this.baseServices.get(this.api_menu.Documentation.CsShipmentSurcharge.getByHBId + "?hbId=" + this.opsTransaction.hblid + "&type=" + type).subscribe((res: any) => {
            if (type === 'BUY') {
                this.ListBuyingRateCharges = res;
                this.ConstListBuyingRateCharges = res;
                this.totalBuyingCharge();
            }
            if (type === 'SELL') {
                this.ListSellingRateCharges = res;
                this.ConstListSellingRateCharges = res;
                this.totalSellingCharge();
            }
            if (type === 'OBH') {
                this.ListOBHCharges = res;
                this.ConstListOBHCharges = res;
                this.totalOBHCharge();
            }
        });
    }

    getAllSurCharges() {
        this.getSurCharges('BUY');
        this.getSurCharges('SELL');
        this.getSurCharges('OBH');
    }



    prepareEditCharge(type: 'BUY' | 'SELL' | 'OBH', charge: any) {
        if (type === 'BUY') {
            this.BuyingRateChargeToEdit = cloneDeep(charge);
            this.BuyingRateChargeToEdit.exchangeDate = { startDate: moment(this.BuyingRateChargeToEdit.exchangeDate), endDate: moment(this.BuyingRateChargeToEdit.exchangeDate) };
        }
        if (type === 'SELL') {
            this.SellingRateChargeToEdit = cloneDeep(charge);
            this.SellingRateChargeToEdit.exchangeDate = { startDate: moment(this.SellingRateChargeToEdit.exchangeDate), endDate: moment(this.SellingRateChargeToEdit.exchangeDate) };
        }
        if (type === 'OBH') {
            this.OBHChargeToEdit = cloneDeep(charge);
            this.OBHChargeToEdit.exchangeDate = { startDate: moment(this.OBHChargeToEdit.exchangeDate), endDate: moment(this.OBHChargeToEdit.exchangeDate) };
        }
    }

    chargeIdToDelete: string = null;
    async DeleteCharge(stt: string, chargeId: string = null) {
        if (stt == "confirm") {
            console.log(chargeId);
            this.chargeIdToDelete = chargeId;
        }
        if (stt == "ok") {
            var res = await this.baseServices.deleteAsync(this.api_menu.Documentation.CsShipmentSurcharge.delete + "?chargId=" + this.chargeIdToDelete);
            if (res.status) {
                this.getAllSurCharges();
            }

        }
    }

    openCreditDebitNote() {

    }

    
    searchCharge(key:string,type:'BUY'|'SELL'|'OBH'){
        const search_key = key.toString().trim().toLowerCase();
        var referenceData : any[] = [];
        if(type==='BUY'){
            referenceData = this.ConstListBuyingRateCharges;
        }
        if(type==='SELL'){
            referenceData = this.ConstListSellingRateCharges;
        }
        if(type==='OBH'){
            referenceData = this.ConstListOBHCharges;
        }
        var results = filter(referenceData, function (x: any) {
            return (
                ((x.partnerName == null ? "" : x.partnerName.toLowerCase().includes(search_key)) ||
                    (x.nameEn == null ? "" : x.nameEn.toLowerCase().includes(search_key)) ||
                    (x.unit == null ? "" : x.unit.toLowerCase().includes(search_key)) ||
                    (x.currency == null ? "" : x.currency.toLowerCase().includes(search_key)) ||
                    (x.notes == null ? "" : x.notes.toLowerCase().includes(search_key)) ||
                    (x.docNo == null ? "" : x.docNo.toLowerCase().includes(search_key)) ||
                    (x.quantity == null ? "" : x.quantity.toString().toLowerCase().includes(search_key)) ||
                    (x.unitPrice == null ? "" : x.unitPrice.toString().toLowerCase().includes(search_key)) ||
                    (x.vatrate == null ? "" : x.vatrate.toString().toLowerCase().includes(search_key)) ||
                    (x.total == null ? "" : x.total.toString().toLowerCase().includes(search_key)))
            )
        });

        return results;
    }

    editCharge(id_form: string, form: NgForm, data: CsShipmentSurcharge) {
        setTimeout(async () => {
            if (form.submitted) {
                var error = $('#' + id_form).find('div.has-danger');
                if (error.length == 0) {
                    var res = await this.baseServices.putAsync(this.api_menu.Documentation.CsShipmentSurcharge.update, data);
                    if (res.status) {
                        $('#' + id_form).modal('hide');
                        this.getAllSurCharges();
                    }
                }
            }
        }, 300);
    }


    closeChargeForm(formId: string, form: NgForm) {
        form.onReset();
        this.resetDisplay();
        $('#' + formId).modal("hide");

        this.BuyingRateChargeToAdd = new CsShipmentSurcharge();
        this.SellingRateChargeToAdd = new CsShipmentSurcharge();
        this.OBHChargeToAdd = new CsShipmentSurcharge();

        this.BuyingRateChargeToEdit = null;
        this.SellingRateChargeToEdit = null
        this.OBHChargeToEdit = null;

    }

    /**
       * Daterange picker
       */
    //selectedRange: any;
    //selectedDate: any;
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


    packagesUnit: Array<string> = ['PKG', 'PCS', 'BOX', 'CNTS'];
    packagesUnitActive = ['PKG'];

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
