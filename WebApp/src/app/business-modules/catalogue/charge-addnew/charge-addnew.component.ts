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
  selector: 'app-charge-addnew',
  templateUrl: './charge-addnew.component.html',
  styleUrls: ['./charge-addnew.component.scss']
})
export class ChargeAddnewComponent implements OnInit {

  constructor(
    private baseServices: BaseService,
    private toastr: ToastrService,
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU,
    private el:ElementRef) { }
    ChargeToAdd : CatChargeToAddOrUpdate = new CatChargeToAddOrUpdate();
    isAddNewLine:boolean = false;
    isMaximumAccountRow:boolean =false;
    isSameVoucherType:boolean = false;
    ngDataUnit:any=[];
    ngDataCurrency:any=[];
    ngDataType = [
      {id:"CREDIT",text:"CREDIT"},
      {id:"DEBIT",text:"DEBIT"},
      {id:"OBH",text:"OBH"}
    ];
    ngDataTypeChargeDefault = [
      {id:"Công Nợ",text:"Công Nợ"},
      {id:"Giải Chi",text:"Giải Chi"},
      {id:"Loại Khác",text:"Loại Khác"}
    ];

    /**
     * Need to update ngDataServices by get data from databse after implement documentation module 
     */
    ngDataService = [
      {text:"Landing Truckinng",id:"LT"},
      {text:"Air Import",id:"AI"},
      {text:"Air Export",id:"AE"},
      {text:"Sea Import",id:"SI"},
      {text:"Sea Export",id:"SE"},      
      {text:"Sea FCL Export",id:"SFCLE"},
      {text:"Sea FCL Import",id:"SFCLI"},
      {text:"Sea LCL Export",id:"SLCLE"},
      {text:"Sea LCL Import",id:"SLCLI"},
      {text:"Sea Consol Export",id:"SCE"},
      {text:"Sea Consol Import",id:"SCI"}
    ];

  async ngOnInit() {
    await this.getNeccessaryData();
   
  }

  async getNeccessaryData(){
    var units = await this.baseServices.getAsync(this.api_menu.Catalogue.Unit.getAll,false,false);
    this.ngDataUnit = units.length==0?[]: units.map(x=>({text:x.code,id:x.id}));

    var currencies = await this.baseServices.getAsync(this.api_menu.Catalogue.Currency.getAll,false,false);
    this.ngDataCurrency = currencies.length==0?[]:currencies.map(x=>({text:x.id + " - "+x.currencyName,id:x.id}));

    console.log({unit:this.ngDataUnit,currency:this.ngDataCurrency});
  }

  addNewChargeDedaultAccount(){
    var obj = new CatChargeDefaultAccount();
    // this.ChargeToAdd.listChargeDefaultAccount.push(obj);   

    if(this.ChargeToAdd.listChargeDefaultAccount.length==0){
      this.ChargeToAdd.listChargeDefaultAccount.push(obj);   
    }
    else{
      if(this.validatateDefaultAcountLine()){
        if(this.ChargeToAdd.listChargeDefaultAccount.length==3){
          this.isMaximumAccountRow = true;
        }else{
          this.ChargeToAdd.listChargeDefaultAccount.push(obj);
          this.isAddNewLine = false;
        }        
      }else{
        this.isAddNewLine = true;
      }

    }
 
    console.log(this.validateChargeDefault);
  }

  RemoveDefaultAccount(index){
    this.ChargeToAdd.listChargeDefaultAccount.splice(index,1);
    this.isMaximumAccountRow = false;
  }

  async addCharge(form:NgForm){ 
   
      if (form.form.status != "INVALID" && this.validatateDefaultAcountLine() && this.isSameVoucherType == false) {
        delete this.ChargeToAdd.charge.Id;
        await this.baseServices.postAsync(this.api_menu.Catalogue.Charge.addNew,this.ChargeToAdd,true,true);
      }  

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

  public selected(value: any,action): void {
    if(action=="unit"){
      this.ChargeToAdd.charge.unitId = value.id;
      
    }
    if(action=="currency"){
      this.ChargeToAdd.charge.currencyId = value.id;
    }
    if(action=="type"){
      this.ChargeToAdd.charge.type = value.id;
    }
    if(action=="service"){
      this.ChargeToAdd.charge.serviceTypeId =this.ChargeToAdd.charge.serviceTypeId==undefined?(value.id+";"): this.ChargeToAdd.charge.serviceTypeId += (value.id+";");
      console.log(this.ChargeToAdd.charge.serviceTypeId);
    }
    //console.log('Selected value is: ', value);
  }

  selectedTypeDefault(value:any,index:number){
    var listAcc = this.ChargeToAdd.listChargeDefaultAccount;
    if(lodash.findIndex(listAcc,function(o){return o.type===value.text})!=-1){
      this.isSameVoucherType =  true;
    }
    else{
      this.isSameVoucherType = false;
      this.ChargeToAdd.listChargeDefaultAccount[index].type =  value.text;
    } 

  }

  removedTypeDefault(value:any,index:number){
    this.ChargeToAdd.listChargeDefaultAccount[index].type = null;
  }

  validateChargeDefault = false;

  validatateDefaultAcountLine(){
    if(this.ChargeToAdd.listChargeDefaultAccount.length>0){
      var index = this.ChargeToAdd.listChargeDefaultAccount.length-1;
      if((this.ChargeToAdd.listChargeDefaultAccount[index].debitAccountNo == '' || this.ChargeToAdd.listChargeDefaultAccount[index].debitAccountNo==null)
        && (this.ChargeToAdd.listChargeDefaultAccount[index].creditAccountNo==''|| this.ChargeToAdd.listChargeDefaultAccount[index].creditAccountNo==null)
        && (this.ChargeToAdd.listChargeDefaultAccount[index].creditVat==null)
        && (this.ChargeToAdd.listChargeDefaultAccount[index].debitVat==null) || this.ChargeToAdd.listChargeDefaultAccount[index].type == null){
         // this.validateChargeDefault = false;
          return false;
        }else{
          // this.validateChargeDefault = true;
          return true;
        }
    }
  
  }

  public removed(value: any,action): void {
    if(action=="service"){
      var s = value.id+";";
      this.ChargeToAdd.charge.serviceTypeId = this.ChargeToAdd.charge.serviceTypeId.replace(s,"");
      console.log(this.ChargeToAdd.charge.serviceTypeId);
    }
    if(action=="unit"){
      this.ChargeToAdd.charge.unitId = null;
    }
    if(action=="currency"){
      this.ChargeToAdd.charge.currencyId = null;
    }
    if(action==="type"){
      this.ChargeToAdd.charge.type = null;
    }
    console.log('Removed value is: ', value);
  }

  // public typed(value: any): void {
  //   console.log('New search input: ', value);
  // }

  public refreshValue(value: any): void {
    this.value = value;
  }
}
