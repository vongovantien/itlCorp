import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, FormControl, FormBuilder } from '@angular/forms';
import { ChargeConstants } from '@constants';

@Component({
    selector: 'form-incoterm',
    templateUrl: './form-incoterm.component.html',
})
export class CommercialFormIncotermComponent extends AppForm implements OnInit {

    formGroup: FormGroup;
    code: FormControl;
    service: FormControl;
    nameEn: FormControl;
    nameLocal: FormControl;
    active: FormControl;
    descriptionEn: FormControl;
    descriptionLocal: FormControl;

    services: CommonInterface.INg2Select[] = [
        { text: ChargeConstants.IT_DES, id: ChargeConstants.IT_CODE },
        { text: ChargeConstants.AI_DES, id: ChargeConstants.AI_CODE },
        { text: ChargeConstants.AE_DES, id: ChargeConstants.AE_CODE },
        { text: ChargeConstants.SFE_DES, id: ChargeConstants.SFE_CODE },
        { text: ChargeConstants.SFI_DES, id: ChargeConstants.SFI_CODE },
        { text: ChargeConstants.SLE_DES, id: ChargeConstants.SLE_CODE },
        { text: ChargeConstants.SLI_DES, id: ChargeConstants.SLI_CODE },
        { text: ChargeConstants.SCE_DES, id: ChargeConstants.SCE_CODE },
        { text: ChargeConstants.SCI_DES, id: ChargeConstants.SCI_CODE },
        { text: ChargeConstants.CL_DES, id: ChargeConstants.CL_CODE }
    ]
    constructor(
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit(): void {
        this.initForm();
    }

    initForm() {
        this.formGroup = this._fb.group({
            code: [],
            service: [],
            nameEn: [],
            nameLocal: [],
            active: [],
            descriptionEn: [],
            descriptionLocal: [],
        });

        this.active = new FormControl();
    }
}
