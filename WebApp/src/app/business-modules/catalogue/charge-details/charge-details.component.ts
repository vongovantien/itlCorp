import { Component, OnInit } from '@angular/core';
import findIndex from 'lodash/findIndex';
import { BaseService } from 'src/app/shared/services/base.service';
import { ToastrService } from 'ngx-toastr';
import { API_MENU } from 'src/constants/api-menu.const';

import { NgForm } from '@angular/forms';

import { CatChargeToAddOrUpdate } from 'src/app/shared/models/catalogue/catChargeToAddOrUpdate.model';

import { CatChargeDefaultAccount } from 'src/app/shared/models/catalogue/catChargeDefaultAccount.model';

import { Router, ActivatedRoute } from '@angular/router';
// declare var $: any;


@Component({
  selector: 'app-charge-details',
  templateUrl: './charge-details.component.html',
  styleUrls: ['./charge-details.component.scss']
})
export class ChargeDetailsComponent implements OnInit {
  constructor(
    private baseServices: BaseService,
    private toastr: ToastrService,
    private api_menu: API_MENU,
    private route: ActivatedRoute,
    private router: Router) { }
  //Charge : CatChargeOrUpdate = new CatChargeOrUpdate();
  isAddNewLine: boolean = false;
  isMaximumAccountRow: boolean = false;
  isSameVoucherType: boolean = false;
  ngDataUnit: any = [];
  ngDataCurrency: any = [];
  Charge: CatChargeToAddOrUpdate = null;
  ngDataType = [
    { id: "CREDIT", text: "CREDIT" },
    { id: "DEBIT", text: "DEBIT" },
    { id: "OBH", text: "OBH" }
  ];
  ngDataTypeChargeDefault = [
    { id: "Công-Nợ", text: "Công Nợ" },
    { id: "Giải-Chi", text: "Giải Chi" },
    { id: "Loại-Khác", text: "Loại Khác" }
  ];

  activeUnit: any = null;
  activeCurrency: any = null;
  activeType: any = null;
  activeServices: any = null;

  /**
   * Need to update ngDataServices by get data from databse after implement documentation module 
   */
  ngDataService = [
    { text: "Inland Trucking", id: "IT" },
    { text: "Air Import", id: "AI" },
    { text: "Air Export", id: "AE" },
    { text: "Sea Import", id: "SI" },
    { text: "Sea Export", id: "SE" },
    { text: "Sea FCL Export", id: "SFE" },
    { text: "Sea FCL Import", id: "SFI" },
    { text: "Sea LCL Export", id: "SLE" },
    { text: "Sea LCL Import", id: "SLI" },
    { text: "Sea Consol Export", id: "SCE" },
    { text: "Sea Consol Import", id: "SCI" },
    { text: "Custom Logistic", id: "CL" }
  ];

  async ngOnInit() {
    await this.getNeccessaryData();
    await this.getChargeDetail();
  }

  getChargeDetail() {
    try {
      this.route.params.subscribe(async params => {
        var id = params.id;
        this.Charge = await this.baseServices.getAsync(this.api_menu.Catalogue.Charge.getById + id, true, true);
        this.activeServices = this.getCurrentActiveService(this.Charge.charge.serviceTypeId);

        const idUnit = this.Charge.charge.unitId;
        const idCurrency = this.Charge.charge.currencyId;
        const type = this.Charge.charge.type;

        var indexCurrentUnit = findIndex(this.ngDataUnit, function (o) { return o['id'] === idUnit });
        var indexCurrentCurrency = findIndex(this.ngDataCurrency, function (o) { return o['id'] === idCurrency });
        var indexType = findIndex(this.ngDataType, function (o) { return o.id === type });

        this.activeUnit = [this.ngDataUnit[indexCurrentUnit]];
        this.activeType = [this.ngDataType[indexType]];
        this.activeCurrency = [this.ngDataCurrency[indexCurrentCurrency]];

      });
    } catch (error) {
      this.toastr.error("Cannot Get Charge Details !");
    }
  }

  getCurrentActiveService(ChargeService: any) {
    var listService = ChargeService.split(";");
    var activeServiceList: any = [];
    listService.forEach(item => {
      const index = findIndex(this.ngDataService, function (o) { return o.id === item });
      if (index != -1) {
        const activeService = this.ngDataService[index];
        activeServiceList.push(activeService);
      }
    });
    return activeServiceList;
  }

  async getNeccessaryData() {
    var units = await this.baseServices.getAsync(this.api_menu.Catalogue.Unit.getAll, false, false);
    this.ngDataUnit = units.length == 0 ? [] : units.map(x => ({ text: x.code, id: x.id }));

    var currencies = await this.baseServices.getAsync(this.api_menu.Catalogue.Currency.getAll, false, false);
    this.ngDataCurrency = currencies.length == 0 ? [] : currencies.map(x => ({ text: x.id + " - " + x.currencyName, id: x.id }));

    console.log({ unit: this.ngDataUnit, currency: this.ngDataCurrency });
  }

  addNewChargeDedaultAccount() {
    var obj = new CatChargeDefaultAccount();
    // this.Charge.listChargeDefaultAccount.push(obj);   

    if (this.Charge.listChargeDefaultAccount.length == 0) {
      this.Charge.listChargeDefaultAccount.push(obj);
    }
    else {
      if (this.validatateDefaultAcountLine()) {
        if (this.Charge.listChargeDefaultAccount.length == 3) {
          this.isMaximumAccountRow = true;
        } else {
          this.Charge.listChargeDefaultAccount.push(obj);
          this.isAddNewLine = false;;
        }
      } else {
        this.isAddNewLine = true;
      }

    }

    console.log(this.validateChargeDefault);
  }

  RemoveDefaultAccount(index) {
    this.Charge.listChargeDefaultAccount.splice(index, 1);
    this.isMaximumAccountRow = false;
  }

  async updateCharge(form: NgForm) {
    // console.log(this.isAddNewLine);
    this.isAddNewLine = true;
    if (form.form.status != "INVALID" && this.validatateDefaultAcountLine() && this.isSameVoucherType == false) {
      var response = await this.baseServices.putAsync(this.api_menu.Catalogue.Charge.update, this.Charge, true, true);
      if (response) {
        this.router.navigate(["/home/catalogue/charge"]);
      }
    }

  }


  getActiveVoucherType(index) {
    // console.log(index);
    const voucherType = this.Charge.listChargeDefaultAccount[index].type;
    if (voucherType === null || voucherType === undefined) {
      return [];
    } else {
      const indexCurrenVoucher = findIndex(this.ngDataTypeChargeDefault, function (o) { return o.text === voucherType });
      const aciveVoucherType = [this.ngDataTypeChargeDefault[indexCurrenVoucher]];
      // console.log(aciveVoucherType);
      return aciveVoucherType;
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

  public selected(value: any, action): void {
    if (action == "unit") {
      this.Charge.charge.unitId = value.id;

    }
    if (action == "currency") {
      this.Charge.charge.currencyId = value.id;
    }
    if (action == "type") {
      this.Charge.charge.type = value.id;
    }
    if (action == "service") {
      this.Charge.charge.serviceTypeId = this.Charge.charge.serviceTypeId == undefined ? (value.id + ";") : this.Charge.charge.serviceTypeId += (value.id + ";");
      console.log(this.Charge.charge.serviceTypeId);
    }
    //console.log('Selected value is: ', value);
  }

  selectedTypeDefault(value: any, index: number) {
    var listAcc = this.Charge.listChargeDefaultAccount;
    if (findIndex(listAcc, function (o) { return o.type === value.text }) != -1) {
      this.isSameVoucherType = true;
    }
    else {
      this.isSameVoucherType = false;
      this.Charge.listChargeDefaultAccount[index].type = value.text;
    }

  }

  removedTypeDefault(value: any, index: number) {
    this.Charge.listChargeDefaultAccount[index].type = null;
  }

  validateChargeDefault = false;

  validatateDefaultAcountLine() {
    if (this.Charge.listChargeDefaultAccount.length > 0) {
      var index = this.Charge.listChargeDefaultAccount.length - 1;
      if ((this.Charge.listChargeDefaultAccount[index].debitAccountNo == '' || this.Charge.listChargeDefaultAccount[index].debitAccountNo == null)
        && (this.Charge.listChargeDefaultAccount[index].creditAccountNo == '' || this.Charge.listChargeDefaultAccount[index].creditAccountNo == null)
        && (this.Charge.listChargeDefaultAccount[index].creditVat == null)
        && (this.Charge.listChargeDefaultAccount[index].debitVat == null) || this.Charge.listChargeDefaultAccount[index].type == null) {
        // this.validateChargeDefault = false;
        return false;
      } else {
        // this.validateChargeDefault = true;
        return true;
      }
    }

  }

  public removed(value: any, action): void {
    if (action == "service") {
      var s = value.id + ";";
      this.Charge.charge.serviceTypeId = this.Charge.charge.serviceTypeId.replace(s, "");
      console.log(this.Charge.charge.serviceTypeId);
    }
    if (action == "unit") {
      this.Charge.charge.unitId = null;
    }
    if (action == "currency") {
      this.Charge.charge.currencyId = null;
    }
    if (action === "type") {
      this.Charge.charge.type = null;
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
