import { Component, ElementRef } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';

declare var $: any;
@Component({
    selector: 'form-add-company',
    templateUrl: './form-add-company.component.html',
    styleUrls: ['./../../company-information.component.scss']
})
export class CompanyInformationFormAddComponent extends AppForm {

    formGroup: FormGroup;
    code: AbstractControl;
    bunameVn: AbstractControl;
    bunameEn: AbstractControl;
    bunameAbbr: AbstractControl;
    website: AbstractControl;
    active: AbstractControl;

    types: CommonInterface.ICommonTitleValue[] = [
        { title: 'Active', value: true },
        { title: 'Inactive', value: false },
    ];
    photoUrl: string = '';
    imgDefault: string = "assets/app/media/img/emptydata.png";

    bearer: string = `Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjhFMTI2MzEwN0VDMUE2RkUxQkIxMjZEREM5QzM5MDVGNkQ4MkIyNjQiLCJ0eXAiOiJKV1QiLCJ4NXQiOiJqaEpqRUg3QnB2NGJzU2JkeWNPUVgyMkNzbVEifQ.eyJuYmYiOjE1NzA2NzQxMDAsImV4cCI6MTU3MDcwMjkwMCwiaXNzIjoiaHR0cDovL3Rlc3QuYXBpLWVmbXMuaXRsdm4uY29tL2lkZW50aXR5c2VydmVyIiwiYXVkIjpbImh0dHA6Ly90ZXN0LmFwaS1lZm1zLml0bHZuLmNvbS9pZGVudGl0eXNlcnZlci9yZXNvdXJjZXMiLCJlZm1zX2FwaSJdLCJjbGllbnRfaWQiOiJlRk1TIiwic3ViIjoiYWRtaW4iLCJhdXRoX3RpbWUiOjE1NzA2NzQxMDAsImlkcCI6ImxvY2FsIiwiaWQiOiJhZG1pbiIsImVtYWlsIjoiYW5keS5ob2FAaXRsdm4uY29tIiwicHJlZmVycmVkX3VzZXJuYW1lIjoiYWRtaW4iLCJwaG9uZV9udW1iZXIiOiIrODQxNjY3MjYzNTM2IiwidXNlck5hbWUiOiJhZG1pbiIsImVtcGxveWVlSWQiOiJEMUZDMzNBOS1GOTM3LTRFQzAtODM0My0wMDA4M0E0MjRFOTgiLCJzY29wZSI6WyJvcGVuaWQiLCJwcm9maWxlIiwiZWZtc19hcGkiLCJvZmZsaW5lX2FjY2VzcyJdLCJhbXIiOlsiY3VzdG9tIl19.SWWA_UMZ8O02dSbTUwHuDKwd9G5KlKG09OM22zU_CLSy1ec5ZQzcEYEoo7U0k3wS92gOEz452MPjb8AMD1lSk33OypQ2Ekx4Z81WXxPhDCJ2MhmbSGhykKJBUwrw6krbgivdB3ChbrO2HQf89o05rGhpH4_xDRJAxKjgBiM27rbgtqGPOTzke9-86stZDt9xR-ih_m_Xq3_f3Cd_rOS4UaKIMt462WuYfdtqs4bq7hQg-AFrFVlJLMhT8jIeXvrh4eaIQRzeLKzQXVjO7s99OthJIu8GjGrmIR4WEz2nLp8C2gSG12BY10d7MRcdksxYQLRHgowZx0w7HXDdBKyT-g`;
    constructor(
        private _fb: FormBuilder,
        private _ele: ElementRef
    ) {
        super();
    }

    ngOnInit(): void {
        this.initForm();
        this.initImageLibary();
    }



    initForm() {
        this.formGroup = this._fb.group({
            code: [],
            bunameVn: [],
            bunameEn: [],
            bunameAbbr: [],
            website: [],
            active: [this.types[0]],
        });

        this.code = this.formGroup.controls['code'];
        this.bunameVn = this.formGroup.controls['bunameVn'];
        this.bunameEn = this.formGroup.controls['bunameEn'];
        this.bunameAbbr = this.formGroup.controls['bunameAbbr'];
        this.website = this.formGroup.controls['website'];
        this.active = this.formGroup.controls['active'];
    }

    initImageLibary() {
        $(this._ele.nativeElement)
            .find('#imgedit')
            .froalaEditor({
                requestWithCORS: true,
                language: 'vi',
                imageEditButtons: ['imageReplace'],
                imageMaxSize: 5 * 1024 * 1024,
                imageAllowedTypes: ['jpeg', 'jpg', 'png'],
                requestHeaders: {
                    // Authorization: this.bearer,
                    Module: 'Company',
                    Path: `dayladuongdanhinh`
                },
                imageUploadURL: 'http://localhost:44360/api/v1/1/SysImageUpload/image',
                imageManagerLoadURL: 'http://localhost:44360/api/v1/1/SysImageUpload/company',
            });
    }

    inputFile() {
        $(this._ele.nativeElement).find('#imgedit').froalaEditor({

        }).on('froalaEditor.contentChanged', async (e) => {
            console.log(e);
        }).on('froalaEditor.image.error', (e, editor, error, response) => {
            console.log(e);
            console.log(editor);
            console.log(error);
            console.log(response);
        });
    }
}

export interface IFormAddCompany {
    code: string;
    bunameVn: string;
    bunameEn: string;
    bunameAbbr: string;
    website: string;
    active: boolean;
}
