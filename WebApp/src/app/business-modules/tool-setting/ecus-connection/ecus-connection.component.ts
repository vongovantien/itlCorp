import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { EcusConnection } from 'src/app/shared/models/tool-setting/ecus-connection';
import { prepareNg2SelectData } from 'src/helper/data.helper';
import { NgForm } from '@angular/forms';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
declare var $: any;
@Component({
    selector: 'app-ecus-connection',
    templateUrl: './ecus-connection.component.html',
    styleUrls: ['./ecus-connection.component.sass']
})
export class EcusConnectionComponent implements OnInit {

    criteria: EcusConnection = new EcusConnection();
    pager: PagerSetting = PAGINGSETTING;
    EcusConnectionAdd: EcusConnection = new EcusConnection();
    EcusConnections: EcusConnection[] = [];
    EcusConnectionEdit : EcusConnection = new EcusConnection ()
    Users: any[] = [];
    constructor(private baseService: BaseService, private api_menu: API_MENU) { }

    ngOnInit() {
        this.initNewPager();
        this.getListUsers();
        this.getEcusConnections(this.pager);
        this.test();
    }
    async test() {
        var s = await this.baseService.getAsync(this.api_menu.ToolSetting.EcusConnection.test, false, true);
        console.log(s);
    }

    initNewPager() {
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
    }

    getListUsers() {
        this.baseService.get(this.api_menu.System.User_Management.getAll).subscribe((data: any) => {
            this.Users = prepareNg2SelectData(data, "id", "username");
        });
    }

    getEcusConnections(pager: PagerSetting) {
        
        this.baseService.spinnerShow();
        this.baseService.post(this.api_menu.ToolSetting.EcusConnection.paging + "?pageNumber=" + pager.currentPage + "&pageSize=" + pager.pageSize, this.criteria).subscribe((res: any) => {
            if (res != null) {
                this.EcusConnections = res.data;
                this.pager.totalItems = res.totalItems;
                console.log(this.EcusConnections);
            }
            else {
                this.EcusConnections = [];
                this.pager.totalItems = 0;
            }
            this.baseService.spinnerHide();
        });
    }

    async getEcusConnectionDetails(id:number){
        this.EcusConnectionEdit = await this.baseService.getAsync(this.api_menu.ToolSetting.EcusConnection.details+"?id="+id,true,false);
        console.log(this.EcusConnectionEdit);
    }

    setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.totalPages = pager.totalPages;
        this.pager.pageSize = pager.pageSize
        this.getEcusConnections(this.pager);
    }

    SubmitNewConnect(form: NgForm) {

        console.log(this.EcusConnectionAdd);
        setTimeout(async () => {
            if (form.submitted) {
                var error = $('#add-connection-modal').find('div.has-danger');
                if (error.length === 0) {
                    var res = await this.baseService.postAsync(this.api_menu.ToolSetting.EcusConnection.addNew, this.EcusConnectionAdd);
                    if (res.status) {
                        this.resetDisplay();
                        $('#add-connection-modal').modal('hide');
                        this.initNewPager();
                        this.getEcusConnections(this.pager);
                    }
                }
            }
        }, 300);

    }
    SubmitEditConnect(form:NgForm){
        console.log(this.EcusConnectionEdit);
        setTimeout(async () => {
            if (form.submitted) {
                var error = $('#edit-connection-modal').find('div.has-danger');
                if (error.length === 0) {
                    var res = await this.baseService.putAsync(this.api_menu.ToolSetting.EcusConnection.update, this.EcusConnectionEdit);
                    if (res.status) {
                        this.resetDisplay();
                        $('#edit-connection-modal').modal('hide');
                        this.initNewPager();
                        this.getEcusConnections(this.pager);
                    }
                }
            }
        }, 300);
    }

    idConnectToDelete:string = null;
    async deleteConfirm(confirm:boolean=false){
        if(confirm===true){
            await this.baseService.deleteAsync(this.api_menu.ToolSetting.EcusConnection.delete+"?id="+this.idConnectToDelete);            
            this.idConnectToDelete = null;
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
