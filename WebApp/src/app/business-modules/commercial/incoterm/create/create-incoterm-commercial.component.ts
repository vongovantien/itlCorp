import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { CommercialFormIncotermComponent } from '../components/form-incoterm/form-incoterm.component';
import _merge from 'lodash/merge';
import { IncotermUpdateModel, Incoterm } from '@models';
import { CommercialListChargeIncotermComponent } from '../components/list-charge/list-charge-incoterm.component';
import { InfoPopupComponent } from '@common';
import { CommonEnum } from '@enums';
import { CatalogueRepo } from '@repositories';
import { catchError, exhaust } from 'rxjs/operators';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'create-incoterm-commercial',
    templateUrl: './create-incoterm-commercial.component.html',
})
export class CommercialCreateIncotermComponent extends AppForm implements OnInit {

    @ViewChild(CommercialFormIncotermComponent, { static: false }) formCreateComponent: CommercialFormIncotermComponent;
    @ViewChild('selling', { static: false }) listChargeSelling: CommercialListChargeIncotermComponent;
    @ViewChild('buying', { static: false }) listChargeBuying: CommercialListChargeIncotermComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    activeTab: string = CommonEnum.SurchargeTypeEnum.SELLING_RATE;

    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _router: Router
    ) {
        super();
    }

    ngOnInit(): void { }

    onSubmit() {
        this.formCreateComponent.isSubmitted = true;

        if (!this.formCreateComponent.formGroup.valid) {
            this.infoPopup.show();
            return;
        }

        if (!!this.listChargeSelling.incotermCharges.length) {
            this.listChargeSelling.isSubmitted = true;
            if (!this.listChargeSelling.validateListCharge()) {
                this.infoPopup.show();

                this.activeTab = CommonEnum.SurchargeTypeEnum.SELLING_RATE;
                return;
            }

            if (!this.listChargeSelling.validateDuplicate()) {
                this.infoPopup.show();
                this.activeTab = CommonEnum.SurchargeTypeEnum.SELLING_RATE;
                return;
            }
        }

        if (!!this.listChargeBuying.incotermCharges.length) {
            this.listChargeSelling.isSubmitted = true;
            if (!this.listChargeBuying.validateListCharge()) {
                this.infoPopup.show();

                this.activeTab = CommonEnum.SurchargeTypeEnum.SELLING_RATE;
                return;
            }

            if (!this.listChargeBuying.validateDuplicate()) {
                this.infoPopup.show();
                this.activeTab = CommonEnum.SurchargeTypeEnum.SELLING_RATE;
                return;
            }
        }

        const formData: Incoterm = this.getFormData();

        const incotermUpdateModel: IncotermUpdateModel = new IncotermUpdateModel();
        incotermUpdateModel.incoterm = formData;
        incotermUpdateModel.buyings = this.listChargeBuying.incotermCharges;
        incotermUpdateModel.sellings = this.listChargeSelling.incotermCharges;

        this.saveIncoterm(incotermUpdateModel);
    }

    getFormData() {
        const form: any = this.formCreateComponent.formGroup.value;
        const formData = {
            service: !!form.service && !!form.service.length ? form.service[0].id : null,
        };

        return Object.assign(_merge(form, formData));
    }

    saveIncoterm(model: IncotermUpdateModel) {
        this._catalogueRepo.createIncoterm(model).pipe(
            catchError(this.catchError),
        ).subscribe(
            (res: CommonInterface.IResult) => {
                if (res.status) {
                    this._toastService.success(res.message);
                }
            }
        );
    }

    gotoList() {
        this._router.navigate(["home/commercial/incoterm"]);
    }

}
