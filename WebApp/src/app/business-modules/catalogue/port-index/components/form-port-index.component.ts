import { Component, OnInit, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';

@Component({
    selector: 'app-form-port-index',
    templateUrl: './form-port-index.component.html'
})
export class FormPortIndexComponent extends PopupBase implements OnInit {
    portindexForm: FormGroup;
    title: string = '';
    countries: any[] = [];
    areas: any[] = [];
    modes: any[] = [];
    isSubmitted: boolean = false;
    isUpdate: boolean = false;

    code: AbstractControl;
    portIndexeNameEN: AbstractControl;
    portIndexeNameLocal: AbstractControl;
    active: AbstractControl;


    constructor(private _fb: FormBuilder) {
        super();
    }

    ngOnInit() {
        this.portindexForm = this._fb.group({
            code: [null, Validators.required],
            portIndexeNameEN: [null, Validators.required],
            active: [true]
        });

        this.code = this.portindexForm.controls['code'];
        this.portIndexeNameEN = this.portindexForm.controls['portIndexeNameEN'];
        this.portIndexeNameLocal = this.portindexForm.controls['portIndexeNameLocal'];
        this.active = this.portindexForm.controls['active'];
    }
    onCancel() {
        this.hide();
    }
}