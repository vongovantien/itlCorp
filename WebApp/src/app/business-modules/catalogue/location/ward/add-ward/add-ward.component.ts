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
export class AddWardPopupComponent extends PopupBase implements OnInit {

    @Output() onChange = new EventEmitter<boolean>();
    wardToAdd = new CatPlaceModel();

    formAddWard: FormGroup;
    code: AbstractControl;
    nameEn: AbstractControl;
    nameVn: AbstractControl;
    countryId: AbstractControl;
    id: AbstractControl;
    active: AbstractControl;
    provinceId: AbstractControl;
    districtId: AbstractControl;

    provinces: any[] = [];
    districts: any[] = [];
    countries: any[];
    initProvince: any[];

    isUpdate: boolean = false;
    isSubmitted = false;

    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        protected _progressService: NgProgress,
        private _toastService: ToastrService) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
        this.getCountry();
        this.getProvince();
        this.getDistrict();
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
            countryId: ['', Validators.compose([
                Validators.required
            ])],
            provinceId: ['', Validators.compose([
                Validators.required
            ])],
            districtId: ['', Validators.compose([
                Validators.required
            ])],
            active: [],
            id: [],
        });
        this.code = this.formAddWard.controls['code'];
        this.nameEn = this.formAddWard.controls['nameEn'];
        this.nameVn = this.formAddWard.controls['nameVn'];
        this.countryId = this.formAddWard.controls['countryId'];
    }

    getCountry() {
        this._catalogueRepo.getListAllCountry()
            .subscribe(
                (res: any) => {
                    this.countries = res;
                }
            );
    }

    getProvince() {
        this._catalogueRepo.getAllProvinces()
            .subscribe(
                (res: any) => {
                    this.provinces = this.initProvince = res;
                }
            );
    }

    getDistrict() {
        // this._catalogueRepo.getAll()
        //     .subscribe(
        //         (res: any) => {
        //             this.provinces = this.initProvince = res;
        //         }
        //     );
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
                        this.onHandleResult(res);
                    }
                );
        }
    }

    cancelAdd() {
        this.hide();
        this.isSubmitted = false;
        this.formAddWard.reset();
    }

    onHandleResult(res: CommonInterface.IResult) {
        if (res.status) {
            this.hide();

            this._toastService.success(res.message);
            this.onChange.emit(true);
        } else {
            this._toastService.error(res.message);
        }
    }
}
