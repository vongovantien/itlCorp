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
        const formData = this.portindexForm.getRawValue();
        if (this.portindexForm.valid) {
            this.portIndex.placeType = PlaceTypeEnum.Port;
            this.portIndex.code = formData.code;
            this.portIndex.nameEn = formData.portIndexeNameEN;
            this.portIndex.nameVn = formData.portIndexeNameEN;
            this.portIndex.countryID = formData.country[0].id;
            this.portIndex.modeOfTransport = formData.mode[0].id;
            this.portIndex.areaID = formData.zone[0].id;
            this.portIndex.active = !!this.isUpdate ? formData.active : true;
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
        } else {
            console.log(this.portindexForm);
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
        let zoneActive = [];
        let modeActive = [];
        let countryActive = [];
        const indexZone = this.areas.findIndex(x => x.id === res.areaId);
        const indexMode = this.modes.findIndex(x => x.id === res.modeOfTransport);
        const indexCountry = this.countries.findIndex(x => x.id === res.countryId);

        if (indexZone > -1) {
            zoneActive = [this.areas[indexZone]];
        }
        if (indexMode > -1) {
            modeActive = [this.modes[indexMode]];
        }
        if (indexCountry > -1) {
            countryActive = [this.countries[indexCountry]];
        }
        this.portindexForm.setValue({
            code: res.code,
            portIndexeNameEN: res.nameEn,
            portIndexeNameLocal: res.nameVn,
            country: countryActive || [],
            zone: zoneActive || [],
            mode: modeActive || [],
            active: res.active
        });
    }
}
