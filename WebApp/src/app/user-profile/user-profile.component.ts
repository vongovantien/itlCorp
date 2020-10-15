import { Component, OnInit, Output, EventEmitter } from '@angular/core';

import { Warehouse } from 'src/app/shared/models/catalogue/ware-house.model';
import { PopupBase } from 'src/app/popup.base';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { FormValidators } from 'src/app/shared/validators/form.validator';
import { CommonEnum } from '@enums';
import { AppList } from '@app';
import { ActivatedRoute, Params } from '@angular/router';
import UUID from 'validator/lib/isUUID';
import { Identifiers } from '@angular/compiler/src/render3/r3_identifiers';

@Component({
    selector: 'user-profile-page',
    templateUrl: './user-profile.component.html',
    styleUrls: ['./user-profile.component.scss']
})
export class UserProfilePageComponent extends AppList {
    //@Output() saveSuccess: EventEmitter<boolean> = new EventEmitter<boolean>();
    //
    currentUserId: string;
    //

    fileName: string = null;
    files: File[] = [];
    //
    formUser: FormGroup;
    // properties enable update

    employeeNameVn: AbstractControl;
    employeeNameEn: AbstractControl;
    title: AbstractControl;
    email: AbstractControl;
    bankAccountNo: AbstractControl;
    bankName: AbstractControl;
    tel: AbstractControl;
    description: AbstractControl;
    // image

    avatar: AbstractControl;
    // dump (viewonly) properties

    staffCode: AbstractControl;
    username: AbstractControl;
    workingStatus: AbstractControl;
    creditLimit: AbstractControl;
    creditRate: AbstractControl;


    constructor(
        private _ngProgressService: NgProgress,
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _activedRoute: ActivatedRoute,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.initForm();
        this._activedRoute.params
            .pipe(
                takeUntil(this.ngUnsubscribe),
                map((p: Params) => {
                    if (p.id && UUID(p.id)) {
                        return p.id;
                    }
                    return null;
                }),
                tap(id => this.currentUserId = id),
            ).subscribe(
                (id: string) => {
                    this.getUserDetail(id);
                }
            );
    }
    initForm() {
        this.formUser = this._fb.group({
            employeeNameVn: [],
            employeeNameEn: [],
            title: [],
            email: [],
            bankAccountNo: [],
            bankName: [],
            tel: [],
            description: [],
            //
            avatar: [],
            // view only
            staffCode: [],
            username: [],
            workingStatus: [],
            creditLimit: [],
            creditRate: [],
        });
        //
        this.employeeNameVn = this.formUser.controls['employeeNameVn'];
        this.employeeNameEn = this.formUser.controls['employeeNameEn'];
        this.title = this.formUser.controls['title'];
        this.email = this.formUser.controls['email'];
        this.bankAccountNo = this.formUser.controls['bankAccountNo'];
        this.bankName = this.formUser.controls['bankName'];
        this.tel = this.formUser.controls['tel'];
        this.description = this.formUser.controls['description'];
        //
        this.avatar = this.formUser.controls['avatar'];
        // view only
        this.staffCode = this.formUser.controls['staffCode'];
        this.username = this.formUser.controls['username'];
        this.workingStatus = this.formUser.controls['workingStatus'];
        this.creditLimit = this.formUser.controls['creditLimit'];
        this.creditRate = this.formUser.controls['creditRate'];

    }

    getUserDetail(id: string) {
        return this._systemRepo.getDetailUser(id)
            .pipe(
                catchError(this.catchError)
            )
            .subscribe((body: any) => {
                if (body.status) {
                    this.setUserForForm(body.data);
                }
            });
    }

    setUserForForm(body: any) {
        this.formUser.patchValue({
            employeeNameVn: body.employeeNameVn,
            employeeNameEn: !!body.sysEmployeeModel ? body.sysEmployeeModel.employeeNameEn : null,
            title: !!body.sysEmployeeModel ? body.sysEmployeeModel.title : null,
            email: !!body.sysEmployeeModel ? body.sysEmployeeModel.title : null,
            bankAccountNo: body.bankAccountNo,
            bankName: body.bankName,
            tel: !!body.sysEmployeeModel ? body.sysEmployeeModel.tel : null,
            description: body.description,
            //
            avatar: body.avatar,
            // dump (viewonly) properties

            staffCode: !!body.sysEmployeeModel ? body.sysEmployeeModel.staffCode : null,
            username: body.username,
            workingStatus: body.workingStatus,
            creditLimit: body.creditLimit,
            creditRate: body.creditRate,
        });
    }


    handleFileInput(event) {
        if (!!event.target['files']) {
            this.fileName = event.target.value;
            this.files = event.target['files'];
        }
        console.log(this.files);
    }

    handleUpdateUser() {
        const form = this.formUser.getRawValue();
        if (this.formUser.invalid) {
            return;
        }

        const body = {
            employeeNameVn: form.employeeNameVn,
            employeeNameEn: form.employeeNameEn,
            title: form.title,
            email: form.email,
            bankAccountNo: form.bankAccountNo,
            bankName: form.bankName,
            tel: form.tel,
            description: form.description,
        };
        this.onUpdate(body, this.files);
    }

    onUpdate(body: any, files: File[] = []) {
        this._progressRef.start();
        this._systemRepo.updateProfile(body, files).pipe(catchError(this.catchError),
            finalize(() => this._progressRef.complete()))
            .subscribe((body: any) => {
                if (body.status) {
                    //saved -> remove current url
                    this.fileName = null;
                    this.files = [];
                    this.getUserDetail(this.currentUserId);
                }
            });
    }
}
