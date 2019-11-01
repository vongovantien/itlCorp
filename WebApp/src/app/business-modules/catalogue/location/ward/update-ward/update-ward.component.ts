import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'app-update-ward',
    templateUrl: './update-ward.component.html'
})
export class UpdateWardComponent extends PopupBase implements OnInit {
    @Output() isSaveSuccess = new EventEmitter<boolean>();
    wardToUpdate: CatPlaceModel;
    formEditWard: FormGroup;
    isSubmitted = false;
    currentId: string;

    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    active: AbstractControl;

    ngSelectDataCountries: any[] = [];
    ngSelectDataProvinces: any[] = [];
    ngSelectDataDistricts: any[] = [];
    resetNg2SelectProvince: boolean = true;
    resetNg2SelectDistrict: boolean = true;
    provinces: any[] = [];
    districts: any[] = [];
    currentActiveCountry: any[] = null;
    currentActiveProvince: any[] = null;
    currentActiveDistrict: any[] = null;

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
    saveWard() {
        this.isSubmitted = true;
        if (this.formEditWard.valid && this.wardToUpdate.countryId != null
            && this.wardToUpdate.provinceId != null
            && this.wardToUpdate.districtId) {
            this.wardToUpdate.code = this.code.value;
            this.wardToUpdate.nameEn = this.nameEn.value;
            this.wardToUpdate.nameVn = this.nameVn.value;
            this._catalogueRepo.addPlace(this.wardToUpdate)
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
    initForm() {
        this.formEditWard = this._fb.group({
            code: ['', Validators.compose([
                Validators.required
            ])],
            nameEn: ['', Validators.compose([
                Validators.required
            ])],
            nameVn: ['', Validators.compose([
                Validators.required
            ])],
            active: [false]
        });
        this.code = this.formEditWard.controls['code'];
        this.nameEn = this.formEditWard.controls['nameEn'];
        this.nameVn = this.formEditWard.controls['nameVn'];
        this.active = this.formEditWard.controls['active'];
    }
    setValueFormGroup(res: any) {
        this.formEditWard.setValue({
            code: res.code,
            nameEn: res.nameEn,
            nameVn: res.nameVn,
            active: res.active
        });
    }
    resetNgSelect(selectName: string) {
        if (selectName === 'province') {
            this.resetNg2SelectProvince = false;
            setTimeout(() => {
                this.resetNg2SelectProvince = true;
            }, 100);
        }
        if (selectName === 'district') {
            this.resetNg2SelectDistrict = false;
            setTimeout(() => {
                this.resetNg2SelectDistrict = true;
            }, 100);
        } else {
            this.resetNg2SelectProvince = false;
            this.resetNg2SelectDistrict = false;
            setTimeout(() => {
                this.resetNg2SelectProvince = true;
                this.resetNg2SelectDistrict = true;
            }, 100);
        }
    }
    selected(value: { id: any, text: any }, type: string) {
        if (type === 'country') {
            this.wardToUpdate.countryId = value.id;
            this.wardToUpdate.provinceId = null;
            this.wardToUpdate.districtId = null;
            this.getProvinceByCountry(value.id);
            this.resetNgSelect('province');
        }
        if (type === 'province') {
            this.wardToUpdate.provinceId = value.id;
            this.wardToUpdate.districtId = null;
            this.getWardByProvince(value.id);
            this.resetNgSelect('district');
        }
        if (type === 'district') {
            this.wardToUpdate.districtId = value.id;
        }
    }
    refreshValue(event) { }
    removed(value: { id: any, text: any }, type: string) {
        if (type === 'country') {
            this.wardToUpdate.countryId = null;
            this.wardToUpdate.provinceId = null;
            this.wardToUpdate.districtId = null;
            this.ngSelectDataProvinces = [];
            this.ngSelectDataDistricts = [];
            this.currentActiveProvince = [];
            this.currentActiveDistrict = [];
            this.resetNgSelect('province');
        }
        if (type === 'province') {
            this.wardToUpdate.provinceId = null;
            this.wardToUpdate.districtId = null;
            this.ngSelectDataDistricts = [];
            this.currentActiveDistrict = [];
            this.resetNgSelect('district');
        }
        if (type === 'district') {
            this.wardToUpdate.districtId = null;
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
    cancelAdd() {
        this.hide();
        this.isSubmitted = false;
        this.formEditWard.reset();
        this.ngSelectDataProvinces = [];
        this.ngSelectDataDistricts = [];
        this.resetNgSelect('');
    }
}
