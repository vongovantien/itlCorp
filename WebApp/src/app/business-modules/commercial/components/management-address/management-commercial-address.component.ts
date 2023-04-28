import { Component, OnInit } from "@angular/core";
import { AbstractControl, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Partner } from "@models";
import { NgProgress } from "@ngx-progressbar/core";
import { CatalogueRepo } from "@repositories";
import { ToastrService } from "ngx-toastr";
import { PopupBase } from "src/app/popup.base";
@Component({
    selector: 'management-address-commercial',
    templateUrl: './management-commercial-address.component.html',
})
export class ManagementAddressComponent extends PopupBase implements OnInit {
    formGroup: FormGroup;

    accountNo: AbstractControl;
    shortName: AbstractControl;
    taxCode: AbstractControl;

    partner: Partner;
    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        protected _progressService: NgProgress,
        private _toastService: ToastrService) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
        console.log(this.partner);
    }
    setDefaultValue(partner: any) {
        if (!!partner) {
            this.formGroup.patchValue({
                accountNo: partner.accountNo,
                shortName: partner.shortName,
                taxCode: partner.taxCode
            });
        }
    }
    initForm() {
        this.formGroup = this._fb.group({
            accountNo: [null, Validators.required],
            shortName: [null, Validators.required],
            taxCode: [null],
        });

        this.accountNo = this.formGroup.controls['accountNo'];
        this.shortName = this.formGroup.controls['shortName'];
        this.taxCode = this.formGroup.controls['taxCode'];
    }
}