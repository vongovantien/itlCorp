import { Component, OnInit } from '@angular/core';
import * as lodash from 'lodash';
import { ActivatedRoute } from '@angular/router';
declare var jquery: any;
declare var $: any;


@Component({
  selector: 'app-job-mangement',
  templateUrl: './job-mangement.component.html',
  styleUrls: ['./job-mangement.component.scss']
})
export class JobMangementComponent implements OnInit {

  constructor(private route:ActivatedRoute) { }

  async ngOnInit() {
    this.route.params.subscribe(prams => {
      if (prams.action == "create_job") {
        $("#create-job-modal").modal('show');
      }
    });
  }

}
