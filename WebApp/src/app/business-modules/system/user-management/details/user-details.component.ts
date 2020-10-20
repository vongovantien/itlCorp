import { Component, ViewChild } from '@angular/core';
import { FormAddUserComponent } from '../components/form-add-user/form-add-user.component';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { SystemRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { User, UserLevel } from '@models';
import { ConfirmPopupComponent } from '@common';
import { AppPage } from 'src/app/app.base';
import { ToastrService } from 'ngx-toastr';

import { IAddUser } from '../addnew/user.addnew.component';
import { catchError, finalize } from 'rxjs/operators';
import { RoutingConstants } from '@constants';


@Component({
    selector: 'app-user-details',
    templateUrl: './user-details.component.html'
})
export class UserDetailsComponent extends AppPage {
    @ViewChild(FormAddUserComponent, { static: false }) formAdd: FormAddUserComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    formData: IAddUser = {
        sysEmployeeModel: null,
        username: '',
        userType: '',
        workingStatus: '',
        isLdap: false,
        active: false,
        description: '',
        //
        creditLimit: null,
        creditRate: null,

    };
    userId: string = '';

    headersuslv: CommonInterface.IHeaderTable[] = [
        { title: 'Group Name', field: 'groupName' },
        { title: 'Company', field: 'companyName' },
        { title: 'Office', field: 'officeName' },
        { title: 'Department', field: 'departmentName' },
        { title: 'Position', field: 'position' },
    ];

    userLevels: UserLevel[] = [];
    selectedUserLevel: UserLevel;

    userDetail: any;

    constructor(
        private _activedRouter: ActivatedRoute,
        private _router: Router,
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: Params) => {
            if (param.id) {
                this.userId = param.id;
                this.getDetailUser(this.userId);
                this.getListUserLevelByUserId();
            } else {
                this._router.navigate([`${RoutingConstants.SYSTEM.USER_MANAGEMENT}`]);
            }
        });
    }

    updateUser() {
        this.formAdd.isSubmited = true;
        if (this.formAdd.formGroup.valid) {
            this._progressRef.start();
            const body: any = {
                id: this.userId,
                sysEmployeeModel: {
                    staffCode: this.formAdd.staffcode.value,
                    employeeNameEn: this.formAdd.employeeNameEn.value,
                    employeeNameVn: this.formAdd.employeeNameVn.value,
                    title: this.formAdd.title.value,
                    email: this.formAdd.email.value,
                    tel: this.formAdd.phone.value,
                    bankAccountNo: this.userDetail.sysEmployeeModel.bankAccountNo,
                    bankName: this.userDetail.sysEmployeeModel.bankName,
                    photo: this.userDetail.avatar,

                },
                username: this.formAdd.username.value,
                userType: this.formAdd.usertype.value.value,
                active: this.formAdd.active.value.value,
                workingStatus: this.formAdd.workingg.value.value,
                isLdap: this.formAdd.ldap.value,
                description: this.formAdd.description.value,
                //
                creditLimit: this.formAdd.creditLimit.value,
                creditRate: this.formAdd.creditRate.value,

            };
            this._systemRepo.updateUser(body)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.getDetailUser(this.userId);

                        } else {
                            this._toastService.warning(res.message);
                        }
                    }
                );
        }
    }

    getDetailUser(id: string) {
        this._progressRef.start();
        this._systemRepo.getDetailUser(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.userDetail = res.data;
                        this.formAdd.SelectedUser = new User(res.data);
                        this.formAdd.isDetail = true;
                        this.formData.isLdap = res.data.isLdap;
                        this.formData.username = res.data.username;
                        this.formData.userType = res.data.userType;
                        this.formData.workingStatus = res.data.workingStatus;
                        if (!!res.data.sysEmployeeModel) {
                            this.formAdd.employeeNameEn.setValue(res.data.sysEmployeeModel.employeeNameEn);
                            this.formAdd.employeeNameVn.setValue(res.data.sysEmployeeModel.employeeNameVn);
                            this.formAdd.staffcode.setValue(res.data.sysEmployeeModel.staffCode);
                            this.formAdd.title.setValue(res.data.sysEmployeeModel.title);
                            this.formAdd.email.setValue(res.data.sysEmployeeModel.email);
                            this.formAdd.phone.setValue(res.data.sysEmployeeModel.tel);
                        }

                        this.formAdd.workingg.setValue(this.formAdd.working.filter(i => i.value === res.data.workingStatus)[0]);

                        this.formAdd.ldap.setValue(res.data.isLdap);
                        this.formAdd.formGroup.patchValue(this.formData);
                        this.formAdd.active.setValue(this.formAdd.status.filter(i => i.value === res.data.active)[0]);
                        this.formAdd.usertype.setValue(this.formAdd.usertypes.filter(i => i.value === res.data.userType)[0]);
                        this.formAdd.description.setValue(res.data.description);
                        //
                        this.formAdd.creditLimit.setValue(res.data.creditLimit);
                        this.formAdd.creditRate.setValue(res.data.creditRate);

                    }
                },
            );

    }

    getListUserLevelByUserId() {
        this._systemRepo.getListUserLevelByUserId(this.userId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    if (!!res) {
                        this.userLevels = res;
                    }
                },
            );
    }

    deleteGroup(lv: UserLevel) {
        this.selectedUserLevel = lv;
        this.confirmDeletePopup.show();
    }

    onSubmitDelete() {
        this._systemRepo.deleteUserLevel(this.selectedUserLevel.id)
            .pipe()
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.confirmDeletePopup.hide();
                        this.getListUserLevelByUserId();
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }
}
