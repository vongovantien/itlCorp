import { Component } from '@angular/core';
import { PermissionCreateComponent } from '../add/add-permission.component';
import { SystemRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { tap, switchMap, catchError, finalize } from 'rxjs/operators';
import { PermissionSample } from 'src/app/shared/models';

@Component({
    selector: 'app-detail-permission',
    templateUrl: './detail-permission.component.html',
    styleUrls: ['./../add/add-permission.component.scss']
})
export class PermissionDetailComponent extends PermissionCreateComponent {

    permissionId: string = '';
    constructor(
        protected _systemRepo: SystemRepo,
        protected _progressService: NgProgress,
        protected _toastService: ToastrService,
        private _activedRouter: ActivatedRoute,
        protected _router: Router,


    ) {
        super(_systemRepo, _progressService, _toastService, _router);
        this._progressRef = this._progressService.ref();
    }

    ngOnInit(): void {
        // this.levelPermissions = [
        //     'Owner', 'Group', 'Department', 'Office', 'Company', 'All'
        // ];
        // this._activedRouter.params
        //     .pipe(
        //         tap((param: Params) => {
        //             if (param.id) {
        //                 this.permissionId = param.id;
        //             }
        //         }),
        //         switchMap(() => this._systemRepo.getPermissionSample(this.permissionId)
        //             .pipe(catchError(this.catchError))
        //         )
        //     )
        //     .subscribe(
        //         (res: any) => {
        //             this.permissionSample = new PermissionSample(res);

        //             setTimeout(() => {
        //                 this.formCreateComponent.formCreate.setValue({
        //                     permissionName: this.permissionSample.name,
        //                     role: this.formCreateComponent.roles.filter(role => role.id === this.permissionSample.roleId)[0],
        //                     type: this.formCreateComponent.types.filter(type => type.value === this.permissionSample.type)[0],
        //                     status: this.formCreateComponent.statuss.filter(status => status.value === this.permissionSample.active)[0],
        //                 });
        //             }, 100);

        //         }
        //     );
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




}
