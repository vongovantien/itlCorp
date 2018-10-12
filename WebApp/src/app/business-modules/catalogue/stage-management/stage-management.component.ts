import { Component, OnInit } from '@angular/core';
import * as lodash from 'lodash';
import { ActivatedRoute, Router } from '@angular/router';
import * as SearchHelper from 'src/helper/SearchHelper';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
declare var jquery: any;
declare var $: any;

@Component({
  selector: 'app-stage-management',
  templateUrl: './stage-management.component.html',
  styleUrls: ['./stage-management.component.sass']
})
export class StageManagementComponent implements OnInit {

  constructor(private baseServices:BaseService,private toastr: ToastrService, private spinnerService: Ng4LoadingSpinnerService) { }
  Stages_List: any;
  Const_Stage_List
  New_Stage:any= {
    "stage_id":"",
    "abbreviation":"",
    "name":"",
    "description":"",
    "name_en":"",
    "description_en":"",
    "exp_time":"",
    "status:":"",
    "role":""
  };
  selected_filter= "All";
  ngOnInit() {
    this.getStages();
  }

  async getStages() {
    this.Stages_List = await this.baseServices.getAsync('./assets/fake-data/stages-list.json', false,false);
    this.Const_Stage_List = this.Stages_List.map(x=>Object.assign({},x));
  }

  id_stage_remove=null;
  remove_stage(index,action){
    if(action=="confirm"){
      this.id_stage_remove = index;
    }

    if(action=='yes'){
      this.spinnerService.show();
      setTimeout(() => {
        this.Stages_List.splice(this.id_stage_remove,1);
        this.spinnerService.hide();
        this.toastr.success("Delete Stage Successful !");
      }, 1500);

      
      
    }
  }

  add_stage(){   
    this.spinnerService.show();
    setTimeout(() => {     
      this.Stages_List.push(this.New_Stage);
      this.spinnerService.hide();
      this.toastr.success("Add Stage Successful !");
      this.New_Stage = {
        "stage_id":"",
        "abbreviation":"",
        "name":"",
        "description":"",
        "name_en":"",
        "description_en":"",
        "exp_time":"",
        "status:":"",
        "role":""
      }
    }, 1500);    
  }

  search_fields:any = ['stage_id','role','name','name_en','abbreviation'];
  condition = "or";
  search_key = "";
  select_filter(filter,event){
    this.search_fields = [];
    this.selected_filter = filter;
    var id_element = document.getElementById(event.target.id);
    if($(id_element).hasClass("active")==false){      
      $(id_element).siblings().removeClass('active');
      id_element.classList.add("active");
    }

    if(filter == "All"){
      this.search_fields = ['stage_id','role','name','name_en','abbreviation'];
    }
    if(filter == "Stage ID"){
      this.search_fields = ['stage_id'];
    }
    if(filter == "Role"){
      this.search_fields = ['role'];
    }
    if(filter == "Name (EN)"){
      this.search_fields = ['name_en'];
    }
    if(filter == "Name (VI)"){
      this.search_fields = ['name'];
    }
    if(filter == "Abbreviation"){
      this.search_fields = ['abbreviation'];
    }    
  }

   search_stage(){
    this.search_fields = SearchHelper.PrepareListFieldSearch(null,this.search_fields,this.search_key,this.condition);
    var source_list = this.Const_Stage_List.map(x=>Object.assign({},x));
    this.spinnerService.show();
    setTimeout(() => {
      this.Stages_List =  SearchHelper.SearchEngine(this.search_fields,source_list,this.condition);
      this.spinnerService.hide();
    }, 3000);
    
  }

  
reset_search(){
  this.search_key = "";
  this.Stages_List = this.Const_Stage_List.map(x=>Object.assign({},x));  
 // this.search_fields = ['stage_id','role','name','name_en','abbreviation'];
}


}
