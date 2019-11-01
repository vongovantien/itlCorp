import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-add-province',
    templateUrl: './add-province.component.html'
})
export class AddProvinceComponent extends PopupBase implements OnInit {
    @Output() isSaveSuccess = new EventEmitter<boolean>();
    formProvince: FormGroup;
    provinceCityToAdd = new CatPlaceModel();
    ngSelectDataCountries: any = [];
    isSubmitted = false;

    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    country: AbstractControl;

    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        protected _progressService: NgProgress,
        private _toastService: ToastrService) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
    }
    initForm() {
        this.formProvince = this._fb.group({
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
        this.code = this.formProvince.controls['code'];
        this.nameEn = this.formProvince.controls['nameEn'];
        this.nameVn = this.formProvince.controls['nameVn'];
        this.country = this.formProvince.controls['country'];
    }
    saveProvince() {
        this.isSubmitted = true;
        if (this.formProvince.valid) {
            this.provinceCityToAdd.placeType = PlaceTypeEnum.Province;
            this.provinceCityToAdd.code = this.code.value;
            this.provinceCityToAdd.nameEn = this.nameEn.value;
            this.provinceCityToAdd.nameVn = this.nameVn.value;
            this.provinceCityToAdd.countryId = this.country.value[0].id;
            this._catalogueRepo.addPlace(this.provinceCityToAdd)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => {
                        this._progressRef.complete();
                    })
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                            this.isSubmitted = false;
                            this.initForm();
                            this.isSaveSuccess.emit(true);
                            this.hide();
                        } else {
                            this._toastService.error(res.message, '');
                        }
                    }
                );
        }
    }
    cancelAdd() {
        this.hide();
        this.isSubmitted = false;
        this.formProvince.reset();
    }
    refreshValue(event) { }
    typed(event) { }
    removed(event) { }
    selectedCountry(event, s: string, k: string) { }
}
