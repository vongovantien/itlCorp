import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { AbstractControl, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-update-district',
    templateUrl: './update-district.component.html'
})
export class UpdateDistrictComponent extends PopupBase implements OnInit {
    @Output() isSaveSuccess = new EventEmitter<boolean>();
    formUpdateDistrict: FormGroup;
    districtToUpdate: CatPlaceModel;
    currentId: any;
    currentActiveCountry: any[] = null;
    currentActiveProvince: any[] = null;
    ngSelectDataCountries: any[] = [];
    ngSelectDataProvinces: any[] = [];
    value: any = {};
    provinces: any[] = [];
    resetNg2SelectProvince: boolean = true;
    isSubmitted: boolean = false;

    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    active: AbstractControl;

    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private toastService: ToastrService,
        private _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
    }

    initForm() {
        this.formUpdateDistrict = this._fb.group({
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
        this.code = this.formUpdateDistrict.controls['code'];
        this.nameEn = this.formUpdateDistrict.controls['nameEn'];
        this.nameVn = this.formUpdateDistrict.controls['nameVn'];
        this.active = this.formUpdateDistrict.controls['active'];
    }
    setValueFormGroup(res: any) {
        this.formUpdateDistrict.setValue({
            code: res.code,
            nameEn: res.nameEn,
            nameVn: res.nameVn,
            active: res.active
        });
    }
    getProvinceByCountry(id: any) {
        this.ngSelectDataProvinces = this.provinces.length === 0 ? [] : this.ngSelectData(this.provinces, id);
    }
    public refreshValue(value: any): void {
        this.value = value;
    }
    selected(value: { id: any, text: any }, type: string) {
        if (type === 'country') {
            this.districtToUpdate.countryId = value.id;
            this.districtToUpdate.provinceId = null;
            this.currentActiveProvince = [];
            this.getProvinceByCountry(value.id);
            this.resetNgSelect();
        }
        if (type === 'province') {
            this.districtToUpdate.provinceId = value.id;
        }
    }
    removed(value: { id: any, text: any }, type: string) {
        if (type === 'country') {
            this.districtToUpdate.countryId = null;
            this.districtToUpdate.provinceId = null;
            this.ngSelectDataProvinces = [];
            this.currentActiveProvince = [];
            this.resetNgSelect();
        }
        if (type === 'province') {
            this.districtToUpdate.provinceId = null;
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
        }, 10);
    }
    cancelAdd() {
        this.hide();
        this.isSubmitted = false;
        this.formUpdateDistrict.reset();
        this.ngSelectDataProvinces = [];
        this.resetNgSelect();
    }
    updateDistrict() {
        this.isSubmitted = true;
        if (this.formUpdateDistrict.valid && this.districtToUpdate.countryId != null && this.districtToUpdate.provinceId != null) {
            this.districtToUpdate.placeType = PlaceTypeEnum.District;
            this.districtToUpdate.code = this.code.value;
            this.districtToUpdate.nameEn = this.nameEn.value;
            this.districtToUpdate.nameVn = this.nameVn.value;
            this.districtToUpdate.active = this.active.value;
            this._catalogueRepo.updatePlace(this.currentId, this.districtToUpdate)
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
