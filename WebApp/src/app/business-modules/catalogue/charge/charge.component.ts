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
    private el:ElementRef) { }

    pager: PagerSetting = {
      currentPage: 1,
      pageSize: 30,
      numberToShow: [3, 5, 10, 15, 30, 50],
      totalPageBtn: 7
    }
    ChargeToUpdate : CatChargeToAddOrUpdate = new CatChargeToAddOrUpdate();
    ChargeToAdd : CatChargeToAddOrUpdate = new CatChargeToAddOrUpdate();
    ListCharges:any=[];
    idChargeToUpdate:any="";
    idChargeToAdd:any="";
    searchKey:string="";
    searchObject:any={};
    
  
    @ViewChild(PaginationComponent) child;

  async ngOnInit() {
    await this.getCharges();
  }

  async getCharges(){
    var response = await this.baseServices.postAsync(this.api_menu.Catalogue.Charge.paging+"?pageNumber="+this.pager.currentPage+"&pageSize="+this.pager.pageSize,this.searchObject,false,true);
    this.ListCharges = response.data;
    console.log(this.ListCharges);
    this.pager.totalItems = response.totalItems;
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
