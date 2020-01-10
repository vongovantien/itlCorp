import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';

@Component({
    selector: 'app-form-port-index',
    templateUrl: './form-port-index.component.html'
})
export class FormPortIndexComponent extends PopupBase implements OnInit {
    @Output() onChange: EventEmitter<boolean> = new EventEmitter<boolean>();
    portindexForm: FormGroup;
    title: string = '';
    countries: any[] = [];
    areas: any[] = [];
    modes: any[] = [];
    isSubmitted: boolean = false;
    isUpdate: boolean = false;
    portIndex: PortIndex = new PortIndex();

    code: AbstractControl;
    portIndexeNameEN: AbstractControl;
    portIndexeNameLocal: AbstractControl;
    country: AbstractControl;
    zone: AbstractControl;
    mode: AbstractControl;
    active: AbstractControl;

    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService) {
        super();
    }

    ngOnInit() {
        this.initForm();
    }

    initForm() {
        this.portindexForm = this._fb.group({
            code: [null, Validators.required],
            portIndexeNameEN: [null, Validators.required],
            portIndexeNameLocal: [null, Validators.required],
            country: [null, Validators.required],
            zone: [null, Validators.required],
            mode: [null, Validators.required],
            active: [true]
        });

        this.code = this.portindexForm.controls['code'];
        this.portIndexeNameEN = this.portindexForm.controls['portIndexeNameEN'];
        this.portIndexeNameLocal = this.portindexForm.controls['portIndexeNameLocal'];
        this.country = this.portindexForm.controls['country'];
        this.zone = this.portindexForm.controls['zone'];
        this.mode = this.portindexForm.controls['mode'];
        this.active = this.portindexForm.controls['active'];
    }

    onSubmit() {
        this.isSubmitted = true;

        // Trick to remove validate ng-select
        this.setError(this.country);
        this.setError(this.zone);
        this.setError(this.mode);

        const formData = this.portindexForm.getRawValue();
        if (this.portindexForm.valid) {

            this.portIndex.placeType = PlaceTypeEnum.Port;
            this.portIndex.active = !!this.isUpdate ? formData.active : true;

            this.portIndex.code = !!formData.code && !!formData.code.length ? formData.code : null;
            this.portIndex.nameEn = !!formData.portIndexeNameEN && !!formData.portIndexeNameEN.length ? formData.portIndexeNameEN : null;
            this.portIndex.nameVn = !!formData.portIndexeNameEN && !!formData.portIndexeNameEN.length ? formData.portIndexeNameEN : null;
            this.portIndex.countryID = !!formData.country && !!formData.country.length ? formData.country[0].id : null;
            this.portIndex.modeOfTransport = !!formData.mode && !!formData.mode.length ? formData.mode[0].id : null;
            this.portIndex.areaID = !!formData.zone && !!formData.zone.length ? formData.zone[0].id : null;

            if (this.isUpdate) {
                this._catalogueRepo.updatePlace(this.portIndex.id, this.portIndex)
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            this.onHandleResult(res);
                        }
                    );
            } else {
                this._catalogueRepo.addPlace(this.portIndex)
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            this.onHandleResult(res);
                        }
                    );
            }
        }
    }

    onHandleResult(res: CommonInterface.IResult) {
        if (res.status) {
            this._toastService.success(res.message);
            this.hide();
            this.onChange.emit(true);
        } else {
            this._toastService.error(res.message);
        }
    }

    onCancel() {
        this.hide();
    }

    setFormValue(res: any) {
        this.portindexForm.setValue({
            code: res.code,
            portIndexeNameEN: res.nameEn,
            portIndexeNameLocal: res.nameVn,
            country: [this.countries.find(x => x.id === res.countryId)] || [],
            zone: [this.areas.find(x => x.id === res.areaId)] || [],
            mode: [this.modes.find(x => x.id === res.modeOfTransport)] || [],
            active: res.active
        });
    }
}
