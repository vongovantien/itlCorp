import { Component, OnInit, AfterViewChecked, AfterContentInit } from '@angular/core';
import * as lodash from 'lodash';
import * as moment from 'moment';
import { ActivatedRoute, Router } from '@angular/router';
import * as SearchHelper from 'src/helper/SearchHelper';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { SystemConstants } from '../../../../constants/system.const';
import { StageModel } from 'src/app/shared/models/catalogue/stage.model';
declare var jquery: any;
declare var $: any;

@Component({
  selector: 'app-stage-management',
  templateUrl: './stage-management.component.html',
  styleUrls: ['./stage-management.component.sass']
})
export class StageManagementComponent implements OnInit {


  // Stages_List: any;
  // Const_Stage_List:any;
  // StageNew:StageModel;

  selected_filter = "All";

  ListStages: any = [];
  ConstStageList: any = [];
  StageToAdd = new StageModel();
  StageToUpdate = new StageModel();
  ListDepartment :any = [];

  constructor(private baseServices: BaseService, private toastr: ToastrService, private spinnerService: Ng4LoadingSpinnerService, private api_menu: API_MENU) {

  }




  async ngOnInit() {
    await this.getStages();
    this.getDepartments();
  }



  async getStages() {
    this.ListStages = await this.baseServices.getAsync(this.api_menu.Catalogue.Stage_Management.getAll, false, false);
    this.ConstStageList = this.ListStages.map(x => Object.assign({}, x));  
  }

  getDepartments(){
    this.baseServices.get(this.api_menu.System.Department.getAll).subscribe(data=>{
      this.ListDepartment = data;
      this.ListDepartment = this.ListDepartment.map(x=>({"text":x.Code,"id":x.id}));
    });
  }

  id_stage_remove = null;
  async remove_stage(index, action) {
    if (action == "confirm") {
      this.id_stage_remove = index;
    }

    if (action == 'yes') {
      var id_stage = this.ListStages[this.id_stage_remove].stage.id;
      await this.baseServices.deleteAsync(this.api_menu.Catalogue.Stage_Management.delete+id_stage,true,true)
      await this.getStages();
    }
  }
  id_stage_edit = null;
  async edit_stage(index, action) {
    if (action == "confirm") {
      this.id_stage_edit = index;
    } else {
      this.StageToUpdate = this.ListStages[this.id_stage_edit].stage;
      await this.baseServices.putAsync(this.api_menu.Catalogue.Stage_Management.update,this.StageToUpdate,true,true);
      await this.getStages();
      this.StageToUpdate = new StageModel();
    }
  }

  async add_stage() { 
    await this.baseServices.postAsync(this.api_menu.Catalogue.Stage_Management.addNew,this.StageToAdd,true,true);
    this.StageToAdd = new StageModel();
    await this.getStages();
  }

  search_fields: any = ['id', 'deparmentId', 'stageNameVn', 'stageNameEn', 'code'];
  condition = "or";
  search_key = "";
  
  select_filter(filter, event) {
    this.search_fields = [];
    this.selected_filter = filter;
    var id_element = document.getElementById(event.target.id);
    if ($(id_element).hasClass("active") == false) {
      $(id_element).siblings().removeClass('active');
      id_element.classList.add("active");
    }

    if (filter == "All") {
      this.search_fields = ['id', 'deparmentId', 'stageNameVn', 'stageNameEn', 'code'];
    }
    if (filter == "Stage ID") {
      this.search_fields = ['id'];
    }
    if (filter == "Role") {
      this.search_fields = ['departmentId'];
    }
    if (filter == "Name (EN)") {
      this.search_fields = ['stageNameEn'];
    }
    if (filter == "Name (VI)") {
      this.search_fields = ['stageNameVn'];
    }
    if (filter == "Abbreviation") {
      this.search_fields = ['code'];
    }
  }

  search_stage() {
    this.search_fields = SearchHelper.PrepareListFieldSearch(null, this.search_fields, this.search_key, this.condition);
    var source_list = this.ConstStageList.map(x => Object.assign({}, x));
    this.ListStages = SearchHelper.SearchEngine(this.search_fields, source_list, this.condition);
    // this.spinnerService.show();
    // setTimeout(() => {
      
    //   this.spinnerService.hide();
    // }, 3000);

  }


  reset_search() {
    this.search_key = "";
    this.ListStages = this.ConstStageList.map(x => Object.assign({}, x));
    // this.search_fields = ['stage_id','role','name','name_en','abbreviation'];
  }

  private value:any = {};
  private _disabledV:string = '0';
  private disabled:boolean = false;
 
  private get disabledV():string {
    return this._disabledV;
  }
 
  private set disabledV(value:string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }
 
  public selected(value:any):void {
    console.log('Selected value is: ', value);
  }
 
  public removed(value:any):void {
    console.log('Removed value is: ', value);
  }
 
  public typed(value:any):void {
    console.log('New search input: ', value);
  }
 
  public refreshValue(value:any):void {
    this.value = value;
  }


}
