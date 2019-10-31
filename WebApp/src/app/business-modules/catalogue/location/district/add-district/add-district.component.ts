import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-add-district',
    templateUrl: './add-district.component.html'
})
export class AddDistrictComponent extends PopupBase implements OnInit {
    @Output() isSaveSuccess = new EventEmitter<boolean>();
    formAddDistrict: FormGroup;
    districtToAdd = new CatPlaceModel();
    isSubmitted: boolean = false;
    ngSelectDataCountries: any[] = [];
    ngSelectDataProvinces: any[] = [];
    provinces: any[] = [];
    value: any = {};
    resetNg2SelectProvince: boolean = true;

    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    country: AbstractControl;

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
        this.formAddDistrict = this._fb.group({
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
        this.code = this.formAddDistrict.controls['code'];
        this.nameEn = this.formAddDistrict.controls['nameEn'];
        this.nameVn = this.formAddDistrict.controls['nameVn'];
        this.country = this.formAddDistrict.controls['country'];
    }

    cancelAdd() {
        this.hide();
        this.isSubmitted = false;
        this.formAddDistrict.reset();
        this.ngSelectDataProvinces = [];
        this.resetNgSelect();
    }
    public refreshValue(value: any): void {
        this.value = value;
    }
    selected(value: { id: any, text: any }, type: string) {
        if (type === 'country') {
            this.districtToAdd.countryId = value.id;
            this.districtToAdd.provinceId = null;
            this.getProvinceByCountry(value.id);
            this.resetNgSelect();
        }
        if (type === 'province') {
            this.districtToAdd.provinceId = value.id;
        }
    }
    getProvinceByCountry(id: any) {
        this.ngSelectDataProvinces = this.provinces.length === 0 ? [] : this.ngSelectData(this.provinces, id);
    }
    removed(value: { id: any, text: any }, type: string) {
        if (type === 'country') {
            this.districtToAdd.countryId = null;
            this.districtToAdd.provinceId = null;
            this.ngSelectDataProvinces = [];
            this.resetNgSelect();
        }
        if (type === 'province') {
            this.districtToAdd.provinceId = null;
        }
    }
    ngSelectData(sourceData: any[], id: any) {
        const data = sourceData.filter(x => x.countryID === id);
        if (data === null) { return []; }
        return data.map(x => ({ "text": x.code + " - " + x.nameEn, "id": x.id }));
    }
    resetNgSelect() {
        this.resetNg2SelectProvince = false;
        setTimeout(() => {
            this.resetNg2SelectProvince = true;
        }, 300);
    }
    saveProvince() {
        this.isSubmitted = true;
        if (this.formAddDistrict.valid && this.districtToAdd.countryId != null && this.districtToAdd.provinceId != null) {
            this.districtToAdd.placeType = PlaceTypeEnum.District;
            this.districtToAdd.code = this.code.value;
            this.districtToAdd.nameEn = this.nameEn.value;
            this.districtToAdd.nameVn = this.nameVn.value;
            this.districtToAdd.countryId = this.country.value[0].id;
            this._catalogueRepo.addPlace(this.districtToAdd)
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
                            this.resetNgSelect();
                            this.hide();
                        } else {
                            this.toastService.error(res.message, '');
                        }
                    }
                );
        }
    }
}
