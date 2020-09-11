import { Component, OnInit, ViewChild } from '@angular/core';
import { Location } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { AppPage } from 'src/app/app.base';

import { DocumentationRepo, OperationRepo } from '@repositories';

import { CustomClearanceFormDetailComponent } from '../components/form-detail-clearance/form-detail-clearance.component';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';

@Component({
    selector: 'app-custom-clearance-addnew',
    templateUrl: './custom-clearance-addnew.component.html',
    styleUrls: ['./custom-clearance-addnew.component.scss']
})
export class CustomClearanceAddnewComponent extends AppPage implements OnInit {

    @ViewChild(CustomClearanceFormDetailComponent, { static: false }) detailComponent: CustomClearanceFormDetailComponent;
    customDeclaration: CustomClearance = new CustomClearance();

    constructor(private _location: Location,
        private _operationRepo: OperationRepo,
        private _documentation: DocumentationRepo,
        private _toastr: ToastrService) {
        super();
    }

    ngOnInit() {
    }

    addClearance() {
        this.detailComponent.isSubmitted = true;
        this.detailComponent.isConvertJob = false;

        this.detailComponent.formGroup.controls['serviceType'].setErrors(null);
        this.detailComponent.formGroup.controls['route'].setErrors(null);
        if (this.detailComponent.isDisableCargo) {
            this.detailComponent.formGroup.controls['cargoType'].setErrors(null);
        }
        if (this.detailComponent.formGroup.invalid) {
            return;
        }
        this.detailComponent.getClearance();
        this.addCustomClearance();
    }

    addCustomClearance() {
        this._operationRepo.addCustomDeclaration(this.detailComponent.customDeclaration)
            .subscribe(respone => {
                if (respone['status'] === true) {
                    this._toastr.success(respone['message']);
                    this._location.back();
                }
            });

    }

    convertClearance() {
        this.detailComponent.isSubmitted = true;
        this.detailComponent.isConvertJob = true;

        this.detailComponent.formGroup.controls['serviceType'].setErrors(null);
        this.detailComponent.formGroup.controls['route'].setErrors(null);
        if (this.detailComponent.isDisableCargo) {
            this.detailComponent.formGroup.controls['cargoType'].setErrors(null);
        }
        if (this.detailComponent.formGroup.invalid) {
            return;
        }
        if (this.detailComponent.formGroup.invalid) {
            return;
        } else {
            if (this.detailComponent.mblid.value == null || this.detailComponent.mblid.value === '') {
                return;
            }
            if (this.detailComponent.hblid.value == null || this.detailComponent.hblid.value === '') {
                return;
            }
        }
        this.detailComponent.getClearance();
        this.saveAndConvertClearance();
    }

    saveAndConvertClearance() {
        this._documentation.convertClearanceToJob(this.detailComponent.customDeclaration).subscribe(
            (response: any) => {
                if (response.status) {
                    this._location.back();
                }
            });
    }
}
