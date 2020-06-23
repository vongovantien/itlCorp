import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Router } from '@angular/router';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { InfoPopupComponent } from '@common';

import { CommercialFormCreateComponent } from '../components/form-create/form-create-commercial.component';
import { CommercialContractListComponent } from '../components/contract/commercial-contract-list.component';
import { Partner } from '@models';
import { catchError } from 'rxjs/operators';

@Component({
    selector: 'app-create-commercial',
    templateUrl: './create-commercial.component.html',
})
export class CommercialCreateComponent extends AppForm implements OnInit {

    @ViewChild(CommercialFormCreateComponent, { static: false }) formCreate: CommercialFormCreateComponent;
    @ViewChild(CommercialContractListComponent, { static: false }) contractList: CommercialContractListComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild('taxCodeInfo', { static: false }) infoPopupTaxCode: InfoPopupComponent;


    invalidTaxCode: string;

    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _catalogueRepo: CatalogueRepo,
    ) {
        super();
    }

    ngOnInit(): void { }

    gotoList() {
        this._router.navigate(["home/commercial/customer"]);
    }

    onSave() {
        this.formCreate.isSubmitted = true;

        if (!this.formCreate.formGroup.valid) {
            this.infoPopup.show();
            return;
        }

        if (!this.contractList.contracts.length) {
            this._toastService.warning("Partner don't have any contract in this period, Please check it again!");
            return;
        }

        const modelAdd: Partner = this.formCreate.formGroup.getRawValue();

        //modelAdd.saleMans = [...this.contractList.contracts];

        this.saveCustomerCommercial(modelAdd);
    }


    saveCustomerCommercial(body) {
        this._catalogueRepo.createPartner(body)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._router.navigate(["/home/commercial/customer"]);
                    }
                }, err => {
                });
    }
}

export interface IValidateTaxCode {
    id: string;
    taxCode: string;
    internalReferenceNo: string;
}
