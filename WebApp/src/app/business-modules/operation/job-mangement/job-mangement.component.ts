import { Component, OnInit, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import * as lodash from 'lodash';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseService } from 'src/services-base/base.service';
import { Observer, Observable } from 'rxjs';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { ToastrService } from 'ngx-toastr';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
declare var jquery: any;
declare var $: any;


@Component({
  selector: 'app-job-mangement',
  templateUrl: './job-mangement.component.html',
  styleUrls: ['./job-mangement.component.scss']
})
export class JobMangementComponent implements OnInit {

  ready = false;
  Jobs_List: any;
  Stages_List: any;
  Const_Jobs_List: any;
  container = {
    "quantity": 0, "cont_type": "", "cont_no": 0,
    "seal_no": 0, "type": "", "unit": 0,
    "descrption": "", "n_w": 0, "g_w": 0, "cbm": 0
  }
  house_bill = {
    "customer": "",
    "hb_l": "",
    "sale_man": ""
  }

  housebill_list: any = [];
  container_list: any = [Object.assign({}, this.container)];
  temp_stages: any = [];
  temp_stages_remove: any = [];
  selected_stages: any = [];
  cbm_sum = 0;
  gw_sum = 0;
  cnts_sum = 0;
  nw_sum = 0;
  fcl_sum = 0;
  lcl_sum = 0;

  pager: PagerSetting = {
    currentPage: 1,
    pageSize: 15,
    numberToShow: [3,5,10,15, 30, 50],
    totalPageBtn:7
  };


  constructor(private route: ActivatedRoute, private router: Router,
    private baseServices: BaseService, private cdRef: ChangeDetectorRef,
    private toastr: ToastrService, private spinnerService: Ng4LoadingSpinnerService) { }

  async ngOnInit() {
    this.toastr.success("mess","title", {
      timeOut:2000
    });

    this.route.params.subscribe(prams => {
      if (prams.action == "create_job") {
        $("#create-job-modal").modal('show');
        this.router.navigate(['/home/operation/job-management']);
      }
    });
   // this.getJobs();
    await this.setPage(this.pager);
    this.getStages();
    this.ready = true;
  }


  status = false;
  async getJobs(pager) {
    this.Jobs_List = await this.baseServices.getAsync('./assets/fake-data/jobs-list.json', true, true);
    this.Const_Jobs_List = this.Jobs_List.map(x => Object.assign({}, x));

    var const_data = this.Const_Jobs_List.map(x => Object.assign({}, x));
    pager.totalItems = const_data.length;
    var return_data = const_data.splice((pager.currentPage - 1) * pager.pageSize, pager.pageSize);
    return return_data;
  }

  async getStages() {
    this.Stages_List = await this.baseServices.getAsync('./assets/fake-data/stages-list.json', true, true);
  }

  save_container() {
    this.cbm_sum = lodash.sumBy(this.container_list, function (o) { return o['cbm'] });
    this.gw_sum = lodash.sumBy(this.container_list, function (o) { return o['g_w'] });
    this.cnts_sum = 0; // special formular 
    this.nw_sum = lodash.sumBy(this.container_list, function (o) { return o['n_w'] });
    var fcl_list = lodash.filter(this.container_list, function (o) { return (o.type == 'FCL') });
    var lcl_list = lodash.filter(this.container_list, function (o) { return (o.type == 'LCL') });

    console.log({ fcl_list, lcl_list });
    this.fcl_sum = lodash.sumBy(fcl_list, function (o) { return o.unit });
    this.lcl_sum = lodash.sumBy(lcl_list, function (o) { return o.unit });

  }

  add_container() {


    if (this.container_list.length == 0 || this.container_list[this.container_list.length - 1]['quantity'] != 0) {
      this.container_list.push(Object.assign({}, this.container));
    }

  }

  remove_container(i) {
    console.log(i);
    this.container_list.splice(i, 1);

    console.log("removed");
  }

  list_id_disabled: any = [];
  list_id_enable: any = [];
  select_stage(i, abbr, event) {
    var id_input = event.target.id;

    // var index = lodash.findIndex(this.Stages_List, function (o) { return o.abbreviation == abbr });

    if (event.target.checked == true) {
      this.list_id_disabled.push(id_input);
      var selected_stage = Object.assign({}, this.Stages_List[i]);
      this.temp_stages.push(selected_stage);
    } else {
      let i = lodash.findIndex(this.temp_stages, function (o) { return o['abbreviation'] == abbr });
      let k = lodash.findIndex(this.list_id_disabled, function (o) { return o == id_input });
      this.list_id_disabled.splice(k, 1);
      this.temp_stages.splice(i, 1);
    }
  }

  add_selected_stages() {

    if (this.temp_stages.length != 0) {
      // /this.selected_stages = [];

      this.selected_stages = lodash.concat(this.selected_stages, this.temp_stages);  //  this.temp_stages.map(x => Object.assign({}, x));

      for (var i = 0; i < this.list_id_disabled.length; i++) {
        var element: any = document.getElementById(this.list_id_disabled[i]);
        element.disabled = true;
        if (i == this.list_id_disabled.length - 1) {
          this.temp_stages = [];
        }
      }
    }

    console.log(this.list_id_disabled);

  }

  select_to_remove_stage(i, abbr, event) {
    var id_input = event.target.id;
    if (event.target.checked == true) {
      this.list_id_enable.push(abbr);
    } else {
      let i = lodash.findIndex(this.list_id_enable, function (o) { return o == abbr });
      this.list_id_enable.splice(i, 1);
    }


  }

  remove_selected_stages() {
    if (this.list_id_enable.length != 0) {
      for (var i = 0; i < this.list_id_enable.length; i++) {
        var lst1 = this.list_id_enable;
        var index = lodash.findIndex(this.selected_stages, function (o) { return o['abbreviation'] == lst1[i] });
        this.selected_stages.splice(index, 1);


        //  var lst2 = this.list_id_enable ;
        var index1 = lodash.findIndex(this.Stages_List, function (o) { return o['abbreviation'] == lst1[i] });
        var id_el = "st-" + index1;
        var element: any = document.getElementById(id_el);


        element.disabled = false;
        element.checked = false;
        var index_in_list_disabled_id = lodash.findIndex(this.list_id_disabled, function (o) { return o == id_el });
        this.list_id_disabled.splice(index_in_list_disabled_id, 1);


        if (i == this.list_id_enable.length - 1) {
          this.list_id_enable = [];

        }
      }
    }


  }

  remove_stage(abbr) {
    this.list_id_enable.push(abbr);
    this.remove_selected_stages();
  }

  abort_add_stage() {
    this.list_id_enable = [];
    this.temp_stages = [];
    $("#form_source_stage :input").prop("checked", false);
    for (var i = 0; i < this.selected_stages.length; i++) {
      this.list_id_enable.push(this.selected_stages[i].abbreviation);
      if (i == this.selected_stages.length - 1) {
        this.remove_selected_stages();
      }
    }
  }


  job_to_add = {
    job_id: "",
    ops_ic: "",
    cargo_op: "",
    assign_route: "",
    service_date: "",
    finish_date: "",
    commodity: "",
    supplier: "",
    w_house: "",
    p_o_no: "",
    note: "",
    agent: "",
    port_of_loading: "",
    inventory_no: "",
    vessel_flight: "",
    port_of_delivery: "",
    m_b_l: "",
    cs_ic: "",
    customer: "",
    stage_list: null,
    house_bill_list: null,
    container_list: null,
    service:""
  }


  start_job() {
    // var new_job = {
    //   "job_id":"",
    //   "ops_ic":"",
    //   "cs_ic":"",
    //   "customer":"",
    //   "house_bill_list":this.housebill_list.map(x=>Object.assign({},x)),
    //   "container_list":this.container_list.map(x=>Object.assign({},x)),
    //   "stage_list":this.selected_stages.map(x=>Object.assign({},x))
    // }

    this.job_to_add.container_list = this.container_list.map(x => Object.assign({}, x));
    this.job_to_add.stage_list = this.selected_stages.map(x => Object.assign({}, x));
    this.job_to_add.house_bill_list = this.housebill_list.map(x => Object.assign({}, x));

    console.log(this.job_to_add);
    this.Jobs_List.push(this.job_to_add);
    this.housebill_list = [];
    this.container_list = [Object.assign({}, this.container)];
    this.abort_add_stage();
    this.cbm_sum = 0; this.cnts_sum = 0; this.fcl_sum = 0;
    this.gw_sum = 0; this.lcl_sum = 0; this.nw_sum = 0

  }
  count_percent(job) {
    var done_list = lodash.filter(job.stage_list, function (o) { return o.status == "success" }).length;
    var total = job.stage_list.length;
    return Math.floor((done_list / total) * 100);
  }



  /**
   * ng2-select
   */
  public items: Array<string> = ['Amsterdam', 'Antwerp', 'Athens', 'Barcelona',
    'Berlin', 'Birmingham', 'Bradford', 'Bremen', 'Brussels', 'Bucharest',];

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


  /**
   * paging
   */

  receiveData(data) {
    this.Jobs_List = this.Const_Jobs_List.slice(data.startIndex, data.endIndex + 1).map(x => Object.assign({}, x));
    this.cdRef.detectChanges();
    // return data;
  }

  addNew_HB() {
    this.housebill_list.push(Object.assign({}, this.house_bill));
  }

  deleteHouseBill(i) {
    this.housebill_list.splice(i, 1);
  }

  selectHb(hb) {
    console.log(hb);
  }


  index_opening_job = null;
  openJobDetails(index) {
    this.index_opening_job = index;
    console.log(this.Jobs_List[index]);
  }

  reason_cancel_job = "";

  submit_cancel_job() {
    this.spinnerService.show();
    setTimeout(() => {
      this.spinnerService.hide();
      this.toastr.success("Request submited successful !");
    }, 3000);
  }

  save_change_job() {
    this.spinnerService.show();
    setTimeout(() => {
      this.spinnerService.hide();
      this.toastr.success("All changes have been saved !");
    }, 3000);
  }

  job_index = null;
  stage_index = null;
  edit_stage_detail(index_job, index_stage, action) {
    console.log(index_job + "  |  " + index_stage)
    if (action == "confirm") {
      this.job_index = index_job==-1?this.index_opening_job:index_job; 
      this.stage_index = index_stage;
      console.log(this.Jobs_List[this.job_index].stage_list[this.stage_index]);
    } else {
      this.spinnerService.show();
      setTimeout(() => {
        this.spinnerService.hide();
        this.toastr.success("Edit stage success !");
      }, 1500);
    }
  }

  async cancel_edit_stage() {
    await this.getJobs(this.pager);
    await this.getStages();
  }

  async setPage(pager) {
    this.Jobs_List = await this.getJobs(pager);
  }

  selectedDate(event){
    
  }



}
