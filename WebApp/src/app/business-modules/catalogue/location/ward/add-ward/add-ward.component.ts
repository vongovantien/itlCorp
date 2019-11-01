import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { PopupBase } from 'src/app/popup.base';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'app-add-ward',
    templateUrl: './add-ward.component.html'
})
export class AddWardComponent extends PopupBase implements OnInit {
    @Output() isSaveSuccess = new EventEmitter<boolean>();
    wardToAdd = new CatPlaceModel();
    formAddWard: FormGroup;
    isSubmitted = false;

    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    country: AbstractControl;

    ngSelectDataCountries: any[] = [];
    ngSelectDataProvinces: any[] = [];
    ngSelectDataDistricts: any[] = [];
    resetNg2SelectProvince: boolean = true;
    resetNg2SelectDistrict: boolean = true;
    provinces: any[] = [];
    districts: any[] = [];

    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        protected _progressService: NgProgress,
        private toastService: ToastrService) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
    }
    initForm() {
        this.formAddWard = this._fb.group({
            code: ['', Validators.compose([
                Validators.required
            ])],
            nameEn: ['', Validators.compose([
                Validators.required
            ])],
            nameVn: ['', Validators.compose([
                Validators.required
            ])],
            country: ['', Validators.compose([
                Validators.required
            ])]
        });
        this.code = this.formAddWard.controls['code'];
        this.nameEn = this.formAddWard.controls['nameEn'];
        this.nameVn = this.formAddWard.controls['nameVn'];
        this.country = this.formAddWard.controls['country'];
    }
    refreshValue(event) { }
    typed(event) { }
    removed(value: { id: any, text: any }, type: string) {
        if (type === 'country') {
            this.wardToAdd.countryId = null;
            this.wardToAdd.provinceId = null;
            this.wardToAdd.districtId = null;
            this.ngSelectDataProvinces = [];
            this.ngSelectDataDistricts = [];
            this.resetNgSelect('province');
        }
        if (type === 'province') {
            this.wardToAdd.provinceId = null;
            this.wardToAdd.districtId = null;
            this.ngSelectDataDistricts = [];
            this.resetNgSelect('district');
        }
        if (type === 'district') {
            this.wardToAdd.districtId = null;
        }
    }
    selected(value: { id: any, text: any }, type: string) {
        if (type === 'country') {
            this.wardToAdd.countryId = value.id;
            this.wardToAdd.provinceId = null;
            this.wardToAdd.districtId = null;
            this.getProvinceByCountry(value.id);
            this.resetNgSelect('province');
        }
        if (type === 'province') {
            this.wardToAdd.provinceId = value.id;
            this.wardToAdd.districtId = null;
            this.getWardByProvince(value.id);
            this.resetNgSelect('district');
        }
        if (type === 'district') {
            this.wardToAdd.districtId = value.id;
        }
    }
    resetNgSelect(selectName: string) {
        if (selectName === 'province') {
            this.resetNg2SelectProvince = false;
            setTimeout(() => {
                this.resetNg2SelectProvince = true;
            }, 10);
        }
        if (selectName === 'district') {
            this.resetNg2SelectDistrict = false;
            setTimeout(() => {
                this.resetNg2SelectDistrict = true;
            }, 10);
        } else {
            this.resetNg2SelectProvince = false;
            this.resetNg2SelectDistrict = false;
            setTimeout(() => {
                this.resetNg2SelectProvince = true;
                this.resetNg2SelectDistrict = true;
            }, 10);
        }
    }
    getProvinceByCountry(id: any) {
        if (this.provinces.length === 0) {
            this.ngSelectDataProvinces = [];
        } else {
            const data = this.provinces.filter(x => x.countryID === id);
            this.ngSelectDataProvinces = this.ngSelectData(data, id);
        }
    }
    getWardByProvince(id: any) {
        if (this.districts.length === 0) {
            this.ngSelectDataProvinces = [];
        } else {
            const data = this.districts.filter(x => x.provinceID === id);
            this.ngSelectDataDistricts = this.ngSelectData(data, id);
        }
    }
    ngSelectData(sourceData: any[], id: any) {
        if (sourceData === null) { return []; }
        return sourceData.map(x => ({ "text": x.code + " - " + x.nameEn, "id": x.id }));
    }
    saveWard() {
        this.isSubmitted = true;
        if (this.formAddWard.valid && this.wardToAdd.countryId != null
            && this.wardToAdd.provinceId != null
            && this.wardToAdd.districtId) {
            this.wardToAdd.placeType = PlaceTypeEnum.Ward;
            this.wardToAdd.code = this.code.value;
            this.wardToAdd.nameEn = this.nameEn.value;
            this.wardToAdd.nameVn = this.nameVn.value;
            this._catalogueRepo.addPlace(this.wardToAdd)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => {
                        this._progressRef.complete();
                    })
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this.toastService.success(res.message, '');
                            this.isSubmitted = false;
                            this.initForm();
                            this.isSaveSuccess.emit(true);
                            this.ngSelectDataProvinces = [];
                            this.ngSelectDataDistricts = [];
                            this.resetNgSelect('');
                            this.hide();
                        } else {
                            this.toastService.error(res.message, '');
                        }
                    }
                );
        }
    }
    cancelAdd() {
        this.hide();
        this.isSubmitted = false;
        this.formAddWard.reset();
        this.ngSelectDataProvinces = [];
        this.ngSelectDataDistricts = [];
        this.resetNgSelect('');
    }
}
