import { Component, OnInit, ViewChild } from '@angular/core';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from './partner-data.columns';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { PartenrGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { AgentComponent } from './agent/agent.component';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';

@Component({
  selector: 'app-partner',
  templateUrl: './partner.component.html',
  styleUrls: ['./partner.component.sass']
})
export class PartnerComponent implements OnInit {
  selectedFilter = "All";
  pager: PagerSetting = PAGINGSETTING;
  partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
  configSearch: any = {
    selectedFilter: this.selectedFilter,
    settingFields: this.partnerDataSettings,
    typeSearch: TypeSearch.intab
  };
  titleConfirmDelete: string = "Do you want to delete this Agent?";
  criteria: any = { partnerGroup: PartenrGroupEnum.CUSTOMER };

  @ViewChild(AgentComponent) agentComponent; 

  constructor() { }

  ngOnInit() {
  }
  onSearch(event){
    if(event.field == "All"){
      this.criteria.all = event.searchString;
    }
    else{
      this.criteria.all = null;
      if(event.field == "id"){
        this.criteria.id = event.searchString;
      }
      if(event.field == "shortName"){
        this.criteria.shortName = event.searchString;
      }
      if(event.field == "addressVn"){
        this.criteria.addressVn = event.searchString;
      }
      if(event.field == "taxCode"){
        this.criteria.taxCode = event.searchString;
      }
      if(event.field == "tel"){
        this.criteria.tel = event.searchString;
      }
      if(event.field == "fax"){
        this.criteria.fax = event.searchString;
      }
      if(event.field == "userCreated"){
        this.criteria.userCreated = event.searchString;
      }
    }
    if(this.criteria.partnerGroup == PartenrGroupEnum.AGENT){
      this.agentComponent.getPartnerData(this.pager, this.criteria);
    }
  }
  tabSelect(tabName){
    if(tabName == "customerTab"){
      this.criteria.partnerGroup = PartenrGroupEnum.CUSTOMER;
    }
    if(tabName == "agentTab"){
      this.criteria.partnerGroup = PartenrGroupEnum.AGENT;
      this.agentComponent.getPartnerData(this.pager, this.criteria);
    }
    if(tabName == "carrierTab"){}
    if(tabName == "consigneeTab"){
      this.criteria.partnerGroup = PartenrGroupEnum.CONSIGNEE;
    }
    if(tabName == "airshipsupTab"){}
    if(tabName == "shipperTab"){
      this.criteria.partnerGroup = PartenrGroupEnum.SHIPPER;
    }
    if(tabName == "allTab"){}
  }
  resetSearch(event){
  }
}
