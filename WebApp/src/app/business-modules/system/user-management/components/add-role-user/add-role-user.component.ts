import { Component, Input, ViewChild } from '@angular/core';
import { Company, Office, PermissionSample } from '@models';
import { SystemRepo } from '@repositories';
import { finalize, catchError, switchMap, tap } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import cloneDeep from 'lodash/cloneDeep';
import { ConfirmPopupComponent } from '@common';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { Router } from '@angular/router';


@Component({
    selector: 'add-role-user',
    templateUrl: 'add-role-user.component.html'
})

export class AddRoleUserComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @Input() userId: string = '';

    companies: Company[] = [];
    listRoles: PermissionSample[] = [];
    offices: Office[] = [];
    roles: any[] = [];
    officeData: Office[] = [];

    idRole: string = '';
    constructor(
        private _systemRepo: SystemRepo,
        protected _toastService: ToastrService,
        private _progressService: NgProgress,
        private _router: Router
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }
    ngOnInit() {
        this.headers = [
            { title: 'Role Name', field: 'name', },
            { title: 'Company', field: '' },
            { title: 'Office', field: 'officeId' },
        ];
        this.getDataCombobox();
        this.getCompanies();
        // this.getOffices();
        this.getPermissionsByUserId();
    }

    getDataCombobox() {
        this._systemRepo.getComboboxPermission()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.roles = res;
                },
            );
    }

    getCompanies() {
        this._systemRepo.getListCompany()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.companies = res;

                },
            );
    }

    getOffices() {
        this._systemRepo.getListOffices()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.offices = res;
                    this.officeData = res;
                },
            );
    }

    getPermissionsByUserId() {
        this._systemRepo.getListPermissionsByUserId(this.userId).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
            tap((roles: any) => this.listRoles = roles),
            switchMap((listRole: any) => this._systemRepo.getListOffices())
        ).subscribe(
            (res: any) => {
                this.officeData = res;
                console.log("office", this.officeData);
                this.listRoles.forEach(i => {
                    i.offices = res;
                });
                this.listRoles.forEach(item => {
                    item.offices.forEach(i => {
                        if (item.buid === i.buid) {
                            item.offices = this.officeData.filter(x => x.buid === item.buid);
                        }
                    });
                });
                console.log(this.listRoles);
            },
        );
    }

    selectedCompany(id: string, item: any) {
        item.offices = cloneDeep(this.officeData.filter(x => x.buid === id));
        console.log(this.offices);
    }

    addNewLine() {
        const psm = new PermissionSample();
        psm.offices = this.officeData;

        this.listRoles.push(psm);

        console.log(this.listRoles);
    }


    deleteRole(index: number, id: string) {
        if (id !== "" && id !== null) {
            this.idRole = id;
            this.confirmDeletePopup.show();

        } else {
            this.listRoles.splice(index, 1);
        }
    }

    onDeleteRole() {
        if (this.idRole !== null && this.idRole !== "") {
            this.confirmDeletePopup.hide();
            this._progressRef.start();
            this._systemRepo.deleteRole(this.idRole)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                ).subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                            this.getPermissionsByUserId();
                        } else {
                            this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                        }
                    },
                );
        }
    }

    saveRole() {
        if (!this.listRoles.length) {
            this._toastService.warning("Please add role");
            return;
        }
        this.listRoles.forEach(item => {
            item.userId = this.userId;
            if (item.id === '') {
                item.id = null;
            }
        });
        this.addRoleToUser(this.listRoles);
    }

    addRoleToUser(role: PermissionSample[]) {
        this._progressRef.start();
        this._systemRepo.addRoleToUser(role)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.getPermissionsByUserId();
                    } else {
                        if (!!res.data) {
                            this._toastService.error(res.message);
                            if (!!res.data && !!res.data.length) {
                                console.log(res.data);
                            }
                        }
                    }
                }
            );
    }

    gotoUserPermission(id: string) {
        const type = 'user';
        this._router.navigate([`home/system/permission/${type}/${id}`]);
    }




}