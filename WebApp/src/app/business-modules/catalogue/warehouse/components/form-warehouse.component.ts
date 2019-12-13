import { Component, OnInit, Output, EventEmitter } from '@angular/core';

import { Warehouse } from 'src/app/shared/models/catalogue/ware-house.model';
import { PopupBase } from 'src/app/popup.base';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'form-warehouse',
    templateUrl: './form-warehouse.component.html'
})
export class FormWarehouseComponent extends PopupBase implements OnInit {
    @Output() saveSuccess: EventEmitter<boolean> = new EventEmitter<boolean>();
    warehouse: Warehouse = new Warehouse();
    warehouseForm: FormGroup;
    title: string;
    countries: any[] = [];
    provinces: any[] = [];
    districts: any[] = [];

    code: AbstractControl;
    warehouseNameEN: AbstractControl;
    warehouseNameVN: AbstractControl;
    country: AbstractControl;
    province: AbstractControl;
    district: AbstractControl;
    address: AbstractControl;
    active: AbstractControl;

    isSubmitted: boolean = false;
    isUpdate: boolean = false;

    constructor(private _fb: FormBuilder,
        private _toastService: ToastrService,
        private _catalogueRepo: CatalogueRepo,
        private _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
    }

    initForm() {
        this.warehouseForm = this._fb.group({
            code: [{ value: null, disabled: true }, Validators.required],
            warehouseNameEN: [null, Validators.required],
            warehouseNameVN: [null, Validators.required],
            country: [null, Validators.required],
            province: [null, Validators.required],
            district: [null, Validators.required],
            address: [null, Validators.required],
            active: [true]
        });
        this.code = this.warehouseForm.controls['code'];
        this.warehouseNameEN = this.warehouseForm.controls['warehouseNameEN'];
        this.warehouseNameVN = this.warehouseForm.controls['warehouseNameVN'];
        this.country = this.warehouseForm.controls['country'];
        this.province = this.warehouseForm.controls['province'];
        this.district = this.warehouseForm.controls['district'];
        this.address = this.warehouseForm.controls['address'];
        this.active = this.warehouseForm.controls['active'];
    }

    setFormValue(res: any) {
        let countryActive = [];
        let provinceActive = [];
        let districtActive = [];
        const indexcountry = this.countries.findIndex(x => x.id === res.countryID);
        const indexProvince = this.provinces.findIndex(x => x.id === res.provinceID);
        const indexDistrict = this.districts.findIndex(x => x.id === res.districtID);

        if (indexcountry > -1) {
            countryActive = [this.countries[indexcountry]];
        }
        if (indexProvince > -1) {
            provinceActive = [this.provinces[indexProvince]];
        }
        if (indexDistrict > -1) {
            districtActive = [this.districts[indexDistrict]];
        }
        this.warehouseForm.setValue({
            code: res.code,
            warehouseNameEN: res.nameEn,
            warehouseNameVN: res.nameVn,
            country: countryActive || [],
            province: provinceActive || [],
            district: districtActive || [],
            address: res.address,
            active: res.active
        });
    }

    onSubmit() {
        this.isSubmitted = true;
        const formData = this.warehouseForm.getRawValue();
        if (this.warehouseForm.valid) {
            this.warehouse.placeType = PlaceTypeEnum.Warehouse;
            this.warehouse.code = formData.code;
            this.warehouse.nameEn = formData.warehouseNameEN;
            this.warehouse.nameVn = formData.warehouseNameVN;
            this.warehouse.countryID = formData.country[0].id;
            this.warehouse.provinceID = formData.province[0].id;
            this.warehouse.districtID = formData.district[0].id;
            this.warehouse.address = formData.address;
            this.warehouse.active = !!this.isUpdate ? formData.active : true;
            if (this.isUpdate) {
                this._catalogueRepo.updatePlace(this.warehouse.id, this.warehouse)
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            this.onHandleResult(res);
                        }
                    );
            } else {
                this._catalogueRepo.addPlace(this.warehouse)
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            this.onHandleResult(res);
                        }
                    );
            }
        } else {
            console.log(this.warehouseForm);
        }
    }
    onHandleResult(res: CommonInterface.IResult) {
        if (res.status) {
            this._toastService.success(res.message);
            this.hide();
            this.saveSuccess.emit(true);
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
                this.getProvinces(event.id);
                break;
            case 'province':
                this.getDistricts(event.id);
                break;
        }
    }
    removed(type: string) {
        switch (type) {
            case 'country':
                this.getProvinces();
                this.warehouseForm.controls['province'].setValue([]);
                this.warehouseForm.controls['district'].setValue([]);
                break;
            case 'province':
                this.getDistricts();
                this.warehouseForm.controls['district'].setValue([]);
                break;
        }
    }

    getProvinces(countryId?: number) {
        if (countryId) {
            this._catalogueRepo.getProvincesBycountry(countryId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.provinces = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                    }
                );
        } else {
            this._catalogueRepo.getProvinces()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.provinces = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
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
                        this.districts = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                    }
                );
        } else {
            this._catalogueRepo.getDistricts()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.districts = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                    }
                );
        }
    }
}
