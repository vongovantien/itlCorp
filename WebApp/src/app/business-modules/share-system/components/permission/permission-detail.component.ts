import { Component, ViewChild } from '@angular/core';
import { SystemRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { tap, switchMap, catchError, finalize } from 'rxjs/operators';
import { PermissionSample } from 'src/app/shared/models';
import { AppPage } from 'src/app/app.base';
import { ConfirmPopupComponent } from '@common';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { PermissionFormCreateComponent } from 'src/app/business-modules/system/permission/components/form-create-permission/form-create-permission.component';

@Component({
    selector: 'detail-permission',
    templateUrl: './permission-detail.component.html',
    styleUrls: ['./../../../system/permission/add/add-permission.component.scss']
})
export class ShareSystemDetailPermissionComponent extends AppPage {
    @ViewChild(PermissionFormCreateComponent, { static: false }) formCreateComponent: PermissionFormCreateComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;

    permissionId: string = '';

    levelPermissions: string[];

    permissionSample: PermissionSample;


    type: string = '';

    userId: string = '';

    id: string = '';

    idUserPermission: string = '';

    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    addButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.add
    };

    constructor(
        protected _systemRepo: SystemRepo,
        protected _progressService: NgProgress,
        protected _toastService: ToastrService,
        private _activedRouter: ActivatedRoute,
        protected _router: Router,


    ) {
        super();
        // super(_systemRepo, _progressService, _toastService, _router);
        this._progressRef = this._progressService.ref();
    }

    ngOnInit(): void {
        this.levelPermissions = [
            'Owner', 'Group', 'Department', 'Office', 'Company', 'All'
        ];
        this._activedRouter.params
            .pipe(
                tap((param: Params) => {
                    if (param.id) {
                        this.permissionId = param.id;
                    }
                    if (param.type) {
                        console.log(param.type);
                        this.type = param.type;
                        this.userId = param.idu;
                        console.log('userid here', this.userId);
                        this.id = param.ido;
                        this.idUserPermission = param.ido;
                    }
                }),
                switchMap(() =>
                    this.type === 'office' ? this._systemRepo.getUserPermission(this.userId, this.id, 'office')
                        : this.type === 'user' ? this._systemRepo.getUserPermission(null, this.idUserPermission, 'user')
                            : this._systemRepo.getPermissionSample(this.permissionId)
                                .pipe(catchError(this.catchError))
                )
            )
            .subscribe(
                (res: any) => {

                    this.permissionSample = new PermissionSample(res);
                    console.log(this.permissionSample);

                    if (this.type !== 'office' && this.type !== 'user') {

                        setTimeout(() => {
                            this.formCreateComponent.formCreate.setValue({
                                permissionName: this.permissionSample.name,
                                role: this.formCreateComponent.roles.filter(role => role.id === this.permissionSample.roleId)[0],
                                type: this.formCreateComponent.types.filter(type => type.value === this.permissionSample.type)[0],
                                status: this.formCreateComponent.statuss.filter(status => status.value === this.permissionSample.active)[0],
                            });

                        }, 100);
                    } else {
                        if (this.permissionSample.id === "") {
                            this._router.navigate([`home/system/office/${this.id}`]);
                            this._toastService.error('This user does not have permission' || 'This user does not have permission', '');
                        }
                    }


                }
            );
    }

    updateUsersPermission() {
        this._progressRef.start();
        this._systemRepo.updateUsersPermission(this.permissionSample)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * get detail
                        if (this.type === 'office') {
                            this._systemRepo.getUserPermission(this.userId, this.id, this.type)
                                .subscribe(
                                    (result: any) => {
                                        if (!!result) {
                                            this.permissionSample = new PermissionSample(result);
                                        }
                                    }
                                );
                        } else if (this.type === 'user') {
                            this._systemRepo.getUserPermission(null, this.idUserPermission, this.type)
                                .subscribe(
                                    (result: any) => {
                                        if (!!result) {
                                            this.permissionSample = new PermissionSample(result);
                                        }
                                    }
                                );
                        }

                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    updatePermission(formDataCreate: { roleName: string; name: string; type: string; roleId: any; active: boolean }) {
        this.updatePermissionModel(formDataCreate);
        this._progressRef.start();
        this._systemRepo.updatePermissionGeneral(this.permissionSample)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * get detail
                        this._systemRepo.getPermissionSample(this.permissionId)
                            .subscribe(
                                (res: any) => {
                                    this.permissionSample = new PermissionSample(res);
                                }
                            )
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onSavePermissionSample() {
        this.confirmPopup.hide();
        if (this.type === 'office' || this.type === 'user') {
            this.updateUsersPermission();
        } else {
            this.formCreateComponent.isSubmitted = true;
            if (this.formCreateComponent.formCreate.valid && !!this.formCreateComponent.role.value) {
                const body: any = {
                    roleName: this.formCreateComponent.formCreate.value.permissionName,
                    name: this.formCreateComponent.formCreate.value.permissionName,
                    roleId: !!this.formCreateComponent.formCreate.value.role ? this.formCreateComponent.formCreate.value.role.id : null,
                    active: this.formCreateComponent.formCreate.value.status.value,
                    type: this.formCreateComponent.formCreate.value.type.value

                };
                this.updatePermission(body);
            }
        }


    }

    showConfirm() {
        this.confirmPopup.show();
    }

    updatePermissionModel(formDataCreate: { roleName: string; name: string; type: string; roleId: any; active: boolean }) {
        this.permissionSample.active = formDataCreate.active;
        this.permissionSample.roleId = formDataCreate.roleId;
        this.permissionSample.roleName = formDataCreate.roleName;
        this.permissionSample.name = formDataCreate.roleName;
        this.permissionSample.type = formDataCreate.type;
    }

    backToDetail() {
        if (this.type === 'office') {
            this._router.navigate([`home/system/office/${this.id}`]);
        } else if (this.type === 'user') {
            this._router.navigate([`home/system/user-management/${this.userId}`]);
        }
    }



}
