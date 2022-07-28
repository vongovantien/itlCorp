import { ChangeDetectionStrategy, Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder } from '@angular/forms';
import { JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, User } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-prepaid-payment',
    templateUrl: './form-search-prepaid-payment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ARPrePaidPaymentFormSearchComponent extends AppForm implements OnInit {
    @Output() onSearch: EventEmitter<IPrepaidCriteria> = new EventEmitter<IPrepaidCriteria>();

    partnerId: AbstractControl;
    salesmanId: AbstractControl;
    searchTypes = ['DebitNo', 'JobID'];

    partners: Observable<Partner[]>;
    salesmans: Observable<User[]>;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    constructor(
        private readonly _fb: FormBuilder,
        private readonly _catalogue: CatalogueRepo,
        private readonly _systemRepo: SystemRepo
    ) {
        super();
        this.requestSearch = this.onSubmit;
        this.requestReset = this.onReset;
    }

    ngOnInit(): void {
        this.form = this._fb.group({
            searchType: [this.searchTypes[0]],
            keywords: [],
            partnerId: [],
            salesmanId: [],
            currency: [],
            status: [],
            agreementType: [{ value: 'Prepaid', disabled: true }],
        });
        this.partnerId = this.form.controls['partnerId'];
        this.salesmanId = this.form.controls['salesmanId'];

        this.partners = this._catalogue.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CUSTOMER]);
        this.salesmans = this._systemRepo.getListSystemUser();

    }

    onSubmit() {
        const formValue = this.form.getRawValue();
        const criteria: IPrepaidCriteria = {
            partnerId: formValue.partnerId,
            currency: formValue.currency,
            status: formValue.status,
            searchType: formValue.searchType,
            salesmanId: formValue.salesmanId,
            agreementType: formValue.agreementType,
            keywords: !!formValue.keywords ? formValue.keywords.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: any) => item.trim()) : [],
        };
        console.log(criteria);
        this.onSearch.emit(criteria);
    }

    onReset() {
        this.form.reset({
            searchType: this.searchTypes[0],
        });
        this.form.controls['agreementType'].setValue('Prepaid');
        this.form.controls['agreementType'].disable();
        this.onSearch.emit(<any>{
            agreementType: this.form.controls['agreementType'].value,
            keywords: [],
            searchType: this.searchTypes[0]
        });

    }
}


export interface IPrepaidCriteria {
    partnerId: string;
    salesmanId: string;
    currency: string;
    status: string;
    agreementType: string;
    keywords: string[],
    searchType: string
}