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
   partner: Partner = new Partner();
   partnerGroups: any;
   partnerGroupActive: any;
   countries: any[];
   billingCountryActive: any;
   provinces: any[];
   billingProvinceActive: any;
   saleMans: any[];
   workPlaces: any[];
   customers: any[];
   departments: any[];
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
    private api_menu: API_MENU,
    private sortService: SortService) { }

  ngOnInit() {
    this.route.params.subscribe(prams => {
      console.log({param:prams});
      if(prams.partnerType != undefined){
        this.partner.partnerGroup = prams.partnerType;
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
    this.getCustomers();
    this.getDepartments();
  }
  getDepartments(): any {
    this.baseService.get(this.api_menu.Catalogue.PartnerData.getDepartments).subscribe((response: any) => {
      if(response != null){
        this.departments = response.map(x=>({"text":x.name,"id":x.id}));
      }
     });
  }
  getCustomers(): any { 
    this.baseService.post(this.api_menu.Catalogue.PartnerData.query, { partnerGroup : 3 }).subscribe((response: any) => {
      if(response.length > 0){
        this.customers = response.data.map(x=>({"text":x.PartnerNameVn,"id":x.id}));
        console.log(this.customers);
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
        this.saleMans = response.map(x=>({"text":x.username,"id":x.id}));
      }
     });
  }
  getProvinces(id?: number){
    let url = this.api_menu.Catalogue.CatPlace.getProvinces;
    if(id != undefined){
      url = url + "?countryId=" + id; 
    }
    this.baseService.get(url).subscribe((response: any) => {
      if(response != null){
        this.provinces = response.map(x=>({"text":x.name_VN,"id":x.id}));
      }
      else{
        this.provinces = [];
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
        this.getPartnerGroupActive(this.partner.partnerGroup);
      }
    });
  }
  getPartnerGroupActive(partnerGroup: any): any {
    if(partnerGroup == PartnerGroupEnum.AGENT){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "AGENT");
    }
    if(partnerGroup == PartnerGroupEnum.AIRSHIPSUP){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "AIRSHIPSUP");
    }
    if(partnerGroup == PartnerGroupEnum.CARRIER){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "CARRIER");
    }
    if(partnerGroup == PartnerGroupEnum.CONSIGNEE){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "CONSIGNEE");
    }
    if(partnerGroup == PartnerGroupEnum.CUSTOMER){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "CUSTOMER");
    }
    if(partnerGroup == PartnerGroupEnum.PAYMENTOBJECT){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "PAYMENTOBJECT");
    }
    if(partnerGroup == PartnerGroupEnum.PETROLSTATION){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "PETROLSTATION");
    }
    if(partnerGroup == PartnerGroupEnum.SHIPPER){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "SHIPPER");
    }
    if(partnerGroup == PartnerGroupEnum.SHIPPINGLINE){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "SHIPPINGLINE");
    }
    if(partnerGroup == PartnerGroupEnum.SUPPLIER){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "SUPPLIER");
    }
    if(partnerGroup == PartnerGroupEnum.SUPPLIERMATERIAL){
      this.partnerGroupActive = this.partnerGroups.find(x => x.id == "SUPPLIERMATERIAL");
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

  public selected(value: any, selectName?: string): void {
    if(selectName == 'billingCountry'){
      this.partner.countryId = value.id;
      this.partner.provinceId = null;
      this.getProvinces(value.id);
    }
    if(selectName == 'billingProvince'){
      this.partner.provinceId = value.id;
    }
    if(selectName == 'shippingCountry'){
      this.partner.countryShippingId = value.id;
      this.partner.provinceId = null;
    }
    if(selectName == 'shippingProvince'){
      this.partner.provinceShippingId = value.id;
      this.getProvinces(value.id);
    }
  }

  public removed(value: any, selectName?: string): void {
    if(selectName == 'billingCountry'){
      this.partner.countryId = null;
      this.partner.provinceId = null;
    }
    if(selectName == 'billingProvince'){
      this.partner.provinceId = value.id;
    }
    if(selectName == 'shippingCountry'){
      this.partner.countryShippingId = null;
      this.partner.provinceId = null;
    }
    if(selectName == 'shippingProvince'){
      this.partner.provinceShippingId = value.id;
      this.getProvinces(value.id);
    }
    console.log('Removed value is: ', value);
  }

  public typed(value: any): void {
    console.log('New search input: ', value);
  }

  public refreshValue(value: any): void {
    this.value = value;
  }

  onSubmit(){
    if(this.form.valid){
      this.addNew();
    }
  }
  addNew(): any {
    this.baseService.post(this.api_menu.Catalogue.PartnerData.add, this.partner).subscribe((response: any) => {
      if (response.status == true){
        this.toastr.success(response.message);
        this.form.onReset();
      }
      else{
        this.toastr.error(response.message);
      }
    }, error => this.baseService.handleError(error));
  }
}
