import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
  selector: 'app-update-country',
  templateUrl: './update-country.component.html'
})
export class UpdateCountryComponent extends PopupBase implements OnInit {
  @Output() isSaveSuccess = new EventEmitter<boolean>();
  countryToUpdate: CountryModel;
  formUpdateCountry: FormGroup;
  isSubmitted = false;
  code: AbstractControl;
  nameEn: AbstractControl;
  nameVn: AbstractControl;
  active: AbstractControl;
  currentId: number;

  constructor(private _fb: FormBuilder,
    private catalogueRepo: CatalogueRepo,
    private toastService: ToastrService,
    private _progressService: NgProgress) {
    super();
    this._progressRef = this._progressService.ref();
  }

  ngOnInit() {
    this.initForm();
  }

  initForm() {
    this.formUpdateCountry = this._fb.group({
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
    this.code = this.formUpdateCountry.controls['code'];
    this.nameEn = this.formUpdateCountry.controls['nameEn'];
    this.nameVn = this.formUpdateCountry.controls['nameVn'];
    this.active = this.formUpdateCountry.controls['active'];
  }
  setValueFormGroup(res: any) {
    this.formUpdateCountry.setValue({
      code: res.code,
      nameEn: res.nameEn,
      nameVn: res.nameVn,
      active: res.active
    });
  }
  cancelAdd() {
    this.hide();
    this.isSubmitted = false;
    this.formUpdateCountry.reset();
  }
  saveCountry() {
    this.isSubmitted = true;
    if (this.formUpdateCountry.valid) {
      this.countryToUpdate.code = this.code.value;
      this.countryToUpdate.nameEn = this.nameEn.value;
      this.countryToUpdate.nameVn = this.nameVn.value;
      this.countryToUpdate.active = this.active.value;
      this.catalogueRepo.updateCountry(this.countryToUpdate)
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
