import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-add-country',
    templateUrl: './add-country.component.html'
})
export class AddCountryComponent extends PopupBase implements OnInit {
    @Output() isSaveSuccess = new EventEmitter<boolean>();
    formAddCountry: FormGroup;
    countryToAdd = new CountryModel();
    isSubmitted = false;
    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;

    constructor(private _fb: FormBuilder,
        private catalogueRepo: CatalogueRepo,
        private toastService: ToastrService,
        protected _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
    }
    initForm() {
        this.formAddCountry = this._fb.group({
            code: ['', Validators.compose([
                Validators.required
            ])],
            nameEn: ['', Validators.compose([
                Validators.required
            ])],
            nameVn: ['', Validators.compose([
                Validators.required
            ])]
        });
        this.code = this.formAddCountry.controls['code'];
        this.nameEn = this.formAddCountry.controls['nameEn'];
        this.nameVn = this.formAddCountry.controls['nameVn'];
    }
    cancelAdd() {
        this.hide();
        this.isSubmitted = false;
        this.formAddCountry.reset();
    }
    saveCountry() {
        this.isSubmitted = true;
        if (this.formAddCountry.valid) {
            this.countryToAdd.code = this.code.value;
            this.countryToAdd.nameEn = this.nameEn.value;
            this.countryToAdd.nameVn = this.nameVn.value;
            this.catalogueRepo.addCountry(this.countryToAdd)
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
                            this.hide();
                        } else {
                            this.toastService.error(res.message, '');
                        }
                    }
                );
        }
    }
}
