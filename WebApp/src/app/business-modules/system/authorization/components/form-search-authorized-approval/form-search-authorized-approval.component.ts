import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { catchError, distinctUntilChanged, map } from 'rxjs/operators';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { SystemRepo, CatalogueRepo } from '@repositories';
import { AppForm } from 'src/app/app.form';
import { formatDate } from '@angular/common';
import { User } from '@models';

@Component({
    selector: 'authorized-approval-form-search',
    templateUrl: 'form-search-authorized-approval.component.html'
})

export class AuthorizedApprovalFormSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ISearchAuthorizedApproval> = new EventEmitter<ISearchAuthorizedApproval>();

    formSearch: FormGroup;
    authorizer: AbstractControl;
    commissioner: AbstractControl;
    type: AbstractControl;


    effectiveDate: AbstractControl;
    personInCharge: AbstractControl;
    expirationDate: AbstractControl;
    authorizedPerson: AbstractControl;
    status: AbstractControl;

    statusList: any[] = [];
    typeList: any[] = [];

    statusActive: any[] = [];
    personInChargeList: any[] = [];
    authorizedPersonList: any[] = [];
    serviceList: any[] = [];
    users: User[] = [];
    minDateExpired: any = null;
    minDateEffective: any = null;

    constructor(
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo,
    ) {
        super();
        this.requestSearch = this.onSubmit;
    }

    ngOnInit() {
        this.initDataInform();
        this.initForm();
        this.typeList = [
            { text: 'Advance', id: 'Advance' },
            { text: 'Settlement', id: 'Settlement' },
            { text: 'Unlock Shipment', "id": 'Unlock Shipment' }
        ];
    }

    initForm() {
        this.formSearch = this._fb.group({

            authorizer: [],
            commissioner: [],
            effectiveDate: [null],
            expirationDate: [null],
            type: [],
            status: [this.statusActive]
        });
        this.authorizer = this.formSearch.controls['authorizer'];
        this.commissioner = this.formSearch.controls['commissioner'];
        this.effectiveDate = this.formSearch.controls['effectiveDate'];
        this.expirationDate = this.formSearch.controls['expirationDate'];
        this.status = this.formSearch.controls['status'];
        this.type = this.formSearch.controls['type'];
        this.minDateEffective = this.minDateExpired = this.minDate;
        this.formSearch.get("effectiveDate").valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                map((data: any) => data.startDate)
            )
            .subscribe((value: any) => {
                this.minDateExpired = this.createMoment(value); // * Update MinDate -> ExpiredDate.
            });
    }

    initDataInform() {
        this.getUsers();
        this.getStatus();
    }

    getUsers() {
        this._systemRepo.getSystemUsers({ active: true })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.users = res;
                    }
                },
            );
    }

    onSelectDataFormInfo($event: any, type: string) {
        if (type === 'commissioner') {
            this.commissioner.setValue($event.id);
        } else {
            this.authorizer.setValue($event.id);
        }
    }

    getStatus() {
        this.statusList = [
            { "text": 'Active', "id": 'active' },
            { "text": 'Inactive', "id": 'inactive' }
        ];
        //Default value: Active
        this.statusActive = [this.statusList[0]];
    }

    onSubmit() {
        const body: ISearchAuthorizedApproval = {
            all: '',
            authorizer: !!this.authorizer.value ? this.authorizer.value : null,
            commissioner: !!this.commissioner.value ? this.commissioner.value : null,
            type: !!this.type.value && this.type.value.length > 0 ? (this.type.value[0].id) : null,
            effectiveDate: this.effectiveDate.value ? (this.effectiveDate.value.startDate !== null ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            expirationDate: this.expirationDate.value ? (this.expirationDate.value.startDate !== null ? formatDate(this.expirationDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            active: this.status.value ? (this.status.value[0].id === 'active' ? true : false) : null,

        };
        this.onSearch.emit(body);
    }

    search() {
        this.onSubmit();
    }

    reset() {
        this.formSearch.reset();
        this.initDataInform();
        this.onSearch.emit(<any>{});
        this.statusActive = [this.statusList[0]];
        this.status.setValue(this.statusActive);
    }
}
interface ISearchAuthorizedApproval {
    all: '';
    authorizer: string;
    commissioner: string;
    type: string;
    effectiveDate: string;
    expirationDate: string;
    active: boolean;
}