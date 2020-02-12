import { Component, ViewChild, Input } from "@angular/core";
import { AppList } from "src/app/app.list";
import { User } from "@models";
import { SystemRepo } from "@repositories";
import { catchError, finalize } from "rxjs/operators";
import { UserLevel } from "src/app/shared/models/system/userlevel";
import { ConfirmPopupComponent } from "@common";
import { ToastrService } from "ngx-toastr";
import { NgProgress } from "@ngx-progressbar/core";
import { Router } from "@angular/router";

@Component({
    selector: 'form-user-level',
    templateUrl: 'add-user.component.html'
})

export class ShareSystemAddUserComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @Input() object: any = {};
    @Input() type: string = null;

    users: any[] = [];
    headers: CommonInterface.IHeaderTable[] = [];
    positions: CommonInterface.INg2Select[];
    usersLevels: UserLevel[] = [];

    value: any = {};
    userData: User[] = [];
    employeeNames: any[] = [];
    indexRemove: number = null;
    isSubmitted: boolean = false;
    isDupUser: boolean = false;
    // userIds: any[] = [];
    criteria: any = {};
    // objUserLevel: UserLevel = new UserLevel();
    idUserLevel: number = null;

    constructor(
        private _systemRepo: SystemRepo,
        protected _toastService: ToastrService,
        private _progressService: NgProgress,
        private _router: Router,

    ) {
        super();
        this._progressRef = this._progressService.ref();

    }
    ngOnInit() {
        this.positions = [
            { id: 'Manager-Leader', text: 'Manager-Leader' },
            { id: 'Deputy', text: 'Deputy' },
            { id: 'Assistant', text: 'Assistant' },
        ];
        this.headers = [
            { title: 'User Name', field: 'username', },
            { title: 'Full Name', field: 'employeeNameVn' },
            { title: 'Position', field: 'Position' },
        ];
        this.getUsers();
        this.queryUserLevel();
    }

    getUsers() {
        this._systemRepo.getListSystemUser({ active: true })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.users = res;
                        console.log(this.users);
                        // this.users = this.users.filter(x => x.active === true);
                    }
                },
            );
    }

    addNewLine() {
        // this.isSubmitted = true;
        // this.objUserLevel = new UserLevel();
        // if (this.usersLevels.length === 0) {
        //     this.employeeNames = [];
        // }
        if (this.usersLevels.length === 0) {
            this.employeeNames = [];

            this.usersLevels.push(new UserLevel());
            console.log(this.usersLevels);
        }
    }

    selectedUser(obj: any, index: number) {
        this.isSubmitted = true;
        // this.fillFullName();
        // this.userIds[index] = this.objUserLevel.userId;

        const object = {};
        const result = [];

        this.usersLevels.forEach(function (item) {
            if (!object[item.userId]) {
                object[item.userId] = 0;
            }
            object[item.userId] += 1;
        });

        for (const prop in object) {
            if (object[prop] >= 2) {
                result.push(prop);
            }
        }

        this.usersLevels.forEach(element => {
            if (result.length === 0) {
                element.isDup = false;
            }
            result.forEach(item => {
                if (element.userId === item) {
                    element.isDup = true;
                } else {
                    element.isDup = false;
                }
            });
        });
        console.log(this.usersLevels);
    }

    deleteUserLevel(index: number, id: number) {
        if (id !== null && id !== 0) {
            this.idUserLevel = id;
            this.confirmDeletePopup.show();

        } else {
            this.indexRemove = index;
            this.usersLevels.splice(this.indexRemove, 1);
            // this.userIds.splice(this.indexRemove, 1);
        }
    }

    onDeleteUserLevel() {
        if (this.idUserLevel !== null) {
            this.confirmDeletePopup.hide();
            this._progressRef.start();
            this._systemRepo.deleteUserLevel(this.idUserLevel)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                ).subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');

                            this.queryUserLevel();
                        } else {
                            this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                        }
                    },
                );
        }
    }

    checkValidate() {
        let valid: boolean = true;
        for (const userlv of this.usersLevels) {
            if (
                userlv.userId === null
                || userlv.position === null
            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }

    saveUserLevel() {
        if (!this.usersLevels.length) {
            this._toastService.warning("Please add user Level");
            return;
        }
        // const checkDupAll = this.usersLevels.filter(x => x.isDup === true);
        // console.log(checkDupAll);

        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }
        if (this.type === 'office') {
            this.usersLevels.forEach(item => {
                item.companyId = this.object.buid;
                item.officeId = this.object.id;
            });

            console.log(this.usersLevels);
            this._progressRef.start();
            this._systemRepo.addUserToOffice(this.usersLevels)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                            this.queryUserLevel();
                        } else {
                            if (!!res.data) {
                                this.usersLevels.forEach(item => {
                                    if (item.id === res.data.id) {
                                        item.isDup = true;
                                    }
                                });
                                if (!!res.data[0]) {
                                    this.usersLevels.forEach(item => {
                                        res.data.forEach(element => {
                                            if (item.userId === element) {
                                                item.isDup = true;
                                            } else {
                                                item.isDup = false;
                                            }
                                        });

                                    });
                                }
                            }
                            this._toastService.error(res.message, '');
                        }
                    }
                );
        }
        if (this.type === 'company') {
            this.usersLevels.forEach(item => {
                item.companyId = this.object.id;
            });
            console.log(this.usersLevels);
        }

        if (this.type === 'department') {
            console.log(this.object);
        }
    }

    // fillFullName() {
    //     this.usersLevels.forEach((item) => {
    //         const objUser = this.users.find(x => x.id === item.userId);
    //         item.employeeNameVn = objUser.employeeNameVn;
    //     });
    // }

    queryUserLevel() {
        if (this.type === 'office') {
            this.criteria.officeId = this.object.id;
            this.criteria.companyId = this.object.buid;
            this.criteria.type = this.type;
        }
        if (this.type === 'company') {
            this.criteria.companyId = this.object.id;
        }
        // TODO another type.

        if (this.type === 'department') {
            this.criteria.departmentId = this.object.id;
            this.criteria.officeId = this.object.branchId;
        }
        this._systemRepo.queryUserLevels(this.criteria).pipe(catchError(this.catchError))
            .subscribe(
                (data: any) => {
                    if (!!data) {
                        this.usersLevels = data.map(lv => new UserLevel(lv));
                        console.log(this.usersLevels);

                        // this.usersLevels = [];
                        // this.usersLevels = data;
                        // this.usersLevels = this.usersLevels.filter(x => x.active === true);
                        // setTimeout(() => {
                        //     this.fillFullName();
                        // }, 300);
                    }
                },
            );
    }

    gotoUserPermission(id: number) {
        if (this.type === 'office') {
            const officeId = this.object.id;
            this._router.navigate([`home/system/permission/${this.type}/${officeId}/${id}`]);
        }
    }
}
