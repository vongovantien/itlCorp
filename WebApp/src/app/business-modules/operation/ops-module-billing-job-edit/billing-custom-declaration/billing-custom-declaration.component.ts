import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { API_MENU } from 'src/constants/api-menu.const';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';

@Component({
  selector: 'app-billing-custom-declaration',
  templateUrl: './billing-custom-declaration.component.html',
  styleUrls: ['./billing-custom-declaration.component.scss']
})
export class BillingCustomDeclarationComponent implements OnInit {
  currentJob: OpsTransaction;
  pager: PagerSetting = PAGINGSETTING;
  customClearances: any[];
  notImportedCustomClearances: any[];

  constructor(private baseServices: BaseService,
    private api_menu: API_MENU) { }

  async ngOnInit() {
    await this.stateChecking();
  }

  async getCustomClearanesOfJob(id: string) {
    this.customClearances = await this.baseServices.getAsync(this.api_menu.ToolSetting.CustomClearance.getByJob + id, false, true);
  }

  stateChecking() {
    setTimeout(() => {
      this.baseServices.dataStorage.subscribe(data => {
        if(data["CurrentOpsTransaction"] != null){
          this.currentJob = data["CurrentOpsTransaction"];
          
          if(this.currentJob != null){
            this.getCustomClearanesOfJob(this.currentJob.id);
            this.getCustomClearancesNotImported();
          }
        }
      }); 
    }, 1000);
  }
  async getCustomClearancesNotImported() {
    this.notImportedCustomClearances = await this.baseServices.postAsync(this.api_menu.ToolSetting.CustomClearance.query, { "imPorted": false }, false, true);
    console.log(this.notImportedCustomClearances);
  }
}
