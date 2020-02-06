import { PopupBase } from "src/app/popup.base";
import { Component, Output, EventEmitter, ViewChild } from "@angular/core";
import { FormGroup, FormBuilder, AbstractControl, Validators } from "@angular/forms";
import { SystemRepo, CatalogueRepo } from "@repositories";
import { ToastrService } from "ngx-toastr";
import { catchError } from "rxjs/operators";
import { Authorization } from "src/app/shared/models/system/authorization";
import { formatDate } from "@angular/common";
import { ConfirmPopupComponent } from "@common";
@Component({
    selector: 'add-authorization-popup',
    templateUrl: './add-authorization.popup.html'
})

export class AuthorizationAddPopupComponent extends PopupBase {
    @Output() onRequestAuthorization: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmUpdatePopup: ConfirmPopupComponent;

    formAuthorization: FormGroup;
    authorization: Authorization = new Authorization();
    authorizationToUpdate: Authorization = new Authorization();

    isSubmited: boolean = false;
    action: string = 'create';

    authorizationName: AbstractControl;
    personInCharge: AbstractControl;
    authorizedPerson: AbstractControl;
    authorizationService: AbstractControl;
    effectiveDate: AbstractControl;
    expirationDate: AbstractControl;
    authorizationNote: AbstractControl;
    authorizationActive: AbstractControl;

    personInChargeList: any[] = [];
    personInChargeActive: any[] = [];
    authorizedPersonList: any[] = [];
    authorizedPersonActive: any[] = [];
    serviceList: any[] = [];
    activeServices: any = [];

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService, ) {
        super();
    }

    ngOnInit() {
        this.getService();
        this.getUsers();
        this.initForm();
    }

    initForm() {
        this.formAuthorization = this._fb.group({
            authorizationName: ['',
                Validators.compose([
                    Validators.required,
                    Validators.maxLength(50)
                ])],
            personInCharge: [],
            authorizedPerson: [null,Validators.required],
            authorizationService: [null,Validators.required],
            effectiveDate: [null,Validators.required],
            expirationDate: [null],
            authorizationNote: [''],
            authorizationActive: []
        });

        this.authorizationName = this.formAuthorization.controls['authorizationName'];
        this.personInCharge = this.formAuthorization.controls['personInCharge'];
        this.authorizedPerson = this.formAuthorization.controls['authorizedPerson'];
        this.authorizationService = this.formAuthorization.controls['authorizationService'];
        this.effectiveDate = this.formAuthorization.controls['effectiveDate'];
        this.expirationDate = this.formAuthorization.controls['expirationDate'];
        this.authorizationNote = this.formAuthorization.controls['authorizationNote'];
        this.authorizationActive = this.formAuthorization.controls['authorizationActive'];
    }

    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.serviceList = res.map(x => ({ "text": x.displayName, "id": x.value })).filter(item => item.id !== 'CL');
                    }
                },
            );
    }

    getUsers() {
        this._systemRepo.getSystemUsers({ active: true })
            .pipe(catchError(this.catchError))
            .subscribe(
                (data: any) => {
                    this.personInChargeList = data.map(x => ({ "text": x.username, "id": x.username }));
                    this.authorizedPersonList = data.map(x => ({ "text": x.username, "id": x.username }));
                },
            );
    }

    saveAuthorization() {
        [this.personInCharge].forEach((control: AbstractControl) => this.setError(control));
        console.log(this.formAuthorization);
        this.isSubmited = true;
        if (this.formAuthorization.valid) {
            const _authorization: Authorization = {
                id: this.authorization.id,
                userId: this.personInCharge.value ? (this.personInCharge.value.length > 0 ? this.personInCharge.value[0].id : '') : '',
                assignTo: this.authorizedPerson.value ? (this.authorizedPerson.value.length > 0 ? this.authorizedPerson.value[0].id : '') : '',
                name: this.authorizationName.value,
                services: this.authorizedPerson.value ? (this.authorizedPerson.value.length > 0 ? this.authorizationService.value[0].id : '') : '',
                description: this.authorizationNote.value,
                startDate: this.effectiveDate.value ? (this.effectiveDate.value.startDate !== null ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
                endDate: this.expirationDate.value ? (this.expirationDate.value.startDate !== null ? formatDate(this.expirationDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
                userCreated: this.authorization.userCreated,
                datetimeCreated: this.authorization.datetimeCreated,
                userModified: this.authorization.userModified,
                datetimeModified: this.authorization.datetimeModified,
                active: this.authorizationActive.value,
                inactiveOn: this.authorization.inactiveOn,
                servicesName: this.authorization.servicesName
            };
            this.authorizationToUpdate = _authorization;
            console.log(_authorization);
            if (this.action == "create") {
                this._systemRepo.addNewAuthorization(_authorization)
                    .pipe(catchError(this.catchError))
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {
                                this._toastService.success(res.message);
                                this.onRequestAuthorization.emit();
                                this.closePopup();
                            } else {
                                this._toastService.error(res.message);
                            }
                        }
                    );
            } else {
                this.confirmUpdatePopup.show();
            }
        }
    }

    onUpdateAuthorization() {
        this.confirmUpdatePopup.hide();
        this._systemRepo.updateAuthorization(this.authorizationToUpdate)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.onRequestAuthorization.emit();
                        this.closePopup();
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    getDetail() {
        console.log(this.authorization)
        let indexPIC = this.personInChargeList.findIndex(x => x.id == this.authorization.userId);
        if (indexPIC > -1) {
            this.personInChargeActive = [this.personInChargeList[indexPIC]];
        }
        console.log(this.personInChargeActive)

        let indexAP = this.authorizedPersonList.findIndex(x => x.id == this.authorization.assignTo);
        if (indexAP > -1) {
            this.authorizedPersonActive = [this.authorizedPersonList[indexAP]];
        }
        console.log(this.authorizedPersonActive)

        this.formAuthorization.setValue({
            authorizationName: this.authorization.name,
            personInCharge: this.personInChargeActive,
            authorizedPerson: this.authorizedPersonActive,
            authorizationService: this.activeServices,
            effectiveDate: !!this.authorization.startDate ? { startDate: new Date(this.authorization.startDate) } : null,
            expirationDate: !!this.authorization.endDate ? { startDate: new Date(this.authorization.endDate) } : null,
            authorizationNote: this.authorization.description,
            authorizationActive: this.authorization.active
        });
    }

    getCurrentActiveService(ChargeService: any) {
        this.getService();
        const listService = ChargeService.split(";");
        const activeServiceList: any = [];
        listService.forEach(item => {
            const element = this.serviceList.find(x => x.id === item);
            if (element !== undefined) {
                const activeService = element;
                activeServiceList.push(activeService);
            }
        });
        return activeServiceList;
    }

    closePopup() {
        this.hide();
        this.isSubmited = false;
        this.authorization = new Authorization();
        this.formAuthorization.reset();
        //this.formAuthorization.updateValueAndValidity();
    }
}