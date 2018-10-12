import { Component, OnInit } from '@angular/core';
import * as lodash from 'lodash';
import { ActivatedRoute, Router } from '@angular/router';
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
  ngOnInit() {
    this.getStages();
  }

  async getStages() {
    this.Stages_List = await this.baseServices.getAsync('./assets/fake-data/stages-list.json', false,false);
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
    this.spinnerService.show();
    setTimeout(() => {     
      this.Stages_List.push(this.New_Stage);
      this.spinnerService.hide();
      this.toastr.success("Add Stage Successful !");
    }, 1500);
    
 
  }

}
