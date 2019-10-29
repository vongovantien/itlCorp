import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { SystemRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
@Component({
    selector: 'app-form-add-user',
    templateUrl: './form-add-user.component.html'
})
export class FormAddUserComponent extends AppList {
    formGroup: FormGroup;
    isSubmited: boolean = false;
    isDetail: boolean = false;
    SelectedUser: any = {};
    staffcode: AbstractControl;
    username: AbstractControl;
    employeeNameVn: AbstractControl;
    employeeNameEn: AbstractControl;
    title: AbstractControl;
    active: AbstractControl;
    usertype: AbstractControl;
    workingg: AbstractControl;
    email: AbstractControl;
    phone: AbstractControl;
    description: AbstractControl;
    ldap: AbstractControl;

    status: CommonInterface.ICommonTitleValue[] = [
        { title: 'Active', value: true },
        { title: 'Inactive', value: false },
    ];

    usertypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Normal User', value: 'Normal User' },
        { title: 'Local Admin', value: 'Local Admin' },
        { title: 'Super Admin', value: 'Super Admin' }
    ];

    working: CommonInterface.ICommonTitleValue[] = [
        { title: 'Working', value: 'Working' },
        { title: 'Maternity leave', value: 'Maternity leave' },
        { title: 'Off', value: 'Off' }
    ];

    constructor(
        private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService,
        private _progressService: NgProgress,

    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    initForm() {
        this.formGroup = this._fb.group({
            staffcode: ['',
                Validators.compose([
                    Validators.required
                ])],
            username: ['',
                Validators.compose([
                    Validators.required
                ])],
            employeeNameVn: ['',
                Validators.compose([
                    Validators.required
                ])],
            employeeNameEn: ['',
                Validators.compose([
                    Validators.required
                ])],
            title: [],
            usertype: [this.usertypes[0]],
            active: [this.status[0]],
            workingg: [this.working[0]],
            email: ['',
                Validators.compose([
                    Validators.required
                ])],
            phone: [],
            description: [],
            ldap: []


        });

        this.staffcode = this.formGroup.controls['staffcode'];
        this.username = this.formGroup.controls['username'];
        this.employeeNameVn = this.formGroup.controls['employeeNameVn'];
        this.employeeNameEn = this.formGroup.controls['employeeNameEn'];
        this.title = this.formGroup.controls['title'];
        this.usertype = this.formGroup.controls['usertype'];
        this.active = this.formGroup.controls['active'];
        this.email = this.formGroup.controls['email'];
        this.phone = this.formGroup.controls['phone'];
        this.description = this.formGroup.controls['description'];
        this.ldap = this.formGroup.controls['ldap'];
        this.workingg = this.formGroup.controls['workingg'];
    }

    ngOnInit() {
        this.initForm();
    }

    resetPassword(id: string) {
        this.isLoading = true;
        this._progressRef.start();
        this._systemRepo.resetPasswordUser(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }




}
