import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AddressPartner, Partner } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import _merge from 'lodash/merge';
import { Observable, pipe } from 'rxjs';
import { Store } from '@ngrx/store';
import { GetCatalogueAddressAction, getCatalogueAddressState } from '@store';
import { SystemConstants } from '@constants';

@Component({
    selector: 'popup-form-address-commercial-catalogue',
    templateUrl: './form-address-commercial-catalogue.component.html'
})
export class FormAddressCommercialCatalogueComponent extends PopupBase implements OnInit {
    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();

    formAddress: FormGroup;

    partnerId: string = '';
    shortName: AbstractControl;
    taxCode: AbstractControl;

    id: string = '';
    accountNo: AbstractControl;
    shortNameAddress: AbstractControl;
    countryId: AbstractControl;
    cityId: AbstractControl;
    districtId: AbstractControl;
    wardId: AbstractControl;
    addressType: AbstractControl;
    streetAddress: AbstractControl;
    contactPerson: AbstractControl;
    tel: AbstractControl;

    provinces: any[] = [];
    districts: any[] = [];
    countries: any[] = [];
    wards: any[] = [];

    selectedAddress: AddressPartner = new AddressPartner();
    partner: Partner;
    isUpdate: boolean = false;
    patchValue: any;
    indexDetailAddress: number = null;
    addressDetail: any = null;
    listAddressType: any[] = ['All', 'Delivery', 'Pickup', 'Shipping', 'Billing'];
    addresses: Observable<AddressPartner[]>;

    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        protected _progressService: NgProgress,
        private _toastService: ToastrService,
        private _store: Store<any>) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
        this._store.dispatch(new GetCatalogueAddressAction());
        this.addresses = this._store.select(getCatalogueAddressState);
    }
    setDefaultValue(partner: any) {
        if (!!partner) {
            this.formAddress.patchValue({
                accountNo: partner.accountNo,
                shortName: partner.shortName,
                taxCode: partner.taxCode
            });
        }
    }
    initForm() {
        this.formAddress = this._fb.group({
            accountNo: [null, Validators.required],
            shortName: [null, Validators.required],
            taxCode: [null],
            shortNameAddress: [null, Validators.required],
            countryId: [null, Validators.required],
            cityId: [null, Validators.required],
            districtId: [null, Validators.required],
            wardId: [null, Validators.required],
            streetAddress: [null, Validators.required],
            addressType: [null, Validators.required],
            contactPerson: [null, Validators.required],
            tel: [null, Validators.compose([
                Validators.required,
                Validators.pattern(SystemConstants.CPATTERN.NUMBER),
            ])],
        });

        this.accountNo = this.formAddress.controls['accountNo'];
        this.shortName = this.formAddress.controls['shortName'];
        this.taxCode = this.formAddress.controls['taxCode'];
        this.shortNameAddress = this.formAddress.controls['shortNameAddress'];
        this.countryId = this.formAddress.controls['countryId'];
        this.cityId = this.formAddress.controls['cityId'];
        this.districtId = this.formAddress.controls['districtId'];
        this.wardId = this.formAddress.controls['wardId'];
        this.streetAddress = this.formAddress.controls['streetAddress'];
        this.addressType = this.formAddress.controls['addressType'];
        this.contactPerson = this.formAddress.controls['contactPerson'];
        this.tel = this.formAddress.controls['tel'];
    }

    setFormValue(res : AddressPartner) {
        this.isSubmitted = true;
        this.addressDetail = res;
        this.formAddress.setValue({
            accountNo: res.accountNo,
            shortName:res.shortName,
            taxCode: res.taxCode,
            shortNameAddress: res.shortNameAddress,
            countryId: { id: res.countryId, text: !!this.countries.find(x => x.id === res.countryId) ? this.countries.find(x => x.id === res.countryId).text : null },
            cityId: { id: res.cityId, text: !!this.provinces.find(x => x.id === res.cityId) ? this.provinces.find(x => x.id === res.cityId).text : null },
            districtId: { id: res.districtId, text: !!this.districts.find(x => x.id === res.districtId) ? this.districts.find(x => x.id === res.districtId).text : null },
            wardId: { id: res.wardId, text: !!this.wards.find(x => x.id === res.wardId) ? this.wards.find(x => x.id === res.wardId).text : null },
            streetAddress: res.streetAddress,
            addressType: this.getTypeAddress(res.addressType),
            contactPerson: res.contactPerson,
            tel: res.tel,
        });

    }
    getTypeAddress(type: any) {
        const listType = type.split(";");
        const addressTypelst: any = [];
        listType.forEach(item => {
            item = item.trim();
            if (item !== undefined) {
                addressTypelst.push(item);
            }
        });
        return addressTypelst;
    }
    assignValueModel() {
        if (this.isUpdate) {
            this.selectedAddress.id = this.id;
        }
        this.selectedAddress.partnerId = this.partnerId;
        this.selectedAddress.accountNo = this.accountNo.value;
        this.selectedAddress.shortName = this.shortName.value;
        this.selectedAddress.taxCode = this.taxCode.value;
        this.selectedAddress.shortNameAddress = this.shortNameAddress.value;    
        this.selectedAddress.countryId = this.countryId.value.id;
        this.selectedAddress.cityId = this.cityId.value.id;
        this.selectedAddress.districtId = this.districtId.value.id;
        this.selectedAddress.wardId = this.wardId.value.id;
        this.selectedAddress.streetAddress = this.streetAddress.value;
        // this.selectedAddress.addressType = this.addressType.value;
        this.selectedAddress.contactPerson = this.contactPerson.value;
        this.selectedAddress.tel = this.tel.value;
        if (this.addressType.value.includes('All')) {
            this.selectedAddress.addressType = this.mapAddressType();
        } else {
            this.selectedAddress.addressType = this.addressType.value.toString().replace(/(?:,)/g, '; ');
        }


    }
    onSubmit() {
        this.isSubmitted = true;
        this.selectedAddress.index = this.indexDetailAddress;
        if (!this.formAddress.valid) {
            return;
        }
        this.assignValueModel();
        if (!this.isUpdate) {
            this._catalogueRepo.addAddress(this.selectedAddress)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.status) {
                            this._toastService.success('New data added');
                            this.onRequest.emit(true);
                            this.hide();
                        } else {
                            this._toastService.error("Opps", "Something getting error!");
                        }
                    }
                );
        } else {
            this._catalogueRepo.updateAddress(this.selectedAddress)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.onRequest.emit(true);
                            this.hide();

                        } else {
                            this._toastService.error("Opps", "Something getting error!");
                        }
                    }
                );


        }
    }

    selectedAddressType($event: any) {
        if ($event.length > 0) {
            if ($event[$event.length - 1] === 'All') {
                this.addressType.setValue(['All']);
            } else {
                const arrNotIncludeAll = $event.filter(x => x !== 'All'); //
                this.addressType.setValue(arrNotIncludeAll);
            }
        }
    }
    mapAddressType() {
        let type = '';
        const address = this.listAddressType.filter(x => x !== 'All');
        type = address.toString().replace(/(?:,)/g, '; ');
        return type;
    }
    onHandleResult(res: CommonInterface.IResult) {
        if (res.status) {
            this._toastService.success(res.message);
            this.hide();
            this.onRequest.emit(true);
        } else {
            this._toastService.error(res.message);
        }
    }
    onCancel() {
        this.hide();
    }
    onChange(event: any, type: string) {
        switch (type) {
            case 'country':
                this.cityId.setValue(null);
                this.districtId.setValue(null);
                this.wardId.setValue(null);
                if (!!event) {
                    this.getProvinces(event.id);
                } else {
                    this.formAddress.controls['cityId'].setValue(null);
                }
                break;
            case 'province':
                this.districtId.setValue(null);
                this.wardId.setValue(null);
                if (!!event) {
                    this.getDistricts(event.id);
                } else {
                    this.formAddress.controls['districtId'].setValue(null);
                }
                break;
            case 'district':
                this.wardId.setValue(null);
                if (!!event) {
                    this.getWards(event.id);
                } else {
                    this.formAddress.controls['wardId '].setValue(null);
                }
                break;
        }
    }

    getProvinces(countryId?: number) {
        if (countryId) {
            this._catalogueRepo.getProvincesBycountry(countryId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.provinces = this.utility.prepareNg2SelectData(res || [], 'id', 'nameVn');
                    }
                );
        } else {
            this._catalogueRepo.getProvinces()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.provinces = this.utility.prepareNg2SelectData(res || [], 'id', 'nameVn');
                    }
                );
        }
    }
    getDistricts(provinceId?: any) {
        if (provinceId) {
            this._catalogueRepo.getDistrictsByProvince(provinceId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.districts = this.utility.prepareNg2SelectData(res || [], 'id', 'nameVn');
                    }
                );
        } else {
            this._catalogueRepo.getDistricts()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.districts = this.utility.prepareNg2SelectData(res || [], 'id', 'nameVn');
                    }
                );
        }
    }
    getWards(districtId?: any) {
        if (districtId) {
            this._catalogueRepo.getWardByDistrict(districtId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.wards = this.utility.prepareNg2SelectData(res || [], 'id', 'nameVn');
                    }
                );
        } else {
            this._catalogueRepo.getWards()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.wards = this.utility.prepareNg2SelectData(res || [], 'id', 'nameVn');
                    }
                );
        }
    }

    close() {
        this.hide();
    }
}


