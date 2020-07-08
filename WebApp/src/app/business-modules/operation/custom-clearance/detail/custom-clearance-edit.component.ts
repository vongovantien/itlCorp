import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { OperationRepo, DocumentationRepo } from '@repositories';
import { AppPage } from 'src/app/app.base';

import { CustomClearanceFormDetailComponent } from '../components/form-detail-clearance/form-detail-clearance.component';
import { ToastrService } from 'ngx-toastr';


@Component({
    selector: 'app-custom-clearance-edit',
    templateUrl: './custom-clearance-edit.component.html',
})
export class CustomClearanceEditComponent extends AppPage implements OnInit {
    @ViewChild(CustomClearanceFormDetailComponent, { static: false }) detailComponent: CustomClearanceFormDetailComponent;
    isImported: boolean = false;
    customDeclaration: CustomClearance;

    constructor(private _operationRepo: OperationRepo,
        private route: ActivatedRoute,
        private _toart: ToastrService,
        private _documentation: DocumentationRepo) {

        super();

    }

    ngOnInit() {
        this.route.params.subscribe(prams => {
            if (!!prams.id) {
                this.getCustomCleanranceById(+prams.id);
            }
        });
        this._operationRepo.getClearanceTypes()
            .subscribe((res: any) => {
                if (!!res) {

                    this.detailComponent.serviceTypes = res.serviceTypes.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.typeClearances = res.types.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.routeClearances = res.routes.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.cargoTypes = res.cargoTypes.map(x => ({ "text": x.displayName, "id": x.value }));
                }
            });
    }

    getCustomCleanranceById(id: number) {
        this._operationRepo.getDetailCustomsDeclaration(id)
            .pipe()
            .subscribe(
                (res: CustomClearance) => {
                    if (!!res) {
                        if (!!res.jobNo) { this.isImported = true; }
                        this.customDeclaration = this.detailComponent.customDeclaration = res;
                        this.detailComponent.setFormValue();
                    }
                }
            );
    }
    saveCustomClearance() {
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
        this.updateCustomClearance();
    }
    async updateCustomClearance() {
        const respone = await this._operationRepo.updateCustomDeclaration(this.customDeclaration).toPromise();
        if (respone['status'] === true) {
            this._toart.success(respone['message']);
            this.getCustomCleanranceById(this.customDeclaration.id);
        }
    }
    saveAndConvert() {
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
        this.updateAndConvertClearance();
    }
    async updateAndConvertClearance() {
        const response = await this._documentation.convertExistedClearanceToJob([this.customDeclaration]).toPromise();
        if (response.status) {
            this._toart.success("Convert Successfull!!!");
            this.getCustomCleanranceById(this.customDeclaration.id);
        }
    }
}
