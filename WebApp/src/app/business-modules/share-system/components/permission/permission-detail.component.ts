import { Component, ViewChild } from '@angular/core';
import { SystemRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { tap, switchMap, catchError } from 'rxjs/operators';
import { PermissionSample } from 'src/app/shared/models';
import { AppPage } from 'src/app/app.base';
import { ConfirmPopupComponent } from '@common';

@Component({
    selector: 'detail-permission',
    templateUrl: './permission-detail.component.html',
    styleUrls: ['./../../../system/permission/add/add-permission.component.scss']
})
export class ShareSystemDetailPermissionComponent extends AppPage {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;

    permissionId: string = '';

    levelPermissions: string[];

    permissionSample: any;


    type: string = '';

    userId: string = '';

    id: string = '';

    constructor(
        protected _systemRepo: SystemRepo,
        protected _progressService: NgProgress,
        protected _toastService: ToastrService,
        private _activedRouter: ActivatedRoute,
        protected _router: Router,


    ) {
        super();
        // super(_systemRepo, _progressService, _toastService, _router);
        // this._progressRef = this._progressService.ref();
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
                        this.id = param.ido;
                    }
                }),
                switchMap(() => this.type === 'office' ? this._systemRepo.getUserPermission(this.userId, this.id, 'office') : this._systemRepo.getPermissionSample(this.permissionId)
                    .pipe(catchError(this.catchError))
                )
            )
            .subscribe(
                (res: any) => {

                    this.permissionSample = new PermissionSample(res);
                    console.log(this.permissionSample);


                }
            );
    }



    // updatePermission(formDataCreate: { roleName: string; name: string; type: string; roleId: any; active: boolean }) {
    //     this.updatePermissionModel(formDataCreate);

    //     this._systemRepo.updatePermissionGeneral(this.permissionSample)
    //         .pipe(
    //             catchError(this.catchError),
    //             finalize(() => this._progressRef.complete()),
    //         )
    //         .subscribe(
    //             (res: CommonInterface.IResult) => {
    //                 if (res.status) {
    //                     this._toastService.success(res.message);

    //                     // * get detail
    //                     this._systemRepo.getPermissionSample(this.permissionId)
    //                         .subscribe(
    //                             (res: any) => {
    //                                 this.permissionSample = new PermissionSample(res);
    //                             }
    //                         )
    //                 } else {
    //                     this._toastService.error(res.message);
    //                 }
    //             }
    //         );
    // }

    // onSavePermissionSample() {
    //     this.confirmPopup.hide();

    //     this.formCreateComponent.isSubmitted = true;
    //     if (this.formCreateComponent.formCreate.valid && !!this.formCreateComponent.role.value) {
    //         const body: any = {
    //             roleName: this.formCreateComponent.formCreate.value.permissionName,
    //             name: this.formCreateComponent.formCreate.value.permissionName,
    //             roleId: !!this.formCreateComponent.formCreate.value.role ? this.formCreateComponent.formCreate.value.role.id : null,
    //             active: this.formCreateComponent.formCreate.value.status.value,
    //             type: this.formCreateComponent.formCreate.value.type.value

    //         };
    //         this.updatePermission(body);
    //     }
    // }

    showConfirm() {
        this.confirmPopup.show();
    }




}
