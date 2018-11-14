import { Component, OnInit, ViewChild,ElementRef } from '@angular/core';
import * as lodash from 'lodash';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';
import { CatChargeToAddOrUpdate } from 'src/app/shared/models/catalogue/catChargeToAddOrUpdate.model';
import {CatCharge} from 'src/app/shared/models/catalogue/catCharge.model';
import {CatChargeDefaultAccount} from 'src/app/shared/models/catalogue/catChargeDefaultAccount.model';
import * as dataHelper from 'src/helper/data.helper';
import { from } from 'rxjs';
import { SystemConstants } from 'src/constants/system.const';
import { CatUnitModel } from 'src/app/shared/models/catalogue/catUnit.model';
import { reserveSlots } from '@angular/core/src/render3/instructions';
import { Router } from '@angular/router';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
// import {DataHelper} from 'src/helper/data.helper';
declare var $: any;

@Component({
  selector: 'app-charge',
  templateUrl: './charge.component.html',
  styleUrls: ['./charge.component.sass']
})
export class ChargeComponent implements OnInit {

  constructor(
    private baseServices: BaseService,
    private toastr: ToastrService,
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private el:ElementRef,
    private router:Router,
    private sortService: SortService) { }

    listFilter = [
      { filter: "All", field: "all" }, { filter: "Code", field: "code" },
      { filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" }];
    selectedFilter = this.listFilter[0].filter;

    pager: PagerSetting = PAGINGSETTING;
    // ChargeToUpdate : CatChargeToAddOrUpdate ;
    // ChargeToAdd : CatChargeToAddOrUpdate ;
    ListCharges:any=[];
    idChargeToUpdate:any="";   
    idChargeToDelete:any=""
    idChargeToAdd:any="";
    searchKey:string="";
    searchObject:any={};
    
  
    @ViewChild(PaginationComponent) child;

  async ngOnInit() {
    await this.getCharges();
  }

  async searchCharge(){
    this.searchObject = {};
    if (this.selectedFilter == "All") {
      this.searchObject.All = this.searchKey;
    } else {
      this.searchObject = {};
      for (var i = 1; i < this.listFilter.length; i++) {
        if (this.selectedFilter == this.listFilter[i].filter) {
          eval("this.searchObject[this.listFilter[i].field]=this.searchKey");
        }

      }
    }
    await this.getCharges();
  }

  async resetSearch(){
    this.searchKey = "";
    this.searchObject = {};
    this.selectedFilter = this.listFilter[0].filter;
    await this.getCharges();
  }

  async setPage(pager:PagerSetting){
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    await this.getCharges();
  }

  setPageAfterDelete() {
    this.child.setPage(this.pager.currentPage);
    if (this.pager.currentPage > this.pager.totalPages) {
      this.pager.currentPage = this.pager.totalPages;
      this.child.setPage(this.pager.currentPage);
    }
  }

  async getCharges(){
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.Charge.paging+"?pageNumber="+this.pager.currentPage+"&pageSize="+this.pager.pageSize,this.searchObject,false,true);
    this.ListCharges = response.data;
    console.log(this.ListCharges);
    this.pager.totalItems = response.totalItems;
  }

  prepareDeleteCharge(id){
    this.idChargeToDelete = id;
  }

  async deleteCharge(){
    await this.baseServices.deleteAsync(this.api_menu.Catalogue.Charge.delete+this.idChargeToDelete,true,true);
    await this.getCharges();
    this.setPageAfterDelete();
  }

  gotoEditPage(id){
    this.router.navigate(["/home/catalogue/charge-edit",{id:id}]);
  }

  isDesc = true;
  sortKey: string = "code";
  sort(property){
      this.sortKey = property;
      this.isDesc = !this.isDesc;
      const temp = this.ListCharges.map(x=>Object.assign({},x));
      this.ListCharges = this.sortService.sort(this.ListCharges.map(x=>Object.assign({},x.charge)), property, this.isDesc);
      // var getDept = this.getDepartmentname;
      // this.ListCharges = this.ListCharges.map(x=>({stage:x,deptName:getDept(x.id,temp)})); 
      this.ListCharges = this.ListCharges.map(x => ({ charge: x }));     
  }
  getDepartmentname(stageId,ListCharges:any[]){
    var inx = lodash.findIndex(ListCharges,function(o){return o.charge.id===stageId});      
    if(inx!=-1){                    
        return ListCharges[inx].deptName;
    }
}
















  /**
   * ng2-select multi
   */
  public items:Array<string> = ['Amsterdam', 'Antwerp', 'Athens', 'Barcelona',
    'Berlin', 'Birmingham', 'Bradford', 'Bremen', 'Brussels'];
 
  private value:any = ['Athens'];
  private _disabledV:string = '0';
  private disabled:boolean = false;
 
  private get disabledV():string {
    return this._disabledV;
  }
 
  private set disabledV(value:string) {
    this._disabledV = value;
    this.disabled = this._disabledV === '1';
  }
 
  public selected(value:any):void {
    console.log('Selected value is: ', value);
  }
 
  public removed(value:any):void {
    console.log('Removed value is: ', value);
  }
 
  public refreshValue(value:any):void {
    this.value = value;
  }
 
  public itemsToString(value:Array<any> = []):string {
    return value
      .map((item:any) => {
        return item.text;
      }).join(',');
  }
}
