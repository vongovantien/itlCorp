import * as moment from 'moment';
import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
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
import { SeaFCLExport } from '../../../shared/models/document/seafclExport.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { NgForm } from '@angular/forms';
import { MasterBillComponent } from './master-bill/master-bill.component';
import { Container } from 'src/app/shared/models/document/container.model';


@Component({
    selector: 'app-sea-fcl-export-create',
    templateUrl: './sea-fcl-export-create.component.html',
    styleUrls: ['./sea-fcl-export-create.component.scss']
})
export class SeaFclExportCreateComponent implements OnInit {
    shipment: SeaFCLExport = new SeaFCLExport();
    containerTypes: any[] = [];
    containers: any[] = [];

    @ViewChild('formAddEdit') formAddEdit: NgForm;
    @ViewChild(MasterBillComponent) masterBillComponent; 
    
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU) {}

    async ngOnInit() {
        this.getContainerTypes();
        if(this.containers.length == 0){
            this.containers.push(new Container());
        }
    }
    async getContainerTypes(){
        const responses = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: "Container", inactive: false }, false, false);
        if(responses != null){
            this.containerTypes = responses;
        }
    }
    onSubmit(){
        console.log(this.formAddEdit);
        this.shipment = this.masterBillComponent.shipment;
        console.log(this.shipment);
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
