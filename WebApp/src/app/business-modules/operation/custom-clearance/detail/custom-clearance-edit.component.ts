import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { OperationRepo, DocumentationRepo } from '@repositories';
import { AppPage } from 'src/app/app.base';

import { CustomClearanceFormDetailComponent } from '../components/form-detail-clearance/form-detail-clearance.component';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { getCurrentUserState, IAppState } from '@store';


@Component({
    selector: 'app-custom-clearance-edit',
    templateUrl: './custom-clearance-edit.component.html',
})
export class CustomClearanceEditComponent extends AppPage implements OnInit {
    @ViewChild(CustomClearanceFormDetailComponent) detailComponent: CustomClearanceFormDetailComponent;

    isImported: boolean = false;
    customDeclaration: CustomClearance;

    constructor(private _operationRepo: OperationRepo,
        private route: ActivatedRoute,
        private _toart: ToastrService,
        private _store: Store<IAppState>,
        private _documentation: DocumentationRepo) {

        super();
    }

    ngOnInit() {
        this.currentUser$ = this._store.select(getCurrentUserState);
        this.route.params.subscribe(prams => {
            if (!!prams.id) {
                this.getCustomClearanceById(+prams.id);
            }
        });
    }

    getCustomClearanceById(id: number) {
        this._operationRepo.getDetailCustomsDeclaration(id)
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

        if (!this.detailComponent.isDisableCargo && !this.detailComponent.cargoType.value || (!this.detailComponent.hblid.value && this.detailComponent.currentUser?.companyCode !== 'ITL')) {
            return;
        }
        if (this.detailComponent.formGroup.invalid || (!!this.detailComponent.clearanceDate.value && !this.detailComponent.clearanceDate.value.startDate)) {
            return;
        }
        this.detailComponent.getClearance();
        this.updateCustomClearance();
    }

    updateCustomClearance() {
        this._operationRepo.updateCustomDeclaration(this.customDeclaration)
            .subscribe((respone) => {
                if (respone['status'] === true) {
                    this._toart.success(respone['message']);
                    this.getCustomClearanceById(this.customDeclaration.id);
                }
            });
    }

    saveAndConvert(isReplicate: boolean) {
        this.detailComponent.isSubmitted = true;
        this.detailComponent.isConvertJob = true;

        if (!this.detailComponent.isDisableCargo && ['Air', 'Express'].includes(this.detailComponent.serviceType.value)) {
            return;
        }
        if (this.detailComponent.formGroup.invalid || (!!this.detailComponent.clearanceDate.value && !this.detailComponent.clearanceDate.value.startDate)) {
            return;
        } else {
            if (!this.detailComponent.mblid.value || !this.detailComponent.hblid.value) {
                return;
            }
        }
        this.detailComponent.getClearance();
        this.customDeclaration.isReplicate = isReplicate;
        this.updateAndConvertClearance(this.customDeclaration);
    }

    updateAndConvertClearance(body: CustomClearance) {
        return this._documentation.convertExistedClearanceToJob([body])
            .subscribe((response) => {
                if (!!response && response.status) {
                    this._toart.success(`Convert ${body.clearanceNo} Successfull`);
                    this.getCustomClearanceById(body.id);
                }
            });
    }
}
