import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { API_MENU } from 'src/constants/api-menu.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { NgForm } from '@angular/forms';
import { SelectComponent } from 'ng2-select';
import { SortService } from 'src/app/shared/services/sort.service';
import * as dataHelper from 'src/helper/data.helper';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';

@Component({
  selector: 'app-partner-data-addnew',
  templateUrl: './partner-data-addnew.component.html',
  styleUrls: ['./partner-data-addnew.component.scss']
})
export class PartnerDataAddnewComponent implements OnInit {
  activeNg: boolean = true;
  partner: Partner = new Partner();
  partnerGroups: any;
  partnerGroupActives: any = [];
  countries: any[];
  billingProvinces: any[];
  shippingProvinces: any[];
  saleMans: any[];
  workPlaces: any[];
  parentCustomers: any[];
  departments: any[];
  partnerType: any;
  isRequiredSaleman = false;
  employee: any = {};
  users: any[] = [];
  @ViewChild('formAddEdit', { static: false }) form: NgForm;
  @ViewChild('chooseBillingCountry', { static: false }) public chooseBillingCountry: SelectComponent;
  @ViewChild('chooseBillingProvince', { static: false }) public chooseBillingProvince: SelectComponent;
  @ViewChild('chooseShippingCountry', { static: false }) public chooseShippingCountry: SelectComponent;
  @ViewChild('chooseShippingProvince', { static: false }) public chooseShippingProvince: SelectComponent;
  @ViewChild('chooseSaleman', { static: false }) public chooseSaleman: SelectComponent;
  @ViewChild('chooseDepartment', { static: false }) public chooseDepartment: SelectComponent;
  @ViewChild('chooseAccountRef', { static: false }) public chooseAccountRef: SelectComponent;
  @ViewChild('chooseWorkplace', { static: false }) public chooseWorkplace: SelectComponent;

  constructor(private route: ActivatedRoute,
    private baseService: BaseService,
    private api_menu: API_MENU,
    private router: Router,
    private sortService: SortService) { }

  ngOnInit() {
    this.route.params.subscribe(prams => {
      console.log({ param: prams });
      if (prams.partnerType != undefined) {
        this.partnerType = prams.partnerType;
      }
    });

    this.getComboboxData();
    this.partner.departmentId = "Head Office";
  }
  getComboboxData(): any {
    this.getPartnerGroups();
    this.getCountries();
    this.getSalemans();
    this.getWorkPlaces();
    this.getparentCustomers();
    this.getDepartments();
  }
  departmentActive: any[] = [];
  getDepartments(): any {
    this.baseService.get(this.api_menu.Catalogue.PartnerData.getDepartments).subscribe((response: any) => {
      if (response != null) {
        this.departments = response.map(x => ({ "text": x.name, "id": x.id }));
      }
      this.departmentActive = ["Head Office"];
    }, err => {
      this.baseService.handleError(err);
    });
  }
  async getparentCustomers() {
    let responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER }, false, true);
    if (responses != null) {
      this.parentCustomers = responses.map(x => ({ "text": x.partnerNameVn, "id": x.id }));
    }
  }
  async getWorkPlaces() {
    let responses = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Branch }, false, true);
    if (responses != null) {
      this.workPlaces = responses.map(x => ({ "text": x.code + ' - ' + x.nameVn, "id": x.id }));
    }
  }
  async getSalemans() {
    let responses = await this.baseService.getAsync(this.api_menu.System.User_Management.getAll, false, true);
    if (responses != null) {
      this.users = responses;
      this.saleMans = responses.map(x => ({ "text": x.username, "id": x.id }));
      this.saleMans = this.sortService.sort(this.saleMans, 'text', true);
    }
  }
  async getProvinces(id: number, isBilling: boolean) {
    let url = this.api_menu.Catalogue.CatPlace.getProvinces;
    if (id != undefined) {
      url = url + "?countryId=" + id;
    }
    let responses = await this.baseService.getAsync(url, false, false);
    if (responses != null) {
      if (isBilling) {
        this.billingProvinces = responses.map(x => ({ "text": x.name_VN, "id": x.id }));
      }
      else {
        this.shippingProvinces = responses.map(x => ({ "text": x.name_VN, "id": x.id }));
      }
    }
    else {
      this.billingProvinces = [];
      this.shippingProvinces = [];
    }
  }
  async getCountries() {
    let responses = await this.baseService.getAsync(this.api_menu.Catalogue.Country.getAllByLanguage, false, true);
    if (responses != null) {
      this.countries = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
    }
    else {
      this.countries = [];
    }
  }
  getPartnerGroups(): any {
    this.baseService.get(this.api_menu.Catalogue.partnerGroup.getAll).subscribe((response: any) => {
      if (response != null) {
        this.partnerGroups = response.map(x => ({ "text": x.id, "id": x.id }));
        this.getPartnerGroupActive(this.partnerType);
      }
    }, err => {
      this.baseService.handleError(err);
    });
  }
  getPartnerGroupActive(partnerGroup: any): any {
    if (partnerGroup == PartnerGroupEnum.AGENT) {
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "AGENT"));
    }
    if (partnerGroup == PartnerGroupEnum.AIRSHIPSUP) {
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "AIRSHIPSUP"));
    }
    if (partnerGroup == PartnerGroupEnum.CARRIER) {
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CARRIER"));
    }
    if (partnerGroup == PartnerGroupEnum.CONSIGNEE) {
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CONSIGNEE"));
    }
    if (partnerGroup == PartnerGroupEnum.CUSTOMER) {
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CUSTOMER"));
    }
    if (partnerGroup == PartnerGroupEnum.SHIPPER) {
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "SHIPPER"));
    }
    if (partnerGroup == PartnerGroupEnum.SUPPLIER) {
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "SUPPLIER"));
    }
    if (partnerGroup == PartnerGroupEnum.ALL) {
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "ALL"));
    }
    this.partner.partnerGroup = '';
    if (this.partnerGroupActives.find(x => x.id == "ALL")) {
      this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
    }
    else {
      this.partnerGroupActives.forEach(element => {
        this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
      });
      if (this.partnerGroupActives.length > 0) {
        this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length - 1));
      }
    }
    this.isRequiredSaleman = this.checkRequireSaleman(this.partner.partnerGroup);
    console.log(this.partner.partnerGroup);
    this.activeNg = false;
    setTimeout(() => {
      this.activeNg = true;
    }, 200);
  }

  onSubmit() {
    if (this.partner.countryId == null || this.partner.provinceId == null
      || this.partner.countryShippingId == null || this.partner.provinceShippingId == null || this.partner.departmentId == null) {
      return;
    }
    if (this.form.valid) {
      this.partner.accountNo = this.partner.id = this.partner.taxCode;
      if (this.isRequiredSaleman && this.partner.salePersonId != null) {
        this.addNew();
      }
      else {
        if (this.isRequiredSaleman == false) {
          this.addNew();
        }
      }
    }
  }
  addNew(): any {
    this.baseService.spinnerShow();
    this.baseService.post(this.api_menu.Catalogue.PartnerData.add, this.partner).subscribe((response: any) => {
      this.baseService.spinnerHide();
      this.baseService.successToast(response.message);
      this.router.navigate(["/home/catalogue/partner-data"]);
      //this.resetForm();     
    }, err => {
      this.baseService.spinnerHide();
      this.baseService.handleError(err);
    });
  }

  resetForm(): any {
    this.form.onReset();
    this.partner.parentId = null;
    this.partner.countryId = null;
    this.partner.provinceId = null;
    this.partner.countryShippingId = null;
    this.partner.provinceShippingId = null;
    this.partner.departmentId = "1";
    //this.partner.partnerGroup = '';
    this.partner.salePersonId = null;
    this.partner.workPlaceId = null;
    this.partner.public = false;
    //this.partnerGroupActives = [];
    this.chooseBillingCountry.active = [];
    this.chooseBillingProvince.active = [];
    this.chooseShippingCountry.active = [];
    this.chooseShippingProvince.active = [];
    this.chooseSaleman.active = [];
    this.chooseDepartment.active = [];
    this.chooseAccountRef.active = [];
    this.chooseWorkplace.active = [];
  }
  getEmployee(employeeId: any): any {
    this.baseService.post(this.api_menu.System.Employee.query, { id: employeeId }).subscribe((responses: any) => {
      if (responses.length > 0) {
        this.employee = responses[0];
      }
      else {
        this.employee = {};
      }
      console.log(this.employee);
    });
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
  public selected(value: any, selectName?: string): void {
    if (selectName == 'billingCountry') {
      this.partner.countryId = value.id;
      this.partner.provinceId = null;
      this.getProvinces(value.id, true);
    }
    if (selectName == 'billingProvince') {
      this.partner.provinceId = value.id;
    }
    if (selectName == 'shippingCountry') {
      this.partner.countryShippingId = value.id;
      this.partner.provinceShippingId = null;
      this.getProvinces(value.id, false);
    }
    if (selectName == 'shippingProvince') {
      this.partner.provinceShippingId = value.id;
    }
    if (selectName == 'saleman') {
      //this.partner.salePersonId = value.id;
      let user = this.users.find(x => x.id == value.id);
      if (user) {
        this.getEmployee(user.employeeId);
      }
    }
    if (selectName == 'category') {
      this.changePartnerGroup(value);
      this.isRequiredSaleman = this.checkRequireSaleman(this.partner.partnerGroup);
      console.log(this.partner.partnerGroup);
    }
  }
  checkRequireSaleman(partnerGroup: string): boolean {
    if (partnerGroup == null) {
      return false;
    }
    else if (partnerGroup.includes('CUSTOMER') || partnerGroup.includes('ALL')) {
      return true;
    }
    else {
      return false;
    }
  }
  changePartnerGroup(value: { id: string; text: any; }) {
    this.partner.partnerGroup = '';
    if (value.id == "ALL") {
      this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
    }
    else {
      this.partnerGroupActives.push({ id: value.id, text: value.text });
      this.partnerGroupActives.forEach(element => {
        this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
      });
      if (this.partnerGroupActives.length > 0) {
        this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length - 1));
      }
    }
  }

  public removed(value: any, selectName?: string): void {
    if (selectName == 'billingCountry') {
      this.partner.countryId = null;
      this.partner.provinceId = null;
      this.billingProvinces = [];
      this.chooseBillingProvince.active = [];
    }
    if (selectName == 'billingProvince') {
      this.partner.provinceId = null;
    }
    if (selectName == 'shippingCountry') {
      this.partner.countryShippingId = null;
      this.partner.provinceShippingId = null;
      this.shippingProvinces = [];
      this.chooseShippingProvince.active = [];
    }
    if (selectName == 'shippingProvince') {
      this.partner.provinceShippingId = null;
    }
    if (selectName == 'category') {
      this.removePartnerGroup(value);
      this.isRequiredSaleman = this.checkRequireSaleman(this.partner.partnerGroup);
    }
    console.log('Removed value is: ', value);
  }
  removePartnerGroup(value: any) {
    var index = this.partnerGroupActives.indexOf(this.partnerGroupActives.find(x => x.id == value.id));
    if (index > -1) {
      this.partnerGroupActives.splice(index, 1);
    }
    this.partner.partnerGroup = null;
    if (value.id != "ALL") {
      this.partnerGroupActives.forEach(element => {
        this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
      });
      if (this.partnerGroupActives.length > 0) {
        this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length - 1));
      }
    }
  }

  public typed(value: any): void {
    console.log('New search input: ', value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }
}
