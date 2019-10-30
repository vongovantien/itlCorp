import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-update-province',
    templateUrl: './update-province.component.html'
})
export class UpdateProvinceComponent extends PopupBase implements OnInit {
    @Output() isSaveSuccess = new EventEmitter<boolean>();
    formProvince: FormGroup;
    provinceCityToUpdate = new CatPlaceModel();
    resetNg2SelectCountry = true;
    ngSelectDataCountries: any = [];
    isSubmitted = false;
    currentActiveCountry: any[] = null;
    currentId: string;

    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    country: AbstractControl;
    active: AbstractControl;

    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService,
        protected _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
    }

    initForm() {
        this.formProvince = this._fb.group({
            code: [{ value: '', disabled: true }, Validators.compose([
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
        this.code = this.formProvince.controls['code'];
        this.nameEn = this.formProvince.controls['nameEn'];
        this.nameVn = this.formProvince.controls['nameVn'];
        this.active = this.formProvince.controls['active'];
    }
    setValueFormGroup(res: any) {
        this.formProvince.setValue({
            code: res.code,
            nameEn: res.nameEn,
            nameVn: res.nameVn,
            active: res.active
        });
    }
    saveProvince() {
        this.isSubmitted = true;
        if (this.formProvince.valid && this.provinceCityToUpdate.countryId != null) {
            this.provinceCityToUpdate.nameEn = this.nameEn.value;
            this.provinceCityToUpdate.nameVn = this.nameVn.value;
            this.provinceCityToUpdate.active = this.active.value;
            this._catalogueRepo.updatePlace(this.currentId, this.provinceCityToUpdate)
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
    removed(event) {
        this.provinceCityToUpdate.countryId = null;
    }
    selected(event) {
        this.provinceCityToUpdate.countryId = event.id;

    }
}
