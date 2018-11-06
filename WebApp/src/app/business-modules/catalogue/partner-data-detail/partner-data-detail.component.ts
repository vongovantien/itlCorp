import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { API_MENU } from 'src/constants/api-menu.const';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { TrustedHtmlString } from '@angular/core/src/sanitization/bypass';
import { NgForm } from '@angular/forms';
import { SelectComponent } from 'ng2-select';

@Component({
  selector: 'app-partner-data-detail',
  templateUrl: './partner-data-detail.component.html',
  styleUrls: ['./partner-data-detail.component.scss']
})
export class PartnerDataDetailComponent implements OnInit {
  departments: any[];
  departmentActive: any;
  partner: Partner;
  parentCustomers: any[];
  parentCustomerActive: any;
  workPlaces: any[];
  workPlaceActive: any;
  saleMans: any[];
  countries: any[];
  billingCountryActive: any;
  billingProvinces: any[];
  billingProvinceActive: any;
  shippingCountryActive: any;
  shippingProvinces: any[];
  shippingProvinceActive: any;
  partnerGroups: any[];
  partnerGroupActives: any[] = [];
  salemanActive: any;
  activeNg = true;
  isRequiredSaleman = false;
  titleConfirmDelete = "You want to delete this Partner?";
  @ViewChild('formAddEdit') form: NgForm;
  @ViewChild('chooseBillingCountry') public chooseBillingCountry: SelectComponent;
  @ViewChild('chooseBillingProvince') public chooseBillingProvince: SelectComponent;
  @ViewChild('chooseShippingCountry') public chooseShippingCountry: SelectComponent;
  @ViewChild('chooseShippingProvince') public chooseShippingProvince: SelectComponent;

  constructor(private route:ActivatedRoute,
    private router:Router,
    private baseService: BaseService,
    private toastr: ToastrService,
    private spinnerService: Ng4LoadingSpinnerService,
    private api_menu: API_MENU) { }

  async ngOnInit() {
    await this.route.params.subscribe(prams => {
      this.partner = new Partner();
      if(prams.id != undefined){
        this.partner.id = prams.id;
      }
    });
    await this.getComboboxData();
    await this.getParnerDetails();
  }
  async getParnerDetails() {
    this.partner =  await this.baseService.getAsync(this.api_menu.Catalogue.PartnerData.getById + this.partner.id, false, true);
    this.getReferenceData();
  }
  getReferenceData(): any {
    if(this.partner.partnerGroup.includes('CUSTOMER')){
      this.isRequiredSaleman = true;
    }
    this.salemanActive = this.saleMans.find(x => x.id == this.partner.salePersonId);
    if(this.partner.partnerGroup.includes('CUSTOMER')){
      this.isRequiredSaleman = true;
    }
    console.log(this.isRequiredSaleman);
    console.log(this.partner.salePersonId);
    this.getPartnerGroupActives(this.partner.partnerGroup.split(';'));
    this.departmentActive = this.departments.find(x => x.id == this.partner.departmentId);
    this.parentCustomerActive = this.parentCustomers.find(x => x.id == this.partner.parentId);
    this.workPlaceActive = this.workPlaces.find(x => x.id == this.partner.workPlaceId);
    this.billingCountryActive = this.countries.find(x => x.id == this.partner.countryId);
    if(this.billingCountryActive){
      this.getProvincesByCountry(this.billingCountryActive.id, true);
    }
    this.shippingCountryActive = this.countries.find(x => x.id == this.partner.countryShippingId);
    if(this.shippingCountryActive){
      this.getProvincesByCountry(this.shippingCountryActive.id, false);
    }
  }
  getPartnerGroupActives(arg0: string[]): any {
    if(arg0.length > 0){
      for(let i=0; i< arg0.length; i++ ){
        let group = this.partnerGroups.find(x => x.id == arg0[i]);
        if(group){
          this.partnerGroupActives.push(group);
        }
      }
    }
    this.activeNg = false;
    setTimeout(() => {
      this.activeNg = true;
    }, 200);
  }
  async getComboboxData() { 
    await this.getPartnerGroups();
    await this.getCountries();
    await this.getSalemans();
    await this.getWorkPlaces();
    await this.getParentCustomers();
    await this.getDepartments();
  }
  async getDepartments(){
    let respones = await this.baseService.getAsync(this.api_menu.Catalogue.PartnerData.getDepartments);
    if(respones != null){
      this.departments = respones.map(x=>({"text":x.name,"id":x.id}));
    }
  }
  async getParentCustomers(){
    let respones = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup : 3 });
    if(respones != null){
      this.parentCustomers = respones.map(x=>({"text":x.partnerNameVn,"id":x.id}));
    }
  }
  async getWorkPlaces(){
    let responses = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.query, { placeType: 2 });
    if(responses != null){
      this.workPlaces = responses.map(x=>({"text":x.code + ' - ' + x.name_VN ,"id":x.id}));
    }
  }
  async getSalemans(){
    let responses = await this.baseService.getAsync(this.api_menu.System.User_Management.getAll);
    this.saleMans = responses.map(x=>({"text":x.username,"id":x.id}));
  }
  async getCountries() {
    let responses = await this.baseService.getAsync(this.api_menu.Catalogue.Country.getAllByLanguage);
    if(responses != null){
        this.countries = responses.map(x=>({"text":x.name,"id":x.id}));
      }
  }
  async getPartnerGroups() {
    let responses = await this.baseService.getAsync(this.api_menu.Catalogue.partnerGroup.getAll);
    this.partnerGroups = responses.map(x=>({"text":x.id,"id":x.id}));
  }
  getProvincesByCountry(countryId: number, isBilling: boolean): any {
    this.baseService.get(this.api_menu.Catalogue.CatPlace.getProvinces + "?countryId=" + countryId).subscribe((response: any) => {
      if(response != null){
        if(isBilling){
          this.billingProvinces = response.map(x=>({"text":x.name_VN,"id":x.id}));
          this.billingProvinceActive = this.billingProvinces.find(x => x.id == this.partner.provinceId);
        }
        else{
          this.shippingProvinces = response.map(x=>({"text":x.name_VN,"id":x.id}));
          this.shippingProvinceActive = this.shippingProvinces.find(x => x.id == this.partner.provinceId);
        }
      }
      else{
        this.billingProvinces = [];
        this.shippingProvinces = [];
      }
    });
  }
  onSubmit(){
    if(this.form.valid && !(this.partner.salePersonId == null && this.isRequiredSaleman)){
      this.partner.accountNo = this.partner.id= this.partner.taxCode;
      this.update();
    }
  }
  update(): any {
    this.baseService.put(this.api_menu.Catalogue.PartnerData.update + this.partner.id, this.partner).subscribe((response: any) => {
      if (response.status == true){
        this.toastr.success(response.message);
      }
      else{
        this.toastr.error(response.message);
      }
    }, error => this.baseService.handleError(error));
  }
  onDelete(event){
    if(event){
      this.baseService.delete(this.api_menu.Catalogue.PartnerData.delete + this.partner.id).subscribe((response: any) => {
        if (response.status == true) {
          this.toastr.success(response.message);
          this.router.navigate(["/home/catalogue/partner-data",{ id: this.partner.id }]);
        }
        if (response.status == false) {
          this.toastr.error(response.message);
        }
      }, error => this.baseService.handleError(error));
    }
  }
  /**
   * ng2-select
   */
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

  public selected(value: any, selectName): void {
    console.log('Selected value is: ', value);
    if(selectName == 'billingCountry'){
      this.partner.countryId = value.id;
      this.getProvincesByCountry(this.partner.countryId, true);
    }
    if(selectName == 'shippingCountry'){
      this.partner.countryShippingId = value.id;
      this.getProvincesByCountry(this.partner.countryShippingId, false);
    }
    if(selectName == 'billingProvince'){
      this.partner.provinceId = value.id;
    }
    if(selectName == 'shippingProvince'){
      this.partner.provinceShippingId = value.id;
    }
    if(selectName == 'department'){
      this.partner.departmentId = value.id;
    }
    if(selectName == 'workplace'){
      this.partner.workPlaceId = value.id;
    }
    if(selectName == 'accountRef'){
      this.partner.parentId = value.id;
    }
    if(selectName == 'saleman'){
      this.partner.salePersonId = value.id;
      if(this.partner.partnerGroup.includes('CUSTOMER')){
        this.isRequiredSaleman = true;
      }
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
      if(this.partner.partnerGroup.includes('CUSTOMER')){
        this.isRequiredSaleman = true;
      }
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
