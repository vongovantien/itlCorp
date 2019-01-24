import { Component, OnInit, ViewChild } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { StageModel } from 'src/app/shared/models/catalogue/stage.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { SortService } from 'src/app/shared/services/sort.service';
import * as lodash from 'lodash';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { SystemConstants } from 'src/constants/system.const';
import * as dataHelper from 'src/helper/data.helper';
// declare var jquery: any;
declare var $: any;

@Component({
    selector: 'app-stage-management',
    templateUrl: './stage-management.component.html',
    styleUrls: ['./stage-management.component.sass']
})
export class StageManagementComponent implements OnInit {

    selected_filter = "All";
    ListStages: any = [];
    ConstStageList: any = [];
    StageToAdd = new StageModel();
    StageToUpdate = new StageModel();
    ListDepartment: any = [];
    pager: PagerSetting = PAGINGSETTING;
    index_stage_edit = null;
    index_current_department = null;

    @ViewChild(PaginationComponent) child;

    constructor(
        private excelService: ExcelService,
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService) {

    }

    async ngOnInit() {
        this.getDepartments();
        await this.setPage(this.pager);
    }

    async setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.totalPages = pager.totalPages;
        this.ListStages = await this.getStages(pager);
    }

    async getStages(pager: PagerSetting) {
        var response = await this.baseServices.postAsync(this.api_menu.Catalogue.Stage_Management.paging + "/" + pager.currentPage + "/" + pager.pageSize, this.searchObject, false, true);
        pager.totalItems = response.totalItems;
        return response.data;
    }

    getDepartments() {
        this.baseServices.get(this.api_menu.System.Department.getAll).subscribe(data => {
            this.ListDepartment = data;
            this.ListDepartment =  dataHelper.prepareNg2SelectData(this.ListDepartment,'id','code');  //this.ListDepartment.map(x => ({ "text": x.code, "id": x.id }));
        });
    }

    index_stage_remove = null;
    async remove_stage(index:number, action:string) {
        if (action == "confirm") {
            this.index_stage_remove = index;
        }

        if (action == 'yes') {
            var id_stage = this.ListStages[this.index_stage_remove].stage.id;
            await this.baseServices.deleteAsync(this.api_menu.Catalogue.Stage_Management.delete + id_stage, true, true)
            // await this.setPage(this.pager);
            await this.getStages(this.pager);

            this.child.setPage(this.pager.currentPage);
            if (this.pager.currentPage > this.pager.totalPages) {
                this.pager.currentPage = this.pager.totalPages;
                this.child.setPage(this.pager.currentPage);
            }


        }
    }


    async edit_stage(index: number, action: string, form: NgForm) {       

        if (action == "confirm") {

            this.index_stage_edit = index;
            var currentStage = this.ListStages[this.index_stage_edit].stage;
            this.index_current_department = lodash.findIndex(this.ListDepartment,function(d){
                return d['id']==currentStage.departmentId; 
            });

        } else {
            if (form.form.status != "INVALID") {
                this.StageToUpdate = this.ListStages[this.index_stage_edit].stage;
                var res = await this.baseServices.putAsync(this.api_menu.Catalogue.Stage_Management.update, this.StageToUpdate, true, true);
                if(res){
                    this.StageToUpdate = new StageModel();
                    $('#edit-stage-management-modal').modal('hide');
                    //await this.setPage(this.pager);
                }
            }
        }
   
    }


    resetNg2Select = true;
    resetNgSelect() {
        this.resetNg2Select = false;
        setTimeout(() => {
            this.resetNg2Select = true;
        }, 200);
    }

    async add_stage(form: NgForm, action: string) {
        if (action == "yes") {
            delete this.StageToAdd.id;
            if (form.form.status != "INVALID") {
                await this.baseServices.postAsync(this.api_menu.Catalogue.Stage_Management.addNew, this.StageToAdd, true, true);
                this.StageToAdd = new StageModel();
                await this.getStages(this.pager);
                this.child.setPage(this.pager.currentPage);
                if (this.pager.currentPage < this.pager.totalPages) {
                    this.pager.currentPage = this.pager.totalPages;
                    this.child.setPage(this.pager.currentPage);
                }

                this.resetNgSelect();
                form.onReset();
                $('#add-stage-management-modal').modal('hide');
            }
        } else {
            this.resetNgSelect();
            form.onReset();
            $('#add-stage-management-modal').modal('hide');
        }

    }

    search_fields: any = ['id', 'deparmentId', 'stageNameVn', 'stageNameEn', 'code'];
    condition = "or";
    search_key = "";
    searchObject = {
        id: 0,
        code: "",
        stageNameVn: "",
        stageNameEn: "",
        condition: "OR",
        departmentName: ""
    }

    select_filter(filter, event) {
        this.searchObject = {
            id: 0,
            code: "",
            stageNameVn: "",
            stageNameEn: "",
            condition: "OR",
            departmentName: ""
        }
        this.selected_filter = filter;
        var id_element = document.getElementById(event.target.id);
        if ($(id_element).hasClass("active") == false) {
            $(id_element).siblings().removeClass('active');
            id_element.classList.add("active");
        }
    }

    async search_stage() {

        if (this.selected_filter == "All") {
            this.searchObject.code = this.search_key.trim() == "" ? "" : this.search_key.trim();
            this.searchObject.condition = "OR";
            this.searchObject.departmentName = this.search_key.trim() == "" ? "" : this.search_key.trim();
            this.searchObject.id = (this.search_key.trim() == "" || isNaN(Number(this.search_key)) ? 0 : parseInt(this.search_key));
            this.searchObject.stageNameEn = this.search_key.trim() == "" ? "" : this.search_key.trim();
            this.searchObject.stageNameVn = this.search_key.trim() == "" ? "" : this.search_key.trim();
        }
        if (this.selected_filter == "Stage ID") {
            this.searchObject.condition = "AND";
            this.searchObject.id = (this.search_key.trim() == "" || isNaN(Number(this.search_key)) ? 0 : parseInt(this.search_key));
        }
        if (this.selected_filter == "Department") {
            this.searchObject.condition = "AND";
            this.searchObject.departmentName = this.search_key.trim() == "" ? "" : this.search_key.trim();
        }
        if (this.selected_filter == "Name (EN)") {
            this.searchObject.condition = "AND";
            this.searchObject.stageNameEn = this.search_key.trim() == "" ? "" : this.search_key.trim();
        }
        if (this.selected_filter == "Name (Local)") {
            this.searchObject.condition = "AND";
            this.searchObject.stageNameVn = this.search_key.trim() == "" ? "" : this.search_key.trim();
        }
        if (this.selected_filter == "Code") {
            this.searchObject.condition = "AND";
            this.searchObject.code = this.search_key.trim() == "" ? "" : this.search_key.trim();
        }

        await this.setPage(this.pager);

    }


    async reset_search() {
        this.searchObject = {
            id: 0,
            code: "",
            stageNameVn: "",
            stageNameEn: "",
            condition: "OR",
            departmentName: ""
        }
        this.search_key = "";
        await this.setPage(this.pager);
    }

    private value: any = {};
    private _disabledV: string = '0';
    private disabled: boolean = false;

    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any, action): void {

        if (action == 'add') {
            this.StageToAdd.departmentId = value.id;
        }

        if (action == 'edit') {
            this.ListStages[this.index_stage_edit].stage.departmentId = value.id;
        }


    }

    public removed(value: any, action): void {
        if (action == "add") {
            this.StageToAdd.departmentId = null;
            console.log(this.StageToAdd.departmentId);
        }
        console.log('Removed value is: ', value);
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }

    isDesc = true;
    sortKey: string = "id";
    sort(property) {
        this.sortKey = property;
        this.isDesc = !this.isDesc;
        if (property === 'deptName') {
            this.ListStages = this.sortService.sort(this.ListStages, property, this.isDesc);
        }
        else {
            const temp = this.ListStages.map(x => Object.assign({}, x));
            this.ListStages = this.sortService.sort(this.ListStages.map(x => Object.assign({}, x.stage)), property, this.isDesc);
            var getDept = this.getDepartmentname;
            this.ListStages = this.ListStages.map(x => ({ stage: x, deptName: getDept(x.id, temp) }));
        }
    }

    getDepartmentname(stageId, ListStages: any[]) {
        var inx = lodash.findIndex(ListStages, function (o) { return o.stage.id === stageId });
        if (inx != -1) {
            return ListStages[inx].deptName;
        }
    }

    async import() {
        
    }

    async export() {
        var stages = await this.baseServices.postAsync(this.api_menu.Catalogue.Stage_Management.query, this.searchObject);
        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
            stages = lodash.map(stages, function (stg, index) {
                return [
                    index + 1,
                    stg['stage']['id'],
                    stg['deptName'],
                    stg['stage']['code'],
                    stg['stage']['stageNameVn'],
                    stg['stage']['stageNameEn'],
                    stg['stage']['descriptionVn'],
                    stg['stage']['descriptionEn'],
                    (stg['stage']['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
                ]
            });
        }

        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
            stages = lodash.map(stages, function (stg, index) {
                return [
                    index + 1,
                    stg['stage']['id'],
                    stg['deptName'],
                    stg['stage']['code'],
                    stg['stage']['stageNameVn'],
                    stg['stage']['stageNameEn'],
                    stg['stage']['descriptionVn'],
                    stg['stage']['descriptionEn'],
                    (stg['stage']['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
                ]
            });
        }

        const exportModel: ExportExcel = new ExportExcel();
        exportModel.title = "Stage List";
        const currrently_user = localStorage.getItem('currently_userName');
        exportModel.author = currrently_user;
        exportModel.header = [
            { name: "No.", width: 10 },
            { name: "Stage ID", width: 10 },
            { name: "Department Name", width: 30 },
            { name: "Code", width: 30 },
            { name: "Name VN", width: 30 },
            { name: "Name EN", width: 30 },
            { name: "Description VN", width: 30 },
            { name: "Description EN", width: 30 },
            { name: "Inactive", width: 30 }
        ]
        exportModel.data = stages;
        exportModel.fileName = "Stage - "+new Date().toISOString();

        this.excelService.generateExcel(exportModel);
    }
}
