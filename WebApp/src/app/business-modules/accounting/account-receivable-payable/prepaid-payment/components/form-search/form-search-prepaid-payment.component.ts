import { formatDate } from '@angular/common';
import { ChangeDetectionStrategy, Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder } from '@angular/forms';
import { JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Department, Office, Partner, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { getCurrentUserState, IAppState } from '@store';
import { Observable } from 'rxjs';
import { map, startWith, switchMap, takeUntil, filter, tap } from 'rxjs/operators';
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
    issueDate: AbstractControl;
    serviceDate: AbstractControl;
    searchTypes = ['DebitNo', 'JobID'];

    partners: Observable<Partner[]>;
    salesmans: Observable<User[]>;
    departments: Observable<any>;
    offices: Observable<Office[]>;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    constructor(
        private readonly _fb: FormBuilder,
        private readonly _catalogue: CatalogueRepo,
        private readonly _systemRepo: SystemRepo,
        private readonly _store: Store<IAppState>
    ) {
        super();
        this.requestSearch = this.onSubmit;
        this.requestReset = this.onReset;
    }

    ngOnInit(): void {
        // this.isCollapsed = false;
        this.form = this._fb.group({
            searchType: [this.searchTypes[0]],
            keywords: [],
            partnerId: [],
            salesmanId: [],
            currency: [],
            status: [],
            departments: [],
            office: [],
            issueDate: [],
            serviceDate: [],
            agreementType: [{ value: 'Prepaid', disabled: true }],
        });
        this.partnerId = this.form.controls['partnerId'];
        this.salesmanId = this.form.controls['salesmanId'];
        this.issueDate = this.form.controls['issueDate'];
        this.serviceDate = this.form.controls['serviceDate'];

        this.partners = this._catalogue.getPartnerByGroups([CommonEnum.PartnerGroupEnum.CUSTOMER]);
        this.salesmans = this._systemRepo.getListSystemUser();
        this.departments = this._systemRepo.getDepartment(null, null, { active: true });
        this.offices = this._store.select(getCurrentUserState)
            .pipe(
                filter((c: any) => !!c.userName),
                tap((c) => this.currentUser = c),
                switchMap((currentUser: SystemInterface.IClaimUser | any) => {
                    if (!!currentUser.userName) {
                        return this._systemRepo.getOfficePermission(currentUser.id, currentUser.companyId)
                            .pipe(startWith([]))
                    }
                }),
                tap((o) => { this.form.controls['office'].setValue(this.currentUser.officeId) }),
                takeUntil(this.ngUnsubscribe),
            )

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
            departmentIds: formValue.departments,
            officeId: formValue.office,
            issueDateFrom: !!formValue.issueDate?.startDate ? formatDate(formValue.issueDate?.startDate, 'yyyy-MM-dd', 'en') : null,
            issueDateTo: !!formValue.issueDate?.startDate ? formatDate(formValue.issueDate?.endDate, 'yyyy-MM-dd', 'en') : null,
            serviceDateFrom: !!formValue.serviceDate?.startDate ? formatDate(formValue.serviceDate?.startDate, 'yyyy-MM-dd', 'en') : null,
            serviceDateTo: !!formValue.serviceDate?.startDate ? formatDate(formValue.serviceDate?.endDate, 'yyyy-MM-dd', 'en') : null,
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

    collapsed() {
        const controls = this.form.controls;
        this.resetFormControl(controls['departments']);
        this.resetFormControl(controls['serviceDate']);
        this.resetFormControl(controls['issueDate']);
        this.form.controls['office'].setValue(this.currentUser?.officeId);
    }
}


export interface IPrepaidCriteria {
    partnerId: string;
    salesmanId: string;
    currency: string;
    status: string;
    agreementType: string;
    keywords: string[],
    searchType: string,
    departmentIds: string[],
    officeId: string,
    issueDateFrom: any,
    issueDateTo: any,
    serviceDateFrom: any,
    serviceDateTo: any,
    [key: string]: any
}