import { Component, OnInit } from '@angular/core';
import * as moment from 'moment';
import { ActivatedRoute } from '@angular/router';
import { CsTransaction } from 'src/app/shared/models/document/csTransaction';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import * as shipmentHelper from 'src/helper/shipment.helper';
import * as dataHelper from 'src/helper/data.helper';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';

@Component({
    selector: 'app-manifest',
    templateUrl: './manifest.component.html',
    styleUrls: ['./manifest.component.scss']
})
export class ManifestComponent implements OnInit {
    shipment: CsTransaction = new CsTransaction();
    manifest: any = {};
    freigtCharges: any[] = [];
    paymentTermActive: any[] = [];
    etdSelected: any;
    coloaders: any[] = [];
    portOfLadings: any[] = [];
    portOfDestinations: any[] = [];
    housebills: any[] = [];
    isLoad = false;
    checkAll = false;

    constructor(private baseServices: BaseService,
        private route: ActivatedRoute,
        private api_menu: API_MENU) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
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
    removeChecked(){
        this.checkAll = false;
        this.checkAllChange();
    }
    async ngOnInit() {
        await this.getShipmentCommonData();
        await this.getPorIndexs(null);
        await this.route.params.subscribe(async prams => {
            if(prams.id != undefined){          
                await this.getShipmentDetail(prams.id);
                this.getManifest();
                await this.getHouseBillList(prams.id);
                this.isLoad = true;
            }
        });
    }
    getManifest(){
        this.manifest["supplierName"] = this.shipment["supplierName"];
        let index = this.freigtCharges.findIndex(x => x.id == this.shipment.paymentTerm);
        if(index > -1){
            this.paymentTermActive = [this.freigtCharges[index]];
            this.manifest["freigtCharge"] = this.freigtCharges[index].id;
        } 
        this.etdSelected = { startDate: moment(this.shipment.etd), endDate: moment(this.shipment.etd) };
        index = this.portOfLadings.findIndex(x => x.id == this.shipment.pol);
        if(index > -1) this.manifest["pol"] = this.portOfLadings[index].id;
        index = this.portOfDestinations.findIndex(x => x.id == this.shipment.pod);
        if(index > -1) this.manifest["pod"] = this.portOfDestinations[index].id;
    }
    refreshManifest(){
        this.manifest["referenceNo"] = null;
        this.manifest["supplierName"] = null;
        this.manifest["attention"] = null;
        this.manifest["markOfNationality"] = null;
        this.manifest["vesselNo"] = null;
        this.manifest["consolidator"] = null;
        this.manifest["deconsolidator"] = null;
        this.manifest["weight"] = null;
        this.manifest["volume"] = null;
        this.manifest["agentAssembled"] = null;
        this.getManifest();
    }
    saveManifest(){
        console.log(this.manifest);
    }
    remove(){
        console.log('move');
        this.housebills.forEach(x => {
            if(x.isChecked){
                x.isRemove = true;
            }
        });
    }
    async getShipmentDetail(id: String) {
        this.shipment = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransaction.getById + id, false, true);
        console.log({"THIS":this.shipment});
    }
    async getShipmentCommonData() {
        const data = await shipmentHelper.getShipmentCommonData(this.baseServices, this.api_menu);
        this.freigtCharges = dataHelper.prepareNg2SelectData(data.freightTerms, 'value', 'displayName');
    }
    async getPorIndexs(searchText: any) {
        let portSearchIndex = { placeType: PlaceTypeEnum.Port, modeOfTransport: 'SEA', all: searchText };
        const portIndexs = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=1&size=20", portSearchIndex, false, false);
        if (portIndexs != null) {
            this.portOfLadings = portIndexs.data;
            this.portOfDestinations = portIndexs.data;
            console.log(this.portOfLadings);
        }
        else{
            this.portOfLadings = [];
            this.portOfDestinations = [];
        }
    }
    async getHouseBillList(jobId: String){
        var responses = await this.baseServices.getAsync(this.api_menu.Documentation.CsTransactionDetail.getByJob + "?jobId=" + jobId, false, false);
        if(responses != null){
            responses.forEach((element: { isChecked: boolean; isRemove: boolean }) => {
                element.isChecked = false;
                element.isRemove = false;
            });
            this.housebills = responses;
        }
        else{
            this.housebills = [];
        }
        console.log(this.shipment.csTransactionDetails);
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
