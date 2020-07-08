import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { NgForm, FormGroup, AbstractControl, Validators, FormBuilder } from '@angular/forms';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { Location, formatDate } from '@angular/common';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services/sort.service';
import { CatalogueRepo, OperationRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { AppPage } from 'src/app/app.base';
import { CommonEnum } from '@enums';
import { AppForm } from 'src/app/app.form';
import { Observable } from 'rxjs';
import { Customer, PortIndex, CountryModel, Commodity } from '@models';
import { GetCataloguePortAction, getCataloguePortState, GetCatalogueCountryAction, getCatalogueCountryState, GetCatalogueCommodityAction, getCatalogueCommodityState } from '@store';
import { Store } from '@ngrx/store';
import { IShareBussinessState } from '@share-bussiness';
import { CustomClearanceFormDetailComponent } from '../components/form-detail-clearance/form-detail-clearance.component';

@Component({
    selector: 'app-custom-clearance-addnew',
    templateUrl: './custom-clearance-addnew.component.html',
    styleUrls: ['./custom-clearance-addnew.component.scss']
})
export class CustomClearanceAddnewComponent extends AppPage implements OnInit {
    @ViewChild(CustomClearanceFormDetailComponent, { static: false }) detailComponent: CustomClearanceFormDetailComponent;
    constructor(private _location: Location,
        private _operationRepo: OperationRepo,
        private _documentation: DocumentationRepo,
        private _toastr: ToastrService) {
        super();
    }


    customDeclaration: CustomClearance = new CustomClearance();
    ngOnInit() {
        this.getClearanceType();
    }
    getClearanceType() {
        this._operationRepo.getClearanceType()
            .subscribe(
                (res: any) => {
                    this.detailComponent.serviceTypes = res.serviceTypes.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.typeClearances = res.types.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.routeClearances = res.routes.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.cargoTypes = res.cargoTypes.map(x => ({ "text": x.displayName, "id": x.value }));
                }
            );
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
    async addCustomClearance() {
        const respone = await this._operationRepo.addCustomDeclaration(this.detailComponent.customDeclaration).toPromise();
        if (respone['status'] === true) {
            this._toastr.success(respone['message']);
            this._location.back();
        }
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
