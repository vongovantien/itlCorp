import { Component, ViewChild, Input } from "@angular/core";
import { AppList } from "src/app/app.list";
import { User } from "@models";
import { SystemRepo } from "@repositories";
import { catchError, finalize } from "rxjs/operators";
import { UserLevel } from "src/app/shared/models/system/userlevel";
import { ConfirmPopupComponent } from "@common";
import { ToastrService } from "ngx-toastr";
import { NgProgress } from "@ngx-progressbar/core";

@Component({
    selector: 'add-user',
    templateUrl: 'add-user.component.html'
})

export class ShareSystemAddUserComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @Input() object: any = {};
    @Input() type: string = null;

    constructor(
        private _systemRepo: SystemRepo,
        protected _toastService: ToastrService,
        private _progressService: NgProgress,
    ) {
        super();
        this._progressRef = this._progressService.ref();

    }
    users: any[] = [];
    headers: CommonInterface.IHeaderTable[] = [];
    positions: CommonInterface.INg2Select[];
    usersLevels: any[] = [];
    value: any = {};
    userData: User[] = [];
    employeeNames: any[] = [];
    indexRemove: number = null;
    isSubmitted: boolean = false;
    isDupUser: boolean = false;
    userIds: any[] = [];
    criteria: any = {};
    objUserLevel: UserLevel = new UserLevel();
    idUserLevel: number = null;


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

    public refreshValue(value: any): void {
        this.value = value;
    }

    getUsers() {
        this._systemRepo.getListSystemUser({})
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.users = res;
                        this.users = this.users.filter(x => x.active === true);
                        console.log(this.users);
                    }
                },
            );
    }

    addNewLine() {
        this.objUserLevel = new UserLevel();
        this.objUserLevel.isDup = false;
        this.isSubmitted = false;
        if (this.usersLevels.length === 0) {
            this.employeeNames = [];
        }

        this.usersLevels.push(this.objUserLevel);
        console.log(this.objUserLevel);
    }



    selectedUser(obj: any, index: number) {
        const objUser = this.users.find(x => x.id === obj.userId);
        this.fillFullName();
        this.userIds[index] = objUser.id;
        console.log(this.userIds);
        const valueArr = this.userIds.map(function (item) { return item; });
        this.isDupUser = valueArr.some(function (item, idx) {
            return valueArr.indexOf(item) !== idx;
        });
        console.log(this.usersLevels);
    }

    deleteUserLevel(index: number, id: number) {
        if (id !== null) {
            this.idUserLevel = id;
            this.confirmDeletePopup.show();

        } else {
            this.indexRemove = index;
            this.usersLevels.splice(this.indexRemove, 1);
            this.userIds.splice(this.indexRemove, 1);
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


        const checkDupAll = this.usersLevels.filter(x => x.isDup === true);
        console.log(checkDupAll);


        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }
        if (this.type === 'office') {
            this._progressRef.start();
            this.usersLevels = this.usersLevels.filter(x => x.id === null);
            this.usersLevels.forEach(item => {
                item.companyId = this.object.buid;
                item.officeId = this.object.id;
                item.id = 0;
                item.groupId = 0;
            });


            console.log(this.usersLevels);
            if (this.usersLevels.length === 0) {
                this._progressRef.complete();
                this._toastService.success('Data Added Success', '');
                this.queryUserLevel();
                return;
            }
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
                            this._toastService.error(res.message, '');
                        }
                    }
                );
        }
    }

    fillFullName() {
        this.usersLevels.forEach((item) => {
            const objUser = this.users.find(x => x.id === item.userId);
            item.employeeNameVn = objUser.employeeNameVn;
            item.isDup = false;
        });
    }

    queryUserLevel() {
        if (this.type === 'office') {
            this.criteria.officeId = this.object.id;
            this.criteria.companyId = this.object.buid;
        }
        this._systemRepo.queryUserLevels(this.criteria).pipe(catchError(this.catchError))
            .subscribe(
                (data: any) => {
                    if (!!data) {
                        this.usersLevels = [];
                        this.usersLevels = data;
                        this.usersLevels = this.usersLevels.filter(x => x.active === true);
                        setTimeout(() => {
                            this.fillFullName();

                        }, 300);
                    }
                },
            );
    }
}
