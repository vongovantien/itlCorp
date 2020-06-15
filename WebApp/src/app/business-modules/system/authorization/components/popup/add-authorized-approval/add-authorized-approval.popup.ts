import { Component, OnInit, Output, ViewChild, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { Validators, FormBuilder, AbstractControl, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { SystemRepo, CatalogueRepo } from '@repositories';
import { ConfirmPopupComponent } from '@common';
import { distinctUntilChanged, map, catchError } from 'rxjs/operators';
import { Authorization, User } from '@models';
import { formatDate } from '@angular/common';
import { AuthorizedApproval } from 'src/app/shared/models/system/authorizedApproval';

@Component({
    selector: 'add-authorized-approval-popup',
    templateUrl: 'add-authorized-approval.popup.html',
})

export class AuthorizedApprovalPopupComponent extends PopupBase {
    @Output() onRequestAuthorizedApproval: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('confirmUpdatePopup', { static: false }) confirmUpdatePopup: ConfirmPopupComponent;
    @ViewChild('confirmCancelPopup', { static: false }) confirmCancelPopup: ConfirmPopupComponent;
    @ViewChild('confirmTurnOfPopup', { static: false }) confirmTurnOfPopup: ConfirmPopupComponent;


    formAuthorizedApproval: FormGroup;
    isUpdate: boolean = false;

    authorizer: AbstractControl;
    commissioner: AbstractControl;
    effectiveDate: AbstractControl;
    expirationDate: AbstractControl;
    status: AbstractControl;
    type: AbstractControl;
    authorized: AuthorizedApproval = new AuthorizedApproval();
    authorizedToUpdate: any = {};

    users: User[] = [];

    statusActive: any[] = [];
    statusList: any[] = [];
    typeList: any[] = [];

    minDateExpired: any = null;
    minDateEffective: any = null;

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService,) {
        super();
    }

    ngOnInit() {
        this.getUsers();
        this.initForm();
        this.typeList = [
            { text: 'Advance', id: 'Advance' },
            { text: 'Settlement', id: 'Settlement' },
            { text: 'Unlock Shipment', "id": 'Unlock Shipment' }
        ];
    }

    initForm() {
        this.formAuthorizedApproval = this._fb.group({
            authorizer: [null, Validators.required],
            commissioner: [null, Validators.required],
            effectiveDate: [null],
            expirationDate: [null],
            type: [],
            status: [true],
            description: []
        });

        this.authorizer = this.formAuthorizedApproval.controls['authorizer'];
        this.commissioner = this.formAuthorizedApproval.controls['commissioner'];
        this.effectiveDate = this.formAuthorizedApproval.controls['effectiveDate'];
        this.expirationDate = this.formAuthorizedApproval.controls['expirationDate'];
        this.status = this.formAuthorizedApproval.controls['status'];
        this.type = this.formAuthorizedApproval.controls['type'];

        this.formAuthorizedApproval.get("effectiveDate").valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                map((data: any) => data.startDate)
            )
            .subscribe((value: any) => {
                this.minDateExpired = this.createMoment(value); // * Update MinDate -> ExpiredDate.
            });
    }

    onSelectDataFormInfo($event: any, type: string) {
        if (type === 'commisioner') {
            this.commissioner.setValue($event.id);
        } else {
            this.authorizer.setValue($event.id);
        }
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


    saveAuthorizedAprroval() {
        [this.type].forEach((control: AbstractControl) => this.setError(control));
        this.isSubmitted = true;
        if (this.formAuthorizedApproval.valid) {
            const body: IAuthorizedArroval = {
                authorizer: !!this.authorizer.value ? this.authorizer.value : null,
                commissioner: !!this.commissioner ? this.commissioner.value : null, //
                effectiveDate: this.effectiveDate.value ? (this.effectiveDate.value.startDate !== null ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
                expirationDate: this.expirationDate.value ? (this.expirationDate.value.startDate !== null ? formatDate(this.expirationDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
                type: this.type.value ? (this.type.value.length > 0 ? this.type.value[0].id : '') : '',
                description: this.formAuthorizedApproval.controls['description'].value,
                active: this.status.value,
            };
            if (!this.isUpdate) {
                this._systemRepo.addNewAuthorizedApproval(body)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                this.onRequestAuthorizedApproval.emit();
                                this.closeAuthorizedApproval();
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            } else {
                body.id = this.authorized.id;
                this.authorizedToUpdate = body;
                this.confirmUpdatePopup.show();
            }
        }
    }

    onUpdateAuthorizedApproval() {
        this.confirmUpdatePopup.hide();
        this._systemRepo.updateAuthorizedApproval(this.authorizedToUpdate)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.onRequestAuthorizedApproval.emit();
                        this.closeAuthorizedApproval();
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onCancelAuthorization() {
        this.confirmCancelPopup.hide();
        this.closeAuthorizedApproval();
    }

    changeStatus(status: boolean) {
        console.log(status);
        if (!status) {
            this.status.setValue(true);
            this.confirmTurnOfPopup.show();
        }
    }

    onChangeStatus() {
        this.confirmTurnOfPopup.hide();
        this.status.setValue(false);
    }

    closePopup() {
        this.confirmCancelPopup.show();
    }

    closeAuthorizedApproval() {
        this.hide();
        this.isSubmitted = false;
        this.formAuthorizedApproval.reset();
        this.minDateExpired = null;
        this.minDateEffective = null;
    }
}
export class IAuthorizedArroval {
    id?: string;
    authorizer: string;
    commissioner: string;
    effectiveDate: string;
    expirationDate: string;
    type: string;
    description: string;
    active: boolean;
}