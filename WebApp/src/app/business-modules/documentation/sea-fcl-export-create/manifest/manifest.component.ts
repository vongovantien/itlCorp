import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import moment from 'moment/moment';
import { ActivatedRoute } from '@angular/router';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { SortService } from 'src/app/shared/services/sort.service';
import { CsManifest } from 'src/app/shared/models/document/manifest.model';
import { NgForm } from '@angular/forms';
import * as stringHelper from 'src/helper/string.helper';
declare var $: any;
import { DomSanitizer, SafeResourceUrl, SafeUrl} from '@angular/platform-browser';

@Component({
    selector: 'app-manifest',
    templateUrl: './manifest.component.html',
    styleUrls: ['./manifest.component.scss']
})
export class ManifestComponent implements OnInit {
    // @ViewChild('formReport',{static:false}) frm:ElementRef;
    shipment: CsTransaction = new CsTransaction();
    manifest: CsManifest;// = new CsManifest();
    paymentTerms: any[] = [];
    paymentTermActive: any[] = [];
    etdSelected: any;
    coloaders: any[] = [];
    portOfLadings: any[] = [];
    portOfDestinations: any[] = [];
    housebills: any[] = [];
    housebillsRemoved: any[] = [];
    isLoad = false;
    checkAll = false;
    totalGW = 0;
    totalCBM = 0;
    searchHouse = '';
    searchHouseRemoved = '';
    dataReport: any;
    previewModalId = "preview-modal";

    constructor(private baseServices: BaseService,
        private route: ActivatedRoute,
        private api_menu: API_MENU,
        private sortService: SortService,
        private sanitizer: DomSanitizer) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }
    
    async ngOnInit() {
        await this.getShipmentCommonData();
        await this.getPortOfLading(null);
        await this.getPortOfDestination(null);
        await this.route.params.subscribe(async prams => {
            if(prams.id != undefined){      
                this.shipment.id = prams.id;    
                await this.getManifest(prams.id);
                await this.getHouseBillList(prams.id);
                if(this.manifest == null){
                    await this.getShipmentDetail(prams.id);
                    this.getNewManifest();
                    this.getTotalWeight();
                }
                else{
                    let index = this.paymentTerms.findIndex(x => x.id == this.manifest.paymentTerm);
                    if(index > -1){
                        this.paymentTermActive = [this.paymentTerms[index]];
                        this.manifest.paymentTerm = this.paymentTerms[index].id;
                    } 
                    this.etdSelected = this.manifest.invoiceDate == null? null: { startDate: moment(this.manifest.invoiceDate), endDate: moment(this.manifest.invoiceDate) };
                    index = this.portOfLadings.findIndex(x => x.id == this.manifest.pol);
                    if(index > -1) {
                        this.manifest.pol = this.portOfLadings[index].id;
                        this.manifest.polName = this.portOfLadings[index].nameEn;
                    }
                    index = this.portOfDestinations.findIndex(x => x.id == this.manifest.pod);
                    if(index > -1) {
                        this.manifest.pod = this.portOfDestinations[index].id;
                        this.manifest.podName = this.portOfDestinations[index].nameEn;
                    }
                }
                await this.getContainerList(prams.id);
                this.isLoad = true;
            }
        });
    }
    async getContainerList(id: any) {
        let responses = await this.baseServices.postAsync(this.api_menu.Documentation.CsMawbcontainer.query, { mblid: id }, false, false);
        this.manifest.csMawbcontainers = responses;
    }
    async getManifest(id: any) {
        this.manifest = await this.baseServices.getAsync(this.api_menu.Documentation.CsManifest.get + "?jobId=" + id, false, true);
    }
    checkAllChange(){
        if(this.checkAll){
            this.housebills.forEach(x =>{
                x.isChecked = true;
            });
        }
        else{
            this.housebills.forEach(x =>{
                x.isChecked = false;
            });
        }
    }
    async previewReport(){
        this.dataReport = null;
        this.manifest.jobId = this.shipment.id;
        this.manifest.csTransactionDetails = this.housebills.filter(x => x.isRemoved == false);
        this.manifest.invoiceDate = dataHelper.dateTimeToUTC(this.etdSelected["startDate"]);
        
        var id = this.previewModalId;
        var _this = this;
        var response = await this.baseServices.postAsync(this.api_menu.Documentation.CsManifest.preview, this.manifest, false, true);
        console.log(response);
        this.dataReport = response;
        var checkExist = setInterval(function() {
            if ($('#frame').length) {
                console.log("Exists!");
                $('#' + id).modal('show');
                clearInterval(checkExist);
            }
         }, 100);
    }
    
    removeAllChecked(){
        this.checkAll = false;
    }
    getTotalWeight(){
        this.totalCBM = 0;
        this.totalGW = 0;
        this.housebills.forEach(x =>{
            if(x.isRemoved == false){
                this.totalGW = this.totalGW + x.gw;
                this.totalCBM = this.totalCBM + x.cbm;
            }
        });
        this.manifest.weight = this.totalGW;
        this.manifest.volume= this.totalCBM;
    }
    changePortLoading(keySearch: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getPortOfLading(keySearch);
    }
    changePortDestination(keySearch: any) {
        if (keySearch !== null && keySearch.length < 3 && keySearch.length > 0) {
            return 0;
        }
        this.getPortOfDestination(keySearch);
    }
    async getNewManifest(){
        //MSEYYMM/#####: YYYY-MM-DDTHH:mm:ss.sssZ
        this.manifest = new CsManifest();
        let date = new Date().toISOString().substr(0, 19);
        let jobNo = this.shipment.jobNo;
        this.manifest.refNo = "MSE" + date.substring(2, 4) + date.substring(5,7) + jobNo.substring(jobNo.length-5, jobNo.length);
        this.manifest.supplier = this.shipment["supplierName"];
        this.manifest.voyNo = this.shipment.flightVesselName;
        let index = this.paymentTerms.findIndex(x => x.id == this.shipment.paymentTerm);
        if(index > -1){
            this.paymentTermActive = [this.paymentTerms[index]];
            this.manifest.paymentTerm = this.paymentTerms[index].id;
        } 
        this.etdSelected = { startDate: moment(this.shipment.etd), endDate: moment(this.shipment.etd) };
        index = this.portOfLadings.findIndex(x => x.id == this.shipment.pol);
        if(index > -1){
            this.manifest.pol = this.portOfLadings[index].id;
            this.manifest.polName = this.portOfLadings[index].nameEn;
        } 
        index = this.portOfDestinations.findIndex(x => x.id == this.shipment.pod);
        if(index > -1)
        {
            this.manifest.pod = this.portOfDestinations[index].id;
            this.manifest.podName = this.portOfDestinations[index].nameEn;
        }
    }
    removeChecked(){
        this.checkAll = false;
        //this.checkAllChange();
    }
    async refreshManifest(){
        this.manifest.refNo = null;
        this.manifest.supplier = null;
        this.manifest.attention = null;
        this.manifest.masksOfRegistration = null;
        this.manifest.voyNo = null;
        this.manifest.consolidator = null;
        this.manifest.deConsolidator = null;
        this.manifest.weight = null;
        this.manifest.volume = null;
        this.manifest.manifestIssuer = null;
        await this.getShipmentDetail(this.shipment.id);
        this.getNewManifest();
        this.housebills = [];
        this.housebillsRemoved = [];
        //this.getHouseBillList(this.shipment.id);
        this.housebillsTemp.forEach(x => {
            var item = x;
            item.isRemoved = false;
            this.housebills.push(item);
        });
        console.log(this.housebillsTemp);
    }
    async saveManifest(form: NgForm){
        this.manifest.jobId = this.shipment.id;
        this.manifest.csTransactionDetails = this.housebills;
        this.manifest.invoiceDate = dataHelper.dateTimeToUTC(this.etdSelected["startDate"]);
        if(form.valid 
            && this.manifest.pod != null
            && this.manifest.paymentTerm != null){
            let response = await this.baseServices.postAsync(this.api_menu.Documentation.CsManifest.update, this.manifest, true, true);
        }
        console.log(this.manifest);
    }
    remove(){
        console.log('move');
        this.housebills.forEach(x => {
            if(x.isChecked){
                x.isRemoved = true;
                x.isChecked = false;
            }
        });
        this.housebillsRemoved = this.housebills.filter(x => x.isRemoved == true);
        this.checkAll = false;
        this.getTotalWeight();
    }
    addHouse(){
        this.housebills.forEach(x => {
            if(x.isChecked){
                x.isRemoved = false;
                x.isChecked = false;
            }
        });
        this.housebillsRemoved = this.housebills.filter(x => x.isRemoved == true);
        this.getTotalWeight();
    }
    async getShipmentDetail(id: String) {
        this.shipment = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransaction.getById + id, false, true);
        console.log({"THIS":this.shipment});
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getShipmentCommonData(this.baseServices, this.api_menu);
        this.paymentTerms = dataHelper.prepareNg2SelectData(data.freightTerms, 'value', 'displayName');
    }
    async getPortOfLading(searchText: any) {
        let portSearchIndex = { placeType: PlaceTypeEnum.Port, modeOfTransport: 'SEA', all: searchText };
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, portSearchIndex, false, false);
        if (portIndexs != null) {
            this.portOfLadings = portIndexs;
            console.log(this.portOfLadings);
        }
        else{
            this.portOfLadings = [];
        }
    }
    async getPortOfDestination(searchText: any) {
        let portSearchIndex = { placeType: PlaceTypeEnum.Port, modeOfTransport: 'SEA', all: searchText };
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, portSearchIndex, false, false);
        if (portIndexs != null) {
            this.portOfDestinations = portIndexs;
            console.log(this.portOfDestinations);
        }
        else{
            this.portOfDestinations = [];
        }
    }
    housebillsTemp: any[];
    async getHouseBillList(jobId: String){
        var responses = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + jobId, false, false);
        if(responses != null){
            responses.forEach((element: { isChecked: boolean; isRemoved: boolean }) => {
                element.isChecked = false;
                element["packageTypes"] = stringHelper.subStringComma(element["packageTypes"]);
                if(element["manifestRefNo"] == null){
                    element.isRemoved = true;
                }
                else{
                    element.isRemoved = false;
                }
            });
            this.housebills = responses;
            this.housebillsTemp = Object.assign([], this.housebills);
            this.housebillsRemoved = this.housebills.filter(x => x.isRemoved == true);
        }
        else{
            this.housebills = [];
            this.housebillsRemoved = [];
            this.housebillsTemp = [];
        }
        console.log(this.shipment.csTransactionDetails);
    }

    isDesc = false;
    sortKey: string = "jobNo";
    sort(property: string) {
      this.isDesc = !this.isDesc;
      this.sortKey = property;
      this.housebills = this.sortService.sort(this.housebills, property, this.isDesc);
    }
    searchHouseBill(keySearch: any){
        keySearch = keySearch != null ? keySearch.trim().toLowerCase() : "";
        this.housebills = Object.assign([], this.housebillsTemp).filter(
            item => ((item.hwbno.toLowerCase().includes(keySearch)
                || (item.containerNames != null ? item.containerNames.toLowerCase() : "").includes(keySearch)
                || (item.gw != null ? item.gw.toString().toLowerCase() : "").includes(keySearch)
                || (item.cbm != null ? item.cbm.toString().toLowerCase() : "").includes(keySearch)
                || (item.podName != null ? item.podName.toLowerCase() : "").includes(keySearch)
                || (item.shipperDescription != null ? item.shipperDescription.toLowerCase() : "").includes(keySearch)
                || (item.consigneeDescription != null ? item.consigneeDescription.toLowerCase() : "").includes(keySearch)
                || (item.desOfGoods != null ? item.desOfGoods.toLowerCase() : "").includes(keySearch)
                || (item.freightPayment != null ? item.freightPayment.toLowerCase() : "").includes(keySearch))
                && item.isRemoved == false
            )
        );
    }
    searchHouseBillRemoved(keySearch: any){
        keySearch = keySearch != null ? keySearch.trim().toLowerCase() : "";
        this.housebillsRemoved = Object.assign([], this.housebillsTemp).filter(
            item => ((item.hwbno.toLowerCase().includes(keySearch)
                || (item.containerNames != null ? item.containerNames.toLowerCase() : "").includes(keySearch)
                || (item.gw != null ? item.gw.toString().toLowerCase() : "").includes(keySearch)
                || (item.cbm != null ? item.cbm.toString().toLowerCase() : "").includes(keySearch)
                || (item.podName != null ? item.podName.toLowerCase() : "").includes(keySearch)
                || (item.shipperDescription != null ? item.shipperDescription.toLowerCase() : "").includes(keySearch)
                || (item.consigneeDescription != null ? item.consigneeDescription.toLowerCase() : "").includes(keySearch)
                || (item.desOfGoods != null ? item.desOfGoods.toLowerCase() : "").includes(keySearch)
                || (item.freightPayment != null ? item.freightPayment.toLowerCase() : "").includes(keySearch))
                && item.isRemoved == true
            )
        );
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
    //public items: Array<string> = ['Prepaid', 'Collect'];

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
