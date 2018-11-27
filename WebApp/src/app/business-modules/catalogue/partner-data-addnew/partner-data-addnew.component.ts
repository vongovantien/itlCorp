import { Component, OnInit, ViewChild } from '@angular/core';
import { RouterLinkActive, ActivatedRoute, Router } from '@angular/router';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { API_MENU } from 'src/constants/api-menu.const';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { SortService } from 'src/app/shared/services/sort.service';
import { ToastrService } from 'ngx-toastr';
import { BaseService } from 'src/services-base/base.service';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { NgForm } from '@angular/forms';
import { SelectComponent } from 'ng2-select';

@Component({
  selector: 'app-partner-data-addnew',
  templateUrl: './partner-data-addnew.component.html',
  styleUrls: ['./partner-data-addnew.component.scss']
})
export class PartnerDataAddnewComponent implements OnInit {
   activeNg:boolean= true;
   partner: Partner = new Partner();
   partnerGroups: any;
   partnerGroupActives: any=[];
   countries: any[];
   billingCountryActive: any;
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
   @ViewChild('formAddEdit') form: NgForm;
   @ViewChild('chooseBillingCountry') public chooseBillingCountry: SelectComponent;
   @ViewChild('chooseBillingProvince') public chooseBillingProvince: SelectComponent;
   @ViewChild('chooseShippingCountry') public chooseShippingCountry: SelectComponent;
   @ViewChild('chooseShippingProvince') public chooseShippingProvince: SelectComponent;
   @ViewChild('chooseSaleman') public chooseSaleman: SelectComponent;
   @ViewChild('chooseDepartment') public chooseDepartment: SelectComponent;
   @ViewChild('chooseAccountRef') public chooseAccountRef: SelectComponent;
   @ViewChild('chooseWorkplace') public chooseWorkplace: SelectComponent;

  constructor(private route:ActivatedRoute,
    private baseService: BaseService,
    private toastr: ToastrService, 
    private api_menu: API_MENU) { }

  ngOnInit() {
    this.route.params.subscribe(prams => {
      console.log({param:prams});
      if(prams.partnerType != undefined){
        this.partnerType = prams.partnerType;
      }
    });

    this.getComboboxData();
    
  }
  getComboboxData(): any {
    this.getPartnerGroups();
    this.getCountries();
    //this.getProvinces();
    this.getSalemans();
    this.getWorkPlaces();
    this.getparentCustomers();
    this.getDepartments();
  }
  getDepartments(): any {
    this.baseService.get(this.api_menu.Catalogue.PartnerData.getDepartments).subscribe((response: any) => {
      if(response != null){
        this.departments = response.map(x=>({"text":x.name,"id":x.id}));
      }
     });
  }
  getparentCustomers(): any { 
    this.baseService.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup : 3 }).subscribe((response: any) => {
      if(response.length > 0){
        this.parentCustomers = response.map(x=>({"text":x.partnerNameVn,"id":x.id}));
        console.log(this.parentCustomers);
      }
    });
  }
  getWorkPlaces(): any {
    this.baseService.post(this.api_menu.Catalogue.CatPlace.query, { placeType: 2 }).subscribe((response: any) => {
      if(response != null){
        this.workPlaces = response.map(x=>({"text":x.code + ' - ' + x.name_VN ,"id":x.id}));
      }
     });
  }
  getSalemans(): any {
    this.baseService.get(this.api_menu.System.User_Management.getAll).subscribe((response: any) => {
      if(response != null){
        this.users = response;
        this.saleMans = response.map(x=>({"text":x.username,"id":x.id}));
      }
     });
  }
  getProvinces(id: number, isBilling: boolean){
    let url = this.api_menu.Catalogue.CatPlace.getProvinces;
    if(id != undefined){
      url = url + "?countryId=" + id; 
    }
    this.baseService.get(url).subscribe((response: any) => {
      if(isBilling){
        this.billingProvinces = response.map(x=>({"text":x.name_VN,"id":x.id}));
      }
      else{
        this.shippingProvinces = response.map(x=>({"text":x.name_VN,"id":x.id}));
      }
    });
  }
  getCountries(): any {
    this.baseService.get(this.api_menu.Catalogue.Country.getAllByLanguage).subscribe((response: any) => {
    if(response != null){
      this.countries = response.map(x=>({"text":x.name,"id":x.id}));
    }
   });
  }
  getPartnerGroups(): any {
    this.baseService.get(this.api_menu.Catalogue.partnerGroup.getAll).subscribe((response: any) => {
      if(response != null){
        this.partnerGroups = response.map(x=>({"text":x.id,"id":x.id}));
        this.getPartnerGroupActive(this.partnerType);
      }
    });
  }
  getPartnerGroupActive(partnerGroup: any): any {
    if(partnerGroup == PartnerGroupEnum.AGENT){
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "AGENT"));
    }
    if(partnerGroup == PartnerGroupEnum.AIRSHIPSUP){
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "AIRSHIPSUP"));
    }
    if(partnerGroup == PartnerGroupEnum.CARRIER){
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CARRIER"));
    }
    if(partnerGroup == PartnerGroupEnum.CONSIGNEE){
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CONSIGNEE"));
    }
    if(partnerGroup == PartnerGroupEnum.CUSTOMER){
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "CUSTOMER"));
    }
    if(partnerGroup == PartnerGroupEnum.SHIPPER){
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "SHIPPER"));
    }
    if(partnerGroup == PartnerGroupEnum.SUPPLIER){
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "SUPPLIER"));
    }
    if(partnerGroup == PartnerGroupEnum.ALL){
      this.partnerGroupActives.push(this.partnerGroups.find(x => x.id == "ALL"));
    }
    this.partner.partnerGroup = '';
    if(this.partnerGroupActives.find(x => x.id == "ALL")){
      this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
    }
    else{
      this.partnerGroupActives.forEach(element => {
        this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
      });
      if(this.partnerGroupActives.length>0){
        this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length-1));
      }
    }
    if(this.partner.partnerGroup.includes('CUSTOMER')){
      this.isRequiredSaleman = true;
    }
    else{
      this.isRequiredSaleman = false;
    }
    console.log(this.partner.partnerGroup);
    this.activeNg = false;
    setTimeout(() => {
      this.activeNg = true;
    }, 200);
  }

  onSubmit(){
    if(this.form.valid){
      this.partner.accountNo = this.partner.id= this.partner.taxCode;
      if(this.isRequiredSaleman && this.partner.salePersonId != null){
        this.addNew();
      }
      else{
        this.addNew();
      }
    }
  }
  addNew(): any {
    this.baseService.post(this.api_menu.Catalogue.PartnerData.add, this.partner).subscribe((response: any) => {
      if (response.status == true){
        this.toastr.success(response.message);
        this.resetForm();
      }
      else{
        this.toastr.error(response.message);
      }
    }, error => this.baseService.handleError(error));
  }
  resetForm(): any {
    this.form.onReset();
    this.partner.parentId = null;
    this.partner.countryId = null;
    this.partner.provinceId = null;
    this.partner.countryShippingId = null;
    this.partner.provinceShippingId = null;
    this.partner.departmentId = null;
    this.partner.partnerGroup = '';
    this.partner.salePersonId = null;
    this.partner.workPlaceId = null;
    this.partner.public = false;
    this.partnerGroupActives = [];
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
    this.baseService.post(this.api_menu.System.Employee.query, { id : employeeId}).subscribe((responses: any) => {
      if(responses.length>0){
        this.employee = responses[0];
      }
      else{
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
    if(selectName == 'billingCountry'){
      this.partner.countryId = value.id;
      this.partner.provinceId = null;
      this.getProvinces(value.id, true);
    }
    if(selectName == 'billingProvince'){
      this.partner.provinceId = value.id;
    }
    if(selectName == 'shippingCountry'){
      this.partner.countryShippingId = value.id;
      this.partner.provinceShippingId = null;
      this.getProvinces(value.id, false);
    }
    if(selectName == 'shippingProvince'){
      this.partner.provinceShippingId = value.id;
    }
    if(selectName == 'saleman'){
      this.partner.salePersonId = value.id;
      let user = this.users.find(x => x.id == value.id);
      if(user){
        this.getEmployee(user.employeeId);
      }
    }
    if(selectName == 'department'){
      this.partner.departmentId = value.id;
    }
    if(selectName == 'accountRef'){
      this.partner.parentId = value.id;
    }
    if(selectName == 'accountRef'){
      this.partner.parentId = value.id;
    }
    if(selectName == 'workplace'){
      this.partner.workPlaceId = value.id;
    }
    if(selectName == 'category'){
      this.partner.partnerGroup = '';
      if(value.id=="ALL"){
        this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
      }
      else{
        this.partnerGroupActives.push({ id: value.id, text: value.text});
        this.partnerGroupActives.forEach(element => {
          this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
        });
        if(this.partnerGroupActives.length>0){
          this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length-1));
        }
      }
      if(this.partner.partnerGroup.includes('CUSTOMER')){
        this.isRequiredSaleman = true;
      }
      else{
        this.isRequiredSaleman = false;
      }
      console.log(this.partner.partnerGroup);
    }
  }

  public removed(value: any, selectName?: string): void {
    if(selectName == 'billingCountry'){
      this.partner.countryId = null;
      this.partner.provinceId = null;
      this.billingProvinces = [];
      this.chooseBillingProvince.active = [];
    }
    if(selectName == 'billingProvince'){
      this.partner.provinceId = null;
    }
    if(selectName == 'shippingCountry'){
      this.partner.countryShippingId = null;
      this.partner.provinceShippingId = null;
      this.shippingProvinces = [];
      this.chooseShippingProvince.active = [];
    }
    if(selectName == 'shippingProvince'){
      this.partner.provinceShippingId = null;
    }
    if(selectName == 'category'){
      var index = this.partnerGroupActives.indexOf(this.partnerGroupActives.find(x => x.id == value.id));
      if (index > -1) {
        this.partnerGroupActives.splice(index, 1);
      }
      this.partner.partnerGroup = '';
      if(value.id=="ALL"){
        this.partner.partnerGroup = 'AGENT;AIRSHIPSUP;CARRIER;CONSIGNEE;CUSTOMER;SHIPPER;SUPPLIER';
      }
      else{
        this.partnerGroupActives.forEach(element => {
          this.partner.partnerGroup = element.text + ';' + this.partner.partnerGroup;
        });
        if(this.partnerGroupActives.length>0){
          this.partner.partnerGroup.substring(0, (this.partner.partnerGroup.length-1));
        }
      }
      if(this.partner.partnerGroup.includes('CUSTOMER')){
        this.isRequiredSaleman = true;
      }
      else{
        this.isRequiredSaleman = false;
      }
    }
    if(selectName == 'saleman'){
      this.partner.salePersonId = null;
    }
    if(selectName == 'department'){
      this.partner.departmentId = null;
    }
    if(selectName == 'accountRef'){
      this.partner.parentId = null;
    }
    if(selectName == 'accountRef'){
      this.partner.parentId = null;
    }
    if(selectName == 'workplace'){
      this.partner.workPlaceId = null;
    }
    console.log('Removed value is: ', value);
  }

  public typed(value: any): void {
    console.log('New search input: ', value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }

}
