import { Component, OnInit, ViewChild } from '@angular/core';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from './partner-data.columns';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { AgentComponent } from './agent/agent.component';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { AllPartnerComponent } from './all/all-partner.component';
import { ConsigneeComponent } from './consignee/consignee.component';
import { CustomerComponent } from './customer/customer.component';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { Router } from '@angular/router';
import { AirShipSupComponent } from './air-ship-sup/air-ship-sup.component';
import { CarrierComponent } from './carrier/carrier.component';
import { ShipperComponent } from './shipper/shipper.component';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { SystemConstants } from 'src/constants/system.const';

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
  titleConfirmDelete: string = "Do you want to delete this partner?";
  criteria: any = { partnerGroup: PartnerGroupEnum.CUSTOMER };
  partner: Partner;
  activeTab: string = 'customerTab';
  @ViewChild(PaginationComponent) child;
  //partnerType: any;

  @ViewChild(AgentComponent) agentComponent; 
  @ViewChild(AllPartnerComponent) allPartnerComponent; 
  @ViewChild(ConsigneeComponent) consigneeComponent; 
  @ViewChild(CustomerComponent) customerComponent; 
  @ViewChild(AirShipSupComponent) airShipSupComponent; 
  @ViewChild(CarrierComponent) carrierComponent; 
  @ViewChild(ShipperComponent) shipperComponent; 

  constructor(private baseService: BaseService,
    private toastr: ToastrService, 
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private router:Router) { }

  ngOnInit() {
    this.tabSelect(this.activeTab);
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
      if(event.field == "userCreatedName"){
        this.criteria.userCreated = event.searchString;
      }
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.CUSTOMER){
      this.customerComponent.getPartnerData(this.pager, this.criteria);
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.AGENT){
      this.agentComponent.getPartnerData(this.pager, this.criteria);
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.CARRIER){
      this.carrierComponent.getPartnerData(this.pager, this.criteria);
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.CONSIGNEE){
      this.consigneeComponent.getPartnerData(this.pager, this.criteria);
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.AIRSHIPSUP){
      this.airShipSupComponent.getPartnerData(this.pager, this.criteria);
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.SHIPPER){
      this.shipperComponent.getPartnerData(this.pager, this.criteria);
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.ALL){
      this.allPartnerComponent.getPartnerData(this.pager, this.criteria);
    }
  }
  tabSelect(tabName){
    this.pager.currentPage = 1;
    this.pager.pageSize = SystemConstants.OPTIONS_PAGE_SIZE;
    this.activeTab = tabName;
    
    if(tabName == "customerTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.CUSTOMER;
      this.pager.totalItems = this.customerComponent.getPartnerData(this.pager, this.criteria);
    }
    if(tabName == "agentTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.AGENT;
      this.pager.totalItems = this.agentComponent.getPartnerData(this.pager, this.criteria);
    }
    if(tabName == "carrierTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.CARRIER;
      this.pager.totalItems = this.carrierComponent.getPartnerData(this.pager, this.criteria);
    }
    if(tabName == "consigneeTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.CONSIGNEE;
      this.pager.totalItems = this.consigneeComponent.getPartnerData(this.pager, this.criteria);
    }
    if(tabName == "airshipsupTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.AIRSHIPSUP;
      this.pager.totalItems = this.airShipSupComponent.getPartnerData(this.pager, this.criteria);
    }
    if(tabName == "shipperTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.SHIPPER;
      this.pager.totalItems = this.shipperComponent.getPartnerData(this.pager, this.criteria);
    }
    if(tabName == "allTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.ALL;
      this.pager.totalItems = this.allPartnerComponent.getPartnerData(this.pager, this.criteria);
    }
  }

  showConfirmDelete(event){
    this.partner = event;
  }
  showDetail(event){
    this.partner = event;
    this.router.navigate(["/home/catalogue/partner-data-detail/",{ id: this.partner.id }]);
  }
  async onDelete(event) {
    if (event) {
      this.baseService.delete(this.api_menu.Catalogue.PartnerData.delete + this.partner.id).subscribe((response: any) => {
        if (response.status == true) {
          this.toastr.success(response.message);
          this.RefreshData();
        }
        if (response.status == false) {
          this.toastr.error(response.message);
        }
      }, error => this.baseService.handleError(error));
    }
  }
  setPageAfterDelete() {
    this.child.setPage(this.pager.currentPage);
    if (this.pager.currentPage > this.pager.totalPages) {
      this.pager.currentPage = this.pager.totalPages;
      this.child.setPage(this.pager.currentPage);
    }
  }
  
  RefreshData(): any {
    if(this.criteria.partnerGroup == PartnerGroupEnum.CUSTOMER){
      this.pager.totalItems = this.customerComponent.getPartnerData(this.pager, this.criteria);
      this.setPageAfterDelete();
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.AGENT){
      this.pager.totalItems = this.agentComponent.getPartnerData(this.pager, this.criteria);
      this.setPageAfterDelete();
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.CARRIER){
      this.pager.totalItems = this.carrierComponent.getPartnerData(this.pager, this.criteria);
      this.setPageAfterDelete();
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.CONSIGNEE){
      this.pager.totalItems = this.consigneeComponent.getPartnerData(this.pager, this.criteria);
      this.setPageAfterDelete();
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.AIRSHIPSUP){
      this.pager.totalItems = this.airShipSupComponent.getPartnerData(this.pager, this.criteria);
      this.setPageAfterDelete();
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.SHIPPER){
      this.pager.totalItems = this.shipperComponent.getPartnerData(this.pager, this.criteria);
      this.setPageAfterDelete();
    }
    if(this.criteria.partnerGroup == PartnerGroupEnum.ALL){
      this.pager.totalItems = this.allPartnerComponent.getPartnerData(this.pager, this.criteria);
      this.setPageAfterDelete();
    }
  }

  addPartner(){
   this.router.navigate(["/home/catalogue/partner-data-addnew",{ partnerType: this.criteria.partnerGroup }]);
  }
  setPage(pager:PagerSetting){
    if(this.activeTab == "customerTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.CUSTOMER;
      this.customerComponent.getPartnerData(pager, this.criteria);
    }
    if(this.activeTab == "agentTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.AGENT;
      this.agentComponent.getPartnerData(pager, this.criteria);
    }
    if(this.activeTab == "carrierTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.CARRIER;
      this.carrierComponent.getPartnerData(pager, this.criteria);
    }
    if(this.activeTab == "consigneeTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.CONSIGNEE;
      this.consigneeComponent.getPartnerData(pager, this.criteria);
    }
    if(this.activeTab == "airshipsupTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.AIRSHIPSUP;
      this.airShipSupComponent.getPartnerData(pager, this.criteria);
    }
    if(this.activeTab == "shipperTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.SHIPPER;
      this.shipperComponent.getPartnerData(pager, this.criteria);
    }
    if(this.activeTab == "allTab"){
      this.criteria.partnerGroup = PartnerGroupEnum.ALL;
      this.allPartnerComponent.getPartnerData(pager, this.criteria);
    }
    this.pager.currentPage = pager.currentPage;
  }
}
