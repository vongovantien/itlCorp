import { Component, OnInit } from '@angular/core';
import * as lodash from 'lodash';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseService } from 'src/services-base/base.service';
declare var jquery: any;
declare var $: any;

@Component({
  selector: 'app-stage-management',
  templateUrl: './stage-management.component.html',
  styleUrls: ['./stage-management.component.sass']
})
export class StageManagementComponent implements OnInit {

  constructor(private baseServices:BaseService) { }
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
    this.Stages_List = await this.baseServices.getAsync('./assets/fake-data/stages-list.json', true, true);
  }

  id_stage_remove=null;
  remove_stage(index,action){
    if(action=="confirm"){
      this.id_stage_remove = index;
    }

    if(action=='yes'){
      this.Stages_List.splice(this.id_stage_remove,1);
    }
  }

  add_stage(){
    this.Stages_List.push(this.New_Stage);
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
  }

}
