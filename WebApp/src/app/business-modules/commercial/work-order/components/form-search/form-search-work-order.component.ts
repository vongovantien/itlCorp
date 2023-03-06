import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder } from '@angular/forms';
import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { AppForm } from 'src/app/app.form';
import { IWorkOrderMngtState, SearchListWorkOrder } from '../../store';

@Component({
    selector: 'form-search-work-order',
    templateUrl: './form-search-work-order.component.html',
})
export class CommercialFormSearchWorkOrderComponent extends AppForm implements OnInit {

    referenceNo: AbstractControl;
    transactionType: AbstractControl;
    partnerId: AbstractControl;
    salesmanId: AbstractControl;
    status: AbstractControl;

    productServices: any[] = [];
    salesmans: Observable<User[]>;
    partners: Observable<Partner[]>;

    displayFieldsPartner = JobConstants.CONFIG.COMBOGRID_PARTNER;

    constructor(
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _systemRepo: SystemRepo,
        private readonly _store: Store<IWorkOrderMngtState>
    ) {
        super();
    }

    ngOnInit(): void {

        this.salesmans = this._systemRepo.getSystemUsers({ active: true });
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
        this.initForm();

    }

    initForm() {
        this.form = this._fb.group({
            referenceNo: [],
            transactionType: [null],
            partnerId: [null],
            salesmanId: [null],
            status: [null],
        });

        this.referenceNo = this.form.controls['referenceNo'];
        this.transactionType = this.form.controls['transactionType'];
        this.partnerId = this.form.controls['partnerId'];
        this.salesmanId = this.form.controls['salesmanId'];
        this.status = this.form.controls['status'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        this.form.controls[type].setValue(data);
    }

    submitSearch() {
        const body: IWorkOrderCriteria = {
            partnerId: this.partnerId.value,
            active: null,
            pod: null,
            pol: null,
            source: null,
            salesmanId: this.salesmanId.value,
            status: this.status.value,
            transactionType: this.transactionType.value,
            approvedStatus: null
        };

        this._store.dispatch(SearchListWorkOrder(body));
    }

    resetSearch() {
        this.form.reset();
        this._store.dispatch(SearchListWorkOrder({}))
    }
}

export interface IWorkOrderCriteria {
    partnerId: string;
    status: string;
    salesmanId: string;
    source: string;
    active: boolean;
    pol: string;
    pod: string;
    approvedStatus: string;
    transactionType: string;

}
