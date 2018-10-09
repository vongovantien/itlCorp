import { Component, OnInit } from '@angular/core';
import * as lodash from 'lodash';
import { ActivatedRoute, Router } from '@angular/router';
import {BaseService} from 'src/services-base/base.service';
declare var jquery: any;
declare var $: any;


@Component({
  selector: 'app-job-mangement',
  templateUrl: './job-mangement.component.html',
  styleUrls: ['./job-mangement.component.scss']
})
export class JobMangementComponent implements OnInit {

  Jobs_List:any;

  constructor(private route:ActivatedRoute,private router:Router,private baseServices:BaseService) { }

  async ngOnInit() {
    this.route.params.subscribe(prams => {
      if (prams.action == "create_job") {
        $("#create-job-modal").modal('show');
        this.router.navigate(['/home/operation/job-management']);
      }
    });

    this.getJobs();
  }


  async getJobs(){
    this.Jobs_List = await this.baseServices.getAsync('./assets/fake-data/jobs-list.json',true,true);
  }

}
