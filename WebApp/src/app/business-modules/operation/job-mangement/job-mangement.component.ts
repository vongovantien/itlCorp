import { Component, OnInit } from '@angular/core';
import * as lodash from 'lodash';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseService } from 'src/services-base/base.service';
declare var jquery: any;
declare var $: any;


@Component({
  selector: 'app-job-mangement',
  templateUrl: './job-mangement.component.html',
  styleUrls: ['./job-mangement.component.scss']
})
export class JobMangementComponent implements OnInit {

  Jobs_List: any;
  Stages_List:any;
  container = {
    "quantity":"","cont_type":"","cont_no":"",
    "seal_no":"","type":"","unit":"",
    "descrption":"","n_w":"","g_w":"","cbm":""
  }
   
  container_list : any =[];
  temp_stages :any=[];
  selected_stages:any=[];
  cbm_sum= 0;
  gw_sum=0;
  cnts_sum = 0;
  nw_sum = 0;
  fcl_sum = 0;
  lcl_sum = 0;
  


  constructor(private route: ActivatedRoute, private router: Router, private baseServices: BaseService) { }

  async ngOnInit() {
    this.route.params.subscribe(prams => {
      if (prams.action == "create_job") {
        $("#create-job-modal").modal('show');
        this.router.navigate(['/home/operation/job-management']);
      }
    });

    this.getJobs();
    this.getStages();
  }


  async getJobs() {
    this.Jobs_List = await this.baseServices.getAsync('./assets/fake-data/jobs-list.json', true, true);
  }

  async getStages(){
    this.Stages_List = await this.baseServices.getAsync('./assets/fake-data/stages-list.json', true, true);
  }

  save_container(){
    this.cbm_sum = lodash.sumBy(this.container_list,function(o){return o.cbm});
    this.gw_sum = lodash.sumBy(this.container_list,function(o){return o.g_w});
    this.cnts_sum = 0; // special formular 
    this.nw_sum = lodash.sumBy(this.container_list,function(o){return o.n_w});
    var fcl_list = lodash.filter(this.container_list,function(o){return (o.type=='FCL')});
    var lcl_list = lodash.filter(this.container_list,function(o){return (o.type=='LCL')});

    console.log({fcl_list,lcl_list});
    this.fcl_sum = lodash.sumBy(fcl_list,function(o){return o.unit});
    this.lcl_sum = lodash.sumBy(lcl_list,function(o){return o.unit});


  //  / this.fcl_sum = lodash.sumBy(this.container_list,function(o){return o.cbm});
   
  }

  add_container(){
    this.container_list.push(Object.assign({},this.container));
  }

  remove_container(i){
    console.log(i);
    this.container_list.splice(i,1);
    
    console.log("removed");
  }

  select_stage(i,e){
    if(e.target.checked==true){
      var selected_stage = this.Stages_List[i];
      this.temp_stages.push(selected_stage);
    }else{
      this.temp_stages.slice(i,1);
    }
  }

  add_selected_stages(){
    this.selected_stages = [];
    console.log(this.temp_stages);
    this.selected_stages = this.temp_stages.map(x=>Object.assign({},x));
  }



  /**
   * ng2-select
   */
  public items: Array<string> = ['Amsterdam', 'Antwerp', 'Athens', 'Barcelona',
    'Berlin', 'Birmingham', 'Bradford', 'Bremen', 'Brussels', 'Bucharest',];

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
