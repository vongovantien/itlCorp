import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { async } from '@angular/core/testing';

@Component({
    selector: 'app-custom-clearance',
    templateUrl: './custom-clearance.component.html',
    styleUrls: ['./custom-clearance.component.scss']
})
export class CustomClearanceComponent implements OnInit {
    ListCustomDeclaration: any = [];
    pager: PagerSetting = PAGINGSETTING;
    searchObject: any = {};

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
    }

    ngOnInit() {
        this.initPager();
        this.getCustomsDeclaration();
        console.log('ABC');
    }
    initPager(): any {
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
    }

    getCustomsDeclaration() {
        // this.baseServices.get(this.api_menu.ToolSetting.CustomClearance.getAll).subscribe(res => {
        //     console.log(res);
        //     this.ListCustomDeclaration = res;
        // }, err => {
        //     console.log(err);
        //     this.ListCustomDeclaration = [];
        //     this.baseServices.handleError(err);
        // });
        console.log(this.pager.currentPage);
        console.log(this.pager.pageSize);
        
        // const res = await this.baseServices.postAsync(this.api_menu.ToolSetting.CustomClearance.paging + "?pageNumber=" + this.pager.currentPage + "&pageSize=" + this.pager.pageSize, {}, true, true);
        // console.log(res);
        // this.ListCustomDeclaration = res.data;
        // this.pager.totalItems = res.totalItems;
        this.baseServices.post(this.api_menu.ToolSetting.CustomClearance.paging + "?pageNumber=" + this.pager.currentPage + "&pageSize=" + this.pager.pageSize, {  }).subscribe((res:any) => {
            this.ListCustomDeclaration = res;
            console.log(this.ListCustomDeclaration);
        });

        // const response = await this.baseServices.getAsync(this.api_menu.ToolSetting.CustomClearance.getAll,true,true);       
        // console.log(response);

    }

    setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;
        this.getCustomsDeclaration();
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
    public items: Array<string> = ['option 1', 'option 2', 'option 3', 'option 4', 'option 5', 'option 6', 'option 7'];

    statusClearance: Array<string> = ['All', 'Imported', 'Not imported'];
    typeClearance: Array<string> = ['All', 'Export', 'Imported'];
    userList: Array<string> = [];
    currentUser = ['Thor'];

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
