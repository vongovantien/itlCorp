import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { Router } from '@angular/router';

@Component({
    selector: 'app-custom-clearance',
    templateUrl: './custom-clearance.component.html',
    styleUrls: ['./custom-clearance.component.scss']
})
export class CustomClearanceComponent implements OnInit {
    listCustomDeclaration: any = [];
    pager: PagerSetting = PAGINGSETTING;
    searchObject: any = {};
    listUser: Array<string> = [];
    clearanceNo: string = '';

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private router: Router) {
        this.keepCalendarOpeningWithRange = true;
        this.selectedDate = Date.now();
        //this.selectedRange = { startDate: moment().startOf('month'), endDate: moment().endOf('month') };
        this.selectedRange = { startDate: moment().subtract(30, 'days'), endDate: moment() };
    }

    async ngOnInit() {
        this.initPager();
        this.getListUser();
        this.currentUser = [localStorage.getItem('currently_userName')];
        this.getListCustomsDeclaration();
    }

    initPager(): any {
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
    }

    async getListCustomsDeclaration() {
        const res = await this.baseServices.postAsync(this.api_menu.ToolSetting.CustomClearance.paging + "?pageNumber=" + this.pager.currentPage + "&pageSize=" + this.pager.pageSize, this.searchObject, true, true);
        console.log(res);
        this.listCustomDeclaration = res.data;
        this.pager.totalItems = res.totalItems;
    }

    getListUser() {
        this.baseServices.get(this.api_menu.System.User_Management.getAll).subscribe((res: any) => {
            console.log(res);
            this.listUser = res.map(x => ({ "text": x.username, "id": x.id }));
        }, err => {
            this.listUser = [];
            this.baseServices.handleError(err);
        });
    }


    setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.pageSize = pager.pageSize;
        this.pager.totalPages = pager.totalPages;
        this.getListCustomsDeclaration();
    }

    async searchUnit() {
        this.initPager();
        this.searchObject = {};

        this.searchObject.ClearanceNo = this.clearanceNo;
        this.searchObject.FromClearanceDate = this.selectedRange.startDate._d;
        this.searchObject.ToClearanceDate = this.selectedRange.endDate._d;
        if(this.defaultImportStatus[0] === 'All'){
            this.searchObject.ImPorted = null;
        } else {
            this.searchObject.ImPorted = this.defaultImportStatus[0] === 'Imported' ? true : false;
        }
        this.searchObject.FromImportDate = this.selectedRangeImportDate.startDate ? this.selectedRangeImportDate.startDate._d : null;
        this.searchObject.ToImportDate = this.selectedRangeImportDate.endDate ? this.selectedRangeImportDate.endDate._d : null;
        if (this.defaultTypeClearance[0] !== 'All') {
            this.searchObject.Type = this.defaultTypeClearance[0];
        } else {
            this.searchObject.Type = null;
        }

        this.searchObject.PersonHandle = this.currentUser[0].toString();
        console.log(this.searchObject);
        this.getListCustomsDeclaration();
    }

    async resetSearch() {
        this.clearanceNo = '';
        this.selectedRange = { startDate: moment().subtract(30, 'days'), endDate: moment() };
        this.defaultImportStatus = ['Not imported'];
        this.defaultTypeClearance = ['All'];
        this.searchObject = {};
        this.initPager();
        this.getListCustomsDeclaration();
    }


    isDesc = true;
    sortKey: string = "";
    sort(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.listCustomDeclaration = this.sortService.sort(this.listCustomDeclaration, property, this.isDesc);
    }
    
    gotoEditPage(id){
        this.router.navigate(["/home/operation/custom-clearance-edit", { id: id }]);
    }
    async getDataFromEcus(){
        await this.baseServices.postAsync(this.api_menu.ToolSetting.CustomClearance.importClearancesFromEcus, null, true, true);
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
        this.getListCustomsDeclaration();
    }

    /**
     * Daterange picker
     */
    selectedRange: any;
    selectedRangeImportDate: any;
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
    typeClearance: Array<string> = ['All', 'Export', 'Import'];
    userList: Array<string> = [];
    currentUser = ['Thor'];
    defaultImportStatus = ['Not imported'];
    defaultTypeClearance = ['All'];

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

    public selectedImportStatus(value: any): void {
        console.log('selected Import Status value is: ', value);
        this.defaultImportStatus = [value.id];
    }

    public selectedTypeClearance(value: any): void {
        console.log('selected Type Clearance value is: ', value);
        this.defaultTypeClearance = [value.id];
    }

}
