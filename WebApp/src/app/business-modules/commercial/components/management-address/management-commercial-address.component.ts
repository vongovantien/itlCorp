import { Component, OnInit, ViewChild } from "@angular/core";
import { AbstractControl, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Partner } from "@models";
import { NgProgress } from "@ngx-progressbar/core";
import { CatalogueRepo } from "@repositories";
import { ToastrService } from "ngx-toastr";
import { PopupBase } from "src/app/popup.base";
import { CommercialAddressListComponent } from 'src/app/business-modules/commercial/components/address/commercial-address-list.component';

@Component({
    selector: 'management-address-commercial',
    templateUrl: './management-commercial-address.component.html',
})
export class ManagementAddressComponent extends PopupBase implements OnInit {
    @ViewChild(CommercialAddressListComponent) addressPartnerList: CommercialAddressListComponent;
    formGroup: FormGroup;

    accountNo: AbstractControl;
    shortName: AbstractControl;
    taxCode: AbstractControl;

    partner: any;
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
            this.partner = partner;
            this.addressPartnerList.getAddressPartner(this.partner.id);
            this.addressPartnerList.partnerId = this.partner.id;
            this.addressPartnerList.partner = this.partner;
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
    close() {
        this.hide();
    }
}