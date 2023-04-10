import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder } from '@angular/forms';
import { ChargeConstants, JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { GetSystemUser, getSystemUserState } from '@store';
import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { IWorkOrderMngtState, SearchListWorkOrder, workOrderSearchState } from '../../store';

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
    active: AbstractControl;

    services: CommonInterface.INg2Select[] = [
        { text: ChargeConstants.TK_DES, id: ChargeConstants.TK_CODE },
        { text: ChargeConstants.AI_DES, id: ChargeConstants.AI_CODE },
        { text: ChargeConstants.AE_DES, id: ChargeConstants.AE_CODE },
        { text: ChargeConstants.SFE_DES, id: ChargeConstants.SFE_CODE },
        { text: ChargeConstants.SFI_DES, id: ChargeConstants.SFI_CODE },
        { text: ChargeConstants.SLE_DES, id: ChargeConstants.SLE_CODE },
        { text: ChargeConstants.SLI_DES, id: ChargeConstants.SLI_CODE },
        { text: ChargeConstants.SCE_DES, id: ChargeConstants.SCE_CODE },
        { text: ChargeConstants.SCI_DES, id: ChargeConstants.SCI_CODE },
        { text: ChargeConstants.CL_DES, id: ChargeConstants.CL_CODE }
    ];

    statusSelects: CommonInterface.INg2Select[] = [
        { text: 'Inactive', id: false },
        { text: 'Active', id: true },
    ];

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
        this.requestReset = this.resetSearch;
        this.requestSearch = this.submitSearch;
    }

    ngOnInit(): void {
        this._store.dispatch(GetSystemUser({ active: true }));
        this.salesmans = this._store.select(getSystemUserState);

        // this._store.dispatch(GetCustomer({ active: true }));

        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);
        this.initForm();

        this.subscriptionSearchParamState();

    }

    initForm() {
        this.form = this._fb.group({
            referenceNo: [],
            transactionType: [null],
            partnerId: [null],
            salesmanId: [null],
            active: [null],
            status: [null],
        });

        this.referenceNo = this.form.controls['referenceNo'];
        this.transactionType = this.form.controls['transactionType'];
        this.partnerId = this.form.controls['partnerId'];
        this.salesmanId = this.form.controls['salesmanId'];
        this.active = this.form.controls['active'];
        this.status = this.form.controls['status'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        this.form.controls[type].setValue(data);
    }

    submitSearch() {
        const body: IWorkOrderCriteria = {
            referenceNos: !(this.referenceNo.value?.trim()) ? [] :
                this.referenceNo.value
                    .trim()
                    .replace(SystemConstants.CPATTERN.LINE, ',')
                    .trim()
                    .split(',')
                    .map((item: any) => item.trim()),
            partnerId: this.partnerId.value,
            active: null,
            pod: null,
            pol: null,
            source: null,
            salesmanId: this.salesmanId.value,
            status: this.status.value,
            transactionType: this.transactionType.value,
            approvedStatus: null,
        };

        this._store.dispatch(SearchListWorkOrder(body));
    }

    resetSearch() {
        this.form.reset();
        this._store.dispatch(SearchListWorkOrder({}));
    }

    subscriptionSearchParamState() {
        this._store.select(workOrderSearchState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    if (data) {
                        let formData: any = {
                            referenceNo: data?.referenceNos?.toString().replace(/[,]/g, "\n") || null,
                            partnerId: data.partnerId,
                            active: data.active,
                            pol: data.pol,
                            pod: data.pod,
                            source: data.source,
                            salesmanId: data.salesmanId,
                            status: data.status,
                            transactionType: data.transactionType,
                            approvedStatus: data.approvedStatus
                        };
                        this.form.patchValue(formData);
                    }
                }
            );
    }


}

export interface IWorkOrderCriteria {
    referenceNos: string[],
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
