import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { EcusConnection } from 'src/app/shared/models/tool-setting/ecus-connection';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { NgForm } from '@angular/forms';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { SortService } from 'src/app/shared/services/sort.service';
declare var $: any;
@Component({
    selector: 'app-ecus-connection',
    templateUrl: './ecus-connection.component.html',
})
export class EcusConnectionComponent implements OnInit {

    criteria: EcusConnection = new EcusConnection();
    pager: PagerSetting = PAGINGSETTING;
    EcusConnectionAdd: EcusConnection = new EcusConnection();
    EcusConnections: EcusConnection[] = [];
    EcusConnectionEdit: EcusConnection = new EcusConnection()
    Users: any[] = [];
    SearchFields = [
        // {fieldName:"",displayName:"All"},
        { fieldName: 'name', displayName: 'Name' },
        { fieldName: 'username', displayName: 'User Name' },
        { fieldName: 'serverName', displayName: 'Server Name' },
        { fieldName: 'dbname', displayName: 'Database Name' }
    ]
    configSearch: any = {
        settingFields: this.SearchFields,
        typeSearch: TypeSearch.outtab
    };

    constructor(private baseService: BaseService, private api_menu: API_MENU,
        private sortService: SortService) { }

    ngOnInit() {
        this.initNewPager();
        this.getListUsers();
        this.getEcusConnections(this.pager);
    }

    initNewPager() {
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
    }

    getListUsers() {
        this.baseService.get(this.api_menu.System.User_Management.getAll).subscribe((data: any) => {
            this.Users = prepareNg2SelectData(data, 'id', 'username');
        });
    }

    onSearch(event: { field: string; searchString: any; }) {
        this.criteria = new EcusConnection();
        this.criteria[event.field] = event.searchString;
        this.initNewPager();
        this.getEcusConnections(this.pager);
    }

    resetSearch(event: { field: string; searchString: any; }) {
        this.criteria = new EcusConnection();
        this.initNewPager();
        this.getEcusConnections(this.pager);
    }

    getEcusConnections(pager: PagerSetting) {

        this.baseService.spinnerShow();
        this.baseService.post(this.api_menu.Operation.EcusConnection.paging
            + '?pageNumber=' + pager.currentPage + '&pageSize=' + pager.pageSize, this.criteria).subscribe((res: any) => {
                if (res != null) {
                    this.pager.totalItems = res.totalItems;
                    this.EcusConnections = this.sortService.sort(res.data, 'name', this.isDesc);
                    console.log(this.EcusConnections);
                }
                else {
                    this.EcusConnections = [];
                    this.pager.totalItems = 0;
                }
                this.baseService.spinnerHide();
            });
        this.baseService.spinnerHide();

    }

    async getEcusConnectionDetails(id: number) {
        this.EcusConnectionEdit = await this.baseService.getAsync(this.api_menu.Operation.EcusConnection.details
            + '?id=' + id, true, false);
    }

    setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.totalPages = pager.totalPages;
        this.pager.pageSize = pager.pageSize;
        this.getEcusConnections(this.pager);
    }

    SubmitNewConnect(form: NgForm) {

        setTimeout(async () => {
            if (form.submitted) {
                const error = $('#add-connection-modal').find('div.has-danger');
                if (error.length === 0) {
                    const res = await this.baseService.postAsync(this.api_menu.Operation.EcusConnection.addNew, this.EcusConnectionAdd);
                    if (res.status) {
                        this.resetDisplay();
                        form.onReset();
                        $('#add-connection-modal').modal('hide');
                        this.initNewPager();
                        this.getEcusConnections(this.pager);
                    }
                }
            }
        }, 300);

    }
    SubmitEditConnect(form: NgForm) {

        setTimeout(async () => {
            if (form.submitted) {
                var error = $('#edit-connection-modal').find('div.has-danger');
                if (error.length === 0) {
                    var res = await this.baseService.putAsync(this.api_menu.Operation.EcusConnection.update, this.EcusConnectionEdit);
                    if (res.status) {
                        this.resetDisplay();
                        form.onReset();
                        $('#edit-connection-modal').modal('hide');
                        this.initNewPager();
                        this.getEcusConnections(this.pager);
                    }
                }
            }
        }, 300);
    }

    indexConnectToDelete: number = -1
    async deleteConfirm(confirm: boolean = false) {
        const id = this.EcusConnections[this.indexConnectToDelete].id;
        if (confirm === true) {
            await this.baseService.deleteAsync(this.api_menu.Operation.EcusConnection.delete + '?id=' + id);
            this.indexConnectToDelete = -1;
            this.initNewPager();
            this.getEcusConnections(this.pager);
            $('#confirm-delete-connection-modal').modal('hide');
        }
    }

    isDisplay: boolean = true;
    resetDisplay() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 300);
    }

    isDesc = true;
    sortKey: string = "name";
    sort(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.EcusConnections = this.sortService.sort(this.EcusConnections, property, this.isDesc);
    }


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


    public closeModal(form: NgForm) {
        form.onReset();
        this.resetDisplay();

        $('#add-connection-modal').modal('hide');
        this.EcusConnectionAdd = new EcusConnection();

        $('#edit-connection-modal').modal('hide');

    }

}
